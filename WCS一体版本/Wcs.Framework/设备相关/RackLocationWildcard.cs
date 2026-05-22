using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 货架位置通配符，即表示一个堆垛机设备能到达的所有位置
    /// </summary>
    public class RackLocationWildcard:RackLocation
    {
        /// <summary>
        /// 构造函数.
        /// </summary>
        /// <param name="device">   位置所属设备. </param>
        public RackLocationWildcard(Device device)
        {
            this.Device = device;
        }
        /// <summary>
        /// 用户编码.
        /// </summary>
        public override string UserCode
        {
            get 
            {
                return string.Format("{0}-***-***", this.Device.DeviceName);
            }
        }
        /// <summary>
        /// 该位置在设备中的编码形式.
        /// </summary>
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
