using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.DefaultImpls.Conveyor;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.SingleLocationDoubleVehicleManager
{
    public class UnLoadStationHoldSingleEquipmentActionSchedulerFilter : EquipmentActionSchedulerFilter
    {
        public override ActionSchedulerFilterResult Filter(EquipmentActionScheduler equipmentActionScheduler, EquipmentAction action)
        {
            if (!(equipmentActionScheduler.Device is SingleLocationDoubleVehicleSubSystem))
                return new ActionSchedulerFilterResult(false, "非单货位一轨双车任务");

            var _device = (SingleLocationDoubleVehicleSubSystem)equipmentActionScheduler.Device;
            Location unloadLocation;
            var _action = (SingleLocationDoubleVehicleSubSystemTransferAction)action;
            unloadLocation = LocationConverter.ToLocation(_action.EndLocation);

            if (_device.Config.GetFreedomStationList().Contains(_action.EndLocation.UserCode))
                return new ActionSchedulerFilterResult(false, "取消过滤站点");

            ConveyorLocation conveyorLocation;
            if (unloadLocation is ConveyorLocation)
                conveyorLocation = (ConveyorLocation)unloadLocation;
            else
                conveyorLocation = (ConveyorLocation)unloadLocation.Synonymous.Single(x => x is ConveyorLocation);

            ConveyorDevice conveyorDevice = (ConveyorDevice)conveyorLocation.Device;
            var simpleHoldSignals = conveyorDevice.ReadStatus<SimpleHoldSignalNetTransferObject>();
            SimpleHoldSignalNetTransferObject holdSignal = null;
            //是否需要检测光电
            bool _checkSensor = true;
            if (simpleHoldSignals != null)
                holdSignal = simpleHoldSignals.FirstOrDefault(x => x.PosNo == Convert.ToInt32(conveyorLocation.DeviceCode));
            if (holdSignal == null)
                return new ActionSchedulerFilterResult(true, string.Format("{0} 未配置占位信息或设备信息未同步", conveyorLocation));
            if (holdSignal.HandShake == HoldSignalNetTransferObjectHandShake.New)
            {
                //是否可以提前放行
                LocationTaskNetTransferObject __locationTask = null;
                if (conveyorDevice.LocationCurrentTasks != null)
                    __locationTask = conveyorDevice.LocationCurrentTasks.FirstOrDefault(x => x.PosNo == Convert.ToInt32(conveyorLocation.DeviceCode));
                if (__locationTask == null)
                    return new ActionSchedulerFilterResult(true, $"{conveyorLocation} 有占位，{conveyorLocation} 未配置货位任务或设备信息未同步");
                if (__locationTask.TaskNo != 0)
                {
                    var _locationTask = conveyorDevice.LocationCurrentTasks.FirstOrDefault(x => x.TaskNo == __locationTask.TaskNo && x.PosNo != __locationTask.PosNo);
                    if (_locationTask == null)
                        return new ActionSchedulerFilterResult(true, $"{conveyorLocation} 货位任务号不等于0，当前无法卸货");
                    var __task = conveyorDevice.Tasks.FirstOrDefault(x => x.AssignmentID == __locationTask.TaskNo);
                    if (__task == null)
                        return new ActionSchedulerFilterResult(true, $"{conveyorLocation} 有占位，并且{conveyorLocation} 货位任务号不等于0，PLC任务列表无该条任务（任务号{__locationTask.TaskNo}）记录，无法判断后续任务去向，暂时无法发送任务");
                    else if (__task.StartMotorNo != __locationTask.PosNo)
                        return new ActionSchedulerFilterResult(true, $"{conveyorLocation} 有占位，并且{conveyorLocation} 货位任务号不等于0，PLC任务列表该条任务（任务号{__locationTask.TaskNo}）记录起点不是{conveyorLocation}位置，暂时无法发送任务");
                    else
                        _checkSensor = false;
                    //return new ActionSchedulerFilterResult(false, "出库口为单节输送机，并且该节任务已经传递到下一节，允许提前放行");
                }
                else
                    return new ActionSchedulerFilterResult(true, $"卸货点{conveyorLocation.DeviceCode}有简单占位且任务号为0，系统判断无法提前放行");
            }

            if (_checkSensor)
            {
                OccupyNetTransferObject occupystate = null;
                if (conveyorDevice.OccupyStatus != null)
                    occupystate = conveyorDevice.OccupyStatus.FirstOrDefault(x => x.PosNo == Convert.ToInt32(conveyorLocation.DeviceCode));
                if (occupystate == null)
                    return new ActionSchedulerFilterResult(true, string.Format("{0} 未配置光电信号或设备信息未同步", conveyorLocation));
                if (occupystate.AftPosPotocell || occupystate.AftProPotocell || occupystate.AftSloPotocell || occupystate.FroPosPotocell || occupystate.FroProPotocell || occupystate.FroSloPotocell)
                    return new ActionSchedulerFilterResult(true, string.Format("{0} 光电有信号，当前无法卸货", conveyorLocation));

                LocationTaskNetTransferObject locationTask = null;
                if (conveyorDevice.LocationCurrentTasks != null)
                    locationTask = conveyorDevice.LocationCurrentTasks.FirstOrDefault(x => x.PosNo == Convert.ToInt32(conveyorLocation.DeviceCode));
                if (locationTask == null)
                    return new ActionSchedulerFilterResult(true, string.Format("{0} 未配置货位任务或设备信息未同步", conveyorLocation));
                if (locationTask.TaskNo != 0)
                    return new ActionSchedulerFilterResult(true, string.Format("{0} 任务号不为0，当前无法卸货", conveyorLocation));
            }

            //卸货任务只执行一条
            if (equipmentActionScheduler is SingleLocationDoubleVehicleSubSystemEquipmentActionScheduler scheduler)
            {
                var actions = scheduler.Actions.Where(x => x.Id != action.Id && x.Movement.EndLocation.DeviceCode == _action.EndLocation.DeviceCode && x.Status != EquipmentActionStatus.Cancelled && x.Status != EquipmentActionStatus.Completed);
                if (actions.Count() > 0)
                {
                    if (actions.Any(x => x.Status == EquipmentActionStatus.Executing))
                        return new ActionSchedulerFilterResult(true, string.Format("至少存在一条当前卸货点是{0}的任务在 执行中 状态，当前无法卸货", conveyorLocation));
                    if (actions.Count(x => x.Status != EquipmentActionStatus.Executing) > 0)
                    {
                        var unExecutingActions = actions.Where(x => x.Status != EquipmentActionStatus.Executing).Select(x => x.Id);
                        if (scheduler.RestoreInfos.Any(x => unExecutingActions.Contains(x.EquipmentActionId)))
                            return new ActionSchedulerFilterResult(true, string.Format("至少存在一条当前卸货点是{0}的任务在 新任务/暂停/错误 状态，但是已经分配给车辆待执行，当前无法卸货", conveyorLocation));
                    }
                }
            }
            if (equipmentActionScheduler is SingleLocationDoubleVehicleSubSystemEquipmentActionScheduler2 scheduler2)
            {
                var actions = scheduler2.Actions.Where(x => x.Id != action.Id && x.Movement.EndLocation.DeviceCode == _action.EndLocation.DeviceCode && x.Status != EquipmentActionStatus.Cancelled && x.Status != EquipmentActionStatus.Completed);
                if (actions.Count() > 0)
                {
                    if (actions.Any(x => x.Status == EquipmentActionStatus.Executing))
                        return new ActionSchedulerFilterResult(true, string.Format("至少存在一条当前卸货点是{0}的任务在 执行中 状态，当前无法卸货", conveyorLocation));
                    if (actions.Count(x => x.Status != EquipmentActionStatus.Executing) > 0)
                    {
                        var unExecutingActions = actions.Where(x => x.Status != EquipmentActionStatus.Executing).Select(x => x.Id);
                        if (scheduler2.RestoreInfos.Any(x => unExecutingActions.Contains(x.EquipmentActionId)))
                            return new ActionSchedulerFilterResult(true, string.Format("至少存在一条当前卸货点是{0}的任务在 新任务/暂停/错误 状态，但是已经分配给车辆待执行，当前无法卸货", conveyorLocation));
                    }
                }
            }

            Boolean _task;
            String[] _locs = conveyorLocation.Synonymous.Select(x => x.UserCode).ToArray();
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                _task = unitOfWork.session.Query<Task>().Any(x => x.CurrentLocation.UserCode == conveyorLocation.UserCode || _locs.Contains(x.CurrentLocation.UserCode));
                unitOfWork.Commit();
            }
            if (_task)
                return new ActionSchedulerFilterResult(true, string.Format("至少存在一条当前位置在 {0}/{1} 的任务，当前无法卸货", conveyorLocation.UserCode, String.Join("/", _locs)));

            return new ActionSchedulerFilterResult(false, "卸货点可以放货");
        }
    }
}
