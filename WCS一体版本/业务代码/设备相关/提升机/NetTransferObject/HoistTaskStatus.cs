using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace BOE
{
    public enum HoistTaskStatus
    {
        [Description("无任务")]
        [EnumMember]
        Initialize = 0,
        // <summary>
        /// 接收任务正常
        /// </summary>
        [Description("接收任务正常")]
        [EnumMember]
        ReceiveTaskNormal = 1,

        /// <summary>
        /// 接收任务错误
        /// </summary>
        [Description("接收任务错误")]
        [EnumMember]
        ReceiveTaskError = 2,

        /// <summary>
        /// 任务执行中
        /// </summary>
        [Description("任务执行中")]
        [EnumMember]
        ReceiveTaskExecuting = 3,
    }
}
