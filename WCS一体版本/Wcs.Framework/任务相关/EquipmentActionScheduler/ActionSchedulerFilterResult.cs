using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 过滤器过滤结果
    /// </summary>
    public struct ActionSchedulerFilterResult
    {
        /// <summary>
        /// 是否被否决
        /// </summary>
        public Boolean Defeated{ get; private set; }
        /// <summary>
        /// 被否决的原因
        /// </summary>
        public String Reason { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="defeated">是否被否决</param>
        /// <param name="reason">否决原因</param>
        public ActionSchedulerFilterResult(Boolean defeated, String reason):this()
        {
            this.Defeated = defeated;
            this.Reason = reason;
        }
    }
}
