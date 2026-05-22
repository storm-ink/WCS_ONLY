using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.Robot
{
    /// <summary>
    /// 机械手站点数据
    /// </summary>
    public class RobotStationState
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public RobotStationState()
        {
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bytes"></param>
        public RobotStationState(byte[] bytes)
        {
            this.Area = BitConverter.ToUInt16(bytes.Take(2).ToArray(), 0);
            this.Area_Cmd_No = BitConverter.ToUInt16(bytes.Skip(2).Take(2).ToArray(), 0);
            this.Area_IsOK = BitConverter.ToUInt16(bytes.Skip(4).Take(2).ToArray(), 0);
           
        }
        /// <summary>
        /// 站点编号
        /// </summary>
        public UInt16 Area { get; set; }
        /// <summary>
        /// 站点类型（0未知，1码垛，2拆垛）
        /// </summary>
        public UInt16 Area_Cmd_No { get; set; }
        /// <summary>
        /// 区域是否可用1：锁定；2：未锁定
        /// </summary>
        public UInt16 Area_IsOK { get; set; }
      
    }
}
