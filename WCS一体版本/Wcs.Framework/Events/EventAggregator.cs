using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Events
{
    public class EventAggregator:IEventAggregator
    {
        Dictionary<Type, List<object>> eventHandlers = new Dictionary<Type, List<object>>();
        object sync = new object();

        static IEventAggregator instance;
        public static IEventAggregator GetInstance()
        {
            lock (typeof(EventAggregator))
            {
                if (instance == null)
                {
                    instance = new EventAggregator();
                }

                return instance;
            }
        }

        EventAggregator()
        {

        }

        public void Subscribe<TEvent>(IEventHandler<TEvent> eventHandler) where TEvent : class, IEvent
        {
            lock (sync)
            {
                var eventType = typeof(TEvent);
                if (eventHandlers.ContainsKey(eventType))
                {
                    var handlers = eventHandlers[eventType];
                    if (handlers != null)
                    {
                        if (!handlers.Exists(deh => eventHandlerEquals(deh, eventHandler)))
                        {
                            handlers.Add(eventHandler);
                        }
                    }
                    else
                    {
                        handlers = new List<object>();
                        handlers.Add(eventHandler);
                    }
                }
                else
                {
                    eventHandlers.Add(eventType, new List<object> { eventHandler });
                }
            }
        }

        public void Unsubscribe<TEvent>(IEventHandler<TEvent> eventHandler) where TEvent : class, IEvent
        {
            lock (sync)
            {
                var eventType = typeof(TEvent);
                if (eventHandlers.ContainsKey(eventType))
                {
                    var handlers = eventHandlers[eventType];
                    if (handlers != null &&
                        handlers.Exists(deh => eventHandlerEquals(deh, eventHandler)))
                    {
                        var handlerToRemove = handlers.First(deh => eventHandlerEquals(deh, eventHandler));
                        handlers.Remove(handlerToRemove);
                    }
                }
            }
        }

        public void Publish<TEvent>(TEvent evnt) where TEvent : class, IEvent
        {
            lock (sync)
            {
                if (evnt == null)
                {
                    throw new ArgumentNullException("evnt");
                }
                var eventType = evnt.GetType();
                if (eventHandlers.ContainsKey(eventType) &&
                    eventHandlers[eventType] != null &&
                    eventHandlers[eventType].Count > 0)
                {
                    var handlers = eventHandlers[eventType];
                    foreach (var handler in handlers)
                    {
                        var eventHandler = handler as IEventHandler<TEvent>;
                        eventHandler.Handle(evnt);
                    }
                }
            }
        }

        Boolean eventHandlerEquals(object o1, object o2)
        {
            var o1Type = o1.GetType();
            var o2Type = o2.GetType();
            if (o1Type.IsGenericType &&
                o1Type.GetGenericTypeDefinition() == typeof(DefaultImplEventHandler<>) &&
                o2Type.IsGenericType &&
                o2Type.GetGenericTypeDefinition() == typeof(DefaultImplEventHandler<>))
                return o1.Equals(o2);
            return o1Type == o2Type;
        }
    }
}
