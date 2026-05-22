using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    public class CancleCommand : TelexTransferObject, Wcs.Framework.IDeviceCommandAdjudicator
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
            //任务号
            result.AddRange(SendconvertNumberToBytes(Task_Id.GetType().Name, Task_Id).Reverse());
            //报警码

            return result.ToArray();
        }

        public override string TypeFlag
        {
            get; set;
        } = "QX";
        public Int32 Task_Id
        {
            get { return 999999901; }
        }
        public override int Length
        {
            get { return 6; }
        }

        public override object this[string name]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public bool SendSuccess(Framework.TaskableDevice taskableDevice, Framework.DeviceCommand command)
        {
            var device = (CraneDevice)taskableDevice;
            return device.LastStatus.TaskId == "00000000" || device.LastStatus.TaskId == this.Task_Id.ToString("00000000");
        }
    }
}