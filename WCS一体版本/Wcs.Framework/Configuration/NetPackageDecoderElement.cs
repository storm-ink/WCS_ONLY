using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework.Devices;

namespace Wcs.Framework.Cfg
{
    public class NetPackageDecoderElement:ConfigurationElement
    {
        public NetPackageDecoder NetPackageDecoder { get; private set; }
        //<netPackageDecoder type="Wcs.Framework.DefaultConveyorNetPackageDecoder, Wcs.Framework" name="netPackageDecoder1">
        public NetPackageDecoderElement(XmlNode node)
            : base(node)
        {

        }
        protected override void Deserialize(System.Xml.XmlNode node)
        {
            if (node.Attributes["type"] == null)
            {
                throw new Exception("未指定 netPackageDecoder 节点的 type 属性");
            }

            if (node.Attributes["name"] == null)
            {
                throw new Exception("未指定 netPackageDecoder 节点的 name 属性");
            }

            string typeName = node.Attributes["type"].Value;
            string name = node.Attributes["name"].Value;
            NetPackageDecoder decoder = CreateInstance<NetPackageDecoder>(typeName, node);
            decoder.Name = name;

            this.NetPackageDecoder = decoder;
        }
    }
}
