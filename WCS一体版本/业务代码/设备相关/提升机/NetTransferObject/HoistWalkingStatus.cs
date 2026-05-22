using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BOE
{
    public enum HoistWalkingStatus
    {
        [Description("无动作任务")]
        [EnumMember]
        Initialize = 0,
        /// <summary>
        /// 前进
        /// </summary>
        [Description("上升")]
        [EnumMember]
        Forwarding = 1,

        /// <summary>
        /// 向后
        /// </summary>
        [Description("下降")]
        [EnumMember]
        Backing = 2,

        /// <summary>
        /// 停止
        /// </summary>
        [Description("停止")]
        [EnumMember]
        Stopping = 3,

    }
}
