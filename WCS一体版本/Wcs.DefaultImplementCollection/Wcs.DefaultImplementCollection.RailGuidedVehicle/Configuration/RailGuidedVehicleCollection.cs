using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    public class RailGuidedVehicleCollection : ParticularDeviceCollection
    {
        public AlarmCollection AlarmCollection { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public RailGuidedVehicleCollection(XmlNode node, WcsConfiguration configuration)
            : base(node,configuration)
        {
            string alarmsFile = GetAttributeOrDefault<String>("alarms");
            XmlNode alarmsNode = node.OwnerDocument.CreateElement("alarms");
            var xmlFileAttr = node.OwnerDocument.CreateAttribute(ConfigurationElement.XmlFileAttributeName);
            xmlFileAttr.Value = alarmsFile;
            alarmsNode.Attributes.Append(xmlFileAttr);

            alarmsNode = GetNodeOrLoadXmlFile(alarmsNode);
            AlarmCollection = new AlarmCollection(alarmsNode, configuration);

        }
        public override DeviceElement CreateDeviceElement(System.Xml.XmlNode deviceNode)
        {
            return new RailGuidedVehicleElement(deviceNode, this.WcsConfiguration);
        }
    }
}
