using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 业务类型
    /// </summary>
    public enum TaskBizType
    {
        /// <summary>
        /// 正常出入库
        /// </summary>
        [System.ComponentModel.Description("正常业务")]
        Normal,
        /// <summary>
        /// 盘点业务
        /// </summary>
        [System.ComponentModel.Description("盘点业务")]
        Counting,
    }
}
