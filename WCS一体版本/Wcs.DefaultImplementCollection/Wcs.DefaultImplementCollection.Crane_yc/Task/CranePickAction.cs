using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 表示一个堆垛机的半自动取货动作
    /// </summary>
    public class CranePickAction : EquipmentAction
    {
        protected CranePickAction()
            : base()
        {
        }
        public CranePickAction(CraneDevice device, EquipmentActionGroup group, Int32 equipmentTaskId, Int16 containerCode, RackLocation pickLocation)
            : base(device, group, equipmentTaskId, containerCode)
        {
            this.PickLocation = LocationConverter.ToLocationInfo(pickLocation);
        }
        /// <summary>
        /// 伸叉方向
        /// </summary>
        public virtual LocationInfo PickLocation { get; set; }

        public override string ToReadableDescription()
        {
            return String.Format("{0} 将货物从 {1} 取出", this.DeviceName, this.PickLocation);
        }

        public override DeviceCommand ToAddCommand()
        {
            CraneDevice craneDevice = DeviceConverter.ToDevice<CraneDevice>(this.DeviceName);

            if (craneDevice.LastStatus == null)
            {
                throw new InvalidOperationException(string.Format("{0} 状态未同步，无法获取当前位置", craneDevice));
            }

            int currentColumn = craneDevice.LastStatus.Column;
            int currentLevel = craneDevice.LastStatus.Level;

            RackLocation pickLocation = (RackLocation)LocationConverter.ToLocation(this.PickLocation);
            if (pickLocation.Column != currentColumn
                || pickLocation.Level != currentLevel)
            {
                throw new InvalidOperationException(string.Format("{0} 所处位置（设备位置）为 {1} 列 {2} 层，与取货位置 {3} 列 {4} 层不符", craneDevice, currentColumn, currentLevel, pickLocation.Column, pickLocation.Level));
            }

            //AddTaskCommand cmd = new AddTaskCommand(this.EquipmentTaskId.ToString("00000000"), AddTaskCommandType.半自动取货, pickLocation, pickLocation);
            AddTaskCommand cmd = new AddTaskCommand(this.EquipmentTaskId.ToString(), AddTaskCommandType.半自动取货, pickLocation, pickLocation, "", false);
            return cmd;
        }

        public override DeviceCommand ToCancelCommand()
        {
            throw new NotSupportedException("堆垛机不支持取消任务操作.");
        }
    }
}
