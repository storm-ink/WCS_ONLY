using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Scanner
{
    public class ScanerDeviceNetPacket : NetPacket
    {
        public ScanerDeviceNetPacket(byte[] packageBytes)
        {
            this.Data = packageBytes;
        }

        public override byte[] ComputerChecksum(byte[] datapart)
        {
            throw new NotImplementedException();
        }

        public override NetPacket CreateNetPackage(byte[] dataPart)
        {
            return new ScanerDeviceNetPacket(dataPart); 
        }
    }
}
