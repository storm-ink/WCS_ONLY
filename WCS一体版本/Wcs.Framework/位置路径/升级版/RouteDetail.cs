using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 路径明细
    /// </summary>
    public class RouteDetail
    {
        /// <summary>
        /// 明细Id
        /// </summary>
        public virtual Int32 DetailID { get; set; }
        /// <summary>
        /// 路径ID
        /// </summary>
        public virtual Int32 Id { get; set; }
        /// <summary>
        /// 设备编号
        /// </summary>
        public virtual String Path { get; set; }
        /// <summary>
        ///所属设备
        /// </summary>
        public virtual String Device { get; set; }
        /// <summary>
        /// 转换成系统认识的编码方式
        /// </summary>
        /// <returns></returns>
        public virtual String ConvertibleCodeToLcation()
        {
            return this.Path + "@" + this.Device;
        }
    }
}
