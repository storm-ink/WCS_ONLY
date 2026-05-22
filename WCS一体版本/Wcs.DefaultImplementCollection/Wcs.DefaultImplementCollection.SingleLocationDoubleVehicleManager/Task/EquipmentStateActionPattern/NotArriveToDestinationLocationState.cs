using System;
using System.Linq;
using Wcs.DefaultImplementCollection.RailGuidedVehicle;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.SingleLocationDoubleVehicleManager
{
    /// <summary>
    /// 需要移动到目的点
    /// </summary>
    public class NotArriveToDestinationLocationState : AbstractState
    {
        public NotArriveToDestinationLocationState(SingleLocationDoubleVehicleSubSystemEquipmentActionScheduler scheduler, StateManagerRestoreInfo context, EquipmentAction action, LocationInfo location, string name)
        {
            _scheduler = scheduler;
            _context = context;
            _action = action;
            railGuidedVehicleDevice = DeviceConverter.ToDevice<RailGuidedVehicleDevice>(_context.ActuatingDeviceName);
            destinationLocation = (SingleLocationDoubleVehicleSubSystemLocation)LocationConverter.ToLocation(location);
            _name = name;
        }
        public NotArriveToDestinationLocationState(SingleLocationDoubleVehicleSubSystemEquipmentActionScheduler scheduler, string giveAwayVehicle, SingleLocationDoubleVehicleSubSystemLocation location)
        {
            railGuidedVehicleDevice = DeviceConverter.ToDevice<RailGuidedVehicleDevice>(giveAwayVehicle);
            _scheduler = scheduler;
            destinationLocation = location;
            _name = "让道";
            _giveAwayMark = true;
        }

        string _name;
        public override string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// 指示当前是否可以发送向取货点移动的指令
        /// </summary>
        /// <remarks>
        /// <para>发送条件：</para>
        /// <para>1、设备处于空闲状态</para>
        /// <para>2、当前状态未完成</para>
        /// <para>3、上下文中的物理动作状态为 Executing</para>
        /// </remarks>
        /// <returns></returns>
        public override CanPerformResult CanPerform()
        {
            if (IsCompleted().Result)
                return new CanPerformResult(false, string.Format("当前状态 {0} 已完成, 不允许发送命令", this.Name));

            if (!_giveAwayMark && _action.Status != EquipmentActionStatus.New && _action.Status != EquipmentActionStatus.Executing)
                return new CanPerformResult(false, string.Format("当前任务处于 {0} 状态(允许发送放货命令的状态应为 新任务/执行中), 不允许发送任务", this.Name));

            if (railGuidedVehicleDevice.IsIdle.Result != true)
                return new CanPerformResult(false, string.Format("当前设备 {0} 不空闲, 不允许发送命令", railGuidedVehicleDevice.Name));

            if (_scheduler.VehicleRecording.ContainsKey(railGuidedVehicleDevice.Name))
            {
                var state = _scheduler.VehicleRecording[railGuidedVehicleDevice.Name];
                if (state.lastSendTaskId == railGuidedVehicleDevice.LastStatus.TaskId)
                    return new CanPerformResult(false, $"当前设备不空闲，{_scheduler.VehicleRecording[railGuidedVehicleDevice.Name]} 驱动中");
            }

            return new CanPerformResult(true, "");
        }

        /// <summary>
        /// 指示堆垛机是否已到达取货点
        /// </summary>
        /// <remarks>
        /// <para>完成条件：</para>
        /// <para>1、设备处于空闲状态</para>
        /// <para>2、设备已连接并且获取到了当前所处位置数据</para>
        /// <para>3、设备的当前位置（站点）和逻辑动作的起始位置（站点）一致</para>
        /// </remarks>
        /// <returns>完成返回 true；未完成返回假 false</returns>
        /// <exception cref="System.NotSupportedException">物理动作所属的逻辑动作起始位置不是穿梭车站点时引发</exception>
        public override IsCompeltedResult IsCompleted()
        {
            if (railGuidedVehicleDevice.IsIdle.Result != true)
                return new IsCompeltedResult(false, "设备不空闲");

            RailGuidedVehicleStation currentStation = railGuidedVehicleDevice.GetCurrentStation();
            if (currentStation == null)
                return new IsCompeltedResult(false, "设备当前位置获取失败");

            if (currentStation.StationNo != destinationLocation.StationNo)
                return new IsCompeltedResult(false, "设备当前位置不是目地位置");

            return new IsCompeltedResult(true, "");
        }

        /// <summary>
        /// 发送向取货点移动的指令
        /// </summary>
        public override void Perform()
        {
            if (!CanPerform().Result)
                //throw new InvalidOperationException("当前状态不可执行移动命令");
                return;

            if (_scheduler.VehicleRecording.ContainsKey(railGuidedVehicleDevice.Name))
                _scheduler.VehicleRecording[railGuidedVehicleDevice.Name] = this;
            else
                _scheduler.VehicleRecording.Add(railGuidedVehicleDevice.Name, this);

            if (_giveAwayMark)
            {
                //if (_scheduler.VehicleRecording.ContainsKey(railGuidedVehicleDevice.Name))
                //    _scheduler.VehicleRecording[railGuidedVehicleDevice.Name] = this;
                //else
                //    _scheduler.VehicleRecording.Add(railGuidedVehicleDevice.Name, this);

                if (railGuidedVehicleDevice.LastStatus.State == RailGuidedVehicleStatus.无货待命)
                {
                    NormalWalkCommand cmd = new NormalWalkCommand(
                        Wcs.Framework.SerialNumberFactory.GenerateRandomValue(80000000, 99999999).ToString(),
                        0,
                        destinationLocation.StationNo);

                    railGuidedVehicleDevice.Write(cmd, cmd.SendSuccess);
                    lastSendTaskId = cmd.TaskId;
                }
                else if (railGuidedVehicleDevice.LastStatus.State == RailGuidedVehicleStatus.有货待命)
                {
                    WalkWithGoodsCommand cmd = new WalkWithGoodsCommand(
                        Wcs.Framework.SerialNumberFactory.GenerateRandomValue(80000000, 99999999).ToString(),
                        0,
                        destinationLocation.StationNo);

                    railGuidedVehicleDevice.Write(cmd, cmd.SendSuccess);
                    lastSendTaskId = cmd.TaskId;
                }
                return;
            }

            if (railGuidedVehicleDevice.LastStatus.State == RailGuidedVehicleStatus.无货待命)
            {
                string taskId;
                if (destinationLocation.StationNo > 99)
                    taskId = String.Format("1{0:0000000}", _action.EquipmentTaskId);
                else
                    taskId = String.Format("1{0:00}{1:00000}", destinationLocation.StationNo, _action.EquipmentTaskId);

                NormalWalkCommand cmd = new NormalWalkCommand(
                    taskId,
                    Convert.ToUInt16(_action.ContainerCode),
                    destinationLocation.StationNo);

                railGuidedVehicleDevice.Write(cmd, cmd.SendSuccess);
                lastSendTaskId = cmd.TaskId;
            }
            else if (railGuidedVehicleDevice.LastStatus.State == RailGuidedVehicleStatus.有货待命)
            {
                string taskId;
                if (destinationLocation.StationNo > 99)
                    taskId = String.Format("3{0:0000000}", _action.EquipmentTaskId);
                else
                    taskId = String.Format("3{0:00}{1:00000}", destinationLocation.StationNo, _action.EquipmentTaskId);
                WalkWithGoodsCommand cmd = new WalkWithGoodsCommand(
                    taskId,
                    Convert.ToUInt16(_action.ContainerCode),
                    destinationLocation.StationNo);

                railGuidedVehicleDevice.Write(cmd, cmd.SendSuccess);
                lastSendTaskId = cmd.TaskId;
            }
        }
    }
}