using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Wcs.Framework;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    public  class ConveyorLocationElement:LocationElement
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public ConveyorLocationElement(XmlNode node, ParticularLocationCollection particularLocationCollection, WcsConfiguration configuration)
            : base(node, particularLocationCollection, configuration, true)
        {
        }

        public Boolean AcceptRequestSignal { get; private set; }
        public Boolean IsEntrance {get;private set;}
        public Boolean IsExit { get; private set; }
        /// <summary>
        /// 是否存在虚拟位置
        /// </summary>
        public Boolean HasFictitousLocation { get; set; }
        /// <summary>
        /// 是否是虚拟位置
        /// </summary>
        public Boolean IsFictitiousLocation { get; set; }
        /// <summary>
        /// 虚拟位置
        /// </summary>
        public String FictitiousConvertibleCode { get; set; }
        public string Region { get; set; }

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

            if (!(device is ConveyorDevice))
            {
                string msg = string.Format("{0} 节点属性 {1} 值 “{2}” 中标识的设备 {3} 并非 {4} 类型",
                    this.Node.GetXPath(),
                    "device",
                    this.DeviceName,
                    device,
                    typeof(ConveyorDevice)
                    );
                throw new ConfigurationErrorsException(msg);
            }

            ConveyorLocation loc;
            if (!this.IsWildcard)
            {
                if (this.LocationType != null)
                {
                    WcsConfiguration._logger.Warn1(string.Format("{0} 节点属性 {1} 指定了一个值 {2}，但该位置不是通配符，已被忽略。", this.Node.GetXPath(), "type", this.LocationType), this);
                }

                loc = new ConveyorLocation(Convert.ToInt32(this.DeviceCode), this.UserCode, device, AcceptRequestSignal, IsEntrance, IsExit, HasFictitousLocation, IsFictitiousLocation, FictitiousConvertibleCode, Region);
            }
            else
            {
                if (this.LocationType != null)
                {
                    if (!this.LocationType.IsSubclassOf(typeof(ConveyorLocationWildcard)))
                    {
                        throw new ConfigurationErrorsException(string.Format("{0} 节点属性 {1} 所指定的类型 {2} 不是 {3} 的子类。", this.Node.GetXPath(), "type", this.LocationType, typeof(ConveyorLocationWildcard)));
                    }

                    loc = ReflectionHelper.CreateInstance<ConveyorLocationWildcard>(this.LocationType, device, this.AbleAsOnlyLocation);
                }
                else
                {
                    loc = new ConveyorLocationWildcard(device, this.AbleAsOnlyLocation);
                }
            }
            ((ConveyorDevice)device).AcceptLocation(loc);
            return loc;
        }

        protected override void Deserialize()
        {
            this.AcceptRequestSignal = GetAttributeOrDefault<Boolean>("acceptRequestSignal");
            this.IsEntrance = GetAttributeOrDefault<Boolean>("isEntrance");
            this.IsExit = GetAttributeOrDefault<Boolean>("isExit");
            this.HasFictitousLocation = GetAttributeOrDefault<Boolean>("hasFictitousLocation",false);
            this.IsFictitiousLocation = GetAttributeOrDefault<Boolean>("isFictitiousLocation", false);
            this.FictitiousConvertibleCode = GetAttributeOrDefault<String>("fictitiousConvertibleCode", null);
            this.Region = GetAttributeOrDefault<string>("region", this.DeviceName);
        }
    }
}
