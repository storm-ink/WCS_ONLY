using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 被延迟的设备状态通知
    /// </summary>
    public class DelayedDeviceStatusChangedNotify
    {
        /// <summary>
        /// 被延迟的项目.
        /// </summary>
        public class _DelayedStatusItem
        {
            public _DelayedStatusItem(string eventName):this(eventName,null)
            {
            }

            public _DelayedStatusItem(string eventName, dynamic state, params Object[] args)
            {
                if (args == null || args.Length==0)
                {
                    this.Args = new List<object>();
                }
                else
                {
                    this.Args = new List<object>(args);
                }
                this.EventName = eventName;
                this.State = state;
            }
            public String EventName { get; set; }
            public dynamic State { get; set; }
            public List<Object> Args { get; set; }
            /// <summary>
            /// 将此对象向指定的事件名及参数集合进行对比，判断是否在含义上相当.
            /// </summary>
            /// <param name="eventName">    事件名称. </param>
            /// <param name="args">         事件参数集合. </param>
            /// <returns>
            /// 相当返回 true,不相同返回 false.
            /// </returns>
            public Boolean IsSame(string eventName, params Object[] args)
            {
                if (!this.EventName.Equals(eventName, StringComparison.CurrentCultureIgnoreCase))
                {
                    return false;
                }

                if (args.Length != 0)
                {
                    if (args.Length != this.Args.Count)
                    {
                        return false;
                    }
                }

                return args.SequenceEqual(args);
            }
        }
        /// <summary>
        /// 任务执行中.
        /// </summary>
        public const string TASK_RUNNING = "taskRunning";
        /// <summary>
        /// 任务发生错误.
        /// </summary>
        public const string TASK_ERROR = "taskError";
        /// <summary>
        /// 任务完成.
        /// </summary>
        public const string TASK_COMPLETED = "taskCompleted";
        /// <summary>
        /// 设备报警.
        /// </summary>
        public const string DEVICE_ERROR = "deviceError";
        /// <summary>
        /// 任务位置改变.
        /// </summary>
        public const string TASK_LOCATION_CHANGED = "task_Location_Changed";

        List<_DelayedStatusItem> _items;
        public DelayedDeviceStatusChangedNotify()
        {
            _items = new List<_DelayedStatusItem>();
        }
        /// <summary>
        /// 向状态延迟集合添加一个状态元素.
        /// </summary>
        /// <param name="eventName">    事件名称. </param>
        /// <param name="state">        状态数据. </param>
        /// <param name="args">         事件参数集合. </param>
        public void Add(string eventName, dynamic state, params Object[] args)
        {
            lock (_items)
            {
                if (_items.Any(x => x.IsSame(eventName, args)))
                {
                    return;
                }

                var newItem = new _DelayedStatusItem(eventName, state, args);
                _items.Add(newItem);
            }
        }

        /// <summary>
        /// 向状态延迟集合添加一个状态元素.
        /// </summary>
        /// <param name="eventName">    事件名称. </param>
        public void Add(string eventName)
        {
            Add(eventName, null);
        }
        /// <summary>
        /// 从状态延迟集合移除一个元素.
        /// </summary>
        /// <param name="eventName">    事件名称. </param>
        /// <param name="state">        状态数据. </param>
        /// <param name="args">         事件参数集合. </param>
        public void Remove(string eventName, dynamic state, params Object[] args)
        {
            lock (_items)
            {
                var item = _items.SingleOrDefault(x => x.IsSame(eventName, args));
                if (item != null)
                {
                    _items.Remove(item);
                }
            }
        }
        /// <summary>
        /// 从状态延迟集合移除一个元素.
        /// </summary>
        /// <param name="eventName">    事件名称. </param>
        public void Remove(string eventName)
        {
            lock (_items)
            {
                var item = _items.SingleOrDefault(x => x.IsSame(eventName));
                if (item != null)
                {
                    _items.Remove(item);
                }
            }
        }
        /// <summary>
        /// 获取一个元素.
        /// </summary>
        /// <param name="eventName">    事件名称. </param>
        public _DelayedStatusItem[] Get(string eventName)
        {
            lock (_items)
            {
                var items = _items.Where(x => x.IsSame(eventName)).ToArray();

                return items;
            }
        }
        /// <summary>
        /// 获取一个元素.
        /// </summary>
        /// <param name="eventName">    事件名称. </param>
        /// <param name="state">        状态数据. </param>
        /// <param name="args">         事件参数集合. </param>
        public _DelayedStatusItem Get(string eventName, dynamic state, params Object[] args)
        {
            lock (_items)
            {
                var item = _items.SingleOrDefault(x => x.IsSame(eventName,state,args));

                return item;
            }
        }
    }
}
