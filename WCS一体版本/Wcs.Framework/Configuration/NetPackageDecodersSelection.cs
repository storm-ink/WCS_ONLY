using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework.Devices;

namespace Wcs.Framework.Cfg
{
    public class NetPackageDecodersSelection:ConfigurationElement
    {
        public static bool Initialized { get; private set; }
        List<NetPackageDecoderElement> netPackageDecoderElements = new List<NetPackageDecoderElement>();
        public NetPackageDecodersSelection(XmlNode node)
            : base(node)
        {
            
        }
        protected override void Deserialize(System.Xml.XmlNode node)
        {
            foreach (XmlNode item in node.SelectNodes("netPackageDecoder"))
            {
                var netPackageDecoderElement = new NetPackageDecoderElement(item);
                if (netPackageDecoderElements.Any(x => x.NetPackageDecoder.Name.Equals(netPackageDecoderElement.NetPackageDecoder.Name, StringComparison.CurrentCultureIgnoreCase)))
                {
                    throw new Exception(string.Format("已存在名为 {0} 的 netPackageDecoder", netPackageDecoderElement.NetPackageDecoder.Name));
                }

                netPackageDecoderElements.Add(netPackageDecoderElement);
            }

            Initialized = true;
        }

        public NetPackageDecoderElement[] NetPackageDecoderElements
        {
            get
            {
                return netPackageDecoderElements.ToArray();
            }
        }
    }
}
