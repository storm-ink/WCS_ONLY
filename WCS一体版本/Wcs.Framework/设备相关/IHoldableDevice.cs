using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 表示实现此接口的设备是一个可以被占用的设备类型
    /// </summary>
    public interface IHoldableDevice
    {
        /// <summary>
        /// 当前持有者
        /// </summary>
        IDeviceHolder Holder { get; }
        /// <summary>
        /// 要求持有
        /// </summary>
        /// <param name="holder">持有对象</param>
        void Hold(IDeviceHolder holder);
        /// <summary>
        /// 释放持有
        /// </summary>
        void Unhold(IDeviceHolder holder);
    }
}
