using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.Framework.EventBus
{
    /// <summary>
    /// 不支持操作化异常
    /// </summary>
    /// <typeparam name="TEvent">事件泛型参数</typeparam>
    [Serializable]
    public class NotSupportedPreservableException<TEvent>:Exception
        where TEvent:class,IEvent
    {
        public TEvent Event { get; private set; }
        public IEventHandler<TEvent> Handler { get; private set; }
        public NotSupportedPreservableException(TEvent evt, IEventHandler<TEvent> handler)
            : base(string.Format("{0} 并不支持持久化操作。此异常通常发生在事件本身不支持持久货操作，但订阅方 {1} 却要求发布方在订单处理失败后必须重新发布该事件。", evt.GetType(),handler.GetType()))
        {
            this.Event = evt;
            this.Handler = handler;
        }
    }
}
