using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 任务来源
    /// </summary>
    public enum TaskSource
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknow,
        /// <summary>
        /// 由 Wms 创建
        /// </summary>
        Wms,
        /// <summary>
        /// 由 Wcs 创建
        /// </summary>
        Wcs,
    }
}
