using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Robot
{
    /// <summary>
    /// Robot位置通配符，即表示一个输送线设备的所有位置
    /// </summary>
    public class RobotLocationWildcard : RobotLocation, ILocationWildcard
    {
        public RobotLocationWildcard(Device device, bool ableAsOnlyLocation = true)
            : base(-1, device.Name, device)
        {
            _ableAsOnlyLocation = ableAsOnlyLocation;
        }
        /// <summary>
        /// 用户编码格式： 00-设备编号（不足三位时补0）-设备货位号（不足三位时补0）.
        /// </summary>
        public override string UserCode
        {
            get
            {
                return String.Format("{0}-{1:000}-***", Device.Name, Device.No);
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
            if (obj == null)
            {
                return false;
            }

            if (base.Equals(obj))
            {
                return true;
            }

            var robotDevice = (RobotDevice)this.Device;

            return robotDevice.Locations
                .Where(x => !(x is ILocationWildcard))
                .Any(x => x.Equals(obj) || obj.Equals(x));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public Location[] GetMatchedLocations()
        {
            RobotDevice robotDevice = (RobotDevice)this.Device;
            return robotDevice
                .Locations
                .Where(x => x != this)
                .ToArray();
        }

        Boolean _ableAsOnlyLocation;
        public bool AbleAsOnlyLocation
        {
            get { return _ableAsOnlyLocation; }
        }
    }
}
