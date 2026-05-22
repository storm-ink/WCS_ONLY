using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Events;

namespace Wcs.Framework.Devices.Events
{
    /// <summary>
    /// 表示设备的连接成功事件
    /// </summary>
    public class EquipmentConnectedEvent : AbstractEvent<Device>
    {
        public EquipmentConnectedEvent(Device source)
            : base(source)
        {

        }
    }
}
