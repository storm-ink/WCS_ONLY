using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Wcs.Framework
{
    /// <summary>
    /// 表示继承此接口的对象是一个任务事件处理程序
    /// </summary>
    /// <typeparam name="TArgs">事件参数类型</typeparam>
    public interface ITaskEventHandler<TArgs>
        where TArgs:HandleableEventArgs
    {
        void Handle(TaskableDevice device, TArgs args);
    }

    /// <summary>
    /// 任务事件处理程序代理
    /// </summary>
    /// <remarks>
    /// 为了保证操作的一致性，将多个任务事件的处理程序加入到同一事务中进行提交，其中任意一个处理失败将导致整个事务失败。
    /// </remarks>
    /// <typeparam name="TArgs">事件参数类型</typeparam>
    public abstract class TaskEventHandlerProxy<TArgs>
        where TArgs:HandleableEventArgs
    {
        List<ITaskEventHandler<TArgs>> _handlers;
        public Logger Logger { get; private set; }
        public TaskEventHandlerProxy()
        {
            _handlers = new List<ITaskEventHandler<TArgs>>();
            Logger = LogManager.GetCurrentClassLogger();
        }

        public TaskEventHandlerProxy(IEnumerable<ITaskEventHandler<TArgs>> handlers)
            : this()
        {
            _handlers.AddRange(handlers);
        }

        public void Add(ITaskEventHandler<TArgs> hander)
        {
            _handlers.Add(hander);
        }

        public Int32 Count
        {
            get
            {
                return _handlers.Count;
            }
        }

        public void Handle(TaskableDevice device, TArgs args)
        {
            using (System.Transactions.TransactionScope tsc = new System.Transactions.TransactionScope())
            {
                foreach (var handler in _handlers)
                {
                    try
                    {
                        handler.Handle(device, args);
                    }
                    catch (Exception ex)
                    {
                        args.Handled = false;
                        Logger.Warn1(string.Format("{0} 在处理 {1} 时发生异常 {2}，本次事务未提交", this, handler,ex),this,handler);
                    }

                    if (args.Handled == false)
                    {
                        Logger.Warn1("{0} 在处理 {1} 时失败，本次事务未提交", this, handler);
                        break;
                    }
                }

                if (args.Handled)
                {
                    tsc.Complete();
                }
            }
        }
    }
}
