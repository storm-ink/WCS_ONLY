using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Devices;
using Wcs.Framework.Cfg;

namespace Wcs.Framework
{
    /// <summary>
    /// 位置信息
    /// </summary>
    public class LocationInfo : IComparer<LocationInfo>
    {
        /// <summary>
        /// 位置类型
        /// </summary>
        public virtual Devices.LocationType Type { get; set; }
        /// <summary>
        /// 位置设备可识别的编码
        /// </summary>
        public virtual String DeviceCode { get; set; }
        /// <summary>
        /// 位置用户可识别的编码
        /// </summary>
        public virtual String UserCode { get; set; }
        /// <summary>
        /// 位置所属设备名称
        /// </summary>
        public virtual String DeviceName { get; set; }
        /// <summary>
        /// 位置所属设备类型
        /// </summary>
        public virtual Devices.DeviceType DeviceType { get; set; }

        /// <summary>
        /// 获取该位置信息表示的具体位置对象
        /// </summary>
        /// <returns></returns>
        public virtual Location GetLocation()
        {
            return Configuration
                .Locations
                .SingleOrDefault(x => x.Device.DeviceType == this.DeviceType
                    && string.Equals(x.Device.DeviceName, this.DeviceName, StringComparison.CurrentCultureIgnoreCase)
                    && x.Type == this.Type
                    && string.Equals(x.DeviceCode, this.DeviceCode, StringComparison.CurrentCultureIgnoreCase)
                    && string.Equals(x.UserCode, this.UserCode, StringComparison.CurrentCultureIgnoreCase)
                    );

        }

        public virtual T GetLocation<T>() where T : Location
        {
            return (T)GetLocation();
        }

        public override int GetHashCode()
        {
            return string
                .Format("{0}_{1}_{2}_{3}_{4}", Type, DeviceCode, UserCode, DeviceName, DeviceType)
                .GetHashCode();
        }

        public override bool Equals(object obj)
        {
            LocationInfo other = (obj as LocationInfo);
            if (other == null)
            {
                return false;
            }

            var x = this.GetLocation();
            var y = other.GetLocation();

            if (x.Equals(y)) return true;

            if(x.SameAs!=null && x.SameAs.Any(location=>location.Equals(y)))return true;

            if (y.SameAs != null && y.SameAs.Any(location => location.Equals(x))) return true;

            return other.GetHashCode() == GetHashCode();
        }

        public int Compare(LocationInfo x, LocationInfo y)
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

            if (x.DeviceType != y.DeviceType || !x.DeviceName.Equals(y.DeviceName,StringComparison.CurrentCultureIgnoreCase) || !x.DeviceCode.Equals(y.DeviceCode,StringComparison.CurrentCultureIgnoreCase))
            {
                return 1;
            }

            return 0;
        }

        public override string ToString()
        {
            return string.Format("{0}", this.UserCode);
        }
    }
}
