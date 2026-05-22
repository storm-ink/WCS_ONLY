using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    [DisplayName("货位当前任务")]
    public class LocationTaskDataLog : ReceivedDataLog
    {
        [DisplayName("货位号")]
        public virtual Int32 PosNo { get; set; }
        [DisplayName("任务号")]
        public virtual Int32 TaskNo { get; set; }
       
        [DisplayName("托盘号")]
        public virtual Int32 TU_ID { get; set; }

        [DisplayName("分路号")]
        public virtual Int32 NextPosNo { get; set; }

        [DisplayName("承载")]
        public virtual Int32 Load { get; set; }
        public virtual Boolean Str_Rcv_X { get; set; }
        public virtual Boolean Fnh_Rcv_X { get; set; }
        public virtual Boolean Rqs_Snt { get; set; }
        public virtual Boolean Rcv_Rdy { get; set; }
        public virtual Boolean Str_Rcv_Y { get; set; }
        public virtual Boolean Fnh_Rcv_Y { get; set; }
        protected LocationTaskDataLog():base()
        {
        }
        public LocationTaskDataLog(Device device, LocationTaskNetTransferObject receivedData)
            : this()
        {
            this.DeviceName = device.Name; 
            this.PosNo = receivedData.PosNo;
            this.TaskNo = Convert.ToInt32(receivedData.TaskNo);
            this.TU_ID = receivedData.TUID ;
            this.Str_Rcv_X = receivedData.Str_Rcv_X;
            this.Fnh_Rcv_X = receivedData.Fnh_Rcv_X;
            this.Rqs_Snt = receivedData.Rqs_Snt;
            this.Rcv_Rdy = receivedData.Rcv_Rdy;
            this.Str_Rcv_Y = receivedData.Str_Rcv_Y;
            this.Fnh_Rcv_Y = receivedData.Fnh_Rcv_Y;
            this.NextPosNo = receivedData.NextPosNo;
            this.Load = receivedData.Load;
        }
      
    }
}
