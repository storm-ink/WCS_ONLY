using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NHibernate.Linq;
using System.Threading;
using Wcs.Framework.Events;
using Wcs.Framework.EventBus;
using Wcs.MethodTrack;
using Wcs;
using Wcs.Framework;
using Wcs.DefaultImplementCollection.RailGuidedVehicle;

namespace Wcs.DefaultImplementCollection.SingleLocationDoubleVehicleManager
{
    public class SingleLocationDoubleVehicleSubSystemEquipmentActionScheduler2 : EquipmentActionScheduler
    {
        List<SingleLocationDoubleVehicleSubSystemLocation> SingleLocationDoubleVehicleSubSystemLocations;
        public SingleLocationDoubleVehicleSubSystemEquipmentActionScheduler2(TaskableDevice device, EquipmentActionSchedulerFilter[] actionFilters)
            : base(device, actionFilters)
        {
        }

        protected SingleLocationDoubleVehicleSubSystemEquipmentActionScheduler2()
            : base()
        {
        }

        /// <summary>
        /// 获取当前可以发送的动作集合（有可能返回 null）
        /// </summary>
        /// <returns></returns>
        protected override IOrderedEnumerable<EquipmentAction> GetAvailableActions()
        {
            _GetAvailableActionsDescription = null;

            if (ExecutiveState.ApplicationExiting)
            {
                _GetAvailableActionsDescription = "应用程序正在退出，不再下发任何物理动作。";
            }

            lock (_currentActionLocker)
            {
                if (!this.Device.AllowConcurrency && this.CurrentAction != null)
                {
                    if (this.CurrentAction.Status == EquipmentActionStatus.New || this.CurrentAction.Status == EquipmentActionStatus.Cancelled)
                    {
                        return (new EquipmentAction[] { this.CurrentAction })
                            .OrderBy(x => x.Id);
                    }
                    else
                    {
                        _GetAvailableActionsDescription = string.Format("当前动作 {0} 的状态不为 New", this.CurrentAction);
                        return null;
                    }
                }
            }
            lock (_actions)
            {
                StringBuilder sb = new StringBuilder();
                var result = this.Actions.Where(x =>
                                                    checkActionStatus(x, sb)
                                                    && ActionFilters.All((filter) =>
                                                    {
                                                        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                                                        sw.Start();
                                                        var r = filter.Filter(this, x);
                                                        sw.Stop();
                                                        this.Log($"任务{x.Movement.Task}，物理动作{x},任务过滤器{filter.GetType()}过滤结果({r.Defeated + "," + r.Reason}),耗时{sw.ElapsedMilliseconds}ms");
                                                        if (r.Defeated)
                                                        {
                                                            sb.AppendLine(string.Format("{0} 被 {1} 否决，原因是 {2}", x, filter, r.Reason));
                                                            _methodDescriptorTree["是否被过滤器否决"].Access(false, string.Format("{0} 被 {1} 否决了发送操作，原因：{2}。", x, filter, r.Reason));
                                                        }
                                                        return r.Defeated == false;
                                                    }))
                                            .ToList()
                                            .OrderByDescending(x => GetStationPriority(x))
                                            .ThenByDescending(x => GetTaskPriority(x))
                                            .ThenBy(x => x.Movement.Task.CreatedAt)
                                            .ThenBy(x => x.SequenceOrdering)
                                            .ThenBy(x => x.Id);

                _GetAvailableActionsDescription = sb.ToString();
                return result;
            }
        }
        /// <summary>
        /// 获取任务优先级
        /// 这里存在一个任务优先级提升的操作
        /// </summary>
        /// <returns></returns>
        private double GetTaskPriority(EquipmentAction action)
        {
            var timesp = DateTime.Now.Subtract(action.Movement.Task.CreatedAt);
            if (timesp.TotalMilliseconds > 10 * 60 * 1000)
                return action.Movement.Task.Priority + timesp.TotalMinutes;
            else
                return action.Movement.Task.Priority;
        }

        /// <summary>
        /// 获取高优先级站点的优先级
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private int GetStationPriority(EquipmentAction action)
        {
            var device = (SingleLocationDoubleVehicleSubSystem)Device;
            var highPriorityStations = device.Config.GetPriorityStations();
            if (highPriorityStations.Contains(action.Movement.StartLocation.UserCode) || highPriorityStations.Contains(action.Movement.EndLocation.UserCode))
                return 100;
            else
                return 0;
        }

        Boolean checkActionStatus(EquipmentAction act, StringBuilder sb)
        {
            if (act.Id == 0)
            {
                sb.AppendLine(string.Format("{0} id 为 0", act));
                return false;
            }
            //该代码设计是在任务创建时修复，但在完成时会被意外调用。
            //            if (act.SupportTransactionObjectStatus != SupportTransactionObjectStatus.Committed 
            //                && act.Status==EquipmentActionStatus.New
            //                && DateTime.Now.Subtract(act.CreatedAt).TotalSeconds>60)
            //            {
            //#warning 以下这段有问题
            //                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.Suppress))
            //                {
            //                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            //                    {
            //                        if (unitOfWork.session.Get<EquipmentAction>(act.Id) != null)
            //                        {
            //                            _logger.Warn1(string.Format("{0} SupportTransactionObjectStatus 为 {1}，预期值应为 {2}，但数据库中已存在该动作。准备修正状态为 {2}。", act, act.SupportTransactionObjectStatus.GetDescription(), SupportTransactionObjectStatus.Committed.GetDescription()), this, act);
            //                            act.SupportTransactionObjectStatus = SupportTransactionObjectStatus.Committed;
            //                            _logger.Warn1(string.Format("{0} SupportTransactionObjectStatus 变更为 {1}。", act, act.SupportTransactionObjectStatus.GetDescription()), this, act);

            //                            unitOfWork.Commit();
            //                            ts.Complete();
            //                        }
            //                        else
            //                        {
            //                            sb.AppendLine(string.Format("{0} SupportTransactionObjectStatus 为 {1}，预期值应为 {2}", act, act.SupportTransactionObjectStatus.GetDescription(), SupportTransactionObjectStatus.Committed.GetDescription()));

            //                            unitOfWork.Commit();
            //                            ts.Complete();

            //                            return false;
            //                        }
            //                    }
            //                }
            //            }

            String reason;
            if (!act.CanPerform(out reason))
            {
                sb.AppendLine(reason);
                return false;
            }

            return true;
        }

        public List<StateManagerRestoreInfo> RestoreInfos = new List<StateManagerRestoreInfo>();
        /// <summary>
        /// 车辆驱动记录
        /// </summary>
        public Dictionary<string, AbstractState2> VehicleRecording = new Dictionary<string, AbstractState2>();
        public List<string> LastCalFreeVehicleResult;
        protected override void Tick()
        {
            this._logger.Trace1(string.Format("{0} 轮询线程已启动", this), this);
            while (true)
            {
                try
                {
                    var subSystem = (SingleLocationDoubleVehicleSubSystem)Device;
                    if (SingleLocationDoubleVehicleSubSystemLocations == null || SingleLocationDoubleVehicleSubSystemLocations.Count() == 0)
                        SingleLocationDoubleVehicleSubSystemLocations = subSystem.Locations.Select(x => (SingleLocationDoubleVehicleSubSystemLocation)x).ToList();

                    Thread.Sleep(this.Interval);

                    if (!restoreFlag)
                        continue;

                    _methodDescriptorTree.ClearAccess();

                    _methodDescriptorTree["开始"].Access(true);

                    if (this.Actions.Count(x => x.Status != EquipmentActionStatus.Cancelled && x.Status != EquipmentActionStatus.Completed) == 0)
                    {
                        _methodDescriptorTree["是否包含动作"].Access(false, "不包含任何动作");

                        if ((VehicleRecording == null || VehicleRecording.Count() == 0) && (RestoreInfos == null || RestoreInfos.Count() == 0))
                            break;
                    }
                    _methodDescriptorTree["是否包含动作"].Access(true);

                    if (!this.Device.IsConnected)
                    {
                        _methodDescriptorTree["设备是否已连接"].Access(false, "设备未连接");
                        continue;
                    }
                    _methodDescriptorTree["设备是否已连接"].Access(true);

                    if (!this.Device.IsIdle.Result)
                    {
                        String msg = "";
                        if (this.Device.Warnings != null)
                        {
                            msg = string.Join("\n", this.Device.Warnings);
                        }
                        _methodDescriptorTree["设备是否空闲"].Access(false, msg);
                        continue;
                    }
                    _methodDescriptorTree["设备是否空闲"].Access(true);

                    var availableActions = GetAvailableActions();
                    if (availableActions == null)
                    {
                        _methodDescriptorTree["是否包含待可发送任务"].Access(false, _GetAvailableActionsDescription);
                        continue;
                    }

                    if (availableActions.Count() == 0)
                        _methodDescriptorTree["是否包含待可发送任务"].Access(false, _GetAvailableActionsDescription);
                    //continue;
                    else
                        _methodDescriptorTree["是否包含待可发送任务"].Access(true);

                    if (availableActions.Count() == 0 || this.Device.Holder != null)
                        _methodDescriptorTree["设备是否已被其它对象持有"].Access(false, string.Format("{0} 已被 {1} 持有", this.Device, this.Device.Holder));
                    else
                        _methodDescriptorTree["设备是否已被其它对象持有"].Access(true);


                    this.Device.Hold(this);
                    _methodDescriptorTree["持有设备"].Access(true);
                    try
                    {
                        this.Log(String.Format("本次准备了{0}个动作准备发送：{1}", availableActions.Count(), String.Join(",", availableActions.Select(x => x.EquipmentTaskId.ToString()))));
                        //_logger.Trace1(String.Format("本次准备了{0}个动作准备发送：{1}", availableActions.Count(), String.Join(",", availableActions.Select(x => x.EquipmentTaskId.ToString()))), this);

                        #region 分配任务给车
                        //获取空闲车辆
                        if (availableActions.Count() > 0)
                        {
                            var freeList = GetFreeVehicles(out LastCalFreeVehicleResult);
                            if (freeList != null || freeList.Count() > 0)
                            {
                                for (int i = 0; i < availableActions.Count(); i++)
                                {
                                    var item = availableActions.ToArray()[i];
                                    bool negatory = false;
                                    foreach (var filter in this.ActionFilters)
                                    {
                                        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                                        sw.Start();
                                        var filterResult = filter.Filter(this, item);
                                        sw.Stop();
                                        this.Log($"任务{item.Movement.Task}，物理动作{item},任务过滤器{filter.GetType()}过滤结果({filterResult.Defeated + "," + filterResult.Reason}),耗时{sw.ElapsedMilliseconds}ms");
                                        if (filterResult.Defeated == true)
                                        {
                                            _methodDescriptorTree["是否被过滤器否决"].Access(false, string.Format("{0} 被 {1} 否决了发送操作，原因：{2}。", item, filter, filterResult.Reason));
                                            negatory = true;
                                            //this._logger.Debug1(string.Format("{0} 被 {1} 否决了发送操作，原因：{2}。", item, filter, filterResult.Reason), this, item);
                                            this.Log(string.Format("{0} 被 {1} 否决了发送操作，原因：{2}。", item, filter, filterResult.Reason));
                                            break;
                                        }
                                    }
                                    if (negatory)
                                        continue;

                                    var resotreInfo = RestoreInfos.FirstOrDefault(x => x.EquipmentActionId == item.Id);
                                    if (resotreInfo != null)
                                    {
                                        UpdateAction(ref item, resotreInfo.ActuatingDeviceName);
                                        continue;
                                    }

                                    if (freeList == null || freeList.Count() == 0)
                                    {
                                        _methodDescriptorTree["是否被过滤器否决"].Access(false, string.Format("{0} 被 {1} 否决了发送操作，原因：{2}。", item, this, "无空闲车辆"));
                                        //_methodDescriptorTree["是否被过滤器否决"].Access(false);
                                        continue;
                                    }

                                    //选取车辆
                                    string vehicle = DispatchVehicle(item, freeList);
                                    if (vehicle == null)
                                    {
                                        _methodDescriptorTree["是否被过滤器否决"].Access(false, string.Format("{0} 被 {1} 否决了发送操作，原因：{2}。", item, this, "分配车辆失败"));
                                        //_methodDescriptorTree["是否被过滤器否决"].Access(false);
                                        continue;
                                    }

                                    //绑定车辆与任务
                                    Device.SendTask(item, vehicle);
                                    UpdateAction(ref item, vehicle);
                                    //移除已分配的车辆
                                    freeList.Remove(vehicle);
                                    //if (freeList.Count() == 0)
                                    //{
                                    //    _methodDescriptorTree["是否被过滤器否决"].Access(false);
                                    //    break;
                                    //}
                                }

                                _methodDescriptorTree["是否被过滤器否决"].Access(true);
                            }
                        }
                        #endregion

                        this.Log($"{this} 开始进行本次调度");
                        #region 检查之前的命令是否已完成
                        List<string> compeleted = new List<string>();
                        List<int> cancled = new List<int>();
                        List<string> deleted = new List<string>();
                        List<string> repired = new List<string>();

                        #region 修复两台车都异常后不再重发任务的异常 20230731 当然相对来说也会影响其他调度
                        var list = VehicleRecording.Select(x => x.Key).ToArray();
                        foreach (var key in list)
                        {
                            var device = DeviceConverter.ToDevice<RailGuidedVehicleDevice>(key);
                            var item = VehicleRecording.First(x => x.Key == key);
                            if (device.IsConnected
                                && device.LastStatus != null
                                && device.LastStatus.TaskId != item.Value.lastSendTaskId
                                && device.LastStatus.TaskId == "00000000"
                                && (device.LastStatus.State == RailGuidedVehicleStatus.无货待命 || device.LastStatus.State == RailGuidedVehicleStatus.有货待命))
                            {
                                repired.Add(key);
                            }
                        }
                        foreach (var item in repired)
                        {
                            VehicleRecording.Remove(item);
                        }
                        #endregion

                        //foreach (var item in VehicleRecording)
                        list = VehicleRecording.Select(x => x.Key).ToArray();
                        //foreach (var item in VehicleRecording)//这里直接循环如果真的发送时会报错：集合已修改
                        foreach (var key in list)
                        {
                            var item = VehicleRecording.First(x => x.Key == key);
                            if (item.Value._giveAwayMark)
                            {
                                try
                                {
                                    var result = item.Value.IsCompleted();
                                    if (result.Result)
                                    {
                                        compeleted.Add(item.Key);
                                        this.Log($"1检测到状态 {item.Value} 已完成，加入到完成列表");
                                        continue;
                                    }
                                    //★★★★★这里会尝试重复发命令，如果报错不需要处理
                                    //是否需要加安全判断

                                    this.Log($"1检测到状态 {item.Value} 由于 {result.Information} 原因未完成，准备尝试给车辆发送命令 {item.Value}");
                                    item.Value.Perform();
                                }
                                catch (Exception ex)
                                {
                                    this.Log(ex.ToString());
                                }
                            }
                            else
                            {
                                var action = this.Actions.FirstOrDefault(x => x.EquipmentTaskId == item.Value._action.EquipmentTaskId);
                                if (action == null || action.Status == EquipmentActionStatus.Cancelled || action.Status == EquipmentActionStatus.Completed)
                                {
                                    cancled.Add(item.Value._action.EquipmentTaskId);
                                    this.Log($"状态 {item.Value} 加入到取消列表");
                                    continue;
                                }
                                else if (action.Status == EquipmentActionStatus.Suspend)
                                {
                                    this.Log($"状态 {item.Value} 对应的任务 {action} 处于完成状态");
                                    continue;
                                }
                                else
                                {
                                    var result = item.Value.IsCompleted();
                                    if (result.Result)
                                    {
                                        compeleted.Add(item.Key);
                                        this.Log($"2检测到状态 {item.Value} 已完成，加入到完成列表");
                                        continue;
                                    }
                                    this.Log($"2检测到状态 {item.Value} 由于 {result.Information} 原因暂未完成，后续继续调度");

                                    try
                                    {
                                        //★★★★★这里会尝试重复发命令，如果报错不需要处理
                                        //是否存在空闲车辆需要让道
                                        if (item.Value is NotArriveToDestinationLocationState2)
                                        {
                                            var freeList = GetFreeVehicles(out LastCalFreeVehicleResult);
                                            if (freeList.Count() != 0)
                                            {
                                                foreach (var free in freeList)
                                                {
                                                    var giveAwayVehicle = DeviceConverter.ToDevice<RailGuidedVehicleDevice>(free);
                                                    if (giveAwayVehicle.LastStatus == null)
                                                        break;

                                                    var state = (NotArriveToDestinationLocationState2)item.Value;
                                                    if (state.railGuidedVehicleDevice.LastStatus == null)
                                                        break;

                                                    var _currentPosition = ConvertPosition(giveAwayVehicle, giveAwayVehicle.LastStatus.Position);
                                                    NotArriveToDestinationLocationState2 giveAwayState;
                                                    this.Log($"1开始尝试进行让道分析,调度状态{state}，让道车辆{giveAwayVehicle}");
                                                    CheckGiveAway(subSystem, giveAwayVehicle, _currentPosition, ref state, out giveAwayState);
                                                    var _giveAwayState = giveAwayState == null ? "NULL" : giveAwayState.ToString();
                                                    var _state = state == null ? "NULL" : state.ToString();
                                                    this.Log($"1本次让道分析结果 giveAwayState={_giveAwayState},State={_state}");

                                                    if (giveAwayState != null)
                                                    {
                                                        this.Log($"2准备给车辆 {giveAwayState.railGuidedVehicleDevice.Name} 发送命令 {giveAwayState}");
                                                        giveAwayState.Perform();
                                                    }
                                                    if (state != null)
                                                    {
                                                        this.Log($"3准备给车辆 {state.railGuidedVehicleDevice.Name} 发送命令 {state}");
                                                        state.Perform();
                                                    }
                                                }
                                            }
                                            else
                                            //这里是否需要做避让判断
                                            //item.Value.Perform();
                                            {
                                                var giveAwayVehicle = subSystem.RailGuidedVehicles.FirstOrDefault(x => x.Name != item.Key);
                                                if (subSystem.GetVehicleState(giveAwayVehicle.Name) == RailGuidVehicleSetStatus.Stoping || subSystem.GetVehicleState(giveAwayVehicle.Name) == RailGuidVehicleSetStatus.Working)
                                                {
                                                    if (giveAwayVehicle.LastStatus == null)
                                                        break;

                                                    var state = (NotArriveToDestinationLocationState2)item.Value;
                                                    if (state.railGuidedVehicleDevice.LastStatus == null)
                                                        break;

                                                    var _currentPosition = ConvertPosition(giveAwayVehicle, giveAwayVehicle.LastStatus.Position);
                                                    NotArriveToDestinationLocationState2 giveAwayState;
                                                    this.Log($"2开始尝试进行让道分析,调度状态{state}，让道车辆{giveAwayVehicle}");
                                                    CheckGiveAway(subSystem, giveAwayVehicle, _currentPosition, ref state, out giveAwayState);
                                                    var _giveAwayState = giveAwayState == null ? "NULL" : giveAwayState.ToString();
                                                    var _state = state == null ? "NULL" : state.ToString();
                                                    this.Log($"2本次让道分析结果 giveAwayState={_giveAwayState},State={_state}");
                                                    if (giveAwayState != null)
                                                    {
                                                        this.Log($"4准备给车辆 {giveAwayState.railGuidedVehicleDevice.Name} 发送命令 {giveAwayState}");
                                                        giveAwayState.Perform();
                                                    }
                                                    if (state != null)
                                                    {
                                                        this.Log($"5准备给车辆 {state.railGuidedVehicleDevice.Name} 发送命令 {state}");
                                                        state.Perform();
                                                    }
                                                }
                                            }
                                        }
                                        //是否存在空闲车辆需要让道
                                        else
                                        {
                                            this.Log($"6准备给车辆 {item.Value.railGuidedVehicleDevice.Name} 发送命令 {item.Value}");
                                            item.Value.Perform();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        this.Log(ex.ToString());
                                    }
                                }
                            }
                        }
                        foreach (var item in deleted)
                        {
                            VehicleRecording.Remove(item);
                        }
                        VehicleRecording = VehicleRecording.Where(x => x.Value._action != null).ToDictionary(x => x.Key, x => x.Value);
                        foreach (var item in cancled)
                        {
                            var _item = VehicleRecording.FirstOrDefault(x => x.Value._action.EquipmentTaskId == item);
                            if (!_item.Equals(default(KeyValuePair<string, AbstractState2>)))
                                VehicleRecording.Remove(_item.Key);
                            var restoreInfo = RestoreInfos.FirstOrDefault(x => x.EquipmentActionId == item);
                            if (restoreInfo != null)
                            {
                                ((SingleLocationDoubleVehicleSubSystem)Device).DeleteBindingTaskAndVehicle(item);
                                RestoreInfos.Remove(restoreInfo);
                            }
                        }
                        foreach (var item in compeleted)
                        {
                            VehicleRecording.Remove(item);
                        }
                        #endregion

                        #region 车辆任务下发
                        Dictionary<StateManagerRestoreInfo, string> _dic = new Dictionary<StateManagerRestoreInfo, string>();
                        //获取车辆下一步动作
                        for (int i = 0; i < RestoreInfos.Count(); i++)
                        {
                            var item = RestoreInfos[i];
                            var action = Actions.FirstOrDefault(x => x.Id == item.EquipmentActionId);
                            if (action == null)
                            {
                                RestoreInfos.Remove(item);
                                _dic = new Dictionary<StateManagerRestoreInfo, string>();
                                break;
                            }
                            if (VehicleRecording.Any(x => x.Key == item.ActuatingDeviceName))
                                continue;

                            var vehicle = DeviceConverter.ToDevice<RailGuidedVehicleDevice>(item.ActuatingDeviceName);
                            if (!vehicle.IsIdle.Result)
                                continue;
                            if (vehicle.LastStatus == null)
                                continue;
                            var currentStation = vehicle.LastStatus.CurrentStation;
                            if (item.CurrentStateName == "移动到起点")
                            {
                                var start = (SingleLocationDoubleVehicleSubSystemLocation)LocationConverter.ToLocation(action.Movement.StartLocation);
                                if (start.StationNo == currentStation && vehicle.LastStatus.AtStation)
                                {
                                    var itemVehicle = DeviceConverter.ToDevice<RailGuidedVehicleDevice>(item.ActuatingDeviceName);
                                    if (itemVehicle.LastStatus.State == RailGuidedVehicleStatus.无货待命)
                                    {
                                        ((SingleLocationDoubleVehicleSubSystem)Device).UpdateTaskAndVehicleInfo("取货", vehicle.Name, item.EquipmentActionId);
                                        //发送取货命令
                                        _dic.Add(item, "取货");
                                    }
                                    else
                                        //发送移动到起点的命令
                                        _dic.Add(item, "移动到起点");
                                }
                                else
                                    //发送移动到起点的命令
                                    _dic.Add(item, "移动到起点");
                            }
                            else if (item.CurrentStateName == "取货")
                            {
                                var start = (SingleLocationDoubleVehicleSubSystemLocation)LocationConverter.ToLocation(action.Movement.StartLocation);
                                var itemVehicle = DeviceConverter.ToDevice<RailGuidedVehicleDevice>(item.ActuatingDeviceName);
                                if (itemVehicle.LastStatus.State == RailGuidedVehicleStatus.有货待命)
                                {
                                    ((SingleLocationDoubleVehicleSubSystem)Device).UpdateTaskAndVehicleInfo("移动到终点", vehicle.Name, item.EquipmentActionId);
                                    //发送取货命令
                                    _dic.Add(item, "移动到终点");

                                    var _task = this.Actions.FirstOrDefault(x => x.Id == item.EquipmentActionId);
                                    if (_task != null)
                                    {
                                        var taskCurrentLocationInfo = LocationConverter.ToLocationInfo(LocationConverter.UserCodeToLcation(item.ActuatingDeviceName));
                                        using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                                        {
                                            var task = unitOfWork.session.Get<Task>(_task.Movement.Task.Id);
                                            task.CurrentLocation = taskCurrentLocationInfo;
                                            unitOfWork.session.Flush();
                                            unitOfWork.Commit();
                                        }

                                        Wcs.Framework.EventBus.EventBus.Instance.Publish(new Wcs.Framework.Events.TaskCurrentLocationChangedEvent(_task.Id, _task.Movement.Task.TaskCode, taskCurrentLocationInfo));
                                    }
                                }
                                else if (start.StationNo != currentStation || !vehicle.LastStatus.AtStation)
                                {
                                    ((SingleLocationDoubleVehicleSubSystem)Device).UpdateTaskAndVehicleInfo("移动到起点", vehicle.Name, item.EquipmentActionId);
                                    //发送取货命令
                                    _dic.Add(item, "移动到起点");
                                }
                                else
                                    //发送取货命令
                                    _dic.Add(item, "取货");
                            }
                            else if (item.CurrentStateName == "移动到终点")
                            {
                                var end = (SingleLocationDoubleVehicleSubSystemLocation)LocationConverter.ToLocation(action.Movement.EndLocation);
                                if (end.StationNo == currentStation && vehicle.LastStatus.AtStation)
                                {
                                    var itemVehicle = DeviceConverter.ToDevice<RailGuidedVehicleDevice>(item.ActuatingDeviceName);
                                    if (itemVehicle.LastStatus.State == RailGuidedVehicleStatus.有货待命)
                                    {
                                        ((SingleLocationDoubleVehicleSubSystem)Device).UpdateTaskAndVehicleInfo("放货", vehicle.Name, item.EquipmentActionId);
                                        //发送放货命令
                                        _dic.Add(item, "放货");
                                    }
                                    else
                                        //发送移动到终点的命令
                                        _dic.Add(item, "移动到终点");
                                }
                                else
                                    //发送移动到终点的命令
                                    _dic.Add(item, "移动到终点");
                            }
                            else if (item.CurrentStateName == "放货")
                            {
                                var end = (SingleLocationDoubleVehicleSubSystemLocation)LocationConverter.ToLocation(action.Movement.EndLocation);
                                var itemVehicle = DeviceConverter.ToDevice<RailGuidedVehicleDevice>(item.ActuatingDeviceName);
                                if (itemVehicle.LastStatus.State == RailGuidedVehicleStatus.无货待命)
                                {
                                    var equipmentActionTaskId = Actions.FirstOrDefault(x => x.Id == item.EquipmentActionId).EquipmentTaskId;
                                    //报完成
                                    FireTaskCompleteEvent(equipmentActionTaskId);
                                    RestoreInfos.Remove(item);
                                    VehicleRecording.Remove(item.ActuatingDeviceName);
                                    ((SingleLocationDoubleVehicleSubSystem)Device).DeleteBindingTaskAndVehicle(item.EquipmentActionId);
                                    _dic = new Dictionary<StateManagerRestoreInfo, string>();
                                    break;
                                }
                                else if (end.StationNo != currentStation || !vehicle.LastStatus.AtStation)
                                {
                                    ((SingleLocationDoubleVehicleSubSystem)Device).UpdateTaskAndVehicleInfo("移动到终点", vehicle.Name, item.EquipmentActionId);
                                    //发送取货命令
                                    _dic.Add(item, "移动到终点");
                                }
                                else
                                    //发送取货命令
                                    _dic.Add(item, "放货");
                            }
                        }

                        //下发命令 取货/放货命令直接优先下发 移动命令需要1.判断能否下发2.判断是否需要让道3.判断谁先下发
                        //优先发送取放货命令
                        if (_dic.Any(x => x.Value == "取货" || x.Value == "放货"))
                        {
                            foreach (var item in _dic)
                            {
                                if (item.Value == "取货")
                                {
                                    NotPickingState2 state = new NotPickingState2(this, item.Key, Actions.First(x => x.Id == item.Key.EquipmentActionId));

                                    this.Log($"7准备给车辆 {state.railGuidedVehicleDevice.Name} 发送命令 {state}");
                                    state.Perform();
                                }
                                else if (item.Value == "放货")
                                {
                                    NotUnloadingState2 state = new NotUnloadingState2(this, item.Key, Actions.First(x => x.Id == item.Key.EquipmentActionId));

                                    this.Log($"8准备给车辆 {state.railGuidedVehicleDevice.Name} 发送命令 {state}");
                                    state.Perform();
                                }
                            }
                        }
                        //尝试发送移动命令
                        var moveCount = _dic.Count(x => x.Value == "移动到起点" || x.Value == "移动到终点");
                        if (moveCount == 0)
                            continue;
                        if (moveCount == 1)
                        {
                            var item = _dic.First(x => x.Value == "移动到起点" || x.Value == "移动到终点");
                            var action = Actions.First(x => x.Id == item.Key.EquipmentActionId);
                            NotArriveToDestinationLocationState2 state = null;
                            if (item.Value == "移动到起点")
                                state = new NotArriveToDestinationLocationState2(this, item.Key, action, action.Movement.StartLocation, item.Value);
                            else //if (item.Value == "移动到终点")
                                state = new NotArriveToDestinationLocationState2(this, item.Key, action, action.Movement.EndLocation, item.Value);

                            if (state == null)
                                continue;
                            //安全计算 另外的车辆在安全位置时可以发送该命令，否则不可以，只可以发送渐进位置
                            //对只配置一辆车的支持
                            var vehicles = subSystem.RailGuidedVehicleDeviceNames.Where(x => x != item.Key.ActuatingDeviceName);
                            if (vehicles.Count() == 0)
                            {
                                this.Log($"9准备给车辆 {state.railGuidedVehicleDevice.Name} 发送命令 {state}");
                                state.Perform();
                                continue;
                            }
                            //对只配置一辆车的支持

                            //只要存在任意一辆未设置状态的车就不允许执行 
                            //Stoping 状态的车默认在最端头不影响另外车可运行范围内的正常运行
                            //Repairing 状态的车默认不影响另外车全轨道运行
                            if (vehicles.Any(x => subSystem.GetVehicleState(x) == RailGuidVehicleSetStatus.Unknow))
                                continue;

                            foreach (var giveAwayVehicleName in vehicles)
                            {
                                if (subSystem.GetVehicleState(giveAwayVehicleName) == RailGuidVehicleSetStatus.Stoping || subSystem.GetVehicleState(giveAwayVehicleName) == RailGuidVehicleSetStatus.Repairing)
                                {
                                    this.Log($"10准备给车辆 {state.railGuidedVehicleDevice.Name} 发送命令 {state}");
                                    state.Perform();
                                }
                                else if (subSystem.GetVehicleState(giveAwayVehicleName) == RailGuidVehicleSetStatus.Working)
                                {
                                    var giveAwayVehicle = DeviceConverter.ToDevice<RailGuidedVehicleDevice>(giveAwayVehicleName);
                                    if (giveAwayVehicle.LastStatus == null)
                                        break;

                                    if (state.railGuidedVehicleDevice.LastStatus == null)
                                        break;

                                    var _currentPosition = ConvertPosition(giveAwayVehicle, giveAwayVehicle.LastStatus.Position);
                                    NotArriveToDestinationLocationState2 giveAwayState;
                                    this.Log($"3开始尝试进行让道分析,调度状态{state}，让道车辆{giveAwayVehicle}");
                                    CheckGiveAway(subSystem, giveAwayVehicle, _currentPosition, ref state, out giveAwayState);
                                    var _giveAwayState = giveAwayState == null ? "NULL" : giveAwayState.ToString();
                                    var _state = state == null ? "NULL" : state.ToString();
                                    this.Log($"3本次让道分析结果 giveAwayState={_giveAwayState},State={_state}");
                                    if (giveAwayState != null)
                                    {
                                        this.Log($"11准备给车辆 {giveAwayState.railGuidedVehicleDevice.Name} 发送命令 {giveAwayState}");
                                        giveAwayState.Perform();
                                    }
                                    if (state != null)
                                    {
                                        this.Log($"12准备给车辆 {state.railGuidedVehicleDevice.Name} 发送命令 {state}");
                                        state.Perform();
                                    }
                                }
                                else
                                {
                                    this.Log($"{this}未发送任务");
                                    Console.WriteLine($"{this}未发送任务");
                                    break;
                                }
                            }
                        }
                        //两辆车都需要移动的时候
                        //1.如果不干涉直接移动，但是得注意先后顺序
                        //2.如果干涉则分别计算让道距离（来代替时间）谁距离近谁先走
                        else if (moveCount == 2)
                        {
                            //先分别计算出state
                            Dictionary<StateManagerRestoreInfo, AbstractState2> __dic = new Dictionary<StateManagerRestoreInfo, AbstractState2>();
                            foreach (var item in _dic)
                            {
                                var action = Actions.First(x => x.Id == item.Key.EquipmentActionId);
                                if (item.Value == "移动到起点")
                                    __dic.Add(item.Key, new NotArriveToDestinationLocationState2(this, item.Key, action, action.Movement.StartLocation, item.Value));
                                else //if (item.Value == "移动到终点")
                                    __dic.Add(item.Key, new NotArriveToDestinationLocationState2(this, item.Key, action, action.Movement.EndLocation, item.Value));
                            }
                            //如果两个车的state 的 destination 不干涉
                            bool both = true;
                            var states = __dic.Values.ToList();
                            foreach (var item in states)
                            {
                                var _state = states.Where(x => x.railGuidedVehicleDevice.Name != item.railGuidedVehicleDevice.Name).First();
                                //判断安全距离
                                if (Math.Abs(_state.destinationLocation.Position - item.destinationLocation.Position) < subSystem.Config.SafeDistance)
                                {
                                    both = false;
                                    break;
                                }
                                else
                                {
                                    ///判断是否交叉
                                    var max = Math.Max(ConvertPosition(item.railGuidedVehicleDevice, item.railGuidedVehicleDevice.LastStatus.Position), item.destinationLocation.Position);
                                    var min = Math.Min(ConvertPosition(item.railGuidedVehicleDevice, item.railGuidedVehicleDevice.LastStatus.Position), item.destinationLocation.Position);
                                    if (_state.destinationLocation.Position > min && _state.destinationLocation.Position < max)
                                    {
                                        both = false;
                                        break;
                                    }
                                    else
                                    {
                                        if (ConvertPosition(item.railGuidedVehicleDevice, item.railGuidedVehicleDevice.LastStatus.Position) < ConvertPosition(_state.railGuidedVehicleDevice, _state.railGuidedVehicleDevice.LastStatus.Position))
                                        {
                                            if (item.destinationLocation.Position > _state.destinationLocation.Position)
                                            {
                                                both = false;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (item.destinationLocation.Position < _state.destinationLocation.Position)
                                            {
                                                both = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            if (both)
                            {
                                foreach (var item in states)
                                {
                                    this.Log($"13准备给车辆 {item.railGuidedVehicleDevice.Name} 发送命令 {item}");
                                    item.Perform();
                                }
                                continue;
                            }

                            //判断谁先执行 当前判断逻辑 谁离目的点近 谁先执行 另外一个做渐近执行
                            states = states.OrderBy(x => GetDistance(x)).ToList();
                            //this.Add($"14准备给车辆发送命令 {_state}");
                            for (int i = states.Count - 1; i >= 0; i--)
                            {
                                var _state = states[i];

                                if (i == states.Count - 1)
                                {
                                    //计算让道站点并执行
                                    var __state = states[i - 1];
                                    SingleLocationDoubleVehicleSubSystemLocation giveAwayStation;
                                    if (ConvertPosition(_state.railGuidedVehicleDevice, _state.railGuidedVehicleDevice.LastStatus.Position) < ConvertPosition(__state.railGuidedVehicleDevice, __state.railGuidedVehicleDevice.LastStatus.Position))
                                    {
                                        giveAwayStation = (SingleLocationDoubleVehicleSubSystemLocation)subSystem.Locations.Select(x => (SingleLocationDoubleVehicleSubSystemLocation)x).Where(x => x.Position > 0 && x.Position < __state.destinationLocation.Position - subSystem.Config.SafeDistance).OrderByDescending(x => x.Position).FirstOrDefault();

                                        this.Log($"11计算出让道站点 {giveAwayStation}");
                                        //直接行走
                                        if (_state.destinationLocation.Position < __state.destinationLocation.Position - subSystem.Config.SafeDistance)
                                        {
                                            giveAwayStation = _state.destinationLocation;

                                            this.Log($"22计算出让道站点 {giveAwayStation}");
                                        }
                                        //顺路行走
                                        else if (_state.destinationLocation.Position >= Math.Min(ConvertPosition(_state.railGuidedVehicleDevice, _state.railGuidedVehicleDevice.LastStatus.Position), giveAwayStation.Position) && _state.destinationLocation.Position <= Math.Max(ConvertPosition(_state.railGuidedVehicleDevice, _state.railGuidedVehicleDevice.LastStatus.Position), giveAwayStation.Position))
                                        {
                                            giveAwayStation = _state.destinationLocation;

                                            this.Log($"33计算出让道站点 {giveAwayStation}");
                                        }
                                    }
                                    else
                                    {
                                        giveAwayStation = (SingleLocationDoubleVehicleSubSystemLocation)subSystem.Locations.Select(x => (SingleLocationDoubleVehicleSubSystemLocation)x).Where(x => x.Position > 0 && x.Position > __state.destinationLocation.Position + subSystem.Config.SafeDistance).OrderBy(x => x.Position).FirstOrDefault();
                                        this.Log($"44计算出让道站点 {giveAwayStation}");
                                        //直接行走
                                        if (_state.destinationLocation.Position > __state.destinationLocation.Position + subSystem.Config.SafeDistance)
                                        {
                                            giveAwayStation = _state.destinationLocation;
                                            this.Log($"55计算出让道站点 {giveAwayStation}");
                                        }
                                        //顺路行走
                                        else if (_state.destinationLocation.Position >= Math.Min(ConvertPosition(_state.railGuidedVehicleDevice, _state.railGuidedVehicleDevice.LastStatus.Position), giveAwayStation.Position) && _state.destinationLocation.Position <= Math.Max(ConvertPosition(_state.railGuidedVehicleDevice, _state.railGuidedVehicleDevice.LastStatus.Position), giveAwayStation.Position))
                                        {
                                            giveAwayStation = _state.destinationLocation;
                                            this.Log($"66计算出让道站点 {giveAwayStation}");
                                        }
                                    }

                                    var currentStation = _state.railGuidedVehicleDevice.GetCurrentStation();
                                    if (currentStation == null || currentStation.StationNo != giveAwayStation.StationNo)
                                    {
                                        if (_state.railGuidedVehicleDevice.IsIdle.Result)
                                        {
                                            this.Log($"让道结果：变更{_state}目标站点为{giveAwayStation}");
                                            _state.destinationLocation = giveAwayStation;
                                        }
                                        else
                                        {
                                            this.Log($"让道结果：{_state.railGuidedVehicleDevice.Name}本次计算目标站点为{giveAwayStation}，但是车辆不处于空闲状态，本次未重置state");
                                        }
                                    }
                                    else
                                        _state = null;
                                }
                                if (_state != null)
                                {
                                    this.Log($"14准备给车辆 {_state.railGuidedVehicleDevice.Name} 发送命令 {_state}");
                                    _state.Perform();
                                }
                            }
                        }
                        else
                            continue;
                        #endregion

                        #region 补充让道处理
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        this._logger.Error1(ex, this);

                        _methodDescriptorTree["结束"].Access(false, ex.ToString());
                    }
                    finally
                    {
                        this.Device.Unhold(this);
                    }

                }
                catch (Exception ex)
                {
                    _methodDescriptorTree["结束"].Access(false, ex.ToString());
                    this._logger.Error1(ex, this);
                }
            }

            this._logger.Trace1(string.Format("{0} 轮询线程已停止", this), this);
            this.IsRunning = false;
        }

        /// <summary>
        /// 位置转换
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        private uint ConvertPosition(RailGuidedVehicleDevice vehicle, uint position)
        {
            var subSystem = (SingleLocationDoubleVehicleSubSystem)Device;
            if (!subSystem.Config.Vehicles.ConvertList.Contains(vehicle.Name))
                return position;
            else
                return (uint)(subSystem.Config.Vehicles.AverageTotalDistance - position);
        }

        private void UpdateAction(ref EquipmentAction action,string vehicle)
        {
            _logger.Trace1(string.Format("{0} 发送成功", action), this, action);

            action.Status = EquipmentActionStatus.Executing;
            action.Movement.Status = LogicMovementStatus.Executing;
            action.Movement.Task.Status = TaskStatus.Executing;

            //此处状态更新失败后亦无影响，原因是如果任务被成功发送。设备将不断发送任务状态信号，任务状态事件的处理程序会处理其关的状态信息
            bool locked = false;
            try
            {
                int relockNum = 0;
            relock:
                if (relockNum != 0)
                {
                    _logger.Debug1(string.Format("准备第{0}尝试锁定{1}...", relockNum, action), this, action);
                }

                locked = action.EnterLock();
                if (!locked)
                {
                    if (relockNum < 3)
                    {
                        relockNum++;
                        System.Threading.Thread.Sleep(200);
                        goto relock;
                    }
                    else
                    {
                        _logger.Warn1(String.Format("{0}在发送成功后，准备更新状态为“{1}”时锁定失败，但仍将尝试直接更新该状态。", action, EquipmentActionStatus.Executing.GetDescription()), this, action);
                    }
                }

                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.Suppress))
                {
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                    {
                        var act = unitOfWork.session.Get<EquipmentAction>(action.Id);
                        act.Status = EquipmentActionStatus.Executing;
                        act.Movement.Status = LogicMovementStatus.Executing;
                        act.Movement.Task.Status = TaskStatus.Executing;
                        if (act.SentAt == null)
                        {
                            act.ActuatingDeviceName = vehicle;
                            act.SentAt = DateTime.Now;
                        }

                        unitOfWork.session.Flush();
                        unitOfWork.Commit();
                    }

                    ts.Complete();
                }
            }
            catch (Exception ex)
            {
                this._logger.Error1(ex, this, action);
                _methodDescriptorTree["结束"].Access(false, ex.ToString());
            }
            finally
            {
                if (locked)
                {
                    action.ExitLock();
                }
            }


            Wcs.Framework.EventBus.EventBus.Instance.Publish(new EquipmentActionStatusChangedEvent(action.Movement.Task.Id, action.Movement.Id, action.Id, action.Status));
            Wcs.Framework.EventBus.EventBus.Instance.Publish(new LogicMovementStatusChangedEvent(action.Movement.Id, action.Movement.Task.Id, action.Movement.Status));
            Wcs.Framework.EventBus.EventBus.Instance.Publish(new TaskStatusChangedEvent(action.Movement.Task.Id, action.Movement.Task.TaskCode, action.Movement.Task.Status, action.Movement.Task.BizType, action.Movement.Task.Source, action.Movement.Task.TaskType));
        }

        bool restoreFlag = false;
        /// <summary>
        /// 恢复上下文
        /// </summary>
        public void Restore()
        {
            var subSystem = (SingleLocationDoubleVehicleSubSystem)Device;
            List<StateManagerRestoreInfo> removeList = new List<StateManagerRestoreInfo>();
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                RestoreInfos = unitOfWork.session.Query<StateManagerRestoreInfo>().Where(x => x.OwnDeviceName == subSystem.Name).ToList();
                var equipmentIds = RestoreInfos.Where(x => x.EquipmentActionId != 0).Select(x => x.EquipmentActionId).ToArray();
                var equipmentActions = unitOfWork.session.Query<SingleLocationDoubleVehicleSubSystemTransferAction>()
                    .Where(x => equipmentIds.Contains(x.Id))
                    .ToList();

                foreach (var item in RestoreInfos)
                {
                    if (equipmentActions.Any(x => x.Id == item.EquipmentActionId && x.Status != EquipmentActionStatus.Cancelled && x.Status != EquipmentActionStatus.Completed && x.Status != EquipmentActionStatus.Error))
                        continue;

                    removeList.Add(item);
                }
                unitOfWork.Commit();
            }
            foreach (var item in removeList)
            {
                RestoreInfos.Remove(item);
                ((SingleLocationDoubleVehicleSubSystem)Device).DeleteBindingTaskAndVehicle(item.EquipmentActionId);
            }

            restoreFlag = true;
        }

        /// <summary>
        /// 给任务分配车辆
        /// 原则：1可分配 2就近分配 3不跨任务分配 4偏向分配（主要参考终点）即左边尽量分配给左边车右边尽量分配给右边车
        /// </summary>
        /// <param name="item"></param>
        /// <param name="freeList"></param>
        /// <returns></returns>
        private string DispatchVehicle(EquipmentAction action, List<string> freeList)
        {
            var subSystem = (SingleLocationDoubleVehicleSubSystem)Device;
            var start = (SingleLocationDoubleVehicleSubSystemLocation)LocationConverter.ToLocation(action.Movement.StartLocation);
            var end = (SingleLocationDoubleVehicleSubSystemLocation)LocationConverter.ToLocation(action.Movement.EndLocation);
            var startGroup = subSystem.Config.Bindings.Groups.FirstOrDefault(x => x.Station == start.UserCode);
            var endGroup = subSystem.Config.Bindings.Groups.FirstOrDefault(x => x.Station == end.UserCode);

            //起点和终点分别位于两台车独占区域时，暂时不处理这种情况
            if (startGroup != null && endGroup != null && startGroup.Vehicle != endGroup.Vehicle)
            {
                TaskHelper.Suspend(action.Movement.Task.TaskCode);
                _logger.Warn1($"由于任务起点和终点分别位于两台车独占区域，系统无法处理，主动暂停该任务{action.Movement.Task}", this, action.Movement.Task);
                return null;
            }

            //独占区域车辆分配
            if (startGroup != null)
            {
                if (freeList.Contains(startGroup.Vehicle))
                    return startGroup.Vehicle;
                else
                    return null;
            }
            if (endGroup != null)
            {
                if (freeList.Contains(endGroup.Vehicle))
                    return endGroup.Vehicle;
                else
                    return null;
            }

            //非独占区域车辆分配
            string select;
            if (freeList.Count() == 0)
                return null;
            else if (freeList.Count() == 1)
                select = freeList.First();
            else
            {
                #region 就近选择算法
                //var vehicles = freeList.Select(x => DeviceConverter.ToDevice<RailGuidedVehicleDevice>(x)).ToList();
                ////非独占区域车辆分配 判断是否满足偏向分配
                //if (start.Position >= MiddlePosition && end.Position >= MiddlePosition)
                //    select = vehicles.OrderByDescending(x => x.LastStatus.Position).First().Name;
                //else if (start.Position <= MiddlePosition && end.Position <= MiddlePosition)
                //    select = vehicles.OrderBy(x => x.LastStatus.Position).First().Name;
                //else //就近选择
                //    select = vehicles.OrderBy(x => Math.Abs(x.LastStatus.Position - start.Position)).First().Name;
                #endregion
                #region 热力图优先级算法
                select = freeList.OrderByDescending(x => CalStationPriority(x, start, end, subSystem, freeList)).First();
                #endregion
            }
            //判断是否跨另外一辆空闲车或者正在执行任务车的任务
            var unSelect = subSystem.RailGuidedVehicleDeviceNames.FirstOrDefault(x => x != select);
            if (unSelect == null)
                return select;

            if (subSystem.GetVehicleState(unSelect) == RailGuidVehicleSetStatus.Working)
            {
                var selectVehicle = DeviceConverter.ToDevice<RailGuidedVehicleDevice>(select);
                var unSelectVehicle = DeviceConverter.ToDevice<RailGuidedVehicleDevice>(unSelect);
                int min, max;
                if (start.Position > ConvertPosition(selectVehicle, selectVehicle.LastStatus.Position))
                {
                    min = (int)ConvertPosition(selectVehicle, selectVehicle.LastStatus.Position);
                    max = start.Position + subSystem.Config.SafeDistance;
                }
                else
                {
                    max = (int)ConvertPosition(selectVehicle, selectVehicle.LastStatus.Position);
                    min = start.Position - subSystem.Config.SafeDistance;
                }
                if (freeList.Contains(unSelect))
                {
                    if (ConvertPosition(unSelectVehicle, unSelectVehicle.LastStatus.Position) > min && ConvertPosition(unSelectVehicle, unSelectVehicle.LastStatus.Position) < max)
                    {
                        //这里假设选择的是unSelectVehicle 对 selectVehicle 做是否跨站的检测
                        if (start.Position > ConvertPosition(unSelectVehicle, unSelectVehicle.LastStatus.Position))
                        {
                            min = (int)ConvertPosition(unSelectVehicle, unSelectVehicle.LastStatus.Position);
                            max = start.Position + subSystem.Config.SafeDistance;
                        }
                        else
                        {
                            max = (int)ConvertPosition(unSelectVehicle, unSelectVehicle.LastStatus.Position);
                            min = start.Position - subSystem.Config.SafeDistance;
                        }
                        if (ConvertPosition(selectVehicle, selectVehicle.LastStatus.Position) > min && ConvertPosition(selectVehicle, selectVehicle.LastStatus.Position) < max)
                            return select;
                        else
                            return unSelect;
                    }
                    else
                        return select;
                }
                else if (VehicleRecording.ContainsKey(unSelect))
                {
                    var recording = VehicleRecording[unSelect];
                    if (recording.destinationLocation.Position > min && recording.destinationLocation.Position < max)
                        //因为不在空闲车辆列表中 此时不分配
                        return null;
                    else
                        return select;
                }
                else if (RestoreInfos.Any(x => x.ActuatingDeviceName == unSelect))
                {
                    var unSelectRestorInfo = RestoreInfos.First(x => x.ActuatingDeviceName == unSelect);
                    var unSelectAction = this.Actions.FirstOrDefault(x => x.Id == unSelectRestorInfo.EquipmentActionId);
                    if (unSelectAction != null)
                    {
                        if (unSelectVehicle.LastStatus.State == RailGuidedVehicleStatus.无货待命 || unSelectVehicle.LastStatus.State == RailGuidedVehicleStatus.无货运行 || unSelectVehicle.LastStatus.State == RailGuidedVehicleStatus.A || unSelectVehicle.LastStatus.State == RailGuidedVehicleStatus.接货中 || unSelectRestorInfo.CurrentStateName == "取货")
                        {
                            var unSelectNextDestinationLocation = (SingleLocationDoubleVehicleSubSystemLocation)LocationConverter.ToLocation(((SingleLocationDoubleVehicleSubSystemTransferAction)unSelectAction).StartLocation);
                            if (unSelectNextDestinationLocation.Position > min && unSelectNextDestinationLocation.Position < max)
                                //因为不在空闲车辆列表中 此时不分配
                                return null;
                            else
                            {
                                unSelectNextDestinationLocation = (SingleLocationDoubleVehicleSubSystemLocation)LocationConverter.ToLocation(((SingleLocationDoubleVehicleSubSystemTransferAction)unSelectAction).EndLocation);
                                if (unSelectNextDestinationLocation.Position > min && unSelectNextDestinationLocation.Position < max)
                                    //因为不在空闲车辆列表中 此时不分配
                                    return null;
                                if (ConvertPosition(selectVehicle, selectVehicle.LastStatus.Position) < ConvertPosition(unSelectVehicle, unSelectVehicle.LastStatus.Position))
                                {
                                    //卸货点还跨当前车
                                    if (unSelectNextDestinationLocation.Position <= ConvertPosition(selectVehicle, selectVehicle.LastStatus.Position))
                                        return null;
                                    return select;
                                }
                                else
                                {
                                    //卸货点还跨当前车
                                    if (unSelectNextDestinationLocation.Position >= ConvertPosition(selectVehicle, selectVehicle.LastStatus.Position))
                                        return null;
                                    return select;
                                }
                            }
                        }
                        else if (unSelectVehicle.LastStatus.State == RailGuidedVehicleStatus.有货待命 || unSelectVehicle.LastStatus.State == RailGuidedVehicleStatus.有货有任务 || unSelectVehicle.LastStatus.State == RailGuidedVehicleStatus.有货运行 || unSelectVehicle.LastStatus.State == RailGuidedVehicleStatus.卸货中 || unSelectRestorInfo.CurrentStateName == "放货")
                        {
                            var unSelectNextDestinationLocation = (SingleLocationDoubleVehicleSubSystemLocation)LocationConverter.ToLocation(((SingleLocationDoubleVehicleSubSystemTransferAction)unSelectAction).EndLocation);
                            if (unSelectNextDestinationLocation.Position > min && unSelectNextDestinationLocation.Position < max)
                                //因为不在空闲车辆列表中 此时不分配
                                return null;
                            else
                                return select;
                        }
                        else
                        {
                            if (ConvertPosition(unSelectVehicle, unSelectVehicle.LastStatus.Position) > min && ConvertPosition(unSelectVehicle, unSelectVehicle.LastStatus.Position) < max)
                                //因为不在空闲车辆列表中 此时不分配
                                return null;
                            else
                                return select;
                        }
                    }

                    if (ConvertPosition(unSelectVehicle, unSelectVehicle.LastStatus.Position) > min && ConvertPosition(unSelectVehicle, unSelectVehicle.LastStatus.Position) < max)
                        return null;
                    else
                        return select;
                }
                else
                {
                    //1.在任务的运行区间内
                    //2.在Select的取货行走区间内
                    //同时满足以上两个条件则此次不分配
                    if (ConvertPosition(unSelectVehicle, unSelectVehicle.LastStatus.Position) > min && ConvertPosition(unSelectVehicle, unSelectVehicle.LastStatus.Position) < max)
                        //因为不在空闲车辆列表中 此时不分配
                        return null;
                    else
                        return select;
                }
            }
            else
                return select;
        }

        private int CalStationPriority(string vehicle, SingleLocationDoubleVehicleSubSystemLocation start, SingleLocationDoubleVehicleSubSystemLocation end, SingleLocationDoubleVehicleSubSystem subSystem, List<string> freeList)
        {
            var locPositions = subSystem.Locations.Select(x => (SingleLocationDoubleVehicleSubSystemLocation)x).Select(x => x.Position).OrderBy(x => x);
            var middle = (locPositions.First() + locPositions.Last()) / 2;
            var unSelect = DeviceConverter.ToDevice<RailGuidedVehicleDevice>(freeList.First(x => x != vehicle));
            var select = DeviceConverter.ToDevice<RailGuidedVehicleDevice>(vehicle);
            if (ConvertPosition(select, select.LastStatus.Position) > ConvertPosition(unSelect, unSelect.LastStatus.Position))
                return (start.Position - middle) + (end.Position - middle);
            else
                return (middle - start.Position) + (middle - end.Position);
        }

        int maxPosition = 0;
        private int MaxPosition
        {
            get
            {
                if (maxPosition == 0)
                {
                    var subSystem = (SingleLocationDoubleVehicleSubSystem)Device;
                    maxPosition = subSystem.Locations.Select(x => (SingleLocationDoubleVehicleSubSystemLocation)x).Max(x => x.Position);
                }
                return maxPosition;
            }
        }

        int minPosition = 0;
        public int MinPosition
        {
            get
            {
                if (minPosition == 0)
                {
                    var subSystem = (SingleLocationDoubleVehicleSubSystem)Device;
                    minPosition = subSystem.Locations.Select(x => (SingleLocationDoubleVehicleSubSystemLocation)x).Where(x => x.Position > 0).Min(x => x.Position);
                }
                return minPosition;
            }
        }

        public int MiddlePosition
        {
            get
            {
                return (MaxPosition + minPosition) / 2;
            }
        }

        object objLock = new object();
        /// <summary>
        /// 获取空闲可分配车辆
        /// </summary>
        /// <returns></returns>
        public List<string> GetFreeVehicles(out List<string> dispatchResult)
        {
            lock (objLock)
            {
                dispatchResult = new List<string>();
                List<string> list = new List<string>();
                var subSystem = (SingleLocationDoubleVehicleSubSystem)Device;
                //启用列表
                List<string> ablelist = new List<string>();
                //停用列表
                List<string> stoplist = new List<string>();
                //维修列表
                List<string> repairList = new List<string>();
                //未设置列表
                List<string> unKnowList = new List<string>();
                foreach (var item in subSystem.RailGuidedVehicleDeviceNames)
                {
                    var state = subSystem.GetVehicleState(item);
                    switch (state)
                    {
                        case RailGuidVehicleSetStatus.Working:
                            ablelist.Add(item);
                            break;
                        case RailGuidVehicleSetStatus.Stoping:
                            dispatchResult.Add($"{item}设置为 停用 状态，未添加到空闲列表");
                            stoplist.Add(item);
                            break;
                        case RailGuidVehicleSetStatus.Repairing:
                            dispatchResult.Add($"{item}设置为 维修 状态，未添加到空闲列表");
                            repairList.Add(item);
                            break;
                        case RailGuidVehicleSetStatus.Unknow:
                        default:
                            dispatchResult.Add($"{item}设置为 未知 状态，未添加到空闲列表");
                            unKnowList.Add(item);
                            break;
                    }
                }

                //有未知状态的车辆则不允许进行调度
                if (unKnowList.Count() != 0)
                {
                    dispatchResult.Add($"计算结果：存在设置为 未知 状态的车辆，空闲车辆计算失败");
                    return null;
                }
                ////停用状态车辆设置，不允许调度该车辆所在站点
                //if (stoplist.Count() != 0)
                //    return null;
                //可用车辆数量为0时不允许进行调度
                if (ablelist.Count == 0)
                {
                    dispatchResult.Add($"计算结果：设置为 启用 状态的车辆数量为0，空闲车辆计算失败");
                    return null;
                }

                foreach (var item in ablelist)
                {
                    if (RestoreInfos.Any(x => x.ActuatingDeviceName == item))
                    {
                        dispatchResult.Add($"{item} 已经分配任务，未添加到空闲列表");
                        continue;
                    }

                    var vehicle = DeviceConverter.ToDevice<RailGuidedVehicleDevice>(item);
                    if (!vehicle.IsIdle.Result)
                    {
                        dispatchResult.Add($"{item} 不在空闲状态，未添加到空闲列表");
                        continue;
                    }
                    var newObject = vehicle.LastStatus;
                    //屏蔽载货分配新任务
                    if (newObject.State == RailGuidedVehicleStatus.有货待命)
                    {
                        dispatchResult.Add($"{item} 处于有货待命状态，未添加到空闲列表");
                        continue;
                    }

                    list.Add(item);
                }

                if (list.Count() > 0)
                    dispatchResult.Add($"计算结果：本次计算空闲车辆列表{string.Join(",", list)}");
                else
                    dispatchResult.Add($"计算结果：本次计算空闲车辆数量为0");

                return list;
            }
        }

        private long GetDistance(AbstractState2 state)
        {
            var currentP = ConvertPosition(state.railGuidedVehicleDevice, state.railGuidedVehicleDevice.LastStatus.Position);
            var stationP = state.destinationLocation.Position;
            return Math.Abs(currentP - stationP);
        }

        private void CheckGiveAway(SingleLocationDoubleVehicleSubSystem subSystem, RailGuidedVehicleDevice giveAwayVehicle, uint giveAwayPosition, ref NotArriveToDestinationLocationState2 state, out NotArriveToDestinationLocationState2 giveAwayState)
        {
            giveAwayState = null;
            //车辆空闲并且未分配任务
            if (giveAwayVehicle.IsIdle.Result && !VehicleRecording.ContainsKey(giveAwayVehicle.Name) && !RestoreInfos.Any(x => x.ActuatingDeviceName == giveAwayVehicle.Name))
            {
                this.Log($"开始让道计算，{giveAwayVehicle} 空闲，无运行记录，无绑定记录");
                var currentP = ConvertPosition(state.railGuidedVehicleDevice, state.railGuidedVehicleDevice.LastStatus.Position);
                var stationP = state.destinationLocation.Position;
                var runningMin = Math.Min(currentP, stationP);
                var runningMax = Math.Max(currentP, stationP);
                //这里是否需要检测可分配任务（包括站点高优先级任务）站点 或者执行 顺道接货的任务？？？可优化的点
                SingleLocationDoubleVehicleSubSystemLocation giveAwayStation;
                if (currentP == runningMin)
                {
                    long safeMax, slowMax;
                    safeMax = runningMax + subSystem.Config.SafeDistance;
                    slowMax = runningMax + subSystem.Config.SlowDistance;

                    //判断减速距离是否有效
                    if (slowMax > safeMax)
                    {
                        if (giveAwayPosition < currentP || giveAwayPosition > slowMax)
                        {
                            this.Log($"0让道计算，让道车辆{giveAwayVehicle}处于安全状态，无需让道");
                            return;
                        }
                        else
                        {
                            giveAwayStation = SingleLocationDoubleVehicleSubSystemLocations
                                              .Where(x => x.Position > slowMax)
                                              .OrderBy(x => x.Position)
                                              .FirstOrDefault();//升序第一个
                            if (giveAwayStation == null)
                            {
                                giveAwayStation = SingleLocationDoubleVehicleSubSystemLocations
                                                  .Where(x => x.Position > safeMax)
                                                  .OrderByDescending(x => x.Position)
                                                  .FirstOrDefault();//降序第一个，即选择条件允许的情况下最远的站点
                                this.Log($"2让道计算，为让道车辆{giveAwayVehicle}，计算让道站点{giveAwayStation}");
                            }
                            else
                            {
                                this.Log($"1让道计算，为让道车辆{giveAwayVehicle}，计算让道站点{giveAwayStation}");
                            }
                        }
                    }
                    else
                    {
                        if (giveAwayPosition < currentP || giveAwayPosition > safeMax)
                        {
                            this.Log($"4让道计算，让道车辆{giveAwayVehicle}处于安全状态，无需让道");
                            return;
                        }
                        else
                        {
                            giveAwayStation = SingleLocationDoubleVehicleSubSystemLocations
                                              .Where(x => x.Position > safeMax)
                                              .OrderBy(x => x.Position)
                                              .FirstOrDefault();//默认选择，升序第一个
                            this.Log($"3让道计算，为让道车辆{giveAwayVehicle}，计算让道站点{giveAwayStation}");
                        }
                    }

                    ////过滤已在站点重复发送命令
                    ////过滤已在安全距离之外错误发送往回跑的命令
                    //if (giveAwayStation != null)
                    //{
                    //    //判断站点
                    //    if (giveAwayStation.StationNo == giveAwayVehicle.GetCurrentStation().StationNo)
                    //        return;
                    //    //判断条码
                    //    if ((giveAwayPosition > currentP && giveAwayStation.Position > giveAwayPosition) || (giveAwayPosition < currentP && giveAwayStation.Position < giveAwayPosition))
                    //        return;
                    //}
                }
                else
                {
                    long safeMin, slowMin;
                    safeMin = runningMin - subSystem.Config.SafeDistance;
                    slowMin = runningMin - subSystem.Config.SlowDistance;

                    //判断减速距离是否有效
                    if (slowMin < safeMin)
                    {
                        if (giveAwayPosition > currentP || giveAwayPosition < slowMin)
                        {
                            this.Log($"1让道计算，让道车辆{giveAwayVehicle}处于安全状态，无需让道");
                            return;
                        }
                        else
                        {
                            giveAwayStation = SingleLocationDoubleVehicleSubSystemLocations
                                              .Where(x => x.Position > 0 && x.Position < slowMin)
                                              .OrderByDescending(x => x.Position)
                                              .FirstOrDefault();//降序第一个
                            if (giveAwayStation == null)
                            {
                                giveAwayStation = SingleLocationDoubleVehicleSubSystemLocations
                                                  .Where(x => x.Position > 0 && x.Position < safeMin)
                                                  .OrderBy(x => x.Position)
                                                  .FirstOrDefault();//升序第一个
                                this.Log($"6让道计算，为让道车辆{giveAwayVehicle}，计算让道站点{giveAwayStation}");
                            }
                            else
                            {
                                this.Log($"5让道计算，为让道车辆{giveAwayVehicle}，计算让道站点{giveAwayStation}");
                            }
                        }
                    }
                    else
                    {
                        if (giveAwayPosition > currentP || giveAwayPosition < safeMin)
                        {
                            this.Log($"2让道计算，让道车辆{giveAwayVehicle}处于安全状态，无需让道");
                            return;
                        }
                        else
                        {
                            giveAwayStation = SingleLocationDoubleVehicleSubSystemLocations
                                              .Where(x => x.Position > 0 && x.Position < safeMin)
                                              .OrderByDescending(x => x.Position)
                                              .FirstOrDefault();//默认选择，降序第一个
                            this.Log($"8让道计算，为让道车辆{giveAwayVehicle}，计算让道站点{giveAwayStation}");
                        }
                    }
                }

                //过滤已在站点重复发送命令
                //过滤已在安全距离之外错误发送往回跑的命令
                if (giveAwayStation != null)
                {
                    var currentStation = giveAwayVehicle.GetCurrentStation();
                    if (currentStation != null && giveAwayStation.StationNo == currentStation.StationNo)
                    {
                        this.Log($"让道计算，让道车辆{giveAwayVehicle}正处于计算让道站点位置，无需让道");
                        return;
                    }
                    if ((giveAwayPosition > currentP && giveAwayPosition > giveAwayStation.Position) || (giveAwayPosition < currentP && giveAwayPosition < giveAwayStation.Position))
                    {
                        this.Log($"3让道计算，让道车辆{giveAwayVehicle}处于安全状态，无需让道");
                        return;
                    }
                }
                else
                    //if (giveAwayStation == null)
                    throw new Exception("计算让道站点失败");
                giveAwayState = new NotArriveToDestinationLocationState2(this, giveAwayVehicle.Name, giveAwayStation);
                this.Log($"让道结果：让道车辆{giveAwayVehicle}计算出让道状态{giveAwayState}");
                return;
            }
            else
            {
                //如果车辆已经分配了行走命令并且正在执行中
                if (VehicleRecording.ContainsKey(giveAwayVehicle.Name))
                {
                    this.Log($"开始让道计算：{giveAwayVehicle} 运行记录{VehicleRecording[giveAwayVehicle.Name]}");
                    giveAwayPosition = (uint)VehicleRecording[giveAwayVehicle.Name].destinationLocation.Position;
                    var currentP = ConvertPosition(state.railGuidedVehicleDevice, state.railGuidedVehicleDevice.LastStatus.Position);
                    var stationP = state.destinationLocation.Position;
                    var min = Math.Min(currentP, stationP);
                    var max = Math.Max(currentP, stationP);
                    if (currentP == min)
                        max += subSystem.Config.SafeDistance;
                    else
                        min -= subSystem.Config.SafeDistance;

                    if ((ConvertPosition(giveAwayVehicle, giveAwayVehicle.LastStatus.Position) < currentP && giveAwayPosition < min)
                        || (ConvertPosition(giveAwayVehicle, giveAwayVehicle.LastStatus.Position) > currentP && giveAwayPosition > max))
                    {
                        this.Log($"让道结果：{giveAwayVehicle} 不在 {state} 当前运行范围内，可以直接运行");
                        return;
                    }
                    else
                    {
                        var giveAwayRecord = VehicleRecording[giveAwayVehicle.Name];
                        //尝试判断当前车辆任务类型 如果判断成功则后续继续计算如果判断失败则做自动让道到最端点的动作
                        if (giveAwayRecord.Name == "放货" || giveAwayVehicle.LastStatus.State == RailGuidedVehicleStatus.卸货中)
                        {
                            this.Log($"让道计算：{giveAwayVehicle} 运行记录{giveAwayRecord} 车辆状态 {giveAwayVehicle.LastStatus.State}");
                            //卸货中的判断接完货之后的目标站点和当前车辆目标站点哪个比较近，比较远的直接让道
                            var _currentP = ConvertPosition(giveAwayVehicle, giveAwayVehicle.LastStatus.Position);
                            var _restoreInfo = RestoreInfos.FirstOrDefault(x => x.ActuatingDeviceName == giveAwayVehicle.Name);
                            if (_restoreInfo != null)
                            {
                                this.Log($"让道计算：查询到{giveAwayVehicle} 对应的绑定记录 {_restoreInfo}");
                                var _restoreAction = this.Actions.FirstOrDefault(x => x.Id == _restoreInfo.EquipmentActionId);
                                if (_restoreAction != null)
                                {
                                    this.Log($"让道计算：查询到{giveAwayVehicle} 对应的绑定任务 {_restoreAction}");
                                    var _ra = (SingleLocationDoubleVehicleSubSystemTransferAction)_restoreAction;
                                    var _end = (SingleLocationDoubleVehicleSubSystemLocation)LocationConverter.ToLocation(_ra.EndLocation);
                                    var _stationP = _end.Position;
                                    if ((_currentP < currentP && _stationP < min) || (_currentP > currentP && _stationP > max))
                                    //直接发送
                                    {
                                        this.Log($"让道结果：{giveAwayVehicle} 不在 {state} 当前运行范围内，可以直接运行");
                                        return;
                                    }
                                    else
                                    {
                                        this.Log($"让道结果：{giveAwayVehicle} 在 {state} 当前运行范围，本次设置state为空");
                                        state = null; //20230522新增不直接发送待后续计算
                                        return;
                                    }
                                }
                                else
                                {
                                    this.Log($"让道结果：未查询到{giveAwayVehicle} 对应的绑定任务，本次设置state为空");
                                    //卸货中的车辆，如果没有查询到任务记录则直接忽略，当前车辆不发送
                                    state = null;
                                    return;
                                }
                            }
                            else
                            {
                                this.Log($"让道结果：未查询到 {giveAwayVehicle} 对应的绑定记录，本次设置state为空");
                                //卸货中的车辆，如果没有查询到任务记录则直接忽略，当前车辆不发送
                                state = null;
                                return;
                            }
                        }
                        else if (giveAwayRecord.Name == "取货" || giveAwayVehicle.LastStatus.State == RailGuidedVehicleStatus.接货中)
                        {
                            this.Log($"让道计算：{giveAwayVehicle} 运行记录{giveAwayRecord} 车辆状态 {giveAwayVehicle.LastStatus.State}");
                            //接货中的判断接完货之后的目标站点和当前车辆目标站点哪个比较近，比较远的直接让道，比较近则直接等待
                            var _currentP = ConvertPosition(giveAwayVehicle, giveAwayVehicle.LastStatus.Position);
                            var _restoreInfo = RestoreInfos.FirstOrDefault(x => x.ActuatingDeviceName == giveAwayVehicle.Name);
                            if (_restoreInfo != null)
                            {
                                this.Log($"让道计算：查询到{giveAwayVehicle} 对应的绑定记录 {_restoreInfo}");
                                var _restoreAction = this.Actions.FirstOrDefault(x => x.Id == _restoreInfo.EquipmentActionId);
                                if (_restoreAction != null)
                                {
                                    this.Log($"让道计算：查询到{giveAwayVehicle} 对应的绑定任务 {_restoreAction}");
                                    var _stationP = ConvertPosition(giveAwayVehicle, giveAwayVehicle.LastStatus.Position);
                                    //var _ra = (SingleLocationDoubleVehicleSubSystemTransferAction)_restoreAction;
                                    //var _end = (SingleLocationDoubleVehicleSubSystemLocation)LocationConverter.ToLocation(_ra.EndLocation);
                                    //var _stationP = _end.Position;
                                    //if ((_currentP < currentP && _stationP < min) || (_currentP > currentP && _stationP > max))
                                    //    //直接发送
                                    //    return;
                                    //else
                                    //{
                                    if (Math.Abs(currentP - stationP) < Math.Abs(_currentP - _stationP))
                                    {
                                        this.Log($"让道结果：{giveAwayVehicle} 在 {state} 当前运行范围，本次设置state为空");
                                        state = null; //20230522新增不直接发送待后续计算
                                        return;
                                    }
                                    else
                                    {
                                        //当前车辆做让道移动
                                        SingleLocationDoubleVehicleSubSystemLocation giveAwayStation;
                                        if (ConvertPosition(giveAwayVehicle, giveAwayVehicle.LastStatus.Position) < currentP)
                                        {
                                            if (stationP > _stationP + subSystem.Config.SafeDistance)
                                            {
                                                giveAwayStation = state.destinationLocation;
                                                this.Log($"1让道计算：计算让道站点{giveAwayStation}");
                                            }
                                            else
                                            {
                                                giveAwayStation = (SingleLocationDoubleVehicleSubSystemLocation)subSystem.Locations.Select(x => (SingleLocationDoubleVehicleSubSystemLocation)x).Where(x => x.Position > 0 && x.Position > _stationP + subSystem.Config.SafeDistance).OrderBy(x => x.Position).FirstOrDefault();
                                                this.Log($"2让道计算：计算让道站点{giveAwayStation}");
                                            }
                                        }
                                        else
                                        {
                                            if (stationP < _stationP - subSystem.Config.SafeDistance)
                                            {
                                                giveAwayStation = state.destinationLocation;
                                                this.Log($"3让道计算：计算让道站点{giveAwayStation}");
                                            }
                                            else
                                            {
                                                giveAwayStation = (SingleLocationDoubleVehicleSubSystemLocation)subSystem.Locations.Select(x => (SingleLocationDoubleVehicleSubSystemLocation)x).Where(x => x.Position > 0 && x.Position < _stationP - subSystem.Config.SafeDistance).OrderByDescending(x => x.Position).FirstOrDefault();
                                                this.Log($"4让道计算：计算让道站点{giveAwayStation}");
                                            }
                                        }

                                        var currentStation = state.railGuidedVehicleDevice.GetCurrentStation();
                                        if (currentStation == null || currentStation.StationNo != giveAwayStation.StationNo)
                                        {
                                            if (state.railGuidedVehicleDevice.IsIdle.Result)
                                            {
                                                this.Log($"让道结果：变更{state}目标站点为{giveAwayStation}");
                                                state.destinationLocation = giveAwayStation;
                                            }
                                            else
                                            {
                                                this.Log($"让道结果：{state.railGuidedVehicleDevice.Name}本次计算目标站点为{giveAwayStation}，但是车辆不处于空闲状态，本次未重置state");
                                            }
                                        }
                                        else
                                        {
                                            var _currentStation = currentStation == null ? "NULL" : $"{currentStation}";
                                            this.Log($"让道结果：车辆当前站点{_currentStation},计算让道站点{giveAwayStation},设置state为空");
                                            state = null;
                                        }
                                        return;
                                    }
                                    //}
                                }
                                else
                                ////接货中的车辆，如果没有查询到任务记录则直接忽略，当前车辆直接发送到目的点
                                //return;
                                {
                                    this.Log($"让道结果：未查询到{giveAwayVehicle} 对应的绑定任务，本次设置state为空");
                                    state = null;
                                    return;
                                }
                            }
                            else
                            ////接货中的车辆，如果没有查询到任务记录则直接忽略，当前车辆直接发送到目的点
                            //return;
                            {
                                this.Log($"让道结果：未查询到 {giveAwayVehicle} 对应的绑定记录，本次设置state为空");
                                state = null;
                                return;
                            }
                        }
                        else if (giveAwayVehicle.LastStatus.State == RailGuidedVehicleStatus.有货有任务 || giveAwayVehicle.LastStatus.State == RailGuidedVehicleStatus.A)
                        {
                            this.Log($"让道结果：让道车辆{giveAwayVehicle}当前状态{giveAwayVehicle.LastStatus.State.GetDescription()}，本次设置state为空");
                            //暂时不发
                            state = null;
                            return;
                        }
                        else
                        {
                            this.Log($"让道计算：{giveAwayVehicle} 状态 {giveAwayVehicle.LastStatus.State.GetDescription()} 开始其他让道计算");
                            //当前车辆做渐进运动
                            SingleLocationDoubleVehicleSubSystemLocation giveAwayStation;
                            if (currentP < ConvertPosition(giveAwayVehicle, giveAwayVehicle.LastStatus.Position))
                            {
                                if (state.destinationLocation.Position < giveAwayPosition - subSystem.Config.SafeDistance)
                                {
                                    this.Log($"1让道结果：{giveAwayVehicle} 不在 {state} 当前运行范围内，可以直接运行");
                                    return;
                                }
                                giveAwayStation = (SingleLocationDoubleVehicleSubSystemLocation)subSystem.Locations
                                    .Select(x => (SingleLocationDoubleVehicleSubSystemLocation)x)
                                    .Where(x => x.Position > 0 && x.Position < giveAwayPosition - subSystem.Config.SafeDistance)
                                    .OrderByDescending(x => x.Position)
                                    .FirstOrDefault();
                                this.Log($"1让道计算：计算让道站点{giveAwayStation}");
                            }
                            else
                            {
                                if (state.destinationLocation.Position > giveAwayPosition + subSystem.Config.SafeDistance)
                                {
                                    this.Log($"2让道结果：{giveAwayVehicle} 不在 {state} 当前运行范围内，可以直接运行");
                                    return;
                                }
                                giveAwayStation = (SingleLocationDoubleVehicleSubSystemLocation)subSystem.Locations
                                    .Select(x => (SingleLocationDoubleVehicleSubSystemLocation)x)
                                    .Where(x => x.Position > 0 && x.Position > giveAwayPosition + subSystem.Config.SafeDistance)
                                    .OrderBy(x => x.Position)
                                    .FirstOrDefault();
                                this.Log($"2让道计算：计算让道站点{giveAwayStation}");
                            }

                            //当车辆不是在站点时小车上报站点编号为0，因此下面的算法无效
                            var currentStation = state.railGuidedVehicleDevice.GetCurrentStation();
                            if (currentStation == null || currentStation.StationNo != giveAwayStation.StationNo)
                            {
                                if (state.railGuidedVehicleDevice.IsIdle.Result)
                                {
                                    this.Log($"让道结果：变更{state}目标站点为{giveAwayStation}");
                                    state.destinationLocation = giveAwayStation;
                                }
                                else
                                {
                                    this.Log($"让道结果：{state.railGuidedVehicleDevice.Name}本次计算目标站点为{giveAwayStation}，但是车辆不处于空闲状态，本次未重置state");
                                }
                            }
                            else
                            {
                                var _currentStation = currentStation == null ? "NULL" : $"{currentStation}";
                                this.Log($"让道结果：车辆当前站点{_currentStation},计算让道站点{giveAwayStation},设置state为空");
                                state = null;
                            }
                            return;
                        }
                    }
                }
                //如果车辆不报警 不空闲 并且未分配任务 如果不在安全区域内 则直接执行 如果在安全区域内则尝试获取本次车辆执行的是什么任务
                else //if (!VehicleRecording.ContainsKey(giveAwayVehicle.Name))
                {
                    this.Log($"开始让道计算：{giveAwayVehicle} 未查询到运行记录");
                    //尝试判断当前车辆任务类型 如果判断成功则后续继续计算如果判断失败则做自动让道到最端点的动作
                    if (giveAwayVehicle.LastStatus.State == RailGuidedVehicleStatus.接货中 || giveAwayVehicle.LastStatus.State == RailGuidedVehicleStatus.卸货中)
                    {
                        this.Log($"让道计算：{giveAwayVehicle} 车辆状态 {giveAwayVehicle.LastStatus.State}");
                        var currentP = ConvertPosition(state.railGuidedVehicleDevice, state.railGuidedVehicleDevice.LastStatus.Position);
                        var stationP = state.destinationLocation.Position;
                        var min = Math.Min(currentP, stationP);
                        var max = Math.Max(currentP, stationP);
                        if (currentP == min)
                            max += subSystem.Config.SafeDistance;
                        else
                            min -= subSystem.Config.SafeDistance;

                        if ((ConvertPosition(giveAwayVehicle, giveAwayVehicle.LastStatus.Position) < currentP && giveAwayPosition < min)
                            || (ConvertPosition(giveAwayVehicle, giveAwayVehicle.LastStatus.Position) > currentP && giveAwayPosition > max))
                        {
                            this.Log($"让道结果：{giveAwayVehicle} 不在 {state} 当前运行范围内，可以直接运行");
                            return;
                        }
                        else
                        {
                            if (giveAwayVehicle.LastStatus.State == RailGuidedVehicleStatus.卸货中)
                            {
                                //卸货中的判断接完货之后的目标站点和当前车辆目标站点哪个比较近，比较远的直接让道
                                var _currentP = ConvertPosition(giveAwayVehicle, giveAwayVehicle.LastStatus.Position);
                                var _restoreInfo = RestoreInfos.FirstOrDefault(x => x.ActuatingDeviceName == giveAwayVehicle.Name);
                                if (_restoreInfo != null)
                                {
                                    this.Log($"让道计算：查询到{giveAwayVehicle} 对应的绑定记录 {_restoreInfo}");
                                    var _restoreAction = this.Actions.FirstOrDefault(x => x.Id == _restoreInfo.EquipmentActionId);
                                    if (_restoreAction != null)
                                    {
                                        var _ra = (SingleLocationDoubleVehicleSubSystemTransferAction)_restoreAction;
                                        var _end = (SingleLocationDoubleVehicleSubSystemLocation)LocationConverter.ToLocation(_ra.EndLocation);
                                        var _stationP = _end.Position;
                                        if ((_currentP < currentP && _stationP < min) || (_currentP > currentP && _stationP > max))
                                        //直接发送
                                        {
                                            this.Log($"让道结果：{giveAwayVehicle} 在 {state} 当前运行范围内，可以直接运行");
                                            return;
                                        }
                                        else
                                        {
                                            this.Log($"让道结果：{giveAwayVehicle} 不在 {state} 当前运行范围内，state置为空");
                                            state = null; //20230522新增不直接发送待后续计算
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        //卸货中的车辆，如果没有查询到任务记录则直接忽略，当前车辆不发送
                                        this.Log($"让道结果：{giveAwayVehicle} 在卸货中但是没有查询到任务记录，state置为空");
                                        state = null;
                                        return;
                                    }
                                }
                                else
                                {
                                    this.Log($"让道结果：未查询到{giveAwayVehicle} 对应的绑定记录");
                                    //卸货中的车辆，如果没有查询到任务记录则直接忽略，当前车辆不发送
                                    state = null;
                                    return;
                                }
                            }
                            else if (giveAwayVehicle.LastStatus.State == RailGuidedVehicleStatus.接货中)
                            {
                                //接货中的判断接完货之后的目标站点和当前车辆目标站点哪个比较近，比较远的直接让道
                                var _currentP = ConvertPosition(giveAwayVehicle, giveAwayVehicle.LastStatus.Position);
                                var _restoreInfo = RestoreInfos.FirstOrDefault(x => x.ActuatingDeviceName == giveAwayVehicle.Name);
                                if (_restoreInfo != null)
                                {
                                    this.Log($"让道计算：查询到{giveAwayVehicle} 对应的绑定记录 {_restoreInfo}");
                                    var _restoreAction = this.Actions.FirstOrDefault(x => x.Id == _restoreInfo.EquipmentActionId);
                                    if (_restoreAction != null)
                                    {
                                        var _stationP = ConvertPosition(giveAwayVehicle, giveAwayVehicle.LastStatus.Position);
                                        //var _ra = (SingleLocationDoubleVehicleSubSystemTransferAction)_restoreAction;
                                        //var _end = (SingleLocationDoubleVehicleSubSystemLocation)LocationConverter.ToLocation(_ra.EndLocation);
                                        //var _stationP = _end.Position;
                                        //if ((ConvertPosition(giveAwayVehicle, giveAwayVehicle.LastStatus.Position) < currentP && _stationP < min)
                                        //    || (ConvertPosition(giveAwayVehicle, giveAwayVehicle.LastStatus.Position) > currentP && _stationP > max))
                                        //    //直接发送
                                        //    return;
                                        //else
                                        {
                                            if (Math.Abs(currentP - stationP) < Math.Abs(_currentP - _stationP))
                                            //当前任务比较近则直接发送
                                            {
                                                this.Log($"让道结果：{giveAwayVehicle} 在 {state} 当前运行范围内，state置空");
                                                state = null; //20230522新增不直接发送待后续计算
                                                return;
                                            }
                                            else
                                            {
                                                //当前车辆做让道移动
                                                SingleLocationDoubleVehicleSubSystemLocation giveAwayStation;
                                                if (ConvertPosition(giveAwayVehicle, giveAwayVehicle.LastStatus.Position) < currentP)
                                                {
                                                    if (stationP > _stationP + subSystem.Config.SafeDistance)
                                                    {
                                                        giveAwayStation = state.destinationLocation;
                                                        this.Log($"1让道计算：计算让道站点{giveAwayStation}");
                                                    }
                                                    else
                                                    {
                                                        giveAwayStation = (SingleLocationDoubleVehicleSubSystemLocation)subSystem.Locations.Select(x => (SingleLocationDoubleVehicleSubSystemLocation)x).Where(x => x.Position > 0 && x.Position > _stationP + subSystem.Config.SafeDistance).OrderBy(x => x.Position).FirstOrDefault();
                                                    }
                                                    this.Log($"1让道计算：计算让道站点{giveAwayStation}");
                                                }
                                                else
                                                {
                                                    if (stationP < _stationP - subSystem.Config.SafeDistance)
                                                    {
                                                        giveAwayStation = state.destinationLocation;
                                                        this.Log($"2让道计算：计算让道站点{giveAwayStation}");
                                                    }
                                                    else
                                                    {
                                                        giveAwayStation = (SingleLocationDoubleVehicleSubSystemLocation)subSystem.Locations.Select(x => (SingleLocationDoubleVehicleSubSystemLocation)x).Where(x => x.Position > 0 && x.Position < _stationP - subSystem.Config.SafeDistance).OrderByDescending(x => x.Position).FirstOrDefault();
                                                        this.Log($"2让道计算：计算让道站点{giveAwayStation}");
                                                    }
                                                }

                                                var currentStation = state.railGuidedVehicleDevice.GetCurrentStation();
                                                if (currentStation == null || currentStation.StationNo != giveAwayStation.StationNo)
                                                {
                                                    if (state.railGuidedVehicleDevice.IsIdle.Result)
                                                    {
                                                        this.Log($"让道结果：变更{state}目标站点为{giveAwayStation}");
                                                        state.destinationLocation = giveAwayStation;
                                                    }
                                                    else
                                                    {
                                                        this.Log($"让道结果：{state.railGuidedVehicleDevice.Name}本次计算目标站点为{giveAwayStation}，但是车辆不处于空闲状态，本次未重置state");
                                                    }
                                                }
                                                else
                                                {
                                                    var _currentStation = currentStation == null ? "NULL" : $"{currentStation}";
                                                    this.Log($"让道结果：车辆当前站点{_currentStation},计算让道站点{giveAwayStation},设置state为空");
                                                    state = null;
                                                }
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        state = null;
                                        return;
                                    }
                                }
                                else
                                {
                                    state = null;
                                    return;
                                }
                            }
                        }
                    }
                    else if (giveAwayVehicle.LastStatus.State == RailGuidedVehicleStatus.无货运行 || giveAwayVehicle.LastStatus.State == RailGuidedVehicleStatus.有货运行)
                    {
                        this.Log($"让道计算：{giveAwayVehicle} 车辆状态 {giveAwayVehicle.LastStatus.State}");
                        //可能是重启导致的系统未记录车辆所执行任务 直接发送到边际点避让 让其执行完成
                        SingleLocationDoubleVehicleSubSystemLocation giveAwayStation;
                        if (ConvertPosition(giveAwayVehicle, giveAwayVehicle.LastStatus.Position) < ConvertPosition(state.railGuidedVehicleDevice, state.railGuidedVehicleDevice.LastStatus.Position))
                        {
                            giveAwayStation = (SingleLocationDoubleVehicleSubSystemLocation)subSystem.Locations.Select(x => (SingleLocationDoubleVehicleSubSystemLocation)x).Where(x => x.Position != 0).OrderByDescending(x => x.Position).FirstOrDefault();
                            this.Log($"1让道计算：计算让道站点{giveAwayStation}");
                        }
                        else
                        {
                            giveAwayStation = (SingleLocationDoubleVehicleSubSystemLocation)subSystem.Locations.Select(x => (SingleLocationDoubleVehicleSubSystemLocation)x).Where(x => x.Position != 0).OrderBy(x => x.Position).FirstOrDefault();
                            this.Log($"2让道计算：计算让道站点{giveAwayStation}");
                        }

                        var currentStation = state.railGuidedVehicleDevice.GetCurrentStation();
                        if (currentStation == null || currentStation.StationNo != giveAwayStation.StationNo)
                        {
                            if (state.railGuidedVehicleDevice.IsIdle.Result)
                            {
                                this.Log($"让道结果：变更{state}目标站点为{giveAwayStation}");
                                state.destinationLocation = giveAwayStation;
                            }
                            else
                            {
                                this.Log($"让道结果：{state.railGuidedVehicleDevice.Name}本次计算目标站点为{giveAwayStation}，但是车辆不处于空闲状态，本次未重置state");
                            }
                        }
                        else
                        {
                            var _currentStation = currentStation == null ? "NULL" : $"{currentStation}";
                            this.Log($"让道结果：车辆当前站点{_currentStation},计算让道站点{giveAwayStation},设置state为空");
                            state = null;
                        }
                    }
                    else if (giveAwayVehicle.LastStatus.State == RailGuidedVehicleStatus.有货有任务 || giveAwayVehicle.LastStatus.State == RailGuidedVehicleStatus.A)
                    {
                        this.Log($"让道结果：让道车辆{giveAwayVehicle}当前状态{giveAwayVehicle.LastStatus.State.GetDescription()}，本次设置state为空");
                        //暂时不发
                        state = null;
                        return;
                    }
                    else
                    {
                        this.Log($"让道计算-其他：{giveAwayVehicle} 车辆状态 {giveAwayVehicle.LastStatus.State}");
                        #region 20230419更新 车辆在其他状态时 如果在安全区域外则当前车辆可以执行任务
                        giveAwayPosition = ConvertPosition(giveAwayVehicle, giveAwayVehicle.LastStatus.Position);
                        var currentP = ConvertPosition(state.railGuidedVehicleDevice, state.railGuidedVehicleDevice.LastStatus.Position);
                        var stationP = state.destinationLocation.Position;
                        var min = Math.Min(currentP, stationP);
                        var max = Math.Max(currentP, stationP);
                        if (currentP == min)
                            max += subSystem.Config.SafeDistance;
                        else
                            min -= subSystem.Config.SafeDistance;

                        if ((ConvertPosition(giveAwayVehicle, giveAwayVehicle.LastStatus.Position) < currentP && giveAwayPosition < min)
                            || (ConvertPosition(giveAwayVehicle, giveAwayVehicle.LastStatus.Position) > currentP && giveAwayPosition > max))
                            return;
                        #endregion
                        else
                        {
                            this.Log($"让道结果：让道车辆{giveAwayVehicle}当前状态{giveAwayVehicle.LastStatus.State.GetDescription()}，本次设置state为空");
                            //车辆状态无法判断，此时无法发送任务
                            state = null;
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 引发任务完成事件
        /// </summary>
        protected virtual void FireTaskCompleteEvent(int equipmentActionTaskId)
        {
            var device = (SingleLocationDoubleVehicleSubSystem)Device;
            Type type = device.GetType();
            var FireTaskCompletedEventMethod = type.GetMethod("FireTaskCompletedEvent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (FireTaskCompletedEventMethod == null)
            {
                throw new MissingMethodException(string.Format("未找到名为 FireTaskCompletedEvent 的方法"));
            }
            FireTaskCompletedEventMethod.Invoke(device, new object[] { new Wcs.Framework.TaskCompletedEventArgs(equipmentActionTaskId) });
        }

        public string GetDispatchShowInfo()
        {
            try
            {
                string showMsg = "";
                if (RestoreInfos == null)
                    showMsg = "▲未设置上下文信息";
                else
                {
                    for (int i = 0; i < RestoreInfos.Count; i++)
                    {
                        var item = RestoreInfos[i];
                        var _action = this.Actions.FirstOrDefault(x => x.Id == item.EquipmentActionId);
                        if (string.IsNullOrWhiteSpace(showMsg))
                            showMsg = $"▲上下文信息：{item}";
                        else
                            showMsg += $"\r\n▲上下文信息：{item}";
                        if (_action != null)
                            showMsg += $"\r\n==>任务信息：{_action.ToReadableDescription()}";
                    }
                }
                if (VehicleRecording == null)
                {
                    if (string.IsNullOrWhiteSpace(showMsg))
                        showMsg = "★未分配车辆动作信息";
                    else
                        showMsg += "\r\n★未分配车辆动作信息";
                }
                else
                {
                    var keys = VehicleRecording.Keys.ToArray();
                    for (int i = 0; i < keys.Length; i++)
                    {
                        var key = keys[i];
                        var value = VehicleRecording[key];
                        if (string.IsNullOrWhiteSpace(showMsg))
                            showMsg = $"★车辆动作信息：{value}";
                        else
                            showMsg += $"\r\n★车辆动作信息：{value}";
                    }
                }
                return showMsg;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public override string ToString()
        {
            return $"{this.DeviceName}EquipmentActionScheduler";
        }
    }
}
