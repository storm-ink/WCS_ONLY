using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Linq;
using Wcs;
using Wcs.Framework;
using Wcs.Framework.Cfg;
using NLog;
using System.Threading;
namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 双货叉堆垛机状态上下文恢复程序
    /// </summary>
    public sealed class RestoreCraneStateManagerApplicationStartup : Wcs.Framework.IApplicationStartup
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();
        static Thread _thread;
        StartupElement _element;
        Int32 _interval = 0;

        public void Initialize(StartupElement element)
        {
            _element = element;
        }

        public void Run(Wcs.IWcsApplication application)
        {
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                var items = unitOfWork.session.Query<StateManagerRestoreInfo>().ToList();
                var equipmentIds = items.Where(x => x.EquipmentActionId != 0).Select(x => x.EquipmentActionId).ToArray();
                _logger.Trace1(string.Format("共找到 {0} 个需要加载的状态上下文信息...", items.Count), this);
                var equipmentActions = unitOfWork.session.Query<CraneAutomaticTransferWithStepByStepAction>()
                    .Where(x => equipmentIds.Contains(x.Id))
                    .ToList();

                int count = 0;
                foreach (var item in items)
                {
                    var act = (CraneAutomaticTransferWithStepByStepAction)equipmentActions.FirstOrDefault(x => x.Id == item.EquipmentActionId);
                    if (act == null)
                    {
                        _logger.Warn1(string.Format("未找到 {0} 关联的物理动作 #{1}或其不是单货叉堆垛机单步任务上下文,未恢复该上下文", item, item.EquipmentActionId), this);
                    }
                    else
                    {
                        var device = DeviceConverter.ToDevice<TaskableDevice>(act.DeviceName);
                        var sm = AbstractStateManager.CreateOrGetContext<PickingAndUnloadingWithStepByStepStateManager>(act, device);

                        var action = device.EquipmentActionScheduler.Actions.FirstOrDefault(x => x.EquipmentTaskId == sm.EquipmentAction.EquipmentTaskId);
                        if (action != null)
                            ((CraneAutomaticTransferWithStepByStepAction)action).StateManager = sm;

                        _logger.Trace1(string.Format("{0} 已恢复.", sm), this);
                        count++;
                    }
                }

                unitOfWork.Commit();

                _logger.Trace1(string.Format("共恢复了 {0} 个状态上下文.", count), this);

                _interval = _element.GetAttributeOrDefault<Int32>("interval", 600000);
                ParameterizedThreadStart start = new ParameterizedThreadStart(pro);
                _thread = new Thread(start);
                _thread.IsBackground = true;
                _thread.Start();

                _logger.Info("StateManagerRestorInfo清理线程已经启动", this);
            }
        }

        private void pro(object obj)
        {
            while (true)
            {
                Thread.Sleep(_interval);
                try
                {
                    List<StateManagerRestoreInfo> deleteStateManagerRestoreInfos = new List<StateManagerRestoreInfo>();
                    List<StateManagerRestoreInfo> noDeleteStateManagerRestoreInfos = new List<StateManagerRestoreInfo>();
                    StateManagerRestoreInfo[] stateManagerRestoreInfos;
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                    {
                        stateManagerRestoreInfos = unitOfWork.session.Query<StateManagerRestoreInfo>().ToArray();
                        var equipmentIds = stateManagerRestoreInfos.Where(x => x.EquipmentActionId != 0).Select(x => x.EquipmentActionId).ToArray();
                        _logger.Trace1(string.Format("共找到 {0} 个可能需要处理的状态上下文信息...", stateManagerRestoreInfos.Length), this);
                        var equipmentActionIds = unitOfWork.session.Query<EquipmentAction>()
                            .Where(x => equipmentIds.Contains(x.Id)).Select(x => x.EquipmentTaskId)
                            .ToList();

                        stateManagerRestoreInfos = stateManagerRestoreInfos.Where(x => !equipmentActionIds.Contains(x.EquipmentActionId)).ToArray();
                        foreach (var item in stateManagerRestoreInfos)
                        {
                            var _confirmSeconed = unitOfWork.session.Query<EquipmentAction>().Any(x => x.Id == item.EquipmentActionId);
                            if (!_confirmSeconed)
                            {
                                unitOfWork.session.Delete(item);
                                deleteStateManagerRestoreInfos.Add(item);
                            }
                            else
                                noDeleteStateManagerRestoreInfos.Add(item);
                        }
                        unitOfWork.Commit();
                    }
                    if (deleteStateManagerRestoreInfos.Count() != 0)
                        _logger.Info(String.Format("共删除 {0} 个状态上下文信息{1}", deleteStateManagerRestoreInfos.Count, String.Join("/", deleteStateManagerRestoreInfos.Select(x => x.ToString())).ToArray()));
                    if (noDeleteStateManagerRestoreInfos.Count() != 0)
                        _logger.Info(String.Format("共计算需要删除 {0} 个状态上下文信息{1}，但是二次确认时未删除", deleteStateManagerRestoreInfos.Count, String.Join("/", noDeleteStateManagerRestoreInfos.Select(x => x.ToString())).ToArray()));
                }
                catch (Exception ex)
                {
                    _logger.Error("清理过时 StateManagerRestorInfo 时发生异常，异常信息：" + ex.Message, this);
                }
            }
        }
    }
}
