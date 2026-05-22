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
    public class LocationLogData : ReceivedDataLog
    {
        [Description("货位号")]
        public Int32 PosNo{get;set;}
        [Description("状态")]
        public LocationNetTransferObjectStatus Status { get; set; }
        public LocationLogData(Device device, LocationNetTransferObject receivedData)
            : base()
        {
            this.DeviceName = device.Name;
            this.PosNo = receivedData.PosNo;
            this.Status = receivedData.Status;
        }
    }
}
