using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.Framework
{
    /// <summary>
    /// 任务绑定设备信息
    /// </summary>
    public class TaskActionBindingDevice
    {
        public TaskActionBindingDevice(AbstractStateManager stateManager, String deviceName)
            : this()
        {
            this.EquipmentActionId = stateManager.EquipmentAction.Id;
            this.DeviceName = deviceName;
        }

        public TaskActionBindingDevice(int equipmentActionId, String deviceName, string deviceType)
            : this()
        {
            this.EquipmentActionId = equipmentActionId;
            this.DeviceName = deviceName;
            this.DeviceType = deviceType;
        }

        protected TaskActionBindingDevice()
        {
            this.CreatedAt = DateTime.Now;
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTime CreatedAt { get; protected set; }

        /// <summary>
        /// 关联的设备名称
        /// </summary>
        public virtual String DeviceName { get; set; }

        /// <summary>
        /// 关联的设备类型
        /// </summary>
        public virtual string DeviceType { get; set; }

        /// <summary>
        /// 关联的物理动作Id（约束）
        /// </summary>
        public virtual Int32 EquipmentActionId { get; protected set; }

        public override string ToString()
        {
            return String.Format("任务绑定设备信息：{0}-{1}", this.EquipmentActionId, this.DeviceName);
        }
    }
}
