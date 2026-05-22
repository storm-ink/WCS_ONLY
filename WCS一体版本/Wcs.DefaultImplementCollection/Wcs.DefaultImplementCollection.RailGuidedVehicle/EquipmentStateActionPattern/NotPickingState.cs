using System;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    /// <summary>
    /// 未取货，等待取货
    /// </summary>
    public class NotPickingState : AbstractState
    {
        public NotPickingState(AbstractStateManager context)
            : base(context) { }

        public override string Name
        {
            get { return "等待取货"; }
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

            if (this.Context.EquipmentAction.Status != EquipmentActionStatus.New && this.Context.EquipmentAction.Status != EquipmentActionStatus.Executing)
                return new CanPerformResult(false, "当前任务不处于 新任务/执行中 状态, 不允许发送任务");

            RailGuidedVehicleDevice railGuidedVehicleDevice = (RailGuidedVehicleDevice)this.Context.Device;
            if (railGuidedVehicleDevice.IsIdle.Result != true)
                return new CanPerformResult(false, $"设备不空闲({railGuidedVehicleDevice.IsIdle.Information})，不允许发送命令");
            
            RailGuidedVehicleStation currentStation = railGuidedVehicleDevice.GetCurrentStation();
            if (currentStation == null)
                return new CanPerformResult(false, "设备当前位置获取失败");
            
            Location startLocation = LocationConverter.ToLocation(((RailGuidedVehicleStepByStepAction)this.Context.EquipmentAction).LoadLocation);
            RailGuidedVehicleStation pickingLocation = (RailGuidedVehicleStation)startLocation;
            if (currentStation.StationNo != pickingLocation.StationNo)
            {
                行走到取货点();
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
            RailGuidedVehicleDevice railGuidedVehicleDevice = (RailGuidedVehicleDevice)this.Context.Device;

            if (railGuidedVehicleDevice.IsIdle.Result != true)
                return new IsCompeltedResult(false, $"设备不空闲({railGuidedVehicleDevice.IsIdle.Information})");

            if (railGuidedVehicleDevice.LastStatus == null)
                return new IsCompeltedResult(false, "设备当前状态未获取");

            if (railGuidedVehicleDevice.LastStatus.State != RailGuidedVehicleStatus.有货待命)
                return new IsCompeltedResult(false, "设备不处于有货待命状态");

            var taskCurrentLocationInfo = LocationConverter.ToLocationInfo(LocationConverter.UserCodeToLcation(railGuidedVehicleDevice.Name));
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                var task = unitOfWork.session.Get<Task>(this.Context.EquipmentAction.Movement.Task.Id);
                task.CurrentLocation = taskCurrentLocationInfo;
                unitOfWork.session.Flush();
                unitOfWork.Commit();
            }

            Wcs.Framework.EventBus.EventBus.Instance.Publish(new Wcs.Framework.Events.TaskCurrentLocationChangedEvent(this.Context.EquipmentAction.Movement.Task.Id, this.Context.EquipmentAction.Movement.Task.TaskCode, taskCurrentLocationInfo));
            return new IsCompeltedResult(true, "");
        }

        /// <summary>
        /// 发送取货指令
        /// </summary>
        public override void Perform()
        {
            if (!CanPerform().Result)
            {
                throw new InvalidOperationException("当前状态不可执行取货的动作");
            }

            RailGuidedVehicleDevice railGuidedVehicleDevice = (RailGuidedVehicleDevice)this.Context.Device;

            Location startLocation = LocationConverter.ToLocation(((RailGuidedVehicleStepByStepAction)this.Context.EquipmentAction).LoadLocation);

            RailGuidedVehicleStation pickingLocation = (RailGuidedVehicleStation)startLocation;

            PickingCommand cmd = new PickingCommand(
                String.Format("20{0:000000}", Context.EquipmentAction.EquipmentTaskId),
                Convert.ToUInt16(Context.EquipmentAction.ContainerCode), 
                pickingLocation.StationNo,
                pickingLocation.PickingChainAction);

            railGuidedVehicleDevice.Write(cmd, cmd.SendSuccess);
        }

        void 行走到取货点()
        {
            RailGuidedVehicleDevice railGuidedVehicleDevice = (RailGuidedVehicleDevice)this.Context.Device;

            Location startLocation = LocationConverter.ToLocation(((RailGuidedVehicleStepByStepAction)this.Context.EquipmentAction).LoadLocation);

            RailGuidedVehicleStation pickingLocation = (RailGuidedVehicleStation)startLocation;

            NormalWalkCommand cmd = new NormalWalkCommand(
                String.Format("10{0:000000}", Context.EquipmentAction.EquipmentTaskId),
                Convert.ToUInt16(Context.EquipmentAction.ContainerCode),
                pickingLocation.StationNo);

            railGuidedVehicleDevice.Write(cmd, cmd.SendSuccess);
        }
    }
}