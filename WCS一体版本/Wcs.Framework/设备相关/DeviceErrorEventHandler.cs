using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 表示将要处理 Device 的 DeviceError 事件的方法
    /// </summary>
    /// <param name="sender">引发错误的设备</param>
    /// <param name="errorCode">错误码</param>
    /// <param name="errorDescription">错误描述</param>
    /// <param name="handled">是否已处理.</param>
    public delegate void DeviceErrorEventHandler(Device sender, String errorCode, String errorDescription, ref Boolean handled);
}
