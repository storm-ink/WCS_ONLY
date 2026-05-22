using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Cfg;
namespace Wcs.Framework
{
    /// <summary>
    /// 位置描述信息
    /// </summary>
    public class LocationInfo : IComparer<LocationInfo>
    {
        protected LocationInfo() { }

        public LocationInfo(string deviceName, string deviceCode, string userCode, string unifiedCode)
        {
            this.DeviceName = deviceName;
            this.DeviceCode = deviceCode;
            this.UserCode = userCode;
            this.UnifiedCode = unifiedCode;
        }
        /// <summary>
        /// 位置设备可识别的编码
        /// </summary>
        public virtual String DeviceCode { get; protected set; }
        /// <summary>
        /// 位置用户可识别的编码
        /// </summary>
        public virtual String UserCode { get; protected set; }
        /// <summary>
        /// 系统可识别的统一编码
        /// </summary>
        public virtual String UnifiedCode { get; protected set; }
        /// <summary>
        /// 位置所属设备名称
        /// </summary>
        public virtual String DeviceName { get; protected set; }
      
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

            if (!x.DeviceName.Equals(y.DeviceName,StringComparison.CurrentCultureIgnoreCase) || !x.DeviceCode.Equals(y.DeviceCode,StringComparison.CurrentCultureIgnoreCase))
            {
                return 1;
            }

            return 0;
        }

        public override string ToString()
        {
            return string.Format("{0}", this.UserCode);
        }

        public override int GetHashCode()
        {
            return 7
                ^ getNotSafeHashCode(DeviceCode)
                ^ getNotSafeHashCode(UserCode)
                ^ getNotSafeHashCode(DeviceName);
        }

        public override bool Equals(object obj)
        {
            LocationInfo locationInfo=obj as LocationInfo;
            if(locationInfo==null)
            {
                return false;
            }

            if (Wcs.Framework.Cfg.WcsConfiguration.IsLoaded)
            {
                return base.Equals(obj) ||
                    (
                        string.Equals(this.DeviceCode, locationInfo.DeviceCode, StringComparison.CurrentCultureIgnoreCase)
                        && string.Equals(this.UserCode, locationInfo.UserCode, StringComparison.CurrentCultureIgnoreCase)
                        && string.Equals(this.DeviceName, locationInfo.DeviceName, StringComparison.CurrentCultureIgnoreCase)
                     )
                     ||
                    LocationConverter.ToLocation(locationInfo).Equals(LocationConverter.ToLocation(this));
            }
            else
            {
                return base.Equals(obj) ||
                    (
                        string.Equals(this.DeviceCode, locationInfo.DeviceCode, StringComparison.CurrentCultureIgnoreCase)
                        && string.Equals(this.UserCode, locationInfo.UserCode, StringComparison.CurrentCultureIgnoreCase)
                        && string.Equals(this.DeviceName, locationInfo.DeviceName, StringComparison.CurrentCultureIgnoreCase)
                     );
            }
        }

        int getNotSafeHashCode(String v)
        {
            if (v == null)
            {
                return 0;
            }

            return v.GetHashCode();
        }
    }
}
