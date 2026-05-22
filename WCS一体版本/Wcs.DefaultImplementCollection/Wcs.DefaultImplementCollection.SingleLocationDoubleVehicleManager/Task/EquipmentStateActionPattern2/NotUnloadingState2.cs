using System;
using Wcs.DefaultImplementCollection.RailGuidedVehicle;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.SingleLocationDoubleVehicleManager
{
    /// <summary>
    /// 未放货，等待放货
    /// </summary>
    public class NotUnloadingState2 : AbstractState2
    {
        public NotUnloadingState2(SingleLocationDoubleVehicleSubSystemEquipmentActionScheduler2 scheduler, StateManagerRestoreInfo context, EquipmentAction action)
        {
            _scheduler = scheduler;
            _context = context;
            _action = action;
            railGuidedVehicleDevice = DeviceConverter.ToDevice<RailGuidedVehicleDevice>(_context.ActuatingDeviceName);
            destinationLocation = (SingleLocationDoubleVehicleSubSystemLocation)LocationConverter.ToLocation(_action.Movement.EndLocation);
        }

        public override string Name
        {
            get { return "放货"; }
        }

        /// <summary>
        /// 指示当前是否可以发送放货动指令
        /// </summary>
        /// <remarks>
        /// <para>发送条件：</para>
        /// <para>1、设备处于空闲状态</para>
        /// <para>2、当前状态未完成</para>
        /// <para>3、设备的当前位置（站点号）和逻辑动作的终点位置（站点号）一致</para>
        /// <para>4、上下文中的物理动作状态为 Executing</para>
        /// </remarks>
        /// <returns></returns>
        public override CanPerformResult CanPerform()
        {
            if (IsCompleted().Result)
                return new CanPerformResult(false, string.Format("当前状态 {0} 已完成, 不允许发送命令", this.Name));

            if (_action.Status != EquipmentActionStatus.New && _action.Status != EquipmentActionStatus.Executing)
                return new CanPerformResult(false, string.Format("当前任务处于 {0} 状态(允许发送放货命令的状态应为 新任务/执行中), 不允许发送任务", this.Name));

            if (railGuidedVehicleDevice.IsIdle.Result != true)
                return new CanPerformResult(false, "设备处于忙碌状态");

            RailGuidedVehicleStation currentStation = railGuidedVehicleDevice.GetCurrentStation();
            if (currentStation == null)
                return new CanPerformResult(false, "设备当前站点为空");

            if (_action.Status != EquipmentActionStatus.Executing && _action.Status != EquipmentActionStatus.New)
                return new CanPerformResult(false, "当前物理动作不处于新任务或者执行中");

            if (_scheduler.VehicleRecording.ContainsKey(railGuidedVehicleDevice.Name))
            {
                var state = _scheduler.VehicleRecording[railGuidedVehicleDevice.Name];
                if (state.lastSendTaskId == railGuidedVehicleDevice.LastStatus.TaskId)
                    return new CanPerformResult(false, $"当前设备不空闲，{_scheduler.VehicleRecording[railGuidedVehicleDevice.Name]} 驱动中");
            }

            if (currentStation.StationNo != destinationLocation.StationNo)
            {
                //行走到放货点(); 这里后续可能要特殊处理
                return new CanPerformResult(false, "设备当前位置和放货位置不一致，已尝试发送行走到放货点的命令");
            }
            return new CanPerformResult(true, "");
        }

        /// <summary>
        /// 指示穿梭车是否已放货
        /// </summary>
        /// <remarks>
        /// <para>完成条件：</para>
        /// <para>1、设备处于空闲状态</para>
        /// <para>2、设备已连接并且状态数据已同步/para>
        /// <para>3、设备的处于无货待命状态</para>
        /// </remarks>
        /// <returns>完成返回 true；未完成返回假 false</returns>
        public override IsCompeltedResult IsCompleted()
        {
            if (railGuidedVehicleDevice.IsIdle.Result != true)
                return new IsCompeltedResult(false, "设备不空闲");

            if (railGuidedVehicleDevice.LastStatus == null)
                return new IsCompeltedResult(false, "设备状态为空");

            if (railGuidedVehicleDevice.LastStatus.State != RailGuidedVehicleStatus.无货待命)
                return new IsCompeltedResult(false, "设备不处于无货待命状态");

            return new IsCompeltedResult(true, "");
        }

        /// <summary>
        /// 发送放货指令
        /// </summary>
        public override void Perform()
        {
            var result = CanPerform();
            if (!result.Result)
            {
                this._scheduler.Log($"被 {result.Information} 原因否决，当前状态不可执行移动命令");
                //throw new InvalidOperationException("当前状态不可执行移动命令");
                return;
            }

            if (_scheduler.VehicleRecording.ContainsKey(railGuidedVehicleDevice.Name))
                _scheduler.VehicleRecording[railGuidedVehicleDevice.Name] = this;
            else
                _scheduler.VehicleRecording.Add(railGuidedVehicleDevice.Name, this);

            PuttingCommand cmd = new PuttingCommand(
                String.Format("40{0:000000}", _action.EquipmentTaskId),
                 Convert.ToUInt16(_action.ContainerCode),
                 destinationLocation.StationNo,
                 destinationLocation.PuttingChainAction);

            this._scheduler.Log($"车辆命令发送记录:即将给 {railGuidedVehicleDevice.Name} 发送命令 {cmd.ToTelex()}(命令类型{cmd.GetType()})");
            railGuidedVehicleDevice.Write(cmd, cmd.SendSuccess);
            this._scheduler.Log($"车辆命令发送记录:给 {railGuidedVehicleDevice.Name} 成功发送命令 {cmd.ToTelex()}(命令类型{cmd.GetType()})");
            lastSendTaskId = cmd.TaskId;
        }
    }
}