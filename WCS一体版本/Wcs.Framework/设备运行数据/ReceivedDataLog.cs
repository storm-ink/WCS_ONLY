using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.Framework
{
    /// <summary>
    /// 接收到的数据日志条目
    /// </summary>
    public abstract class ReceivedDataLog
    {
        public virtual Int32 Id { get; protected set; }
        /// <summary>
        /// 设备名称
        /// </summary>
        public virtual String DeviceName { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTime CreatedAt { get; set; }
        /// <summary>
        /// 数据级别
        /// </summary>
        public virtual ReceivedDataLogLevel DataLevel { get; set; }
        protected ReceivedDataLog()
        {
            this.CreatedAt = DateTime.Now;
        }
    }
}
