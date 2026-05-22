using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework.MessageBoard
{
    /// <summary>
    /// 表示一个抽象的消息看板
    /// </summary>
    public abstract class AbstractMessageBoard:IDisposable
    {
        public static AbstractMessageBoard Instance { get; internal set; }

        public AbstractMessageBoard(XmlNode cfg)
        {

        }

        Object _listenersLocker = new object();
        Object _taskLocker = new object();
        AbstractMessageBoardListener[] _listeners = new AbstractMessageBoardListener[0];
        AbstractMessageBoardTask[] _tasks = new AbstractMessageBoardTask[0];
        /// <summary>
        /// 获取当前看板中的所有消息侦听者
        /// </summary>
        public virtual AbstractMessageBoardListener[] Listeners
        {
            get
            {
                lock (_listenersLocker)
                {
                    return _listeners;
                }
            }
        }
        /// <summary>
        /// 获取当前看板中的所有任务
        /// </summary>
        public virtual AbstractMessageBoardTask[] Tasks
        {
            get
            {
                lock (_taskLocker)
                {
                    return _tasks;
                }
            }
        }
        /// <summary>
        /// 当消息被添加时发生
        /// </summary>
        public event MessageAddedEventHandler Added;
        /// <summary>
        /// 当消息被移除时发生
        /// </summary>
        public event MessageRemovedEventHandler Removed;
        /// <summary>
        /// 添加一个新的消息
        /// </summary>
        /// <param name="message"></param>
        public abstract void Add(AbstractMessage message);
        /// <summary>
        /// 添加一个新的消息
        /// </summary>
        /// <param name="message"></param>
        public abstract void Remove(AbstractMessage message);
        public abstract AbstractMessage Get(ulong id);
        /// <summary>
        /// 返回当前所有消息
        /// </summary>
        public abstract AbstractMessage[] Messages { get; }

        public virtual void AddListener(AbstractMessageBoardListener listener)
        {
            if (listener == null)
            {
                throw new ArgumentNullException("listener");
            }

            lock (_listenersLocker)
            {
                if (_listeners.Any(x => x == listener))
                {
                    throw new InvalidOperationException("{0} 已存在，无法重复添加。");
                }

                _listeners = _listeners
                    .Concat(new AbstractMessageBoardListener[] { listener })
                    .ToArray();
            }
        }

        public virtual void RemoveListener(AbstractMessageBoardListener listener)
        {
            if (listener == null)
            {
                throw new ArgumentNullException("listener");
            }

            lock (_listenersLocker)
            {
                _listeners = _listeners.Where(x => x != listener).ToArray();
            }
        }

        public virtual void AddTask(AbstractMessageBoardTask task)
        {
            if (task == null)
            {
                throw new ArgumentNullException("task");
            }

            task.SetMessageBoard(this);

            lock (_taskLocker)
            {
                if (_tasks.Any(x => x == task))
                {
                    throw new InvalidOperationException("{0} 已存在，无法重复添加。");
                }

                _tasks = _tasks
                    .Concat(new AbstractMessageBoardTask[] { task })
                    .ToArray();
            }
        }

        public virtual void RemoveTask(AbstractMessageBoardTask task)
        {
            if (task == null)
            {
                throw new ArgumentNullException("task");
            }

            lock (_listenersLocker)
            {
                _tasks = _tasks.Where(x => x != task).ToArray();
            }
        }
        
        protected virtual void FireRemovedEvent(AbstractMessage message)
        {
            if (Removed != null)
            {
                try
                {
                    Removed.Invoke(this, message);
                }
                catch (Exception)
                {

                }
            }
        }
        protected virtual void FireAddedEvent(AbstractMessage message)
        {
            if (Added != null)
            {
                try
                {
                    Added.Invoke(this, message);
                }
                catch (Exception)
                {

                }
            }
        }

        protected virtual void WriteToListeners(AbstractMessage message)
        {
            foreach (var lsn in _listeners)
            {
                lsn.Write(this, message);
            }
        }

        public abstract void Dispose();
    }
}
