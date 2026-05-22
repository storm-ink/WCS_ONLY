using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 占位请求处理结果
    /// </summary>
    public class RequestSignalHandleResult
    {
        /// <summary>
        /// 指示是否已取消
        /// </summary>
        public Boolean Cancelled { get; set; }
        /// <summary>
        /// 处理对应的操作的理由（原因）
        /// </summary>
        public String Reason { get; set; }
    }
}
