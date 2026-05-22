using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.DefaultImplementCollection.Scanner
{
    public class BarcodeReceivedArgs : EventArgs
    {
        public String Barcode { get; set; }

        public BarcodeReceivedArgs(String barcode)
            : base()
        {
            this.Barcode = barcode;
        }
    }
}
