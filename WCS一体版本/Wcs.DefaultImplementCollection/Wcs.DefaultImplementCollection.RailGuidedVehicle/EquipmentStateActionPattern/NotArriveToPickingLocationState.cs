using System;
using Wcs.Framework;
using System.Linq;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    /// <summary>
    /// 未到达取货点，需要移动到取货点
    /// </summary>
    public class NotArriveToPickingLocationState :AbstractState
    {
        public NotArriveToPickingLocationState(AbstractStateManager context)
            : base(context) { }

        public override string Name
        {
            get { return "需要移动到取货点"; }
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

            if (this.Context.EquipmentAction.Status != EquipmentActionStatus.New && this.Context.EquipmentAction.Status != EquipmentActionStatus.Executing)
                return new CanPerformResult(false, string.Format("当前任务处于 {0} 状态(允许发送放货命令的状态应为 新任务/执行中), 不允许发送任务", this.Name));

            RailGuidedVehicleDevice railGuidedVehicleDevice = (RailGuidedVehicleDevice)this.Context.Device;
            if (railGuidedVehicleDevice.IsIdle.Result != true)
                return new CanPerformResult(false, string.Format("当前设备 {0} 不空闲({1}), 不允许发送命令", railGuidedVehicleDevice.Name, railGuidedVehicleDevice.IsIdle.Information));

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
            RailGuidedVehicleDevice railGuidedVehicleDevice = (RailGuidedVehicleDevice)this.Context.Device;
            if (railGuidedVehicleDevice.IsIdle.Result != true)
                return new IsCompeltedResult(false, $"设备不空闲({railGuidedVehicleDevice.IsIdle.Information})");

            RailGuidedVehicleStation currentStation = railGuidedVehicleDevice.GetCurrentStation();
            if (currentStation == null)
                return new IsCompeltedResult(false, "设备当前位置获取失败");

            Location startLocation = LocationConverter.ToLocation(((RailGuidedVehicleStepByStepAction)this.Context.EquipmentAction).LoadLocation);
            RailGuidedVehicleStation pickingLocation;
            if (startLocation is RailGuidedVehicleStation)
                pickingLocation = (RailGuidedVehicleStation)startLocation;
            else
                pickingLocation = (RailGuidedVehicleStation)startLocation.Synonymous.FirstOrDefault(x => x is RailGuidedVehicleStation);

            if (currentStation.StationNo != pickingLocation.StationNo)
                return new IsCompeltedResult(false, "设备当前位置不是取货位置");

            return new IsCompeltedResult(true, "");
        }

        /// <summary>
        /// 发送向取货点移动的指令
        /// </summary>
        public override void Perform()
        {
            if (!CanPerform().Result)
            {
                throw new InvalidOperationException("当前状态不可执行移动到取货点的动作");
            }

            RailGuidedVehicleDevice railGuidedVehicleDevice = (RailGuidedVehicleDevice)this.Context.Device;

            Location startLocation = LocationConverter.ToLocation(((RailGuidedVehicleStepByStepAction)this.Context.EquipmentAction).LoadLocation);

            RailGuidedVehicleStation pickingLocation = (RailGuidedVehicleStation)startLocation;

            NormalWalkCommand cmd = new NormalWalkCommand(
                String.Format("10{0:000000}",Context.EquipmentAction.EquipmentTaskId),
                Convert.ToUInt16(Context.EquipmentAction.ContainerCode), 
                pickingLocation.StationNo);

            railGuidedVehicleDevice.Write(cmd,cmd.SendSuccess);
        }
    }
}