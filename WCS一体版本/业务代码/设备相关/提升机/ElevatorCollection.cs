using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Wcs.Framework.Cfg;

namespace BOE
{
    public class ElevatorDeviceCollection : ParticularDeviceCollection
    {

        public ElevatorDeviceCollection(XmlNode node, WcsConfiguration configuration) : base(node, configuration) { }
        
        public override DeviceElement CreateDeviceElement(System.Xml.XmlNode deviceNode)
        {
            return new ElevatorDeviceElement(deviceNode, this.WcsConfiguration);
        }
    }
}
