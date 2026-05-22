using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 是否空闲结果
    /// </summary>
    public struct IsIdleResult
    {
        /// <summary>
        /// 是否空闲
        /// true-空闲，false-不空闲
        /// </summary>
        public Boolean Result { get; private set; }
        /// <summary>
        /// 不空闲原因
        /// </summary>
        public String Information { get; private set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateAt { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="result">是否空闲</param>
        /// <param name="reason">不空闲原因</param>
        public IsIdleResult(Boolean result, String information)
            : this()
        {
            this.Result = result;
            this.Information = information;
            this.CreateAt = DateTime.Now;
        }
    }
}
