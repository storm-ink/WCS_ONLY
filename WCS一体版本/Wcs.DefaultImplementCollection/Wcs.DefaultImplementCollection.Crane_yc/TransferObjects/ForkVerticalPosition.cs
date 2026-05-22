using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 位置垂直方向的位置状态（高低位）
    /// </summary>
    [DataContract]
    public enum ForkVerticalPosition
    {
        /// <summary>中位</summary>
        [Description("中位")]
        [EnumMember]
        Middle = 0,

        /// <summary>高位</summary>
        [Description("高位")]
        [EnumMember]
        Top,

        /// <summary>低位</summary>
        [Description("低位")]
        [EnumMember]
        Bottom,

        /// <summary>未知</summary>
        [Description("未知")]
        [EnumMember]
        Unknow,
    }
}
