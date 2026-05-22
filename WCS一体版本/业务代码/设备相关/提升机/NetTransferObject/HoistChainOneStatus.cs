using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BOE
{
    /// <summary>
    ///链条的状态
    /// </summary>
    public enum HoistChainStatus
    {
        [Description("无动作任务")]
        [EnumMember]
        Initialize = 0,
        // <summary>
        /// 左上
        /// </summary>
        [Description("左上")]
        [EnumMember]
        LeftPicking = 1,

        /// <summary>
        /// 左卸
        /// </summary>
        [Description("左卸")]
        [EnumMember]
        LeftUnloading = 2,

        /// <summary>
        /// 右上
        /// </summary>
        [Description("右上")]
        [EnumMember]
        RightPicking = 3,

        /// <summary>
        /// 右卸
        /// </summary>
        [Description("右卸")]
        [EnumMember]
        RightUnloading = 4,
    }
}
