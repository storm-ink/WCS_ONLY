using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 回原点命令
    /// </summary>
    public sealed class BackToTheOriginCommand : TelexTransferObject
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
            get;set;
        }= "ZH";
        public  Int32 Task_Id
        {
            get { return 1; }
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
            return device.LastStatus.State == CraneStatus.BackToTheOrigin || device.LastStatus.Event == CraneEvent.BackToTheOrigin;
        }
    }
}
