using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    [System.ComponentModel.DisplayName("扫码称重状态")]
    [JsonObject]
    public class ScanningAndWeighingTransferObject : NetTransferObject
    {
        /// <summary>
        /// 货位号
        /// </summary>
        [DisplayName("货位编号")]
        public virtual UInt16 PosNo { get; set; }
        /// <summary>
        /// 扫码状态
        /// </summary>
        [DisplayName("扫码状态")]
        public virtual Byte ScanningStatus { get; set; }
        /// <summary>
        /// 称重状态
        /// </summary>
        [DisplayName("称重状态")]
        public virtual Byte WeighingStatus { get; set; }

        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "PosNo":
                        return this.PosNo;
                    case "ScanningStatus":
                        return this.ScanningStatus;
                    case "WeighingStatus":
                        return this.WeighingStatus;
                    default:
                        throw new NotSupportedException(String.Format("无法识别属性名 {0}", name));
                }
            }
            set
            {
                switch (name)
                {
                    case "PosNo":
                        this.PosNo = Convert.ToUInt16(value);
                        break;
                    case "ScanningStatus":
                        this.ScanningStatus = Convert.ToByte(value);
                        break;
                    case "WeighingStatus":
                        this.WeighingStatus = Convert.ToByte(value);
                        break;
                    default:
                        throw new NotSupportedException(String.Format("无法识别属性名 {0}", name));
                }
            }
        }
        public override string ToString()
        {
            return String.Format("货位编号#{0}，扫码状态{1}，称重状态{2}", this.PosNo, this.ScanningStatus, this.WeighingStatus);
        }

        public override ReceivedDataLog ToLogData(Device device)
        {
            return new ScanAndWeighDataLog(device, this);
        }

    }
}
