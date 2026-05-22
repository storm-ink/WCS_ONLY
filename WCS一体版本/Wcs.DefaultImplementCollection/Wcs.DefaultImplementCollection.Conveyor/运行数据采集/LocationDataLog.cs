using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Wcs.Framework;
namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 输送线货位状态对象.
    /// </summary>
    [Description("输送线货位状态")]
    public class LocationDataLog : ReceivedDataLog
    {
        [Description("货位号")]
        public virtual Int32 PosNo { get; set; }
        [Description("状态")]
        public virtual LocationNetTransferObjectStatus Status { get; set; }
        protected LocationDataLog():base()
        {
        }
        public LocationDataLog(Device device, LocationNetTransferObject receivedData)
            : this()
        {
            this.DeviceName = device.Name;
            this.PosNo = receivedData.PosNo;
            this.Status = receivedData.Status;
        }
      
    }
}
