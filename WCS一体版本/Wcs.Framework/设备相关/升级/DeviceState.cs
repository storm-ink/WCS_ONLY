using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 设备状态
    /// </summary>
    public class DeviceState
    {
        public DeviceState()
        {
            TaskLock = 0;
        }

        /// <summary>
        /// 编号
        /// </summary>
        public virtual Int32 Id { get; set; }
        /// <summary>
        /// 设备名称
        /// </summary>
        public virtual String DeviceName { get; set; }
        /// <summary>
        /// 设备状态
        /// </summary>
        public virtual DeviceStatus State { get; set; }
        /// <summary>
        /// 所属设备
        /// </summary>
        public virtual String Device { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public virtual String Description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual Int32 TaskLock { get; set; }
    }
}
