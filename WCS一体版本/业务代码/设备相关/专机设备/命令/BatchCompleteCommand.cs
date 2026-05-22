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

    public class BatchCompleteCommand : DeviceCommand, IDeviceCommandAdjudicator
    {
       

        public BatchCompleteCommand() { }
        public UInt16 PosNo { get; set; }
        public UInt16 WCSReplayOver { get; set; }
        public UInt16 DataID { get; set; } = (UInt16)new Random().Next(1, UInt16.MaxValue);

        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "PosNo":
                        return this.PosNo;
                    case "WCSReplayOver":
                        return this.WCSReplayOver;
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
                    case "WCSReplayOver":
                        this.WCSReplayOver = Convert.ToUInt16(value);
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
            var taskBlocks = conveyorDevice.ReadStatus<ReportPalletDataObject>();
            if (taskBlocks == null || taskBlocks.Length == 0)
                return false;
            else if (taskBlocks.Any(x => x.PosNo == this.PosNo && x.WCSReplayOver == this.WCSReplayOver) )
                return true;
            else
                return false;
        }
    }
}
