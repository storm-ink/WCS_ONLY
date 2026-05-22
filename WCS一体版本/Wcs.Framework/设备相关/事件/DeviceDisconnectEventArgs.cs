using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 设备断开连接的原因
    /// </summary>
    public enum DisconnectReason
    {
        /// <summary>
        /// 连接失败
        /// </summary>
        ConnectFailed,
        /// <summary>
        /// 发生错误
        /// </summary>
        Error,
        /// <summary>
        /// 用户强制断开
        /// </summary>
        User
    }

    /// <summary>
    /// 设备断开连接的事件数据
    /// </summary>
    public class DisconnectEventArgs : HandleableEventArgs
    {
        /// <summary>
        /// 断开连接的原因
        /// </summary>
        public DisconnectReason Reason { get; private set; }
        /// <summary>
        /// 引起设备断开的异常
        /// </summary>
        public Exception Exception { get; private set; }
        /// <summary>
        /// 构造函数.
        /// </summary>
        /// <param name="reason">   断开连接的原因. </param>
        public DisconnectEventArgs(DisconnectReason reason)
            : this(reason, null)
        {
        }
        /// <summary>
        /// 构造函数.
        /// </summary>
        /// <param name="reason">   断开连接的原因. </param>
        /// <param name="ex">异常信息</param>
        public DisconnectEventArgs(DisconnectReason reason,Exception ex)
        {
            this.Reason = reason;
            this.Exception = ex;
        }
    }
}
