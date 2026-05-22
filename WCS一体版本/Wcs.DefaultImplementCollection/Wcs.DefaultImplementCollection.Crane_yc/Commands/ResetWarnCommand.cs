using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 表示一个报警复位命令
    /// </summary>
    public class ResetWarnCommand:TelexTransferObject
    {
        public override byte[] ToTelex()
        {
            List<byte> result = new List<byte>();
            //报文头
            result.AddRange(Prefix);
            //数据长度
            result.AddRange(SendconvertNumberToBytes(Length.GetType().Name, Length).Reverse());
            //功能码
            result.AddRange(System.Text.Encoding.Default.GetBytes(TypeFlag));
           
            return result.ToArray();
        }

        public override string TypeFlag
        {
            get;set;
        }= "FW";

        public override int Length
        {
            get { return 2; }
        }

        public override object this[string name]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public bool SendSuccess(Framework.TaskableDevice taskableDevice, Framework.DeviceCommand command)
        {
            var device = (CraneDevice)taskableDevice;
            return device.LastStatus.State != CraneStatus.AlarmAndShutdown
                && device.LastStatus.Event != CraneEvent.EmergencyStop;
        }
    }
}
