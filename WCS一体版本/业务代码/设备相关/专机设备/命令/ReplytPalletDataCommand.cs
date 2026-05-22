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

    public class ReplytPalletDataCommand : DeviceCommand, IDeviceCommandAdjudicator
    {
        public UInt16 PosNo { get; set; }
        public UInt16 WCSReplayRequestId { get; set; }
        public UInt16 WMSErrorCode { get; set; }
        public UInt16 DataID { get; set; } = (UInt16)new Random().Next(1, UInt16.MaxValue);

        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "PosNo":
                        return this.PosNo;
                    case "WCSReplayRequestId":
                        return this.WCSReplayRequestId;
                    case "WMSErrorCode":
                        return this.WMSErrorCode;
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
                    case "WCSReplayRequestId":
                        this.WCSReplayRequestId = Convert.ToUInt16(value);
                        break;
                    case "WMSErrorCode":
                        this.WMSErrorCode = Convert.ToUInt16(value);
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
            if (taskBlocks.Any(x => x.PosNo == this.PosNo && x.WCSReplayRequestId == this.WCSReplayRequestId))
                return true;
            else
                return false;
        }
    }
}
