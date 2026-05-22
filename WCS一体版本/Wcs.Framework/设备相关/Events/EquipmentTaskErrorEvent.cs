using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Events;

namespace Wcs.Framework.Devices.Events
{
    public class EquipmentTaskErrorEvent : AbstractEvent<Device>
    {
        readonly UInt32 equipmentTaskId;
        readonly String errorCode;
        readonly String errorDescription;

        public EquipmentTaskErrorEvent(Device source,UInt32 equipmentTaskId, String errorCode, String errorDescription)
            : base(source)
        {
            this.equipmentTaskId = equipmentTaskId;
            this.errorCode = errorCode;
            this.errorDescription = errorDescription;
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
        /// 获取错误编码
        /// </summary>
        public String ErrorCode
        {
            get
            {
                return errorCode;
            }
        }

        /// <summary>
        /// 获取错误描述
        /// </summary>
        public String ErrorDescription
        {
            get
            {
                return errorDescription;
            }
        }

        /// <summary>
        /// 获取或设置是否已处理的标志位
        /// </summary>
        public Boolean Handled { get; set; }
    }
}
