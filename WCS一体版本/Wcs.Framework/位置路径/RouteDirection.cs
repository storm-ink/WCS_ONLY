using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 路径方向<br />
    /// 这个方向值只主要用来判断两个任务的交叉情况，具体所指含义由使用方解释
    /// </summary>
    public enum RouteDirection
    {
        /// <summary>
        /// 入
        /// </summary>
        In = 0,
        /// <summary>
        /// 出
        /// </summary>
        Out = 1
    }
}
