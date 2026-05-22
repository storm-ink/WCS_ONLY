using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.Framework
{
    /// <summary>
    /// 表示继承此接口的对象是一个支持事务的对象
    /// </summary>
    public interface ISupportTransactionObject
    {
        /// <summary>
        /// 对象在事务中的状态
        /// </summary>
        SupportTransactionObjectStatus SupportTransactionObjectStatus { get; set; }
    }

    /// <summary>
    /// 支持事务的对象状态
    /// </summary>
    public enum SupportTransactionObjectStatus
    {
        /// <summary>
        /// 已初始化的
        /// </summary>
        Initialized,
        /// <summary>
        /// 已预处理的
        /// </summary>
        Prepared,
        /// <summary>
        /// 已提交的
        /// </summary>
        Committed,
        /// <summary>
        /// 已回滚的
        /// </summary>
        Rolledback,
    }
}
