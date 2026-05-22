using System;

namespace BOE.设备相关.AGV.GSAGV
{
    /// <summary>
    /// 子系统位置
    /// </summary>
    public class AGVSubSystemLocation : Wcs.Framework.Location
    {
        public Int32 Position_X { get; set; }
        public Int32 Position_Y { get; set; }

        public Int32 Level { get; set; }

        public UInt32 Position_Angle { get; set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="device">位置所属设备</param>
        /// <param name="userCode">设备编码</param>
        /// <param name="userCode">用户编码</param>
        /// <param name="stationNo">站号</param>
        public AGVSubSystemLocation(GSAGVDevice device, String deviceCode, String userCode, int stationNo, int position_x, int position_y, UInt32 position_angle, int level)
            : base(deviceCode, userCode, device)
        {
            this.StationNo = stationNo;
            this.Position_X = position_x;
            this.Position_Y = position_y;
            this.Position_Angle = position_angle;
            this.Level = level;
        }

        /// <summary>
        /// 站号
        /// </summary>
        public int StationNo { get; set; }
    }
}
