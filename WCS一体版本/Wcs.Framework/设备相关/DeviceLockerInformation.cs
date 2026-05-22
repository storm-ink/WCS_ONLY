using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 设备锁信息的持久化对象
    /// </summary>
    public class DeviceLockerInformation:IAggregateRoot
    {
        /// <summary>
        /// 锁信息.
        /// </summary>
        public virtual LockerInfo LockerInfo { get; set; }
        /// <summary>
        /// 设备信息.
        /// </summary>
        public virtual DeviceInfo DeviceInfo { get; set; }
    }
}
