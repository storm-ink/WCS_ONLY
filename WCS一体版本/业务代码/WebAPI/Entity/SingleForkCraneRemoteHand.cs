using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZHQXC
{
    public class SingleForkCraneRemoteHand
    {
        public string Device { get; set; }
        public HandTypes HandType { get; set; }

        public bool Result { get; set; }
        public string Message { get; set; }
    }

    public enum HandTypes
    { 
        Unknow,
        CancleTask,
        ResetAlarm,
        RemoteEmergency,
        CancleRemoteEmergency
    }
}
