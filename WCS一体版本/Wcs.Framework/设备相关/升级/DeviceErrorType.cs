using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 设备错误类型
    /// </summary>
    public class DeviceErrorType
    {
        /// <summary>
        /// 记录ID
        /// </summary>
        public virtual Int32 Id { get; set; }
        /// <summary>
        /// 故障分类
        /// </summary>
        public virtual Int32 AlarmCategory { get; set; }
        /// <summary>
        /// 设备类型
        /// </summary>
        public virtual String DeviceType { get; set; }
        /// <summary>
        /// 故障码
        /// </summary>
        public virtual string DeviceErrorCode { get; set; }
        /// <summary>
        /// 故障名称
        /// </summary>
        public virtual String ErrorName { get; set; }
        /// <summary>
        /// 故障描述
        /// </summary>
        public virtual String Description { get; set; }
        /// <summary>
        /// 故障等级
        /// </summary>
        public virtual Int16 Levle { get; set; }
        /// <summary>
        /// 解决方案
        /// </summary>
        public virtual String Solution { get; set; }
        /// <summary>
        /// 是否属于故障
        /// </summary>
        public virtual Boolean IsFault { get; set; }
    }
}
