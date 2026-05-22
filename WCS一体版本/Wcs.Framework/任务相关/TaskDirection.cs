using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Wcs.Framework
{
    /// <summary>
    /// 任务方向
    /// </summary>
    public enum TaskDirection
    {
        /// <summary>
        /// 不明确的
        /// </summary>
        [Description("未知")]
        Unknow,
        /// <summary>
        /// 入库
        /// </summary>
        [Description("入库")]
        In,
        /// <summary>
        /// 出库
        /// </summary>
        [Description("出库")]
        Out
    }
}
