using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 路径和任务的关系
    /// </summary>
    public enum RouteVsActionRelations
    {
        /// <summary>
        /// 一对一
        /// </summary>
        OneToOne = 0,
        /// <summary>
        /// 一对多
        /// </summary>
        OneToMany = 1
    }
}