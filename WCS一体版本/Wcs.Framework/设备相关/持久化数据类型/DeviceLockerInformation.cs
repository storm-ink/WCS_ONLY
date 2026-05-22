using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 设备锁信息的持久化对象
    /// </summary>
    public class DeviceLockerInformation
    {
        protected DeviceLockerInformation() 
        { 

        }
        public DeviceLockerInformation(String deviceName, LockerInfo lockerInfo)
            :this()
        {
            this.DeviceName = deviceName;
            this.LockerInfo = lockerInfo;
        }
        /// <summary>
        /// 锁信息.
        /// </summary>
        public virtual LockerInfo LockerInfo { get; protected set; }
        /// <summary>
        /// 设备信息.
        /// </summary>
        public virtual String DeviceName { get; protected set; }

        public override string ToString()
        {
            return string.Format("{0}->{1}", DeviceName, this.LockerInfo);
        }
    }
}
