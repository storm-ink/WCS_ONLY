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
        [System.ComponentModel.Description("未知")]
        Unknow,
        /// <summary>
        /// 由 Wms 创建
        /// </summary>
        [System.ComponentModel.Description("Wms")]
        Wms,
        /// <summary>
        /// 由 Wcs 创建
        /// </summary>
        [System.ComponentModel.Description("Wcs")]
        Wcs,
        /// <summary>
        /// 由 Master 创建（通常指在分区设计后的主控Wcs系统）
        /// </summary>
        [System.ComponentModel.Description("Master")]
        Master,
    }
}
