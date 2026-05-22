using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Wcs;
using Wcs.Framework;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Scanner
{
    public class ScanerDeviceCollection : ParticularDeviceCollection
    {
        public ScanerDeviceCollection(XmlNode node, WcsConfiguration wcsConfiguration)
            : base(node,wcsConfiguration)
        { 
            
        }

        public override DeviceElement CreateDeviceElement(XmlNode deviceNode)
        {
            return new ScanerDeviceElement(deviceNode, this.WcsConfiguration); 
        }
    }
}
