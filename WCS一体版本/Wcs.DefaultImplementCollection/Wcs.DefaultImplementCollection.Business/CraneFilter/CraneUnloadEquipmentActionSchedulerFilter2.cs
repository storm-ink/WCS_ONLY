using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs;

using Wcs.Framework;
using NHibernate.Linq;

namespace Wcs.DefaultImplementCollection.Business
{
    /// <summary>
    /// CraneUnloadEquipmentActionSchedulerFilter 和 CraneUnloadEquipmentActionSchedulerFilter2 的区别在于一个是判断传统意义上的占位即DB901的占位 一个是判断简单占位即DB902
    /// CraneUnloadEquipmentActionSchedulerFilter中用到的占位是传统意义上的占位即DB901的占位
    /// CraneUnloadEquipmentActionSchedulerFilter2中用到的占位是简单占位即DB902
    /// </summary>
    public class CraneUnloadEquipmentActionSchedulerFilter2 : EquipmentActionSchedulerFilter
    {
        public override ActionSchedulerFilterResult Filter(EquipmentActionScheduler equipmentActionScheduler, EquipmentAction action)
        {
            if (!(equipmentActionScheduler.Device is CraneDevice))
                return new ActionSchedulerFilterResult(false, "非堆垛机任务");

            String _堆垛机特殊出库策略_当前堆垛机 = "堆垛机特殊出库策略_急速出库_" + equipmentActionScheduler.Device.Name;
            var _启用急速出库 = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<Boolean>(_堆垛机特殊出库策略_当前堆垛机, false);
            if (_启用急速出库)
            {
                ///解决急速出库情况下当堆垛机在出入库口正好有入库任务时不执行入库任务的问题

                CraneDevice _device = (CraneDevice)equipmentActionScheduler.Device;
                var _currentLocation = _device.GetCurrentLocation();
                ///通过当前位置的同义位判断是否存在一个非堆垛机位置来确定堆垛机属否在出入口
                ///如果在出入口位置的情况下是否存在一个入库任务getDistance小于当前任务
                if (_currentLocation.Synonymous.Any(x => !(x is RackLocation)))
                {
                    ///获取所有入库口位置
                    var racks = _device.Locations.Select(x => (RackLocation)x).Where(x => x.ForkAction == ForkAction.Pickup).ToArray();
                    ///★★★★★★★★★★尝试获取附近的出入口？？？如何判断是相近的出入口？？？后期可以尝试用分组来解决，将出入库口分组，暂时先写死兼容 + -1
                    var enters = racks.Where(x => (x.Column + 1 >= _currentLocation.Column && x.Column - 1 <= _currentLocation.Column) || (x.Level + 1 >= _currentLocation.Level && x.Level - 1 <= _currentLocation.Level)).ToArray();
                    if (enters.Count() != 0)
                    {
                        var _enters = enters.Select(x => x.DeviceCode).ToArray();
                        if (_enters.Any(x => x == action.Movement.StartLocation.DeviceCode))
                            return new ActionSchedulerFilterResult(false, "堆垛机当前在出库口位置，同时检测到当前任务为附近入口的入库任务，此入库任务可以执行");
                        ///目前没有什么要求条件显示入库任务不可以执行
                        List<CraneAutomaticTransferWithStepByStepAction> _actions;
                        using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                        {
                            _actions = unitOfWork.session.Query<CraneAutomaticTransferWithStepByStepAction>().Where(x => _enters.Contains(x.LoadLocation.DeviceCode) && x.Status == EquipmentActionStatus.New).ToList();
                            unitOfWork.Commit();
                        }
                        if (_actions.Count() != 0 && _actions.Any(x=>x.Id != action.Id))
                            return new ActionSchedulerFilterResult(true, "堆垛机当前在出库口位置，同时检测到附近入口有入库任务，此种情况优先执行入库任务");
                    }
                }

                return new ActionSchedulerFilterResult(false, "已启用急速出库");
            }

            Location unloadLocation;
            if (action is CraneAutomaticTransferWithStepByStepAction)
            {
                var craneAutomaticTransferAction = (CraneAutomaticTransferWithStepByStepAction)action;
                unloadLocation = LocationConverter.ToLocation(craneAutomaticTransferAction.UnloadLocation);
                if (unloadLocation is RackLocation && unloadLocation.Synonymous.All(x => x is RackLocation))
                {
                    return new ActionSchedulerFilterResult(false, string.Format("结束位置 {0} 在货架内",unloadLocation));
                }
            }
            else if (action is CraneAutomaticTransferAction)
            {
                var craneAutomaticTransferAction = (CraneAutomaticTransferAction)action;
                unloadLocation = LocationConverter.ToLocation(craneAutomaticTransferAction.UnloadLocation);
                if (unloadLocation is RackLocation && unloadLocation.Synonymous.All(x => x is RackLocation))
                {
                    return new ActionSchedulerFilterResult(false, string.Format("结束位置 {0} 在货架内",unloadLocation));
                }
            }
            else
                return new ActionSchedulerFilterResult(false, "非需要过滤的堆垛机任务类型");

            ConveyorLocation conveyorLocation;
            if (unloadLocation is ConveyorLocation)
            {
                conveyorLocation = (ConveyorLocation)unloadLocation;
            }
            else
            {
                conveyorLocation = (ConveyorLocation)unloadLocation.Synonymous.Single(x => x is ConveyorLocation);
            }

            ConveyorDevice conveyorDevice = (ConveyorDevice)conveyorLocation.Device;
            SimpleHoldSignalNetTransferObject holdSignal = null;
            if (conveyorDevice.OccupiedSignals != null)
                holdSignal = conveyorDevice.ReadStatus<SimpleHoldSignalNetTransferObject>().FirstOrDefault(x => x.PosNo == Convert.ToInt32(conveyorLocation.DeviceCode));
            if (holdSignal == null)
                return new ActionSchedulerFilterResult(true, string.Format("{0} 未配置占位信息或设备信息未同步", conveyorLocation)); 
            if (holdSignal.HandShake == HoldSignalNetTransferObjectHandShake.New)
                return new ActionSchedulerFilterResult(true, string.Format("{0} 有占位，当前无法卸货", conveyorLocation));

            if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("CraneCommandVersion", "") != "V2")
            {
                OccupyNetTransferObject occupystate = null;
                if (conveyorDevice.OccupyStatus != null)
                    occupystate = conveyorDevice.OccupyStatus.FirstOrDefault(x => x.PosNo == Convert.ToInt32(conveyorLocation.DeviceCode));
                if (occupystate == null)
                    return new ActionSchedulerFilterResult(true, string.Format("{0} 未配置光电信号或设备信息未同步", conveyorLocation));
                if (occupystate.AftPosPotocell || occupystate.AftProPotocell || occupystate.AftSloPotocell || occupystate.FroPosPotocell || occupystate.FroProPotocell || occupystate.FroSloPotocell)
                    return new ActionSchedulerFilterResult(true, string.Format("{0} 光电有信号，当前无法卸货", conveyorLocation));
            }

            LocationTaskNetTransferObject locationTask = null;
            if (conveyorDevice.LocationCurrentTasks != null)
                locationTask = conveyorDevice.LocationCurrentTasks.FirstOrDefault(x => x.PosNo == Convert.ToInt32(conveyorLocation.DeviceCode));
            if (locationTask == null)
                return new ActionSchedulerFilterResult(true, string.Format("{0} 未配置货位任务或设备信息未同步", conveyorLocation));
            if (locationTask.TaskNo != 0)
                return new ActionSchedulerFilterResult(true, string.Format("{0} 任务号不为0，当前无法卸货", conveyorLocation));

            Boolean _task;
            String[] _locs = conveyorLocation.Synonymous.Select(x=>x.UserCode).ToArray();
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                _task = unitOfWork.session.Query<Task>().Any(x => x.CurrentLocation.UserCode == conveyorLocation.UserCode || _locs.Contains(x.CurrentLocation.UserCode));
                unitOfWork.Commit();
            }
            if (_task)
                return new ActionSchedulerFilterResult(true, string.Format("至少存在一条当前位置在 {0}/{1} 的任务，当前无法卸货", conveyorLocation.UserCode, String.Join("/", _locs)));

            return new ActionSchedulerFilterResult(false, "出库口可以放货");
        }
    }
}
