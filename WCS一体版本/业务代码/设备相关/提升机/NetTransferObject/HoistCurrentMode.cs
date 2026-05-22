using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BOE
{
    public enum HoistCurrentMode
    {
        [Description("初始化")]
        [EnumMember]
         Initialize = 0,
        /// <summary>
        /// 全自动任务
        /// </summary>
        [Description("全自动任务")]
        [EnumMember]
        AutomaticTask = 1,

        /// <summary>半自动任务</summary>
        [Description("半自动任务")]
        [EnumMember]
        SemiAutomatic = 2,

        /// <summary>回原点</summary>
        [Description("回原点")]
        [EnumMember]
        isOriginPoint = 3,


        /// <summary>手动</summary>
        [Description("手动")]
        [EnumMember]
        IsManual = 4,

    }
}
