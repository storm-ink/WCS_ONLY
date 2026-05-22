using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    [DisplayName("扫码称重状态")]
    public class ScanAndWeighDataLog : ReceivedDataLog
    {
        [DisplayName("货位编号")]
        public virtual UInt16 PosNo1 { get; set; }
        [DisplayName("扫码状态")]
        public virtual Byte ScanningStatus1 { get; set; }
        [DisplayName("称重状态")]
        public virtual Byte WeighingStatus1 { get; set; }
        protected ScanAndWeighDataLog() : base()
        {
        }
        public ScanAndWeighDataLog(Device device, ScanningAndWeighingTransferObject receivedData)
           : this()
        {
            this.DeviceName = device.Name;
            this.PosNo1 = receivedData.PosNo;
            this.ScanningStatus1 = receivedData.ScanningStatus;
            this.WeighingStatus1 = receivedData.WeighingStatus;
        }

    }
}
