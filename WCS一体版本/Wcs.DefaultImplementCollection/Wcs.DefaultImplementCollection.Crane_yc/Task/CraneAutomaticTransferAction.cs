using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 表示一个堆垛机的全自动物理动作
    /// </summary>
    public class CraneAutomaticTransferAction : EquipmentAction
    {
        protected CraneAutomaticTransferAction()
            : base()
        {
        }
        public CraneAutomaticTransferAction(CraneDevice device, EquipmentActionGroup group, Int32 equipmentTaskId, RackLocation loadLocation, RackLocation unloadLocation, Int16 containerCode)
            : base(device, group, equipmentTaskId, containerCode)
        {
            this.LoadLocation = LocationConverter.ToLocationInfo(loadLocation);
            this.UnloadLocation = LocationConverter.ToLocationInfo(unloadLocation); 
        }
        /// <summary>
        /// 取货位置
        /// </summary>
        public virtual LocationInfo LoadLocation { get; set; }
        /// <summary>
        /// 卸货位置
        /// </summary>
        public virtual LocationInfo UnloadLocation { get; set; }

        public override string ToReadableDescription()
        {
            return String.Format("{0} 将货物从 {1} 运送到 {2}", this.DeviceName, this.LoadLocation, this.UnloadLocation);
        }

        public override DeviceCommand ToAddCommand()
        {
            RackLocation startLocation = (RackLocation)WcsConfiguration.TryParseLocation(this.LoadLocation.DeviceName, this.LoadLocation.DeviceCode);
            RackLocation endLocation = (RackLocation)WcsConfiguration.TryParseLocation(this.UnloadLocation.DeviceName, this.UnloadLocation.DeviceCode);

            //AddTaskCommand cmd = new AddTaskCommand(this.EquipmentTaskId.ToString("00000000"), AddTaskCommandType.全自动, startLocation, endLocation);
            AddTaskCommand cmd = new AddTaskCommand(this.EquipmentTaskId.ToString(), AddTaskCommandType.全自动, startLocation, endLocation, "", false);
            return cmd;
        }

        public override DeviceCommand ToCancelCommand()
        {
            var craneDevice = DeviceConverter.ToDevice<CraneDevice>(this.DeviceName);
            if (craneDevice.LastStatus == null)
            {
                return new CancelCraneAutomaticTransferActionCommand();
            }

            if (craneDevice.ParseEquipmentTaskId(craneDevice.LastStatus) != this.EquipmentTaskId)
            {
                return new CancelCraneAutomaticTransferActionCommand();
            }

            if (craneDevice.LastStatus.State == CraneStatus.无货待命
                || craneDevice.LastStatus.State == CraneStatus.有货待命
                || craneDevice.LastStatus.State == CraneStatus.ResetAlarm
                || craneDevice.LastStatus.State == CraneStatus.AlarmAndShutdown
                || craneDevice.LastStatus.State == CraneStatus.Initialized
                || craneDevice.LastStatus.State == CraneStatus.ManualMode
                || craneDevice.LastStatus.State == CraneStatus.BackToTheOrigin
                )
            {
                return new CancelCraneAutomaticTransferActionCommand();
            }

            throw new NotSupportedException("堆垛机不支持取消任务操作.");
        }
    }
}
