using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;
using NHibernate.Linq;
using NLog;

namespace Wcs.DefaultImplementCollection.Robot
{
    /// <summary>
    /// 表示一个Robot调度的物理动作
    /// </summary>
    public class RobotTransferAction : EquipmentAction
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 状态上下文
        /// </summary>
        public virtual RobotByStepStateManager StateManager { get; set; }

        protected RobotTransferAction()
            : base()
        {
        }
        public RobotTransferAction(RobotDevice device, EquipmentActionGroup group, Int32 equipmentTaskId, Int32 routeId, RobotLocation startLocation, RobotLocation endLocation)
            : base(device, group, equipmentTaskId, 0)
        {
            this.StartLocation = LocationConverter.ToLocationInfo(startLocation);
            this.EndLocation = LocationConverter.ToLocationInfo(endLocation);
            this.RouteId = routeId;
        }
        /// <summary>
        /// 起点位置
        /// </summary>
        public virtual LocationInfo StartLocation { get; set; }
        /// <summary>
        /// 终点位置
        /// </summary>
        public virtual LocationInfo EndLocation { get; set; }
        /// <summary>
        /// 路径号
        /// </summary>
        public virtual Int32 RouteId { get; set; }

        public override string ToReadableDescription()
        {
            return String.Format("{0} 将货物(条码：{1})从 {2} 运送到 {3}", this.DeviceName, String.Join(",", this.Movement.Task.ContainerCodes), this.StartLocation, this.EndLocation);
        }
        static Random _rnd = new Random();
        public override DeviceCommand ToAddCommand()
        {
            RobotTaskCommand cmd = new RobotTaskCommand();
            cmd.TaskId = (uint)this.EquipmentTaskId;
            cmd.HandShake = HandShake.New;
            cmd.Pick = Convert.ToUInt16(this.Movement.StartLocation.DeviceCode);
            cmd.Put = Convert.ToUInt16(this.Movement.EndLocation.DeviceCode);

            var device = DeviceConverter.ToDevice<RobotDevice>(this.DeviceName);
            if (device.EquipmentActionToAddCommandPlugin != null)
                device.EquipmentActionToAddCommandPlugin.EquipmentActionToAddCommandPlugin(this, ref cmd);

            return cmd;
        }

        public override DeviceCommand ToCancelCommand()
        {
            RobotTaskCommand cmd = new RobotTaskCommand();
            cmd.TaskId = (uint)this.EquipmentTaskId;
            cmd.HandShake = HandShake.Delete;
            cmd.Pick = Convert.ToUInt16(this.Movement.StartLocation.DeviceCode);
            cmd.Put = Convert.ToUInt16(this.Movement.EndLocation.DeviceCode);
            return cmd;
        }
    }
}
