using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Events;
namespace Wcs.Framework.Devices.Events
{
    /// <summary>
    /// 表示设备任务运行的事件
    /// </summary>
    public class EquipmentTaskRunningEvent:AbstractEvent<Device>
    {
        readonly UInt32 equipmentTaskId;
        public EquipmentTaskRunningEvent(Device source,UInt32 equipmentTaskId)
            : base(source)
        {
            this.equipmentTaskId = equipmentTaskId;
        }

        /// <summary>
        /// 获取设备任务号
        /// </summary>
        public UInt32 EquipmentTaskId
        {
            get
            {
                return equipmentTaskId;
            }
        }
        /// <summary>
        /// 获取或设置是否已处理的标志位
        /// </summary>
        public Boolean Handled { get; set; }
    }
}
