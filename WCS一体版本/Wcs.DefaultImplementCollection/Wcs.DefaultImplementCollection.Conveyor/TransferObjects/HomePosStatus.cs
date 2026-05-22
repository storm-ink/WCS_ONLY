using System.ComponentModel;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 活动平台位置
    /// </summary>
    [Description("活动平台Home点位置")]
    public enum HomePosStatus
    {
        /// <summary>
        /// 未知位置
        /// </summary>
        [Description("未知位置")]
        UnKnow =0,
        /// <summary>
        /// 左侧/低位
        /// </summary>
        [Description("左侧/低位")]
        LeftDown =1,
        /// <summary>
        /// 右侧/高位
        /// </summary>
        [Description("右侧/高位")]
        RightUp =2
    }
}