using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs.DefaultImplementCollection.Conveyor;

using Wcs.Framework;

namespace ZHQXC
{

    public class BoxInfomationCommand : DeviceCommand, IDeviceCommandAdjudicator
    {


        public BoxInfomationCommand() { }
        public UInt16 PosNo { get; set; }
        public UInt16 Room1Floot { get; set; }
        public UInt16 Room2Floot { get; set; }
        public UInt16 Room3Floot { get; set; }
        public UInt16 Room4Floot { get; set; }
        public string Box { get; set; }
        public UInt16 DataID { get; set; } = (UInt16)new Random().Next(1, UInt16.MaxValue);
        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "PosNo":
                        return this.PosNo;
                   
                    case "Room1Floot":
                        return this.Room1Floot;
                    case "Room2Floot":
                        return this.Room2Floot;
                    case "Room3Floot":
                        return this.Room3Floot;
                    case "Room4Floot":
                        return this.Room4Floot;
                    case "Box":
                        return this.Box;
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
                  
                    case "Room1Floot":
                        this.Room1Floot = Convert.ToUInt16(value);
                        break;
                    case "Room2Floot":
                        this.Room2Floot = Convert.ToUInt16(value);
                        break;
                    case "Room3Floot":
                        this.Room3Floot = Convert.ToUInt16(value);
                        break;
                    case "Room4Floot":
                        this.Room4Floot = Convert.ToUInt16(value);
                        break;
                    case "Box":
                        this.Box = value.ToString().Trim();
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
            ConveyorDevice conveyorDevice = (ConveyorDevice)taskableDevice;
            var taskBlocks = conveyorDevice.ReadStatus<BoxInfomationDataObject>();
            if (taskBlocks.Any(x => x.PosNo == this.PosNo ))
                return true;
            else
                return false;
        }
    }
}
