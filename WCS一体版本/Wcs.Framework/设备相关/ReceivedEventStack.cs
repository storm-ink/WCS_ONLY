using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
{
    public class ReceivedEventStack:IEnumerable<ReceivedEvent>
    {
        List<ReceivedEvent> list = new List<ReceivedEvent>();
        public void Push<TEventArgs>(EventHandler<TEventArgs> handler, Object sender, TEventArgs args)
            where TEventArgs:HandleableEventArgs
        {
            if (handler == null)
            {
                return;
            }
            ReceivedEvent receivedEvent = new ReceivedEvent((Object arg1, HandleableEventArgs arg2) =>
            {
                handler(arg1, (TEventArgs)arg2);
            }, sender, args);

            Push(receivedEvent);
        }

        public void Push(ReceivedEvent @event)
        {
            list.Add(@event);
        }

        public ReceivedEvent Pop()
        {
            return list.FirstOrDefault();
        }

        public IEnumerator<ReceivedEvent> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }

    /// <summary>
    /// 接受到的事件对象
    /// </summary>
    public class ReceivedEvent
    {
        /// <summary>
        /// 源事件对象
        /// </summary>
        public Action<Object,HandleableEventArgs> Event { get; private set; }
        /// <summary>
        /// 源事件参数
        /// </summary>
        public HandleableEventArgs Args { get; private set; }
        /// <summary>
        /// 源事件的事件源
        /// </summary>
        public Object Sender{get;private set;}

        /// <summary>
        /// 默认构造函数
        /// </summary>
        /// <param name="event">事件</param>
        /// <param name="sender">事件源</param>
        /// <param name="args">事件数据</param>
        public ReceivedEvent(Action<Object, HandleableEventArgs> @event, Object sender, HandleableEventArgs args)
        {
            this.Event = @event;
            this.Sender = sender;
            this.Args = args;
        }

        /// <summary>
        /// 引发事件
        /// </summary>
        public void Fire()
        {
            Event.Invoke(Sender, Args);
        }
    }
}
