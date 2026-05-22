using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 设备断开连接的事件数据
    /// </summary>
    public class DeviceDisconnectEventArgs:EventArgs
    {
        /// <summary>
        /// 断开连接的原因
        /// </summary>
        public DeviceDisconnectReason Reason { get; private set; }
        /// <summary>
        /// 构造函数.
        /// </summary>
        /// <param name="reason">   断开连接的原因. </param>
        public DeviceDisconnectEventArgs(DeviceDisconnectReason reason)
        {
            this.Reason = reason;
        }
    }
}
