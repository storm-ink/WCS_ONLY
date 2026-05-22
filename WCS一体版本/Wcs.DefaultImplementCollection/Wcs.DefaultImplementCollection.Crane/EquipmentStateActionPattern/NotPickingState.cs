using NLog;
using System;
using System.Linq;


using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Crane
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
        /// <para>3、设备的当前位置（层、列）和逻辑动作的起始位置（层、列）一致</para>
        /// <para>4、上下文中的物理动作状态为 Executing</para>
        /// </remarks>
        /// <returns></returns>
        public override CanPerformResult CanPerform()
        {
            if (IsCompleted().Result)
                return new CanPerformResult(false, "当前状态已完成, 不允许发送命令");

            if (this.Context.EquipmentAction.Status != EquipmentActionStatus.New && this.Context.EquipmentAction.Status != EquipmentActionStatus.Executing)
                return new CanPerformResult(false, "当前任务不处于 新任务/执行 状态, 不允许发送任务");

            CraneDevice craneDevice = (CraneDevice)this.Context.Device;
            var isidle = craneDevice.IsIdle;
            if (!isidle.Result)
                return new CanPerformResult(false, $"当前设备不空闲(tips:{isidle.Information}), 不允许发送命令");

            RackLocation currentLocation = craneDevice.GetCurrentLocation();
            if (currentLocation == null)
                return new CanPerformResult(false, "设备当前位置获取失败，不允许发送命令");

            if (craneDevice.LastStatus.DeviceState != CraneStatus.Watting || craneDevice.LastStatus.IsLoaded != 2)
                return new CanPerformResult(false, "设备当前状态不是 无货 待命 状态，不允许发送命令");

            return new CanPerformResult(true, "");
        }

        /// <summary>
        /// 指示堆垛机是否已到取货
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
            if (this.Context.EquipmentAction == null)
                return new IsCompeltedResult(true, "任务查询为空");

            if (this.Context.EquipmentAction.Status == EquipmentActionStatus.Cancelled || this.Context.EquipmentAction.Status == EquipmentActionStatus.Completed)
                return new IsCompeltedResult(true,"任务状态为取消或者完成");

            if (this.Context.EquipmentAction.Status != EquipmentActionStatus.New && this.Context.EquipmentAction.Status != EquipmentActionStatus.Executing)
                return new IsCompeltedResult(false, "任务不为新任务或者执行中状态");

            CraneDevice craneDevice = (CraneDevice)this.Context.Device;
            var isidle = craneDevice.IsIdle;
            if (!isidle.Result)
                return new IsCompeltedResult(false, $"设备不空闲(tips:{isidle.Information})");

            if (craneDevice.LastStatus == null)
                return new IsCompeltedResult(false, "设备状态为空");

            //if (String.IsNullOrWhiteSpace(this.StepSerialNo))
            //    return false;

            if (craneDevice.LastStatus.IsLoaded == 1)
            {
                var taskCurrentLocationInfo =  LocationConverter.ToLocationInfo(LocationConverter.UserCodeToLcation(craneDevice.Name));
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    var task = unitOfWork.session.Get<Task>(this.Context.EquipmentAction.Movement.Task.Id);
                    if (task.CurrentLocation.UnifiedCode != taskCurrentLocationInfo.UnifiedCode)
                    {
                        task.CurrentLocation = taskCurrentLocationInfo;
                        unitOfWork.session.Flush();
                    }
                    unitOfWork.Commit();
                }

                Wcs.Framework.EventBus.EventBus.Instance.Publish(new Wcs.Framework.Events.TaskCurrentLocationChangedEvent(this.Context.EquipmentAction.Movement.Task.Id, this.Context.EquipmentAction.Movement.Task.TaskCode, taskCurrentLocationInfo));
                return new IsCompeltedResult(true, "");
            }
            else
                return new IsCompeltedResult(false, "设备不处于有货待命状态");
        }

        /// <summary>
        /// 发送取货指令
        /// </summary>
        public override void Perform()
        {
            if (!CanPerform().Result)
                throw new InvalidOperationException("当前状态不可执取货动作");

            CraneDevice craneDevice = (CraneDevice)this.Context.Device;

            Location startLocation = LocationConverter.ToLocation(this.Context.EquipmentAction.Movement.StartLocation);
            Location endLocation = LocationConverter.ToLocation(this.Context.EquipmentAction.Movement.EndLocation);
            var barcode = this.Context.EquipmentAction.Movement.Task.ContainerCodes == null || this.Context.EquipmentAction.Movement.Task.ContainerCodes.Count() == 0 ? "" : this.Context.EquipmentAction.Movement.Task.ContainerCodes.FirstOrDefault();

            var equipmentTaskId = craneDevice.GetEquipmentTaskId(CraneDevice.CraneEquipmentTaskType.WcsTaskStep_Pick);
            var cmd = new CraneCommand(CmdTypes.NewTask, CraneTaskTypes.StepPick, (RackLocation)startLocation, (RackLocation)endLocation, equipmentTaskId, (UInt32)this.Context.EquipmentAction.EquipmentTaskId, barcode);

            craneDevice.Write(cmd, cmd.SendSuccess);
        }
    }
}