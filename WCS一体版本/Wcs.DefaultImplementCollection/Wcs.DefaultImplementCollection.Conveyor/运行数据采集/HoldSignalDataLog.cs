using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    [DisplayName("占位信号")]
    public class HoldSignalDataLog : ReceivedDataLog
    {
        [DisplayName("货位号")]
        public virtual Int32 PosNo { get; set; }
        [DisplayName("握手")]
        public virtual HoldSignalNetTransferObjectHandShake HandShake { get; set; }
      
        [DisplayName("业务数据")]
        public virtual Int32 IO_Data { get; set; }
        [DisplayName("随机数")]
        public UInt16 Data_ID { get; set; }
        protected HoldSignalDataLog():base()
        {
        }
        public HoldSignalDataLog(Device device, HoldSignalNetTransferObject receivedData)
            : this()
        {
            this.DeviceName = device.Name;
            this.PosNo=receivedData.PosNo;
            this.HandShake=receivedData.HandShake;
            this.IO_Data=receivedData.IO_Data;
            this.Data_ID = receivedData.Data_ID;
        }

       
        public HoldSignalDataLog(Device device, SimpleHoldSignalNetTransferObject receivedData)
          : this()
        {
            this.DeviceName = device.Name;
            this.PosNo = receivedData.PosNo;
            this.HandShake = receivedData.HandShake;
        }
    }
}
