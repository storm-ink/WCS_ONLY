using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.Framework.EventBus
{
    /// <summary>
    /// 事件总线
    /// 发布与订阅处理逻辑
    /// 核心功能代码
    /// </summary>
    public class EventBus
    {
        private EventBus() { }
        static EventBus()
        {
            _eventBus = new EventBus();
        }
        static Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private static EventBus _eventBus = null;
        private readonly object sync = new object();
        /// <summary>
        /// 对于事件数据的存储，目前采用内存字典
        /// </summary>
        private static Dictionary<Type, List<object>> eventHandlers = new Dictionary<Type, List<object>>();

        /// <summary>
        /// 初始化空的事件总件
        /// </summary>
        public static EventBus Instance
        {
            get
            {
                return _eventBus;
            }
        }

        #region 事件订阅&取消订阅，可以扩展
        /// <summary>
        /// 订阅事件列表
        /// </summary>
        /// <param name="type"></param>
        /// <param name="subTypeList"></param>
        public void Subscribe<TEvent>(IEventHandler<TEvent> eventHandler)
            where TEvent : class, IEvent
        {
            lock (sync)
            {
                var eventType = typeof(TEvent);
                if (eventHandlers.ContainsKey(eventType))
                {
                    var handlers = eventHandlers[eventType];
                    if (handlers != null)
                    {
                        if (!handlers.Exists(deh => eventHandlerEquals(deh,eventHandler)))
                            handlers.Add(eventHandler);
                    }
                    else
                    {
                        handlers = new List<object>();
                        handlers.Add(eventHandler);
                    }
                }
                else
                    eventHandlers.Add(eventType, new List<object> { eventHandler });
            }
        }

        public void Subscribe<TEvent>(Action<TEvent> eventHandlerFunc)
            where TEvent : class, IEvent
        {
            Subscribe<TEvent>(new ActionDelegatedEventHandler<TEvent>(eventHandlerFunc,false));
        }

        public void Subscribe<TEvent>(Action<TEvent> eventHandlerFunc,Boolean hold)
            where TEvent : class, IEvent
        {
            Subscribe<TEvent>(new ActionDelegatedEventHandler<TEvent>(eventHandlerFunc, hold));
        }

        public void Subscribe<TEvent>(IEnumerable<IEventHandler<TEvent>> eventHandlers)
            where TEvent : class, IEvent
        {
            foreach (var eventHandler in eventHandlers)
                Subscribe<TEvent>(eventHandler);
        }
        /// <summary>
        /// 取消订阅事件
        /// </summary>
        /// <param name="type"></param>
        /// <param name="subType"></param>
        public void Unsubscribe<TEvent>(IEventHandler<TEvent> eventHandler)
            where TEvent : class, IEvent
        {
            lock (sync)
            {
                var eventType = typeof(TEvent);
                if (eventHandlers.ContainsKey(eventType))
                {
                    var handlers = eventHandlers[eventType];
                    if (handlers != null
                        && handlers.Exists(deh => eventHandlerEquals(deh,eventHandler)))
                    {
                        var handlerToRemove = handlers.First(deh => eventHandlerEquals(deh,eventHandler));
                        handlers.Remove(handlerToRemove);
                    }
                }
            }
        }

        public void Unsubscribe<TEvent>(Action<TEvent> eventHandlerFunc)
            where TEvent : class, IEvent
        {
            Unsubscribe<TEvent>(new ActionDelegatedEventHandler<TEvent>(eventHandlerFunc,false));
        }

        public void Unsubscribe<TEvent>(IEnumerable<IEventHandler<TEvent>> eventHandlers)
          where TEvent : class, IEvent
        {
            foreach (var eventHandler in eventHandlers)
                Unsubscribe<TEvent>(eventHandler);
        }
        #endregion

        #region 事件发布
        /// <summary>
        /// 发布事件，支持异步事件
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="evnt"></param>
        public void Publish<TEvent>(TEvent evnt)
           where TEvent : class, IEvent
        {
            if (evnt == null)
                throw new ArgumentNullException("evnt");
            var eventType = evnt.GetType();
            if (eventHandlers.ContainsKey(eventType)
                && eventHandlers[eventType] != null
                && eventHandlers[eventType].Count > 0)
            {
                object[] handlers = new object[eventHandlers[eventType].Count];
                eventHandlers[eventType]
                    .ToArray()
                    .CopyTo(handlers, 0);


                //EventBusEventPublisher.Publish(
                //    eventHandlers.Select(x=>x as IEventHandler<TEvent>).ToArray(), 
                //    evnt
                //    );
                Boolean success = true;
                foreach (var item in handlers)
                {
                    var eventHandler = item as IEventHandler<TEvent>;
                    if (eventHandler == null)
                    {
                        continue;
                    }

                    if (success)
                    {
                        success = EventBusEventPublisher.Publish(eventHandler, evnt);
                    }
                    else
                    {
                        EventBusEventPublisher.Publish(eventHandler, evnt);
                    }
                }

                try
                {
                    if (success)
                    {
                        EventBusEventPublisher.Pop(evnt);
                    }
                    else
                    {
                        if (evnt.Preservable)
                        {
                            EventBusEventPublisher.Push(evnt);
                        }
                        else
                        {
                            EventBusEventPublisher.Pop(evnt);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, null);
                }
            }
        }

        /// <summary>
        /// 发布事件，支持异步事件
        /// </summary>
        /// <param name="events"></param>
        public void Publish(params IEvent[] events)
        {
            if (events == null)
                throw new ArgumentNullException("evnt");

            if (events.Length == 0)
            {
                return;
            }

            Action<IEvent[]> act = (args) =>
            {
                var method = this.GetType().GetMethods().Single(x => x.Name == "Publish" && x.GetParameters()[0].ParameterType.Name == "TEvent");

                foreach (var evnt in args)
                {
                    try
                    {
                         var invokeMethod = method.MakeGenericMethod(new Type[] { evnt.GetType() });
                         invokeMethod.Invoke(this, new object[] { evnt });
                    }
                    catch (Exception ex)
                    {
                        _logger.Error1(ex, null);
                    }
                }
            };

            act.BeginInvoke(events, null, "EventBus.Publish");
        }

        #endregion


        private Boolean eventHandlerEquals(object o1,object o2)
        {
            var o1Type = o1.GetType();
            var o2Type = o2.GetType();
            if (o1Type.IsGenericType &&
                o1Type.GetGenericTypeDefinition() == typeof(ActionDelegatedEventHandler<>) &&
                o2Type.IsGenericType &&
                o2Type.GetGenericTypeDefinition() == typeof(ActionDelegatedEventHandler<>))
                return o1.Equals(o2);
            return o1Type == o2Type;
        }
    }
}
