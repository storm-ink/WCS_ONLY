using System;
using Wcs.Framework;
using System.Linq;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    /// <summary>
    /// 未到达放货点，需要移动到放货点
    /// </summary>
    public class NotArriveToUnloadingLocationState : AbstractState
    {
        public NotArriveToUnloadingLocationState(AbstractStateManager context)
            : base(context) { }

        public override string Name
        {
            get { return "需要移动到放货点"; }
        }

        /// <summary>
        /// 指示当前是否可以发送向放货点移动的指令
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

            if (this.Context.EquipmentAction.Status != EquipmentActionStatus.New && this.Context.EquipmentAction.Status != EquipmentActionStatus.Executing)
                return new CanPerformResult(false, string.Format("当前任务处于 {0} 状态(允许发送放货命令的状态应为 新任务/执行中), 不允许发送任务", this.Name));

            RailGuidedVehicleDevice railGuidedVehicleDevice = (RailGuidedVehicleDevice)this.Context.Device;
            if (railGuidedVehicleDevice.IsIdle.Result != true)
                return new CanPerformResult(false, string.Format("当前设备 {0} 不空闲{1}, 不允许发送命令", railGuidedVehicleDevice.Name, railGuidedVehicleDevice.IsIdle.Information));

            return new CanPerformResult(true, "");
        }

        /// <summary>
        /// 指示堆垛机是否已到达放货点
        /// </summary>
        /// <remarks>
        /// <para>完成条件：</para>
        /// <para>1、设备处于空闲状态</para>
        /// <para>2、设备已连接并且获取到了当前所处站点数据</para>
        /// <para>3、设备的当前位置（站点号）和逻辑动作的结束位置（站点号）一致</para>
        /// </remarks>
        /// <returns>完成返回 true；未完成返回假 false</returns>
        /// <exception cref="System.NotSupportedException">物理动作所属的逻辑动作结束位置不是货架位置是引发</exception>
        public override IsCompeltedResult IsCompleted()
        {
            RailGuidedVehicleDevice railGuidedVehicleDevice = (RailGuidedVehicleDevice)this.Context.Device;
            if (railGuidedVehicleDevice.IsIdle.Result != true)
                return new IsCompeltedResult(false, $"设备不空闲({railGuidedVehicleDevice.IsIdle.Information})");

            RailGuidedVehicleStation currentStation = railGuidedVehicleDevice.GetCurrentStation();
            if (currentStation == null)
                return new IsCompeltedResult(false, "设备当前位置获取失败");

            Location endLocation = LocationConverter.ToLocation(((RailGuidedVehicleStepByStepAction)this.Context.EquipmentAction).UnloadLocation);
            RailGuidedVehicleStation state;
            if (!(endLocation is RailGuidedVehicleStation))
                state = (RailGuidedVehicleStation)endLocation.Synonymous.FirstOrDefault(x => x is RailGuidedVehicleStation);
            else
                state = (RailGuidedVehicleStation)endLocation;

            if (currentStation.StationNo != state.StationNo)
                return new IsCompeltedResult(false, "设备当前位置不在放货站点");

            return new IsCompeltedResult(true, "");
        }

        /// <summary>
        /// 发送向放货点移动指令
        /// </summary>
        public override void Perform()
        {
            if (!CanPerform().Result)
            {
                throw new InvalidOperationException("当前状态不可执行移动到取货点的动作");
            }

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