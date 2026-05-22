using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 表示一个可以接受处理的事件的事件数据
    /// </summary>
    public abstract class HandleableEventArgs:EventArgs
    {
        List<EventBus.IEvent> _lazyEvents = new List<EventBus.IEvent>();
        /// <summary>
        /// 是否已处理
        /// </summary>
        public Boolean Handled { get;  set; }
        /// <summary>
        /// 获取本次处理过程中被延迟发布的事件（准备在本次事件处理成功后发布的事件）
        /// </summary>
        public EventBus.IEvent[] LazyEvents
        {
            get
            {
                return _lazyEvents.ToArray();
            }
        }
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public HandleableEventArgs()
            : base()
        {

        }

        /// <summary>
        /// 添加一个俗延迟发布（准备在本次事件处理成功后发布的事件）
        /// </summary>
        /// <param name="lazyEvent"></param>
        public void AddLazyEvent(EventBus.IEvent lazyEvent)
        {
            _lazyEvents.Add(lazyEvent);
        }

        /// <summary>
        /// 添加多个俗延迟发布（准备在本次事件处理成功后发布的事件）
        /// </summary>
        /// <param name="lazyEvents"></param>
        public void AddLazyEvents(IEnumerable<EventBus.IEvent> lazyEvents)
        {
            _lazyEvents.AddRange(lazyEvents);
        }

        /// <summary>
        /// 派发延迟事件
        /// </summary>
        /// <param name="async">是否异步引发</param>
        public void FireLazyEvents(Boolean async=false)
        {
            if (LazyEvents == null || LazyEvents.Length == 0)
            {
                return;
            }

            if (async)
            {
                System.Threading.ThreadPool.QueueUserWorkItem((stat) => 
                {                    
                    EventBus.EventBus.Instance.Publish((EventBus.IEvent[])stat);
                }, LazyEvents.ToArray());
            }
            else
            {
                EventBus.EventBus.Instance.Publish(LazyEvents);
            }
        }

        /// <summary>
        /// 重置本对象。
        /// 1、恢复Handled标志为 false
        /// 2、清空LazyEvents中的延迟事件列表
        /// </summary>
        public void Reset()
        {
            Handled = false;
            _lazyEvents.Clear();
        }
    }
}
