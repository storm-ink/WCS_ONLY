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
    /// 位置水平方向的位置状态（左右位）
    /// </summary>
    [DataContract]
    public enum ForkHorizontalPosition
    {
        /// <summary>未知</summary>
        [Description("未知")]
        [EnumMember]
        Unknow = 0,
        /// <summary>中位</summary>
        [Description("中位")]
        [EnumMember]
        Center = 1,
        /// <summary>左位</summary>
        [Description("左")]
        [EnumMember]
        Left=2,
        /// <summary>左极限</summary>
        [Description("左极限")]
        [EnumMember]
        LeftLimit=3,
        /// <summary>右位</summary>
        [Description("右")]
        [EnumMember]
        Right=4,
        /// <summary>右极限</summary>
        [Description("右极限")]
        [EnumMember]
        RightLimit=5,    
        /// <summary>左（外）</summary>
        [Description("左（外）")]
        [EnumMember]
        E6,
        /// <summary>左（外）极限</summary>
        [Description("左（外）极限")]
        [EnumMember]
        E7,
        /// <summary>右（外）</summary>
        [Description("右（外）")]
        [EnumMember]
        E8,
        /// <summary>右（外）极限</summary>
        [Description("右（外）极限")]
        [EnumMember]
        E9
    }
}
