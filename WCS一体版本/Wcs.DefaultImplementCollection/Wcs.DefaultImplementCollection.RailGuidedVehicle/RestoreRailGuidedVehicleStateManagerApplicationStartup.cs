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
namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    /// <summary>
    /// 穿梭车状态上下文恢复程序
    /// </summary>
    public sealed class RestoreRailGuidedVehicleStateManagerApplicationStartup:Wcs.Framework.IApplicationStartup
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();

        public void Run(Wcs.IWcsApplication application)
        {
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                var items = unitOfWork.session.Query<StateManagerRestoreInfo>().ToList();
                var equipmentIds = items.Select(x => x.EquipmentActionId).ToArray();
                _logger.Trace1(string.Format("共找到 {0} 个需要加载的状态上下文信息...", items.Count), this);
                var equipmentActions = unitOfWork.session.Query<RailGuidedVehicleStepByStepAction>()
                    .Where(x => equipmentIds.Contains(x.Id))
                    .ToList();

                int count = 0;
                foreach (var item in items)
                {
                    var act = equipmentActions.FirstOrDefault(x => x.Id == item.EquipmentActionId);
                    if (act == null)
                    {
                        _logger.Warn1(string.Format("未找到 {0} 关联的物理动作 #{1}或其不是穿梭车上下文,未恢复该上下文", item, item.EquipmentActionId), this);
                    }
                    else
                    {
                        var device=DeviceConverter.ToDevice<TaskableDevice>(act.DeviceName);
                        var sm = AbstractStateManager.CreateOrGetContext<PickingAndUnloadingWithStepByStepStateManager>(act, device);

                        var action = device.EquipmentActionScheduler.Actions.FirstOrDefault(x => x.EquipmentTaskId == sm.EquipmentAction.EquipmentTaskId);
                        if (action != null)
                            ((RailGuidedVehicleStepByStepAction)action).StateManager = sm;

                        _logger.Trace1(string.Format("{0} 已恢复.",sm), this);
                        count++;
                    }
                }

                _logger.Trace1(string.Format("共恢复了 {0} 个状态上下文.", count), this);
            }
        }

        public void Initialize(StartupElement element)
        {

        }
    }
}
