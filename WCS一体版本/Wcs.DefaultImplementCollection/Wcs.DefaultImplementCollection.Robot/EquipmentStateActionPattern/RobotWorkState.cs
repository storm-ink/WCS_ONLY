using System;
using Wcs.Framework;
using System.Linq;
using System.Collections.Generic;
using NHibernate.Linq;

namespace Wcs.DefaultImplementCollection.Robot
{
    /// <summary>
    /// robot工作步骤 - 执行拆码垛任务
    /// </summary>
    public class RobotWorkState : AbstractState
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="context"></param>
        public RobotWorkState(AbstractStateManager context)
            : base(context) { }

        /// <summary>
        /// 步骤名称
        /// </summary>
        public override string Name
        {
            get { return "执行拆码垛任务"; }
        }

        /// <summary>
        /// 指示当前是否可以发送指令
        /// </summary>
        /// <remarks>
        /// <para>发送条件：</para>
        /// <para>1、设备处于空闲状态</para>
        /// <para>2、当前状态未完成</para>
        /// <para>3、设备的当前位置（层、列）和逻辑动作的终点位置（层、列）一致</para>
        /// <para>4、上下文中的物理动作状态为 Executing</para>
        /// </remarks>
        /// <returns></returns>
        public override CanPerformResult CanPerform()
        {
            if (IsCompleted().Result)
                return new CanPerformResult(false, string.Format("当前状态 {0} 已完成, 不允许发送命令", this.Name));

            if (this.Context.EquipmentAction.Status != EquipmentActionStatus.New && this.Context.EquipmentAction.Status != EquipmentActionStatus.Executing)
                return new CanPerformResult(false, string.Format("当前任务处于 {0} 状态(允许发送放货命令的状态应为 新任务/执行中), 不允许发送任务", this.Name));

            RobotDevice device = (RobotDevice)this.Context.Device;
            if (device.IsIdle.Result != true)
                return new CanPerformResult(false, string.Format("当前设备 {0} 不空闲, 不允许发送命令", device.Name));

            if (device.LastState.RobotTask.TaskId != 0)
                return new CanPerformResult(false, string.Format("设备 {0} 任务号不为 0, 不允许发送命令", device.Name));

            if (device.LastState.RobotTask.HandShake != HandShake.Unknown)
                return new CanPerformResult(false, string.Format("设备 {0} 任务状态（HandShake）不为 0, 不允许发送命令", device.Name));

            return new CanPerformResult(true, "可以发送命令");
        }

        /// <summary>
        /// 指示Robot是否已完成任务
        /// </summary>
        /// <remarks>
        /// <para>完成条件：</para>
        /// <para>1、设备处于任务处于完成状态</para>
        /// </remarks>
        /// <returns>完成返回 true；未完成返回假 false</returns>
        public override IsCompeltedResult IsCompleted()
        {
            string msg;
            if (this.Context._equipmentAction == null)
            {
                if (this.Context.EquipmentAction == null)
                {
                    msg = String.Format("Robot单步任务完成：由于未查询到对应的任务 {0} 完成", this.Name);
                    _logger.Info(msg);
                    return new IsCompeltedResult(true, msg);
                }

                msg = String.Format("Robot单步任务完成：由于上下文中任务信息为空 {0} 完成", this.Name);
                _logger.Info(msg);
                return new IsCompeltedResult(true, msg);
            }

            RobotDevice device = (RobotDevice)this.Context.Device;
            if (device.LastState == null)
                return new IsCompeltedResult(false, "当前设备未获取状态");

          

            if (this.Context.EquipmentAction.Status == EquipmentActionStatus.Cancelled || this.Context.EquipmentAction.Status == EquipmentActionStatus.Completed)
            {
                msg = String.Format("Robot单步任务完成：由于任务已处于完成或者取消状态 {0}", this.Context.EquipmentAction.EquipmentTaskId);
                _logger.Info1(msg, this);
                return new IsCompeltedResult(true, msg);
            }

            if (this.Context.EquipmentAction.Status != EquipmentActionStatus.New && this.Context.EquipmentAction.Status != EquipmentActionStatus.Executing)
                return new IsCompeltedResult(false, "当前物理动作不处于 新任务/执行中 状态");

            if (!device.IsIdle.Result)
                return new IsCompeltedResult(false, "当前设备处于 忙碌 状态");

            return new IsCompeltedResult(false, "当前任务不处于 完成 状态");
        }

        /// <summary>
        /// 发送指令
        /// </summary>
        public override void Perform()
        {

            if (!CanPerform().Result)
            {
                throw new InvalidOperationException("当前状态不可发送放货指令");
            }
            RobotTransferByStepAction RobotDeviceEquipmentAction = (RobotTransferByStepAction)this.Context.EquipmentAction;
            RobotTaskCommand cmd = new RobotTaskCommand();
            cmd.TaskId = (uint)this.Context.EquipmentAction.EquipmentTaskId;
            cmd.HandShake = HandShake.New;
            cmd.Pick = Convert.ToUInt16(RobotDeviceEquipmentAction.StartLocation.DeviceCode);
            //cmd.Put = Convert.ToUInt16(this.Context.EquipmentAction.Movement.EndLocation.DeviceCode);
            cmd.Put = Convert.ToUInt16(RobotDeviceEquipmentAction.EndLocation.DeviceCode);


            RobotDevice device = (RobotDevice)this.Context.Device;
            if (device.EquipmentActionToAddCommandPlugin != null)
                device.EquipmentActionToAddCommandPlugin.EquipmentActionToAddCommandPlugin(this.Context.EquipmentAction, ref cmd);

            device.Write<RobotTaskCommand>(cmd, cmd.SendSuccess);
        }
    }
}