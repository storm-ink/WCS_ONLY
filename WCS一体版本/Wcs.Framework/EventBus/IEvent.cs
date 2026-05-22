using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.Framework.EventBus
{
    /// <summary>
    /// 事件接口，实际上是事件数据上的概念
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// 事件标志键值（可识别事件对象的键值，唯一键）
        /// </summary>
        String EventKey { get; set; }
        /// <summary>
        /// 指示该数据是否支持持久化
        /// </summary>
        Boolean Preservable { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        DateTime CreatedAt { get; set; }
        /// <summary>
        /// 转换为描述信息
        /// </summary>
        String ToDescription();
    }
}
