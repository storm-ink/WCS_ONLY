using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Wcs.Framework
{
    /// <summary>
    /// 请求状态
    /// </summary>
    public enum RequestStatus
    {
        /// <summary>
        /// 新请求
        /// </summary>
        [Description("新请求")]
        New,
        /// <summary>
        /// 已处理的
        /// </summary>
        [Description("已处理")]
        Processed,
        /// <summary>
        /// 被取消的
        /// </summary>
        [Description("已取消")]
        Cancelled
    }
}
