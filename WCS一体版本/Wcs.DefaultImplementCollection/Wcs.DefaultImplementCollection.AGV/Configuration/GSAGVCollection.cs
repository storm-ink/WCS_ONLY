using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.AGV
{
    public class SinevaAGVCollection : Wcs.Framework.Cfg.ParticularDeviceCollection
    {
        public SinevaAGVCollection(System.Xml.XmlNode node, WcsConfiguration configuration)
            : base(node, configuration)
        {
        }
        public override Wcs.Framework.Cfg.DeviceElement CreateDeviceElement(System.Xml.XmlNode deviceNode)
        {
            return new SinevaAGVElement(deviceNode, WcsConfiguration);
        }
    }
}
