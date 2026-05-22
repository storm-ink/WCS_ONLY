using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 表示一个堆垛机的半自动移动动作
    /// </summary>
    public class CraneMoveAction : EquipmentAction
    {
        protected CraneMoveAction()
            : base()
        {
        }
        public CraneMoveAction(CraneDevice device, EquipmentActionGroup group, Int32 equipmentTaskId, RackLocation toLocation, Int16 containerCode)
            : base(device, group, equipmentTaskId, containerCode)
        {
            this.ToLocation = LocationConverter.ToLocationInfo(toLocation);
        }
        /// <summary>
        /// 终点位置
        /// </summary>
        public virtual LocationInfo ToLocation { get; set; }

        public override string ToReadableDescription()
        {
            return String.Format("{0} 将从当前所在位置移动到 {1}", this.DeviceName, this.ToLocation);
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

            RackLocation startLocation = craneDevice.Locations.Select(x => (RackLocation)x).FirstOrDefault(x => x.Column == currentColumn && x.Level == currentLevel);
            if (startLocation == null)
            {
                throw new InvalidOperationException(string.Format("{0} 所处位置（设备位置）为 {1} 列 {2} 层未在设备中定义，系统无法识别", craneDevice, currentColumn, currentLevel));
            }
            RackLocation endLocation = (RackLocation)WcsConfiguration.TryParseLocation(this.ToLocation.DeviceName, this.ToLocation.DeviceCode);
            AddTaskCommand cmd;
            if (this.Movement.Task.TaskType == "盘点任务")
                cmd = new AddTaskCommand(this.EquipmentTaskId.ToString(), AddTaskCommandType.半自动行走, startLocation, endLocation, "", false);
            // cmd = new AddTaskCommand("PD" + this.EquipmentTaskId.ToString("000000"), AddTaskCommandType.半自动行走, startLocation, endLocation);
            else
                cmd = new AddTaskCommand(this.EquipmentTaskId.ToString(), AddTaskCommandType.半自动行走, startLocation, endLocation, "", false);
            // cmd = new AddTaskCommand(this.EquipmentTaskId.ToString("00000000"), AddTaskCommandType.半自动行走, startLocation, endLocation);
            return cmd;
        }

        public override DeviceCommand ToCancelCommand()
        {
            throw new NotSupportedException("堆垛机不支持取消任务操作.");
        }
    }
}
