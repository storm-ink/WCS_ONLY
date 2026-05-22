using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Events
{
    /// <summary>
    /// 事件接口
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// 获取事件ID。每个事件在创建时都会生成一个全局唯一的标识。
        /// </summary>
        Guid ID { get; }
        /// <summary>
        /// 获取事件创建时间。
        /// </summary>
        DateTime TimeStamp { get; }
    }
}
