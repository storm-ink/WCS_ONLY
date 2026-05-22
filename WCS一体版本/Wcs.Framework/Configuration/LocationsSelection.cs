using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 位置集合节点
    /// </summary>
    /// <example>
    /// <code lang="xml">
    /// <locations></locations>
    /// </code>
    /// <seealso cref="T:Wcs.Framework.Cfg.LocationElement"/>
    /// </example>
    public class LocationsSelection:ConfigurationElement
    {
        List<LocationElement> locationElements = new List<LocationElement>();
        /// <summary>
        /// 获取该位置集合节点关联的设备对象
        /// </summary>
        public DeviceElement DeviceElement { get; private set; }
        /// <summary>
        /// 获取该集合集合节点中所有的位置配置节点
        /// </summary>
        public LocationElement[] LocationElements
        {
            get
            {
                return locationElements.ToArray();
            }
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        /// <param name="deviceElement">设备配置节点</param>
        public LocationsSelection(XmlNode node,DeviceElement deviceElement)
            : base(node,false)
        {
            this.DeviceElement = deviceElement;
            this.Deserialize(node);
        }
        /// <summary>
        /// 执行解析
        /// </summary>
        /// <param name="node">配置节点</param>
        protected override void Deserialize(System.Xml.XmlNode node)
        {
            if (node == null) return;
            foreach (XmlNode item in node.SelectNodes("location"))
            {
                LocationElement locationElement = new LocationElement(item, DeviceElement);
                locationElements.Add(locationElement);
            }
        }
    }
}
