using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.Robot
{
    /// <summary>
    /// 机械手位置信息
    /// </summary>
    public class RobotPosition
    {
        /// <summary>
        /// robotX轴位置
        /// </summary>
        public UInt16 X_POSITION { get; set; }
        /// <summary>
        /// robotY轴位置
        /// </summary>
        public UInt16 Y_POSITION { get; set; }
        /// <summary>
        /// robotZ轴位置
        /// </summary>
        public UInt16 Z_POSITION { get; set; }
        /// <summary>
        /// robotA轴位置
        /// </summary>
        public Int32 A_POSITION { get; set; }
        /// <summary>
        /// robotB轴位置
        /// </summary>
        public Int32 B_POSITION { get; set; }
        /// <summary>
        /// robotC轴位置
        /// </summary>
        public Int32 C_POSITION { get; set; }

        public RobotPosition()
        { 
        
        }

        public RobotPosition(byte[] bytes)
        { 
            this.X_POSITION= BitConverter.ToUInt16(bytes.Take(2).ToArray(), 0);
            this.Y_POSITION = BitConverter.ToUInt16(bytes.Skip(2).Take(2).ToArray(), 0);
            this.Z_POSITION = BitConverter.ToUInt16(bytes.Skip(4).Take(2).ToArray(), 0);
            this.A_POSITION = BitConverter.ToInt32(bytes.Skip(6).Take(4).ToArray(), 0);
            this.B_POSITION = BitConverter.ToInt32(bytes.Skip(10).Take(4).ToArray(), 0);
            this.C_POSITION = BitConverter.ToInt32(bytes.Skip(14).Take(4).ToArray(), 0);
        }
    }
}
