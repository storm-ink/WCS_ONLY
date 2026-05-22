using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Robot
{
    /// <summary>
    /// 机械手任务命令
    /// </summary>
    public class RobotTaskCommand : DeviceCommand, Wcs.Framework.IDeviceCommandAdjudicator
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public RobotTaskCommand()
        {

            Raw = "00000000000000000000";
        }

       
        /// <summary>
        /// 任务号，wcs和机械手交互唯一标志
        /// </summary>
        public UInt32 TaskId { get; set; }
        /// <summary>
        /// 握手
        /// </summary>
        public HandShake HandShake { get; set; }
       
        /// <summary>
        /// 取货站台
        /// </summary>
        public UInt16 Pick { get; set; }
        /// <summary>
        /// 放货站台
        /// </summary>
        public UInt16 Put { get; set; }
       
        public UInt16 Count { get; set; }
        /// <summary>
        /// 20字节 拆垛母托盘条码号（Ascii码值，兼容字母）
        /// </summary>
        public String Raw { get; set; } = " ";
       
        public override object this[string name] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        //public byte[] ToTelex()
        //{
        //    List<byte> bytes = new List<byte>();
        //    var prefix = new byte[] { 0x5B };
        //    var suffix = new byte[] { 0x5D};
        //    bytes.AddRange(prefix);
        //    bytes.AddRange(BitConverter.GetBytes(this.TaskId));           
        //    bytes.AddRange(BitConverter.GetBytes((ushort)this.HandShake));          
        //    bytes.AddRange(BitConverter.GetBytes(this.Pick));             
        //    bytes.AddRange(BitConverter.GetBytes(this.Put));                   
        //    bytes.AddRange(BitConverter.GetBytes(this.Count));            
        //    bytes.AddRange(suffix);                                                            
        //    return bytes.ToArray();
        //}
         
        public byte[] ToTelex()
        {
            // 1. 各字段按宽度转字符串
            string taskId = this.TaskId.ToString("D10");           // 10
            string handShake = ((ushort)this.HandShake).ToString("D1");       // 1
            string pick = this.Pick.ToString("D2");              // 2
            string put = this.Put.ToString("D2");               // 2
            string count = this.Count.ToString("D1");             // 1
            string raw = this.Raw.PadRight(20).Substring(0, 20); // 20
            // 2. 用 ; 拼接 → 41 字节 ASCII
            string payload = string.Join(";", taskId, handShake, pick, put, count, raw);
            List<byte> bytes = new List<byte>();
            var prefix = new byte[] { 0x5B };
            var suffix = new byte[] { 0x5D};
            bytes.AddRange(prefix);
            bytes.AddRange(Encoding.ASCII.GetBytes(payload));
            bytes.AddRange(suffix);
            return bytes.ToArray();
        }



        public bool SendSuccess(TaskableDevice taskableDevice, DeviceCommand command)
        {
            var robot = (RobotDevice)taskableDevice;
            if (robot.LastState == null)
                return false;

            switch (this.HandShake)
            {
                case HandShake.New:
                    return robot.LastState.RobotTask.TaskId == this.TaskId;
                case HandShake.Delete:
                case HandShake.Clear:
                    return robot.LastState.RobotTask.TaskId == 0;
                case HandShake.Suspend:
                    return robot.LastState.RobotTask.TaskId == this.TaskId && robot.LastState.RobotTask.HandShake == HandShake.Suspend;
                case HandShake.Resum:
                    return robot.LastState.RobotTask.TaskId == this.TaskId && robot.LastState.RobotTask.HandShake == HandShake.Resum;
                case HandShake.Readed:
                    return robot.LastState.RobotTask.TaskId == this.TaskId;
                case HandShake.Unknown:
                    return robot.LastState.RobotTask.TaskId == this.TaskId;
                default:
                    return true;
            }                
        }
    }
}