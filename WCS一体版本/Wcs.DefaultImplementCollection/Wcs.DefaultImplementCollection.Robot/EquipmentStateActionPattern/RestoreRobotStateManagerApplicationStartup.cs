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

namespace Wcs.DefaultImplementCollection.Robot
{
    /// <summary>
    /// Robot状态上下文恢复程序
    /// </summary>
    public sealed class RestoreRobotStateManagerApplicationStartup : Wcs.Framework.IApplicationStartup
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();

        public void Initialize(StartupElement element)
        {
        }

        public void Run(Wcs.IWcsApplication application)
        {
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                var items = unitOfWork.session.Query<StateManagerRestoreInfo>().ToList();
                var equipmentIds = items.Where(x => x.EquipmentActionId != 0).Select(x => x.EquipmentActionId).ToArray();
                _logger.Trace1(string.Format("共找到 {0} 个需要加载的状态上下文信息...", items.Count), this);
                var equipmentActions = unitOfWork.session.Query<RobotTransferByStepAction>()
                    .Where(x => equipmentIds.Contains(x.Id) 
                    && x.Status != EquipmentActionStatus.Cancelled 
                    && x.Status != EquipmentActionStatus.Completed
                    && x.Status != EquipmentActionStatus.Error
                    ).ToList();

                int count = 0;
                foreach (var item in items)
                {
                    var act = (RobotTransferByStepAction)equipmentActions.FirstOrDefault(x => x.Id == item.EquipmentActionId);
                    if (act == null)
                    {
                        _logger.Warn1(string.Format("未找到 {0} 关联的物理动作 #{1}或其不是Robot单步任务上下文,未恢复该上下文", item, item.EquipmentActionId), this);
                    }
                    else
                    {
                        var device = DeviceConverter.ToDevice<TaskableDevice>(act.DeviceName);
                        var sm = AbstractStateManager.CreateOrGetContext<RobotByStepStateManager>(act, device);

                        _logger.Trace1(string.Format("{0} 已恢复.", sm), this);
                        count++;
                    }
                }

                unitOfWork.Commit();

                _logger.Trace1(string.Format("共恢复了 {0} 个状态上下文.", count), this);
            }
        }
    }
}
