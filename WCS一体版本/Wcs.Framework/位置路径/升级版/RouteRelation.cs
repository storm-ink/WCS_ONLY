using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 路径关系
    /// </summary>
    public class RouteRelation
    {
        /// <summary>
        /// 自增长ID
        /// </summary>
        public virtual Int32 RelationID { get; set; }
        /// <summary>
        /// 开始路径ID
        /// </summary>
        public virtual Int32 Id { get; set; }
        /// <summary>
        /// 结束路径ID
        /// </summary>
        public virtual Int32 Adjoins { get; set; }
    }
}
