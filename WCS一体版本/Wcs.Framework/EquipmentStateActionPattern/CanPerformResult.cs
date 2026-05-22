using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 是否完成结果
    /// </summary>
    public struct CanPerformResult
    {
        /// <summary>
        /// 是否被否决
        /// </summary>
        public Boolean Result{ get; private set; }
        /// <summary>
        /// 被否决的原因
        /// </summary>
        public String Information { get; private set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateAt { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="result">是否被否决</param>
        /// <param name="reason">否决原因</param>
        public CanPerformResult(Boolean result, String information)
            : this()
        {
            this.Result = result;
            this.Information = information;
            this.CreateAt = DateTime.Now;
        }
    }
}
