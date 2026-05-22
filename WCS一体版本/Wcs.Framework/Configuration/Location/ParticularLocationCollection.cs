using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    public abstract class ParticularLocationCollection:ConfigurationElement
    {
        DeviceElement _deviceElement;

        /// <summary>
        /// 字典，用于检查配置内的货位是否重复
        /// </summary>
        Dictionary<String, Location> locationsDict = new Dictionary<string, Location>();
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public ParticularLocationCollection(XmlNode node, WcsConfiguration configuration)
            : this(node, configuration, true)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        /// <param name="deserialize">是否立即解析</param>
        public ParticularLocationCollection(XmlNode node, WcsConfiguration configuration, Boolean deserialize)
            : base(node, configuration, false)
        {
            LocationElements = new LocationElement[0];
            DeviceName = GetAttributeOrDefault<String>("device");
            if (!String.IsNullOrWhiteSpace(DeviceName))
            {
                _deviceElement = this.WcsConfiguration
                .DeviceCollection
                .ParticularDeviceCollection
                .SelectMany(x => x.DeviceElements)
                .Where(x => string.Equals(x.Device.Name, this.DeviceName, StringComparison.CurrentCultureIgnoreCase))
                .SingleOrDefault();

                if (_deviceElement == null)
                {
                    string msg = string.Format("未找到 {0} 节点属性 {1} 值 “{2}” 中标识的设备",
                        this.Node.GetXPath(),
                        "device",
                        this.DeviceName);
                    throw new ConfigurationErrorsException(msg);
                }
            }

            if (deserialize)
            {
                Deserialize();
            }
        }

        /// <summary>
        /// 默认的所属设备名称(有可能为 null)
        /// </summary>
        public String DeviceName { get; protected set; }
        /// <summary>
        /// 获取设备配置节点集合
        /// </summary>
        public virtual LocationElement[] LocationElements { get; protected set; }
        /// <summary>
        /// 设备配置节点类型
        /// </summary>
        public abstract LocationElement CreateLocationElement(XmlNode LocationNode);
        /// <summary>
        /// 默认的所属设备（如果指定了默认的所属设备名称）
        /// </summary>
        /// <returns></returns>
        public virtual DeviceElement GetDeviceElement()
        {
            return _deviceElement;
        }
        /// <summary>
        /// 获取当前位置集合节点包含的所有位置对象
        /// </summary>
        public virtual Location[] Locations
        {
            get
            {
                return LocationElements.Select(x => x.Location)
                    .ToArray();
            }
        }

        protected override void Deserialize()
        {
            List<LocationElement> locationElements = new List<LocationElement>();
            foreach (XmlNode item in this.Node.ChildNodes)
            {
                if (item.NodeType == XmlNodeType.Element)
                {
                    var locationElement = CreateLocationElement(item);
                    if (locationsDict.ContainsKey(locationElement.Location.ToConvertibleCode()))
                    {
                        throw new ConfigurationErrorsException(String.Format("位置 {0} 重复", locationElement.Location));
                    }

                    locationElements.Add(locationElement);
                    
                    locationsDict.Add(locationElement.Location.ToConvertibleCode(), locationElement.Location);
                }
                else
                {
                    WcsConfiguration._logger.Warn1("未处理的内容 " + item.OuterXml, this);
                }
            }

            this.LocationElements = this.LocationElements.Concat(locationElements).ToArray();

            locationsDict.Clear();
            locationsDict = null;
        }
    }
}
