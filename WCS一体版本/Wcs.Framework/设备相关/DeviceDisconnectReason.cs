using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 设备断开连接的原因
    /// </summary>
    public enum DeviceDisconnectReason
    {
        /// <summary>
        /// 发生错误
        /// </summary>
        Error,
        /// <summary>
        /// 用户强制断开
        /// </summary>
        User
    }
}
