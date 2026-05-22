using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Scanner
{
    public class ScanerDeviceTelexTransferObjectLogData : Wcs.Framework.ReceivedDataLog
    {
        public virtual String Barcode { get; set; }
        protected ScanerDeviceTelexTransferObjectLogData()
            : base()
        {
        }
        public ScanerDeviceTelexTransferObjectLogData(Device device, ScanerDeviceTelexTransferObject receivedData)
            : this()
        {
            this.DeviceName = device.Name;
            this.Barcode = receivedData.ToString();
        }
    }
}
