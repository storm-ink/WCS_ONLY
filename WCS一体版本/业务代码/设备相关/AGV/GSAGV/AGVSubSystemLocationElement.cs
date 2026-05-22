using System;
using System.Configuration;
using System.Linq;
using System.Xml;
using Wcs;
using Wcs.Framework;
using Wcs.Framework.Cfg;

namespace BOE.设备相关.AGV.GSAGV
{
    public class AGVSubSystemLocationElement : LocationElement
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public AGVSubSystemLocationElement(XmlNode node, ParticularLocationCollection particularLocationCollection, WcsConfiguration configuration)
            : base(node, particularLocationCollection, configuration, true)
        {

        }
        /// <summary>
        /// 位置X值
        /// </summary>
        public Int32 Position_X { get; set; }
        /// <summary>
        /// 位置Y值
        /// </summary>
        public Int32 Position_Y { get; set; }
        /// <summary>
        /// 位置角度值
        /// </summary>
        public UInt32 Position_Angle { get; set; }

        /// <summary>
        /// ceng
        /// </summary>
        public int Level { get; set; }
        /// <summary>
        /// 站点
        /// </summary>
        public int StationNo { get; set; }
        protected override Wcs.Framework.Location CreateLocation()
        {
            Device device;
            if (String.Equals(this.DeviceName, this.ParticularLocationCollection.DeviceName, StringComparison.CurrentCultureIgnoreCase))
            {
                device = this.ParticularLocationCollection.GetDeviceElement().Device;
            }
            else
            {
                device = this.WcsConfiguration
                .DeviceCollection
                .ParticularDeviceCollection
                .SelectMany(x => x.DeviceElements)
                .Where(x => string.Equals(x.Device.Name, this.DeviceName, StringComparison.CurrentCultureIgnoreCase))
                .Select(x => x.Device)
                .SingleOrDefault();

                if (device == null)
                {
                    string msg = string.Format("未找到 {0} 节点属性 {1} 值 “{2}” 中标识的设备",
                        this.Node.GetXPath(),
                        "device",
                        this.DeviceName);
                    throw new ConfigurationErrorsException(msg);
                }
            }

            if (!(device is GSAGVDevice))
            {
                string msg = string.Format("{0} 节点属性 {1} 值 “{2}” 中标识的设备 {3} 并非 {4} 类型",
                    this.Node.GetXPath(),
                    "device",
                    this.DeviceName,
                    device,
                    typeof(GSAGVDevice)
                    );
                throw new ConfigurationErrorsException(msg);
            }

            AGVSubSystemLocation loc;
            if (!this.IsWildcard)
            {
                if (this.LocationType != null)
                {
                    WcsConfiguration._logger.Warn1(string.Format("{0} 节点属性 {1} 指定了一个值 {2}，但该位置不是通配符，已被忽略。", this.Node.GetXPath(), "type", this.LocationType), this);
                }

                loc = new AGVSubSystemLocation((GSAGVDevice)device, DeviceCode, UserCode, StationNo, Position_X, Position_Y, Position_Angle, Level);
            }
            else
            {
                if (this.LocationType != null)
                {
                    if (!this.LocationType.IsSubclassOf(typeof(AGVSubSystemLocationWildcard)))
                    {
                        throw new ConfigurationErrorsException(string.Format("{0} 节点属性 {1} 所指定的类型 {2} 不是 {3} 的子类。", this.Node.GetXPath(), "type", this.LocationType, typeof(AGVSubSystemLocationWildcard)));
                    }

                    loc = ReflectionHelper.CreateInstance<AGVSubSystemLocationWildcard>(this.LocationType, device, this);
                }
                else
                {
                    loc = new AGVSubSystemLocationWildcard((GSAGVDevice)device, this);
                }
            }

            ((GSAGVDevice)device).AcceptLocation(loc);

            return loc;
        }

        protected override void Deserialize()
        {
            if (!this.IsWildcard)
            {
                this.Position_X = GetAttribute<Int32>("position_x");
                this.Position_Y = GetAttribute<Int32>("position_y");
                this.Position_Angle = GetAttribute<UInt32>("position_angle");
                this.Level = GetAttribute<Int32>("level");
                this.StationNo = GetAttribute<Int32>("stationNo");
            }
        }
    }
}
