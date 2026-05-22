using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    public static class LocationConverter
    {
        static List<AbstractLocationConverter> _converters = new List<AbstractLocationConverter>();
        public static AbstractLocationConverter[] Converters
        {
            get
            {
                return _converters.ToArray();
            }
        }

        public static void AddConverter(AbstractLocationConverter converter)
        {
            if (_converters.Any(x => x == converter 
                || (String.Equals(x.DeviceName,converter.DeviceName,StringComparison.CurrentCultureIgnoreCase) && x.GetType()==converter.GetType())
                ))
            {
                throw new InvalidOperationException(string.Format("已存在类型为 {0}，设备名为 {1} 的转换器", converter.GetType(), converter.DeviceName));
            }
            _converters.Add(converter);
        }

        public static void RemoveConverter(AbstractLocationConverter converter)
        {
            var c = _converters.FirstOrDefault(x => x == converter);
            if (c == null)
            {
                throw new InvalidOperationException(string.Format("未找到转换指定的转换器"));
            }

            _converters.Remove(c);
        }

        static AbstractLocationConverter[] GetConverters(String deviceName)
        {
            if (!string.IsNullOrWhiteSpace(deviceName))
            {
                return _converters
                    .Where(x => string.Equals(x.DeviceName, deviceName, StringComparison.CurrentCultureIgnoreCase))
                    .ToArray();
            }
            else
            {
                return _converters.ToArray();
            }
        }

        /// <summary>
        /// 将指定的位置对象转换为位置描述信息
        /// </summary>
        /// <param name="location">要转换的位置对象</param>
        /// <exception cref="System.ArgumentNullException">location 为 null</exception>
        /// <returns>位置描述信息</returns>
        public static LocationInfo ToLocationInfo(Location location)
        {
            if (location == null)
            {
                throw new ArgumentNullException("location");
            }

            return new LocationInfo(
                location.Device.Name,
                location.DeviceCode, 
                location.UserCode,
                location.UnifiedCode);
        }

        /// <summary>
        /// 将指定的位置描述信息转换为位置对象
        /// </summary>
        /// <param name="locationInfo">要转换的位置描述信息</param>
        /// <exception cref="System.ArgumentNullException">locationInfo 为 null</exception>
        /// <exception cref="System.InvalidOperationException">未找到与 locationInfo 值匹配的位置</exception>
        /// <returns>位置描述信息所表示的位置对象。</returns>
        public static Location ToLocation(LocationInfo locationInfo)
        {
            if (locationInfo == null)
            {
                throw new ArgumentNullException("locationInfo");
            }

            foreach (var converter in GetConverters(locationInfo.DeviceName))
            {
                var l=converter.DeviceCodeToLocation(locationInfo.DeviceCode);
                if (l != null)
                {
                    return l;
                }
            }
            
            var loc = Wcs.Framework.Cfg.WcsConfiguration
                .Instance
                .LocationCollection
                .Locations
                .SingleOrDefault(x =>
                    string.Equals(x.Device.Name, locationInfo.DeviceName, StringComparison.CurrentCultureIgnoreCase)
                    && string.Equals(x.UserCode,locationInfo.UserCode,StringComparison.CurrentCultureIgnoreCase)
                    && string.Equals(x.DeviceCode, locationInfo.DeviceCode, StringComparison.CurrentCultureIgnoreCase)
                );

            if (loc == null)
            {
                throw new InvalidOperationException(String.Format("{0} 无法转换为位置对象",locationInfo));
            }

            return loc;
        }

        /// <summary>
        /// 将指定的位置设备编码转换为指定的设备中的位置对象
        /// </summary>
        /// <param name="deviceName">要匹配的位置所属的设备的名称</param>
        /// <param name="locationDeviceCode">要转换的位置设备编码值</param>
        /// <exception cref="System.ArgumentNullException">deviceName 或 locationDeviceCode 为 null、空或是仅由空白字符组成</exception>
        /// <exception cref="System.InvalidOperationException">未找到与 deviceName、locationDeviceCode 值匹配的位置</exception>
        /// <returns>位置设备编码值所表示的位置对象</returns>
        public static Location ToLocation(String deviceName, String locationDeviceCode)
        {
            if (String.IsNullOrWhiteSpace(deviceName))
            {
                throw new ArgumentNullException("deviceName");
            }

            if (String.IsNullOrWhiteSpace(locationDeviceCode))
            {
                throw new ArgumentNullException("locationDeviceCode");
            }

            foreach (var converter in GetConverters(deviceName))
            {
                var l = converter.DeviceCodeToLocation(locationDeviceCode);
                if (l != null)
                {
                    return l;
                }
            }

            var loc = Wcs.Framework.Cfg.WcsConfiguration
                .Instance
                .LocationCollection
                .Locations
                .SingleOrDefault(x =>
                    string.Equals(x.Device.Name, deviceName, StringComparison.CurrentCultureIgnoreCase)
                    && string.Equals(x.DeviceCode, locationDeviceCode, StringComparison.CurrentCultureIgnoreCase)
                );
            
            if (loc == null)
            {
                throw new InvalidOperationException(String.Format("设备名称 {0}，位置设备编码 {1} 无法转换为位置对象", deviceName,locationDeviceCode));
            }

            return loc;
        }

        /// <summary>
        /// 将指定的位置转换信息转换为位置对象
        /// </summary>
        /// <param name="locationInfo">要转换的位置转换信息</param>
        /// <exception cref="System.ArgumentNullException">convertibleCode 为 null、空或是仅由空白字符组成</exception>
        /// <exception cref="System.InvalidOperationException">未找到与 convertibleCode 值匹配的位置</exception>
        /// <returns>位置转换信息所表示的位置对象。在找不到匹配的位置对象时返回 null</returns>
        public static Location ConvertibleCodeToLcation(String convertibleCode)
        {
            if (String.IsNullOrWhiteSpace(convertibleCode))
            {
                throw new ArgumentNullException("convertibleCode");
            }

            foreach (var converter in GetConverters(null))
            {
                var l = converter.ConvertibleCodeToLcation(convertibleCode);
                if (l != null)
                {
                    return l;
                }
            }

            var loc = Wcs.Framework.Cfg.WcsConfiguration
                .Instance
                .LocationCollection
                .Locations
                .SingleOrDefault(x =>string.Equals(x.ToConvertibleCode(), convertibleCode, StringComparison.CurrentCultureIgnoreCase));

            if (loc == null)
            {
                throw new InvalidOperationException(String.Format("可转换编码值 {0} 无法转换为位置对象", convertibleCode));
            }

            return loc;
        }

        /// <summary>
        /// 将指定的位置用户编码转换为位置对象
        /// </summary>
        /// <param name="locationInfo">要转换的位置用户编码</param>
        /// <exception cref="System.ArgumentNullException">locationUserCode 为 null、空或是仅由空白字符组成</exception>
        /// <exception cref="System.InvalidOperationException">未找到用户编码和 locationUserCode 值匹配的位置</exception>
        /// <exception cref="System.InvalidOperationException">找到 1 个以上用户编码和 locationUserCode 值匹配的位置对象</exception>
        /// <returns>位置用户编码所表示的位置对象。在找不到匹配的位置对象时返回 null</returns>
        public static Location UserCodeToLcation(String locationUserCode)
        {
            if (String.IsNullOrWhiteSpace(locationUserCode))
            {
                throw new ArgumentNullException("locationUserCode");
            }

            foreach (var converter in GetConverters(null))
            {
                var l = converter.DeviceCodeToLocation(locationUserCode);
                if (l != null)
                {
                    return l;
                }
            }

            var locations = Wcs.Framework.Cfg.WcsConfiguration
                .Instance
                .LocationCollection
                .Locations
                .Where(x => string.Equals(x.UserCode, locationUserCode, StringComparison.CurrentCultureIgnoreCase))
                .ToList();

            if (locations.Count == 0)
            {
                throw new InvalidOperationException(String.Format("用户编码 {0} 无法转换为位置对象", locationUserCode));
            }

            if (locations.Count > 1)
            {
                throw new InvalidOperationException(string.Format("找到 {0} 个用户编码与 {1} 匹配的位置对象", locations.Count, locationUserCode));
            }
            
            var loc = locations.Single();

            return loc;
        }
    }
}
