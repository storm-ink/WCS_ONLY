using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 路径锁
    /// </summary>
    public class RouteLock
    {
        /// <summary>
        /// id
        /// </summary>
        public virtual Int32 Id { get; set; }
        /// <summary>
        /// 物理动作ID
        /// </summary>
        public virtual Int32 ActionId { get; set; }
        /// <summary>
        /// 路径Id
        /// </summary>
        public virtual Int32 RouteHeadId { get; set; }
    }
}
