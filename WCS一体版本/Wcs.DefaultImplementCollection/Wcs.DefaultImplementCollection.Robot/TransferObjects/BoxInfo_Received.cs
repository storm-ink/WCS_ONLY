using System;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.Robot
{
    /// <summary>
    /// Robot上报的BoxInfo
    /// </summary>
    public class BoxInfo_Received
    {
        public BoxInfo_Received()
        {
            BoxId = "";
            Level = 0;
            Position = 0;
            SerialNo = 0;
            State = 0;
        }

        public BoxInfo_Received(byte[] bytes)
        {
            BoxId = ASCIIEncoding.ASCII.GetString(bytes.Take(20).ToArray()).Trim();
            Level = BitConverter.ToUInt16(bytes.Skip(20).Take(2).ToArray(), 0);
            Position = bytes[22];
            SerialNo = bytes[23];
            State = (BoxStatus)BitConverter.ToUInt16(bytes.Skip(24).Take(2).ToArray(), 0);
        }

        /// <summary>
        /// 条码号（Ascii码值，兼容字母）
        /// </summary>
        public String BoxId { get; set; }
        /// <summary>
        /// 当前Box所在层
        /// </summary>
        public UInt16 Level { get; set; }
        /// <summary>
        /// 在托盘上的编号
        /// </summary>
        public Byte Position { get; set; }
        /// <summary>
        /// 编号（boxId在本次拆垛任务中的编号）从1开始 最大值Count字段
        /// </summary>
        public Byte SerialNo { get; set; }
        /// <summary>
        /// Box状态（0-初始化，1-未完成，2-自动完成，3-手动完成，4-异常，5-执行中，6-WCS完成）
        /// </summary>
        public BoxStatus State { get; set; }
    }
}