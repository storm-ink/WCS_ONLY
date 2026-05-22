using System;

using Wcs.Framework;
using System.Linq;

namespace Wcs.DefaultImplementCollection.Crane
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
                return new CanPerformResult(false, "当前状态已完成, 不允许发送命令");

            if (this.Context.EquipmentAction.Status != EquipmentActionStatus.New && this.Context.EquipmentAction.Status != EquipmentActionStatus.Executing)
                return new CanPerformResult(false, "当前任务不处于 新任务/执行中 状态, 不允许发送任务");

            CraneDevice craneDevice = (CraneDevice)this.Context.Device;
            var isidle = craneDevice.IsIdle;
            if (!isidle.Result)
                return new CanPerformResult(false, $"当前设备不空闲(tips:{isidle.Information}), 不允许发送命令");

            return new CanPerformResult(true, "");
        }

        /// <summary>
        /// 指示堆垛机是否已到达放货点
        /// </summary>
        /// <remarks>
        /// <para>完成条件：</para>
        /// <para>1、设备处于空闲状态</para>
        /// <para>2、设备已连接并且获取到了当前所处位置数据</para>
        /// <para>3、设备的当前位置（层、列）和逻辑动作的结束位置（层、列）一致</para>
        /// </remarks>
        /// <returns>完成返回 true；未完成返回假 false</returns>
        /// <exception cref="System.NotSupportedException">物理动作所属的逻辑动作结束位置不是货架位置是引发</exception>
        public override IsCompeltedResult IsCompleted()
        {
            if (this.Context.EquipmentAction == null)
                return new IsCompeltedResult(true, "任务为空");

            if (this.Context.EquipmentAction.Status == EquipmentActionStatus.Cancelled || this.Context.EquipmentAction.Status == EquipmentActionStatus.Completed)
                return new IsCompeltedResult(true, "任务为取消或者完成状态");
            if (this.Context.EquipmentAction.Status != EquipmentActionStatus.New && this.Context.EquipmentAction.Status != EquipmentActionStatus.Executing)
                return new IsCompeltedResult(false, "当前任务不处于 新任务/执行中 状态");
            CraneDevice craneDevice = (CraneDevice)this.Context.Device;

            var isidle = craneDevice.IsIdle;
            if (!isidle.Result)
                return new IsCompeltedResult(false, $"设备不空闲(tips:{isidle.Information})");

            RackLocation currentLocation = craneDevice.GetCurrentLocation();
            if (currentLocation == null)
                return new IsCompeltedResult(false, "未获取设备当前位置");

            Location endLocation = LocationConverter.ToLocation(this.Context.EquipmentAction.Movement.EndLocation);
            RackLocation unloadingLocation;
            if (endLocation is RackLocation)
                unloadingLocation = (RackLocation)endLocation;
            else
                unloadingLocation = (RackLocation)endLocation.Synonymous.FirstOrDefault(x => x is RackLocation);
            if (currentLocation.Column != unloadingLocation.Column || currentLocation.Level != unloadingLocation.Level)
                return new IsCompeltedResult(false, "设备当前不在放货点");

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

            CraneDevice craneDevice = (CraneDevice)this.Context.Device;
            Location endLocation = LocationConverter.ToLocation(this.Context.EquipmentAction.Movement.EndLocation);

            var barcode = this.Context.EquipmentAction.Movement.Task.ContainerCodes == null || this.Context.EquipmentAction.Movement.Task.ContainerCodes.Count() == 0 ? "" : this.Context.EquipmentAction.Movement.Task.ContainerCodes.FirstOrDefault();

            var equipmentTaskId = craneDevice.GetEquipmentTaskId(CraneDevice.CraneEquipmentTaskType.WcsTaskStep_Move);
            var cmd = new CraneCommand(CmdTypes.NewTask, CraneTaskTypes.StepWalk, null, (RackLocation)endLocation, equipmentTaskId, (UInt32)this.Context.EquipmentAction.EquipmentTaskId, barcode);

            craneDevice.Write(cmd, cmd.SendSuccess);
        }
    }
}