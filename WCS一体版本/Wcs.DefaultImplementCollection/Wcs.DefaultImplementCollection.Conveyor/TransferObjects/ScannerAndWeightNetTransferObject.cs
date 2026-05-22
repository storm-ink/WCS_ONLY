using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    [System.ComponentModel.DisplayName("扫码称重结果")]
    public class ScannerAndWeightNetTransferObject : NetTransferObject
    {
        [System.ComponentModel.DisplayName("货位号")]
        public UInt16 PosNo { get; set; }

        [System.ComponentModel.DisplayName("条码")]
        public string BarCode { get; set; }

        [System.ComponentModel.DisplayName("重量")]
        public string Weight { get; set; }

        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "PosNo":
                        return this.PosNo;
                    case "BarCode":
                        return this.BarCode;
                    case "Weight":
                        return this.Weight;
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
                    case "BarCode":
                        this.BarCode = value.ToString();
                        break;
                    case "Weight":
                        this.Weight = value.ToString();
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }

        public override ReceivedDataLog ToLogData(Device device)
        {
            return null;
        }
    }
}
