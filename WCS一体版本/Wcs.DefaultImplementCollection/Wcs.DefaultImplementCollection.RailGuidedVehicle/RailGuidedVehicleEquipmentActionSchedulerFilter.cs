using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs;
using Wcs.DefaultImpls.Conveyor;
using Wcs.Framework;
using NHibernate.Linq;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    public sealed class RailGuidedVehicleEquipmentActionSchedulerFilter : EquipmentActionSchedulerFilter
    {
        public override ActionSchedulerFilterResult Filter(EquipmentActionScheduler equipmentActionScheduler, EquipmentAction action)
        {
            if (!(action is RailGuidedVehicleStepByStepAction))
                throw new InvalidOperationException(String.Format("{0} 不是穿梭车动作", action));

            RailGuidedVehicleStepByStepAction rgvAct = (RailGuidedVehicleStepByStepAction)action;

            var unloadStation = (RailGuidedVehicleStation)LocationConverter.ToLocation(rgvAct.UnloadLocation);
            var loc = unloadStation.Synonymous.FirstOrDefault(x => x is ConveyorLocation);
            if (loc != null)
            {
                ConveyorLocation conveyorLocation = (ConveyorLocation)loc;
                List<ConveyorLocation> _list = new List<ConveyorLocation>();
                _list.Add(conveyorLocation);
                if (conveyorLocation.IsFictitiousLocation || conveyorLocation.HasFictitousLocation)
                    _list.Add((ConveyorLocation)LocationConverter.ConvertibleCodeToLcation(conveyorLocation.FictitiousConvertibleCode));

                ///这个条件的前提是 卸货点及虚拟货位必须在同一个设备内
                if (!_list.Any(x => x.Device.Name != conveyorLocation.Device.Name))
                {
                    ConveyorDevice _conveyor = (ConveyorDevice)conveyorLocation.Device;
                    if (!_conveyor.IsConnected)
                        return new ActionSchedulerFilterResult(true, "所在输送线设备未连接");

                    var poss = _list.Select(x => Convert.ToUInt16(x.DeviceCode));
                    List<LocationTaskNetTransferObject> _locTasks = _conveyor.LocationCurrentTasks.Where(x => poss.Contains(x.PosNo)).ToList();
                    if (_locTasks.Count() == 0 || _locTasks.Count() != poss.Count())
                        return new ActionSchedulerFilterResult(true, "所在输送线未配置或者缺少卸货点货位任务信息");
                    if (_locTasks.Any(x => x.TaskNo != 0))
                    {
                        var taskNos = _locTasks.Select(x => x.TaskNo).Where(x => x != 0);
                        if (_conveyor.LocationCurrentTasks.Any(x => taskNos.Contains(x.TaskNo) && !poss.Contains(x.PosNo)) 
                            && _conveyor.Tasks.Any(x=>taskNos.Contains(x.AssignmentID) && poss.Contains(x.StartMotorNo)))
                            return new ActionSchedulerFilterResult(false, "卸货点开始执行送货任务");
                    }
                }
                ///这个条件的前提是 卸货点及虚拟货位必须在同一个设备内

                foreach (var item in _list)
                {
                    ConveyorDevice _conveyor = (ConveyorDevice)item.Device;
                    if (!_conveyor.IsConnected)
                        return new ActionSchedulerFilterResult(true, "所在输送线设备未连接");

                    if (_list.Count() == 1)
                    {
                        LocationTaskNetTransferObject _locTask = _conveyor.LocationCurrentTasks.FirstOrDefault(x => x.PosNo == Convert.ToUInt16(item.DeviceCode));
                        if (_locTask != null && _locTask.TaskNo != 0 && _conveyor.LocationCurrentTasks.Any(x => x.TaskNo == _locTask.TaskNo && x.PosNo != _locTask.PosNo))
                            return new ActionSchedulerFilterResult(false, "卸货点开始执行送货任务");
                    }

                    ///判断占位
                    HoldSignalNetTransferObject holdSingle = _conveyor.OccupiedSignals.FirstOrDefault(x => x.PosNo == Convert.ToUInt16(item.DeviceCode));
                    SimpleHoldSignalNetTransferObject simpleHoldSingle = _conveyor.ReadStatus<SimpleHoldSignalNetTransferObject>().FirstOrDefault(x => x.PosNo == Convert.ToUInt16(item.DeviceCode));
                    if (holdSingle == null && simpleHoldSingle == null)
                        return new ActionSchedulerFilterResult(true, "所在设备占位信息和简单占位都未空");
                    if (holdSingle != null && holdSingle.HandShake != HoldSignalNetTransferObjectHandShake.Empty)
                        return new ActionSchedulerFilterResult(true, "卸货点占位信息不为空");
                    if (simpleHoldSingle != null && simpleHoldSingle.HandShake != HoldSignalNetTransferObjectHandShake.Empty)
                        return new ActionSchedulerFilterResult(true, "卸货点简单占位信息不为空");
                    ///判断光电
                    OccupyNetTransferObject occupy = _conveyor.OccupyStatus.FirstOrDefault(x => x.PosNo == Convert.ToUInt16(item.DeviceCode));
                    if (occupy == null)
                        return new ActionSchedulerFilterResult(true, "所在设备未查询到光电信息");
                    if (occupy.AftPosPotocell || occupy.AftProPotocell || occupy.AftSloPotocell || occupy.FroPosPotocell || occupy.FroProPotocell || occupy.FroSloPotocell)
                        return new ActionSchedulerFilterResult(true, "卸货点货位光电有信号");
                    ///判断任务
                    LocationTaskNetTransferObject _locationTask = _conveyor.LocationCurrentTasks.FirstOrDefault(x => x.PosNo == Convert.ToUInt16(item.DeviceCode));
                    if (_locationTask == null)
                        return new ActionSchedulerFilterResult(true, "卸货点货位任务信息为空");
                    if (_locationTask.TaskNo != 0)
                        return new ActionSchedulerFilterResult(true, "卸货点货位任务号不为0");
                    ///判断输送线状态
                    LocationNetTransferObject locationState = _conveyor.ConveyorLocationStates.FirstOrDefault(x => x.PosNo == Convert.ToUInt16(item.DeviceCode));
                    if (locationState == null)
                        return new ActionSchedulerFilterResult(true, "卸货点货位状态获取失败");
                    if (locationState.Status == LocationNetTransferObjectStatus.Manual || locationState.Status == LocationNetTransferObjectStatus.Offline || locationState.Status == LocationNetTransferObjectStatus.Running)
                        return new ActionSchedulerFilterResult(true, "卸货点输送线处于手动、离线或者运行状态");
                    ///查询数据库
                    Boolean _task;
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                    {
                        _task = unitOfWork.session.Query<Task>().Any(x => x.CurrentLocation.DeviceCode == item.DeviceCode);
                        unitOfWork.Commit();
                    }
                    if (_task)
                        return new ActionSchedulerFilterResult(true, String.Format("至少存在一条任务当前位置在{0}位置，当前不能发送放货命令", item.DeviceCode));
                }
            }

            return new ActionSchedulerFilterResult(false, "");
        }
    }
}
