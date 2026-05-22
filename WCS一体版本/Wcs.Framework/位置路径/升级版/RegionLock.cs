using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    public class RegionLock
    {
        /// <summary>
        /// id
        /// </summary>
        public virtual Int32 Id { get; set; }
        /// <summary>
        /// 区域名称
        /// </summary>
        public virtual String RegionName { get; set; }
        /// <summary>
        /// 物理动作ID
        /// </summary>
        public virtual String TaskCode { get; set; }
        /// <summary>
        /// 锁定标志
        /// </summary>
        public virtual String CheckDeviceCode { get; set; }
        /// <summary>
        /// 锁定类型
        /// </summary>
        public virtual String CheckDeviceCodeType { get; set; }
    }
}
