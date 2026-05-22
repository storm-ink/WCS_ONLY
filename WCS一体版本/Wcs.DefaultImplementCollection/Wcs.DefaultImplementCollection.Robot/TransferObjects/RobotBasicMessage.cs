using System;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.Robot
{
    public class RobotBasicMessage
    {
        public RobotBasicMessage()
        { }
        public RobotBasicMessage(byte[] bytes)
        {
            string s = Encoding.ASCII.GetString(bytes.Skip(0).Take(2).ToArray()); 
            this.DeviceNO = UInt16.Parse(s);
            this.Mode = (RobotModes)byte.Parse(Encoding.ASCII.GetString(bytes.Skip(2).Take(1).ToArray()).Trim());
            this.Alarm = byte.Parse(Encoding.ASCII.GetString(bytes.Skip(3).Take(1).ToArray()).Trim());
            this.Origin = byte.Parse(Encoding.ASCII.GetString(bytes.Skip(4).Take(1).ToArray()).Trim());
            this.Catch = byte.Parse(Encoding.ASCII.GetString(bytes.Skip(5).Take(1).ToArray()).Trim());
            this.CountTotal = UInt16.Parse(Encoding.ASCII.GetString(bytes.Skip(6).Take(1).ToArray()).Trim());
        }
        /// <summary>
        /// 机械手编号
        /// </summary>
        public UInt16 DeviceNO { get; set; }
        /// <summary>
        /// 工作模式上报
        /// </summary>
        public RobotModes Mode { get; set; }
        /// <summary>
        /// 是否故障（1故障，0正常）
        /// </summary>
        public Byte Alarm { get; set; }
        /// <summary>
        /// 是否在原点
        /// </summary>
        public Byte Origin { get; set; }
       
        /// <summary>
        /// 机械手是否抓取箱子1:取到箱子；0:未取到箱子
        /// </summary>
        public Byte Catch { get; set; }
        
        /// <summary>
        /// 取货站box总数
        /// </summary>
        public UInt16 CountTotal { get; set; }
        
       
    }
}