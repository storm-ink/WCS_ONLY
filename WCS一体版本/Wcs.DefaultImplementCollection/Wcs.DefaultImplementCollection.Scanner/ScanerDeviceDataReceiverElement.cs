using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Wcs.Framework;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Scanner
{
    public class ScanerDeviceDataReceiverElement : DataReceiverElement
    {
        public ScanerDeviceDataReceiverElement(XmlNode node, WcsConfiguration wcsConfiguration)
            : base(node,wcsConfiguration)
        { 
        
        }

        public override IDataReceiver CreateDataReceiver(string deviceName)
        {
            return new ScanerDeviceDataReceiver(this.Name);
        }
    }
}
