using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Cfg;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 设备位置<br />
    /// 系统内所有路径都是通过位置来进行连通
    /// </summary>
    public abstract class Location
    {
        /// <summary>
        /// 位置所属的设备
        /// </summary>
        public Device Device { get; set; }
        /// <summary>
        /// 用户编码
        /// </summary>
        public abstract String UserCode { get; }
        /// <summary>
        /// 该位置在设备中的编码形式
        /// </summary>
        public abstract String DeviceCode { get;}

        /// <summary>
        /// 类型
        /// </summary>
        public abstract LocationType Type { get; }

        /// <summary>
        /// 和此对象表示的位置一样，但表述不一样的对象
        /// 如00-101-001（输送线 1 号货位） 和 01-000-001（堆垛机 1 排 0 列 1层）
        /// </summary>
        public Location[] SameAs { get; set; }
        /// <summary>
        /// 确定指定的 <see cref="T:Wcs.Framework.Devices.Location" /> 是否等于当前的 <see cref="T:Wcs.Framework.Devices.Location" />。.
        /// </summary>
        /// <param name="obj">  与当前的 <see cref="T:Wcs.Framework.Devices.Location" /> 进行比较的
        ///                     <see cref="T:Wcs.Framework.Devices.Location" />。. </param>
        /// <returns>
        /// 如果指定的 <see cref="T:Wcs.Framework.Devices.Location" />  等于当前的 <see cref="T:Wcs.Framework.Devices.Location" /> 或当前的 <see cref="T:Wcs.Framework.Devices.Location.SameAs" /> 内的对象 ，则为 true；否则为 false。.
        /// </returns>
        public override bool Equals(object obj)
        {
            Location location = obj as Location;
            if (location == null)
            {
                return false;
            }

            if (this == location) return true;

            if (this.SameAs != null && this.SameAs.Any(lct => lct.Equals(location))) return true;

            if (location.SameAs != null && location.SameAs.Any(lct => lct.Equals(this))) return true;

            //如果是位置通配符，只要判断设备是否相关即可
            if (this is RackLocationWildcard || location is RackLocationWildcard)
            {
                return this.Device == location.Device;
            }

            if (this is ConveyorLocationWildcard || location is ConveyorLocationWildcard)
            {
                return this.Device == location.Device;
            }

            return this.Device == location.Device && string.Equals(this.DeviceCode, location.DeviceCode, StringComparison.CurrentCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            return string.Concat("{0}_{1}_{2}", Device.DeviceType, Device.DeviceName, this.DeviceCode).GetHashCode();
        }

        /// <summary>
        /// 将指定的字符串转换为位置对象
        /// </summary>
        /// <param name="locationCode">描述一个位置对象的值，可以是位置设备编码，也可以是带描述设备信息的编码</param>
        /// <param name="locations">要搜索的位置数据集合，可以为null。该参数为 null 时，则将从 Configuration 加载位置数据</param>
        /// <param name="device">这个位置属于哪个设备，可以为null。该参数为 null 时，则 locationCode 必须包含描述位置所属设备的值，如1@conveyor1</param>
        /// <returns>位置对象</returns>
        public static Location TryParse(string locationCode, IEnumerable<Location> locations, Device device)
        {
            if (device == null || locationCode.Contains("@"))
            {
                if (!locationCode.Contains("@"))
                {
                    return null;
                }

                if (locationCode.Split('@').Length != 2)
                {
                    return null;
                }

                string deviceName = locationCode.Split('@')[1].Trim();

                device = Configuration.Devices.FirstOrDefault(x => x.DeviceName.Equals(deviceName, StringComparison.CurrentCultureIgnoreCase));
             }

            if (device == null)
            {
                return null;
            }

            string locationDeviceCode = locationCode.Split('@')[0].Trim();

            Location location;
            if (locations == null)
            {
                location = Configuration.Locations.SingleOrDefault(x => x.Device == device && x.DeviceCode.Equals(locationDeviceCode, StringComparison.CurrentCultureIgnoreCase));
            }
            else
            {
                location = locations.SingleOrDefault(x => x.Device == device && x.DeviceCode.Equals(locationDeviceCode, StringComparison.CurrentCultureIgnoreCase));
            }
            return location;
        }

        /// <summary>
        /// 将指定的字符串转换为位置对象
        /// </summary>
        /// <param name="locationCode">描述一个位置对象的值，可以是位置设备编码，也可以是带描述设备信息的编码</param>
        public static Location TryParse(string locationCode)
        {
            return TryParse(locationCode, null,null);
        }

        /// <summary>
        /// 将位置用户编码转换为位置对象
        /// </summary>
        /// <param name="userCode">用户编码</param>
        /// <returns>位置对象</returns>
        public static Location ParseUserCodeToLocation(string userCode)
        {
            return ParseUserCodeToLocation(userCode, null);
        }

        /// <summary>
        /// 将位置用户编码转换为位置对象
        /// </summary>
        /// <param name="userCode">用户编码</param>
        /// <returns>位置对象</returns>
        public static Location ParseUserCodeToLocation(string userCode,Device device)
        {
            if (device==null && !Configuration.Initialized)
            {
                throw new Exception("Configuration 未初始化");
            }

            if (device == null)
            {
                return Configuration
                    .Locations
                    .SingleOrDefault(x => string.Equals(x.UserCode, userCode, StringComparison.CurrentCultureIgnoreCase));
            }
            else
            {
                return Configuration
                    .Locations
                    .SingleOrDefault(x => x.Device==device && string.Equals(x.UserCode, userCode, StringComparison.CurrentCultureIgnoreCase));
            }
        }

        public override string ToString()
        {
            return this.UserCode;
        }
        /// <summary>
        /// 将当前位置对象转换为描述该位置的信息数据
        /// </summary>
        /// <returns>能准确描述该位置的信息数据</returns>
        public LocationInfo CreateLocationInfo()
        {
            return new LocationInfo
            {
                DeviceCode =this.DeviceCode,
                DeviceName= this.Device.DeviceName,
                UserCode = this.UserCode,
                DeviceType = this.Device.DeviceType,
                Type = this.Type
            };
        }

        /// <summary>
        /// 判断两个点是否可能连通
        /// </summary>
        /// <param name="startLocation">起始位置</param>
        /// <param name="endLocation">结束位置</param>
        /// <param name="findOptions">查询参数(指定适应查询的路径类型)</param>
        /// <returns>Boolean 值，连通返回 true，不连通返回 false</returns>
        public static Boolean CanReach(Location startLocation, Location endLocation, DeviceRouteType findOptions)
        {
            return Configuration
                .Nets
                .Any(x=>x.CanReach(startLocation,endLocation,startLocation,null,findOptions));
        }

        /// <summary>
        /// 转换为系统可识别（可二次转换的）编码值
        /// </summary>
        /// <returns>字符串，格式为：位置在设备中的编码形式@设备名称，如：01001001@c001</returns>
        public String GetConvertibleCode()
        {
            return String.Format("{0}@{1}", this.DeviceCode, this.Device.DeviceName);
        }
    }
}
