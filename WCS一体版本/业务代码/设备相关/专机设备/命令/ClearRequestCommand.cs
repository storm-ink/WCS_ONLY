using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Wcs.DefaultImplementCollection.Conveyor;

using Wcs.Framework;

namespace ZHQXC
{
  
    public class ClearRequestCommand : DeviceCommand, IDeviceCommandAdjudicator
    {


        public ClearRequestCommand() { }

        public ushort PosNo { get; set; }
        public RequestHandShakes HandShake { get; set; } = RequestHandShakes.ApplyForDelete;
        public ushort DataID { get; set; } = (UInt16)new Random().Next(1, UInt16.MaxValue);
     

        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "PosNo":
                        return this.PosNo;
                    case "HandShake":
                        return this.HandShake;
                    case "DataID":
                        return this.DataID;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的取值操作", this.GetType(), name));
                }
            }
            set
            {
                switch (name)
                {
                    case "PosNo":
                        this.PosNo = Convert.ToUInt16(value);
                        break;
                    case "HandShake":
                        this.HandShake = (RequestHandShakes)Convert.ToInt16(value);
                        break;
                    case "DataID":
                        this.DataID = Convert.ToUInt16(value);
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }

        public bool SendSuccess(TaskableDevice taskableDevice, DeviceCommand command)
        {
            return true;
        }
    }

}



