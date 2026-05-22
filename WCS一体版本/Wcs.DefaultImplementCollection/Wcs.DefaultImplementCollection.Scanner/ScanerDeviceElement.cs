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
    public class ScanerDeviceElement : TcpProtocolDeviceElement
    {
        public ScanerDeviceElement(XmlNode node, WcsConfiguration wcsConfiguration)
            : base (node,wcsConfiguration)
        { 
        
        }

        protected override Device CreateDevice()
        {
            String bindingLocation = GetAttributeOrDefault<String>("bindingLocation");
            return new ScanerDevice(this.Name, this.No, this.ReceiveTimeout, this.ConnectTimeout, this.SendTimeout, this.IPEndPoint, this.DataReceiver, bindingLocation);
        }

    }
}
