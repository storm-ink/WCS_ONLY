using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Wcs.Framework;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Robot
{
    public  class RobotLocationElement : LocationElement
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public RobotLocationElement(XmlNode node, ParticularLocationCollection particularLocationCollection, WcsConfiguration configuration)
            : base(node, particularLocationCollection, configuration, true)
        {
        }

        protected override Framework.Location CreateLocation()
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

            if (!(device is RobotDevice))
            {
                string msg = string.Format("{0} 节点属性 {1} 值 “{2}” 中标识的设备 {3} 并非 {4} 类型",
                    this.Node.GetXPath(),
                    "device",
                    this.DeviceName,
                    device,
                    typeof(RobotDevice)
                    );
                throw new ConfigurationErrorsException(msg);
            }

            RobotLocation loc;
            if (!this.IsWildcard)
            {
                if (this.LocationType != null)
                {
                    WcsConfiguration._logger.Warn1(string.Format("{0} 节点属性 {1} 指定了一个值 {2}，但该位置不是通配符，已被忽略。", this.Node.GetXPath(), "type", this.LocationType), this);
                }

                loc = new RobotLocation(Convert.ToInt32(this.DeviceCode), this.UserCode, device);
            }
            else
            {
                if (this.LocationType != null)
                {
                    if (!this.LocationType.IsSubclassOf(typeof(RobotLocationWildcard)))
                    {
                        throw new ConfigurationErrorsException(string.Format("{0} 节点属性 {1} 所指定的类型 {2} 不是 {3} 的子类。", this.Node.GetXPath(), "type", this.LocationType, typeof(RobotLocationWildcard)));
                    }

                    loc = ReflectionHelper.CreateInstance<RobotLocationWildcard>(this.LocationType, device, this.AbleAsOnlyLocation);
                }
                else
                {
                    loc = new RobotLocationWildcard(device, this.AbleAsOnlyLocation);
                }
            }
            ((RobotDevice)device).AcceptLocation(loc);
            return loc;
        }

        protected override void Deserialize()
        {
        }
    }
}
