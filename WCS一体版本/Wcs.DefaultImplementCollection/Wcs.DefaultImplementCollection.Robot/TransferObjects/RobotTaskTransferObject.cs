using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Robot
{
    /// <summary>
    /// Robot上报的Task信息
    /// </summary>
    public class RobotTaskTransferObject
    {
        public RobotTaskTransferObject()
        {
           // BoxInfo = new BoxInfo_Received[36];
        }
        public RobotTaskTransferObject(byte[] bytes)
        {
            string s = Encoding.ASCII.GetString(bytes.Skip(0).Take(10).ToArray()).Trim();
            this.TaskId = UInt32.Parse(s);
            //this.TaskId = UInt32.Parse(Encoding.ASCII.GetString(bytes.Skip(0).Take(20).ToArray()).Trim());
            this.HandShake = (HandShake)byte.Parse(Encoding.ASCII.GetString(bytes.Skip(10).Take(1).ToArray()).Trim());
            this.Pick = UInt16.Parse(Encoding.ASCII.GetString(bytes.Skip(11).Take(2).ToArray()).Trim());
            this.Put = UInt16.Parse(Encoding.ASCII.GetString(bytes.Skip(13).Take(2).ToArray()).Trim());
            this.Count = UInt16.Parse(Encoding.ASCII.GetString(bytes.Skip(15).Take(1).ToArray()).Trim());
            this.Raw = Encoding.ASCII.GetString(bytes.Skip(16).Take(20).ToArray()).Trim();
            //this.TaskId = BitConverter.ToUInt32(bytes.Take(4).ToArray(), 0);
            //this.HandShake = (HandShake)BitConverter.ToUInt16(bytes.Skip(4).Take(2).ToArray(), 0);
            //this.Pick = BitConverter.ToUInt16(bytes.Skip(6).Take(2).ToArray(), 0);
            //this.Put = BitConverter.ToUInt16(bytes.Skip(8).Take(2).ToArray(), 0);
            //this.Count = BitConverter.ToUInt16(bytes.Skip(24).Take(2).ToArray(), 0);
            //this.Raw = ASCIIEncoding.ASCII.GetString(bytes.Skip(26).Take(20).ToArray()).Trim();
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
        /// <summary>
        /// Box尺寸-长度
        /// </summary>
      
        public UInt16 Count { get; set; }
        /// <summary>
        /// 20字节 拆垛母托盘条码号（Ascii码值，兼容字母）
        /// </summary>
        public String Raw { get; set; }
       


    }
}