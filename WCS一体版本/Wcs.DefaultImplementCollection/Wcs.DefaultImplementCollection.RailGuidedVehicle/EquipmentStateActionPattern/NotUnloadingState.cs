using System;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    /// <summary>
    /// 未放货，等待放货
    /// </summary>
    public class NotUnloadingState : AbstractState
    {
        public NotUnloadingState(AbstractStateManager context)
            : base(context) { }

        public override string Name
        {
            get { return "等待放货"; }
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

            if (this.Context.EquipmentAction.Status != EquipmentActionStatus.New && this.Context.EquipmentAction.Status != EquipmentActionStatus.Executing)
                return new CanPerformResult(false, string.Format("当前任务处于 {0} 状态(允许发送放货命令的状态应为 新任务/执行中), 不允许发送任务", this.Name));

            RailGuidedVehicleDevice railGuidedVehicleDevice = (RailGuidedVehicleDevice)this.Context.Device;
            if (railGuidedVehicleDevice.IsIdle.Result != true)
                return new CanPerformResult(false, $"设备处于忙碌状态({railGuidedVehicleDevice.IsIdle.Information})");

            RailGuidedVehicleStation currentStation = railGuidedVehicleDevice.GetCurrentStation();
            if (currentStation == null)
                return new CanPerformResult(false, "设备当前站点为空");

            if (this.Context.EquipmentAction.Status != EquipmentActionStatus.Executing && this.Context.EquipmentAction.Status != EquipmentActionStatus.New)
                return new CanPerformResult(false, "当前物理动作不处于新任务或者执行中");


            Location endLocation = LocationConverter.ToLocation(((RailGuidedVehicleStepByStepAction)this.Context.EquipmentAction).UnloadLocation);

            RailGuidedVehicleStation unloadingLocation = (RailGuidedVehicleStation)endLocation;

            if (currentStation.StationNo!=unloadingLocation.StationNo)
            {
                行走到放货点();
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
            RailGuidedVehicleDevice railGuidedVehicleDevice = (RailGuidedVehicleDevice)this.Context.Device;
            if (railGuidedVehicleDevice.IsIdle.Result != true)
                return new IsCompeltedResult(false, $"设备不空闲({railGuidedVehicleDevice.IsIdle.Information})");

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
            if (!CanPerform().Result)
            {
                throw new InvalidOperationException("当前状态不可发送放货指令");
            }

            RailGuidedVehicleDevice railGuidedVehicleDevice = (RailGuidedVehicleDevice)this.Context.Device;

            Location endLocation = LocationConverter.ToLocation(((RailGuidedVehicleStepByStepAction)this.Context.EquipmentAction).UnloadLocation);

            RailGuidedVehicleStation unloadingLocation = (RailGuidedVehicleStation)endLocation;

            PuttingCommand cmd = new PuttingCommand(
                String.Format("40{0:000000}", Context.EquipmentAction.EquipmentTaskId),
                 Convert.ToUInt16(Context.EquipmentAction.ContainerCode), 
                 unloadingLocation.StationNo,
                 unloadingLocation.PuttingChainAction);

            railGuidedVehicleDevice.Write(cmd, cmd.SendSuccess);
        }

        void 行走到放货点()
        {
            RailGuidedVehicleDevice railGuidedVehicleDevice = (RailGuidedVehicleDevice)this.Context.Device;

            Location endLocation = LocationConverter.ToLocation(((RailGuidedVehicleStepByStepAction)this.Context.EquipmentAction).UnloadLocation);

            RailGuidedVehicleStation puttingLocation = (RailGuidedVehicleStation)endLocation;

            WalkWithGoodsCommand cmd = new WalkWithGoodsCommand(
                String.Format("30{0:000000}", Context.EquipmentAction.EquipmentTaskId),
                Convert.ToUInt16(Context.EquipmentAction.ContainerCode),
                puttingLocation.StationNo);

            railGuidedVehicleDevice.Write(cmd, cmd.SendSuccess);
        }
    }
}