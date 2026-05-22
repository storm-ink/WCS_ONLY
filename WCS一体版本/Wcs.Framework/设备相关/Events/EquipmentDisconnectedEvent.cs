using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Events;

namespace Wcs.Framework.Devices.Events
{
    /// <summary>
    /// 表示设备的连接断开事件
    /// </summary>
    public class EquipmentDisconnectedEvent : AbstractEvent<Device>
    {
        readonly DeviceDisconnectReason reason;
        public EquipmentDisconnectedEvent(Device source,DeviceDisconnectReason reason)
            : base(source)
        {
            this.reason=reason;
        }

        /// <summary>
        /// 断开连接的原因
        /// </summary>
        public DeviceDisconnectReason Reason 
        {
            get
            {
                return reason;
            }
        }
    }
}
