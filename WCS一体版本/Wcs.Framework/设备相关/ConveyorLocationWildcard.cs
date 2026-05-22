using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 输送线位置通配符，即表示一个输送线设备的所有位置
    /// </summary>
    public class ConveyorLocationWildcard:ConveyorLocation
    {
        public ConveyorLocationWildcard(Device device)
            : base(-1, null, false, false, false, device)
        {
            this.Device = device;
        }
        /// <summary>
        /// 用户编码格式： 00-设备编号（不足三位时补0）-设备货位号（不足三位时补0）.
        /// </summary>
        public override string UserCode
        {
            get 
            {
                return String.Format("{0}-{1:000}-***",Device.DeviceName, Device.DeviceNo);
            }
        }
        /// <summary>
        /// 该位置在设备中的编码形式.
        /// </summary>
        /// <value>
        /// 字符 "*"
        /// </value>
        public override string DeviceCode
        {
            get
            {
                return "*";
            }
        }

        public override bool Equals(object obj)
        {
            Location location = obj as Location;
            if (location == null)
            {
                return false;
            }

            return location.Device == this.Device;
        }
        /// <summary>
        /// 类型.
        /// </summary>
        /// <value>
        /// LocationType.LocationWildcard.
        /// </value>
        public override LocationType Type
        {
            get
            {
                return LocationType.LocationWildcard;
            }
        }
    }
}
