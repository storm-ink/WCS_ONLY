using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 急停命令
    /// </summary>
    internal sealed class EmergencyStopCommand:TelexTransferObject,Wcs.Framework.IDeviceCommandAdjudicator
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
            get; set;
        } = "JT";

        public override int Length
        {
            get { return 2; }
        }

        public override object this[string name]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool SendSuccess(Framework.TaskableDevice taskableDevice, Framework.DeviceCommand command)
        {
            CraneDevice craneDevice = (CraneDevice)taskableDevice;
            return craneDevice.LastStatus != null
                && (craneDevice.LastStatus.Event == CraneEvent.EmergencyStop || craneDevice.LastStatus.State == CraneStatus.AlarmAndShutdown);
        }
    }
}
