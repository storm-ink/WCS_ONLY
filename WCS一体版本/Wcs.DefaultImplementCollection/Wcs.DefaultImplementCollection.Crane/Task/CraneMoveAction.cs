using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Crane
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

            int currentColumn = craneDevice.LastStatus.XColumn;
            int currentLevel = craneDevice.LastStatus.YLevel;

            RackLocation startLocation = craneDevice.Locations.Select(x => (RackLocation)x).FirstOrDefault(x => x.Column == currentColumn && x.Level == currentLevel);
            if (startLocation == null)
            {
                throw new InvalidOperationException(string.Format("{0} 所处位置（设备位置）为 {1} 列 {2} 层未在设备中定义，系统无法识别", craneDevice, currentColumn, currentLevel));
            }
            RackLocation endLocation = (RackLocation)WcsConfiguration.TryParseLocation(this.ToLocation.DeviceName, this.ToLocation.DeviceCode);
            CraneCommand cmd;
            if (this.Movement.Task.AdditionalInfo.ContainsKey("SensorCheckInventory") && this.Movement.Task.AdditionalInfo["SensorCheckInventory"].ToUpper() == "TRUE")
            {
                var equipmentTaskId = craneDevice.GetEquipmentTaskId(CraneDevice.CraneEquipmentTaskType.WcsTaskStep_SensorCheckInventory);
                cmd = new CraneCommand(CmdTypes.NewTask, CraneTaskTypes.SensorCheckInventory, null, endLocation, equipmentTaskId, (UInt32)this.EquipmentTaskId, this.Movement.Task.ContainerCodes.FirstOrDefault());
            }
            else if (this.Movement.Task.AdditionalInfo.ContainsKey("ScanCheckInventory") && this.Movement.Task.AdditionalInfo["ScanCheckInventory"].ToUpper() == "TRUE")
            {
                var equipmentTaskId = craneDevice.GetEquipmentTaskId(CraneDevice.CraneEquipmentTaskType.WcsTaskStep_ScanCheckInventory);
                cmd = new CraneCommand(CmdTypes.NewTask, CraneTaskTypes.SensorCheckInventory, null, endLocation, equipmentTaskId, (UInt32)this.EquipmentTaskId, this.Movement.Task.ContainerCodes.FirstOrDefault());
            }
            else
            {
                var equipmentTaskId = craneDevice.GetEquipmentTaskId(CraneDevice.CraneEquipmentTaskType.WcsTaskStep_Move);
                cmd = new CraneCommand(CmdTypes.NewTask, CraneTaskTypes.StepWalk, null, endLocation, equipmentTaskId, (UInt32)this.EquipmentTaskId, this.Movement.Task.ContainerCodes.FirstOrDefault());
            }

            return cmd;
        }

        public override DeviceCommand ToCancelCommand()
        {
            throw new NotSupportedException("堆垛机不支持取消任务操作.");
        }
    }
}
