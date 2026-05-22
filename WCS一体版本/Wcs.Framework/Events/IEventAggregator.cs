using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Events
{
    /// <summary>
    /// 事件聚合器接口
    /// </summary>
    public interface IEventAggregator
    {
        void Subscribe<TEvent>(IEventHandler<TEvent> eventHandler) where TEvent : class,IEvent;
        void Unsubscribe<TEvent>(IEventHandler<TEvent> eventHandler) where TEvent : class,IEvent;
        void Publish<TEvent>(TEvent evnt) where TEvent : class,IEvent;
    }
}
