using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace ZHQXC
{
    public class ReportPalletDataObject : NetTransferObject
    {
        public UInt16 PosNo { get; set; }
        public UInt16 HandShake { get; set; }
        public String BatchNumber { get; set; }
        public String BatchNumberOld { get; set; }


        public UInt16 Over { get; set; }
        public UInt16 Total { get; set; }

        public UInt16 WCSReplayOver { get; set; }
        public UInt16 Mix { get; set; }
        public String Lot1 { get; set; }
        public UInt16 Qty1 { get; set; }
        public String Lot2 { get; set; }
        public UInt16 Qty2 { get; set; }
        public UInt16 Room { get; set; }
        public UInt16 Floot { get; set; }
        public UInt16 RequestID { get; set; }

        public UInt16 WCSReplayRequestId { get; set; }
        public UInt16 WMSErrorCode { get; set; }
 

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
                    case "BatchNumber":
                        return this.BatchNumber;
                    case "BatchNumberOld":
                        return this.BatchNumberOld;
                    case "Over":
                        return this.Over;
                    case "Total":
                        return this.Total;
                    case "WCSReplayOver":
                        return this.WCSReplayOver;
                    case "Mix":
                        return this.Mix;
                    case "Lot1":
                        return this.Lot1;
                    case "Qty1":
                        return this.Qty1;
                    case "Lot2":
                        return this.Lot2;
                    case "Qty2":
                        return this.Qty2;
                    case "Room":
                        return this.Room;
                    case "Floot":
                        return this.Floot;
                    case "RequestID":
                        return this.RequestID;
                    case "WCSReplayRequestId":
                        return this.WCSReplayRequestId;
                    case "WMSErrorCode":
                        return this.WMSErrorCode;
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
                        this.HandShake = Convert.ToUInt16(value);
                        break;
                    case "BatchNumber":
                        this.BatchNumber = value.ToString().Trim();
                        break;
                    case "BatchNumberOld":
                        this.BatchNumberOld = value.ToString().Trim();
                        break;
                    case "Over":
                        this.Over = Convert.ToUInt16(value);
                        break;
                    case "Total":
                        this.Total = Convert.ToUInt16(value);
                        break;
                    case "WCSReplayOver":
                        this.WCSReplayOver = Convert.ToUInt16(value);
                        break;
                    case "Mix":
                        this.Mix = Convert.ToUInt16(value);
                        break;
                    case "Lot1":
                        this.Lot1 = value.ToString().Trim();
                        break;
                    case "Qty1":
                        this.Qty1 = Convert.ToUInt16(value);
                        break;
                    case "Lot2":
                        this.Lot2 = value.ToString().Trim();
                        break;
                    case "Qty2":
                        this.Qty2 = Convert.ToUInt16(value);
                        break;
                    case "Room":
                        this.Room = Convert.ToUInt16(value);
                        break;
                    case "Floot":
                        this.Floot = Convert.ToUInt16(value);
                        break;
                    case "RequestID":
                        this.RequestID = Convert.ToUInt16(value);
                        break;
                    case "WCSReplayRequestId":
                        this.WCSReplayRequestId = Convert.ToUInt16(value);
                        break;
                    case "WMSErrorCode":
                        this.WMSErrorCode = Convert.ToUInt16(value);
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }


}
