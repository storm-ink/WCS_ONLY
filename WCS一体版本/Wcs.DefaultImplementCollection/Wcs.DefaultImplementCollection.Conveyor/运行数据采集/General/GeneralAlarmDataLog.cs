using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 报警状态.
    /// </summary>
    [DisplayName("报警状态")]
    public class GeneralAlarmDataLog : ReceivedDataLog
    {
        [DisplayName("货位号")]
        public virtual Int32 PosNo { get; set; }
        [DisplayName("报警列表")]
        public byte[] Alarms { get; set; }
        protected GeneralAlarmDataLog()
        {
        }
        public GeneralAlarmDataLog(Device device, GeneralAlarmNetTransferObject receivedData)
            : base()
        {
            this.DeviceName = device.Name;
            this.PosNo = receivedData.PosNo;
            this.Alarms = receivedData.Alarms;
        }
    }
}
