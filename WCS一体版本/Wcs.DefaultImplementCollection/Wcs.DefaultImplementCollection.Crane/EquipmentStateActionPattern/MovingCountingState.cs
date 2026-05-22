using System;
using Wcs.Framework;
using System.Linq;

namespace Wcs.DefaultImplementCollection.Crane
{
    /// <summary>
    /// 移动盘点
    /// </summary>
    public class MovingCountingState : AbstractState
    {
        public MovingCountingState(AbstractStateManager context)
            : base(context) { }

        public override string Name
        {
            get { return "需要移动到检测点（放货点）"; }
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
        /// 指示堆垛机是否已到达取货点
        /// </summary>
        /// <remarks>
        /// <para>完成条件：</para>
        /// <para>1、设备处于空闲状态</para>
        /// <para>2、设备已连接并且获取到了当前所处位置数据</para>
        /// <para>3、设备的当前位置（层、列）和逻辑动作的起始位置（层、列）一致</para>
        /// </remarks>
        /// <returns>完成返回 true；未完成返回假 false</returns>
        /// <exception cref="System.NotSupportedException">物理动作所属的逻辑动作起始位置不是货架位置是引发</exception>
        public override IsCompeltedResult IsCompleted()
        {
            if (this.Context.EquipmentAction == null)
                return new IsCompeltedResult(true, "未查询到对应的任务");

            if (this.Context.EquipmentAction.Status == EquipmentActionStatus.Cancelled || this.Context.EquipmentAction.Status == EquipmentActionStatus.Completed)
            {
                return new IsCompeltedResult(true, "任务已处于完成或者取消状态");
            }

            if (this.Context.EquipmentAction.Status != EquipmentActionStatus.New && this.Context.EquipmentAction.Status != EquipmentActionStatus.Executing)
                return new IsCompeltedResult(false, "任务不处于新任务或执行中状态");
            CraneDevice craneDevice = (CraneDevice)this.Context.Device;
            var isidle = craneDevice.IsIdle;
            if (!isidle.Result)
                return new IsCompeltedResult(false, $"设备不空闲(tips:{isidle.Information})");

            RackLocation currentLocation = craneDevice.GetCurrentLocation();
            if (currentLocation == null)
                return new IsCompeltedResult(false, "未获取设备当前位置");

            ///★★★★★★★★★★★先默认这个左右两层都是一样的★★★★★★★★★★★
            Location endLocation = LocationConverter.ToLocation(this.Context.EquipmentAction.Movement.EndLocation);
            RackLocation dropingLocation;
            if (endLocation is RackLocation)
                dropingLocation = (RackLocation)endLocation;
            else
                dropingLocation = (RackLocation)endLocation.Synonymous.FirstOrDefault(x => x is RackLocation);
            if (currentLocation.Column == currentLocation.Column
                && currentLocation.Level == currentLocation.Level
                && craneDevice.LastStatus.DeviceState == CraneStatus.Watting)
                return new IsCompeltedResult(true, "");

            return new IsCompeltedResult(false, String.Format("设备不在终点位置并且任务号为", this.Context.EquipmentAction.EquipmentTaskId.ToString("000000")));
        }

        /// <summary>
        /// 发送向取货点移动的指令
        /// </summary>
        public override void Perform()
        {
            //if (!CanPerform().Result)
            //{
            //    throw new InvalidOperationException("当前状态不可执行移动到取货点的动作");
            //}

            //CraneDevice craneDevice = (CraneDevice)this.Context.Device;

            //RackLocation currentLocation = craneDevice.GetCurrentLocation();
            //Location startLocation = LocationConverter.ToLocation(this.Context.EquipmentAction.Movement.StartLocation);
            //RackLocation pickingLocation = (RackLocation)startLocation;
            //Location endLocation = LocationConverter.ToLocation(this.Context.EquipmentAction.Movement.EndLocation);
            //RackLocation dropingLocation = (RackLocation)endLocation;

            //var barcode = this.Context.EquipmentAction.Movement.Task.ContainerCodes == null || this.Context.EquipmentAction.Movement.Task.ContainerCodes.Count() == 0 ? "" : this.Context.EquipmentAction.Movement.Task.ContainerCodes.FirstOrDefault();
            //if (currentLocation != null)
            //{
            //    ///不在起点位置且不在终点位置的时候需要先移动到起点或者终点
            //    if (
            //        (currentLocation.Column != pickingLocation.Column || currentLocation.Level != pickingLocation.Level)
            //        && (currentLocation.Column != dropingLocation.Column || currentLocation.Level != dropingLocation.Level)
            //        )
            //    {
            //        var toLocation = getToLocation(currentLocation, pickingLocation, dropingLocation);
            //        // this.StepSerialNo = "PZ" + this.Context.EquipmentAction.EquipmentTaskId.ToString("000000");
            //        this.StepSerialNo = this.Context.EquipmentAction.EquipmentTaskId.ToString("00000000");
            //        if (craneDevice.LastStatus.TaskId == this.StepSerialNo)
            //            this.StepSerialNo = "01" + this.Context.EquipmentAction.EquipmentTaskId.ToString("000000");

            //        AddTaskCommand cmd = new AddTaskCommand(
            //            this.StepSerialNo,
            //            AddTaskCommandType.半自动行走,
            //            currentLocation,
            //            toLocation,
            //            barcode,
            //            true);

            //        craneDevice.Write(cmd, cmd.SendSuccess);
            //    }
            //    else
            //    {
            //        if (currentLocation.Column == pickingLocation.Column && currentLocation.Level == pickingLocation.Level)
            //        {
            //            //this.StepSerialNo = "PD" + this.Context.EquipmentAction.EquipmentTaskId.ToString("000000");
            //            this.StepSerialNo = this.Context.EquipmentAction.EquipmentTaskId.ToString("00000000");
            //            if (craneDevice.LastStatus.TaskId == this.StepSerialNo)
            //                this.StepSerialNo = "01" + this.Context.EquipmentAction.EquipmentTaskId.ToString("000000");

            //            AddTaskCommand cmd = new AddTaskCommand(
            //                this.StepSerialNo,
            //                AddTaskCommandType.半自动行走,
            //                pickingLocation,
            //                dropingLocation,
            //                barcode,
            //                true);

            //            craneDevice.Write(cmd, cmd.SendSuccess);
            //        }
            //        else
            //        {
            //            //this.StepSerialNo = "PD" + this.Context.EquipmentAction.EquipmentTaskId.ToString("000000");
            //            this.StepSerialNo = this.Context.EquipmentAction.EquipmentTaskId.ToString("00000000");
            //            //if (craneDevice.LastStatus.TaskId == this.StepSerialNo)
            //            //    this.StepSerialNo = "01" + this.Context.EquipmentAction.EquipmentTaskId.ToString("000000");

            //            AddTaskCommand cmd = new AddTaskCommand(
            //                this.StepSerialNo,
            //                AddTaskCommandType.半自动行走,
            //                dropingLocation,
            //                pickingLocation,
            //                barcode,
            //                true);

            //            craneDevice.Write(cmd, cmd.SendSuccess);
            //        }
            //    }
            //}
        }

        private RackLocation getToLocation(RackLocation currentLocation, RackLocation pickingLocation, RackLocation dropingLocation)
        {
            var curToPick = Math.Abs(currentLocation.Column - pickingLocation.Column) + Math.Abs(currentLocation.Level - pickingLocation.Level);
            var curToDrop = Math.Abs(currentLocation.Column - dropingLocation.Column) + Math.Abs(currentLocation.Level - dropingLocation.Level);
            if (curToPick <= curToDrop)
                return pickingLocation;
            else
                return dropingLocation;
        }
    }
}