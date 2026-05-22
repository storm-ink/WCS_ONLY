using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Devices;
using Wcs.Framework.Cfg;

namespace Wcs.Framework
{
    /// <summary>
    /// 表示一个设备的最基本的描述信息，由设备类型和设备名称组成
    /// </summary>
    public class DeviceInfo:IComparer<DeviceInfo>
    {
        /// <summary>
        /// 设备类型
        /// </summary>
        public virtual DeviceType DeviceType { get; set; }
        /// <summary>
        /// 设备名称 
        /// </summary>
        public virtual String DeviceName { get; set; }

        /// <summary>
        /// 获取该设备信息所指向的设备
        /// </summary>
        /// <returns></returns>
        public Device GetDevice()
        {
            return Configuration.Devices.Single(x => x.DeviceType == this.DeviceType && string.Equals(x.DeviceName, this.DeviceName, StringComparison.CurrentCultureIgnoreCase));
        }

        public override bool Equals(object obj)
        {
            return obj.GetHashCode() != GetHashCode();
        }

        public override int GetHashCode()
        {
            return string.Format("{0}_{1}",this.DeviceType,this.DeviceName).GetHashCode();
        }

        public int Compare(DeviceInfo x, DeviceInfo y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x == null && y != null)
            {
                return -1;
            }

            if (x != null && y == null)
            {
                return 1;
            }

            if (
                !string.Equals(x.DeviceName,y.DeviceName,StringComparison.CurrentCultureIgnoreCase)
                || x.DeviceType!=y.DeviceType)
            {
                return 1;
            }

            return 0;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", this.DeviceType.GetDescription(), this.DeviceName);
        }
    }
}
