using System;
using Wcs.DefaultImplementCollection.RailGuidedVehicle;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.SingleLocationDoubleVehicleManager
{
    /// <summary>
    /// 未取货，等待取货
    /// </summary>
    public class NotPickingState : AbstractState
    {
        public NotPickingState(SingleLocationDoubleVehicleSubSystemEquipmentActionScheduler scheduler, StateManagerRestoreInfo context, EquipmentAction action)
        {
            _scheduler = scheduler;
            _context = context;
            _action = action;
            railGuidedVehicleDevice = DeviceConverter.ToDevice<RailGuidedVehicleDevice>(_context.ActuatingDeviceName);
            destinationLocation = (SingleLocationDoubleVehicleSubSystemLocation)LocationConverter.ToLocation(_action.Movement.StartLocation);
        }

        public override string Name
        {
            get { return "取货"; }
        }

        /// <summary>
        /// 指示当前是否可以发送取货指令
        /// </summary>
        /// <remarks>
        /// <para>发送条件：</para>
        /// <para>1、设备处于空闲状态</para>
        /// <para>2、当前状态未完成</para>
        /// <para>3、设备的当前位置（站点号）和逻辑动作的起始位置（站点号）一致</para>
        /// <para>4、上下文中的物理动作状态为 Executing</para>
        /// </remarks>
        /// <returns></returns>
        public override CanPerformResult CanPerform()
        {
            if (IsCompleted().Result)
                return new CanPerformResult(false, "当前状态已完成, 不允许发送命令");

            if (_action.Status != EquipmentActionStatus.New && _action.Status != EquipmentActionStatus.Executing)
                return new CanPerformResult(false, "当前任务不处于 新任务/执行中 状态, 不允许发送任务");

            if (railGuidedVehicleDevice.IsIdle.Result != true)
                return new CanPerformResult(false, "设备不空闲，不允许发送命令");
            
            RailGuidedVehicleStation currentStation = railGuidedVehicleDevice.GetCurrentStation();
            if (currentStation == null)
                return new CanPerformResult(false, "设备当前位置获取失败");

            if (_scheduler.VehicleRecording.ContainsKey(railGuidedVehicleDevice.Name))
            {
                var state = _scheduler.VehicleRecording[railGuidedVehicleDevice.Name];
                if (state.lastSendTaskId == railGuidedVehicleDevice.LastStatus.TaskId)
                    return new CanPerformResult(false, $"当前设备不空闲，{_scheduler.VehicleRecording[railGuidedVehicleDevice.Name]} 驱动中");
            }

            if (currentStation.StationNo != destinationLocation.StationNo)
            {
                //行走到取货点();//这里后续可能需要特殊处理
                return new CanPerformResult(false, "设备当前不在取货站点，已尝试发送到取货点的行走任务");
            }
            return new CanPerformResult(true, "");
        }

        /// <summary>
        /// 指示穿梭车是否已到取货
        /// </summary>
        /// <remarks>
        /// <para>完成条件：</para>
        /// <para>1、设备处于空闲状态</para>
        /// <para>2、设备已连接并且状态数据已同步/para>
        /// <para>3、设备的处于有货待命状态</para>
        /// </remarks>
        /// <returns>完成返回 true；未完成返回假 false</returns>
        public override IsCompeltedResult IsCompleted()
        {
            if (railGuidedVehicleDevice.IsIdle.Result != true)
                return new IsCompeltedResult(false, "设备不空闲");

            if (railGuidedVehicleDevice.LastStatus == null)
                return new IsCompeltedResult(false, "设备当前状态未获取");

            if (railGuidedVehicleDevice.LastStatus.State != RailGuidedVehicleStatus.有货待命)
                return new IsCompeltedResult(false, "设备不处于有货待命状态");

            var taskCurrentLocationInfo = LocationConverter.ToLocationInfo(LocationConverter.UserCodeToLcation(railGuidedVehicleDevice.Name));
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                var task = unitOfWork.session.Get<Task>(_action.Movement.Task.Id);
                task.CurrentLocation = taskCurrentLocationInfo;
                unitOfWork.session.Flush();
                unitOfWork.Commit();
            }

            Wcs.Framework.EventBus.EventBus.Instance.Publish(new Wcs.Framework.Events.TaskCurrentLocationChangedEvent(_action.Movement.Task.Id, _action.Movement.Task.TaskCode, taskCurrentLocationInfo));
            return new IsCompeltedResult(true, "");
        }

        /// <summary>
        /// 发送取货指令
        /// </summary>
        public override void Perform()
        {
            if (!CanPerform().Result)
                //throw new InvalidOperationException("当前状态不可执行取货的动作");
                return;

            if (_scheduler.VehicleRecording.ContainsKey(railGuidedVehicleDevice.Name))
                _scheduler.VehicleRecording[railGuidedVehicleDevice.Name] = this;
            else
                _scheduler.VehicleRecording.Add(railGuidedVehicleDevice.Name, this);

            PickingCommand cmd = new PickingCommand(
                String.Format("20{0:000000}", _action.EquipmentTaskId),
                Convert.ToUInt16(_action.ContainerCode), 
                destinationLocation.StationNo,
                destinationLocation.PickingChainAction);

            railGuidedVehicleDevice.Write(cmd, cmd.SendSuccess);
            lastSendTaskId = cmd.TaskId;
        }

        void 行走到取货点()
        {
            if (_scheduler.VehicleRecording.ContainsKey(railGuidedVehicleDevice.Name))
                _scheduler.VehicleRecording[railGuidedVehicleDevice.Name] = this;
            else
                _scheduler.VehicleRecording.Add(railGuidedVehicleDevice.Name, this);

            NormalWalkCommand cmd = new NormalWalkCommand(
                String.Format("10{0:000000}", _action.EquipmentTaskId),
                Convert.ToUInt16(_action.ContainerCode),
                destinationLocation.StationNo);

            railGuidedVehicleDevice.Write(cmd, cmd.SendSuccess);
            lastSendTaskId = cmd.TaskId;
        }
    }
}