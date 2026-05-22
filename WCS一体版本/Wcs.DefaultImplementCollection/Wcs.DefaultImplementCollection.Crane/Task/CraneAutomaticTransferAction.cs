using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Crane
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

            var craneDevice = DeviceConverter.ToDevice<CraneDevice>(this.DeviceName);
            var equipmentTaskId = craneDevice.GetEquipmentTaskId(CraneDevice.CraneEquipmentTaskType.WcsTaskAuto);
            CraneCommand cmd = new CraneCommand(CmdTypes.NewTask, CraneTaskTypes.Auto, startLocation, endLocation, equipmentTaskId, (UInt32)this.EquipmentTaskId);
            return cmd;
        }

        public override DeviceCommand ToCancelCommand()
        {
            var craneDevice = DeviceConverter.ToDevice<CraneDevice>(this.DeviceName);
            if (!craneDevice.IsConnected || craneDevice.LastStatus == null)
                throw new NotSupportedException("堆垛机未连接，当前不支持取消任务操作.");

            if (craneDevice.LastStatus.WcsTaskId != this.EquipmentTaskId)
                return new CraneCommand(CmdTypes.ClearTask);

            if (craneDevice.LastStatus.DeviceState ==  CraneStatus.AlarmDown || craneDevice.LastStatus.DeviceState == CraneStatus.Watting)
                return new CraneCommand(CmdTypes.ClearTask);

            throw new NotSupportedException("堆垛机不支持取消任务操作.");
        }
    }
}
