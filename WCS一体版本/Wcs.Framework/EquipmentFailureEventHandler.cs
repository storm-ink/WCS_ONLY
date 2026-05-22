using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Wcs.Framework
{

    /// <summary>
    /// 设备故障事件处理程序
    /// </summary>
    /// <remarks>
    /// 为了保证操作的一致性，将多个任务事件的处理程序加入到同一事务中进行提交，其中任意一个处理失败将导致整个事务失败。
    /// </remarks>
    /// <typeparam name="TArgs">事件参数类型</typeparam>
    public abstract class EquipmentFailureEventHandler<TArgs>
        where TArgs : EquipmentFailureEventArgs
    {
        List<IEquipmentFailureEventHandler<TArgs>> _handlers;
        public Logger Logger { get; private set; }
        public EquipmentFailureEventHandler()
        {
            _handlers = new List<IEquipmentFailureEventHandler<TArgs>>();
            Logger = LogManager.GetCurrentClassLogger();
        }

        public EquipmentFailureEventHandler(IEnumerable<IEquipmentFailureEventHandler<TArgs>> handlers)
            : this()
        {
            _handlers.AddRange(handlers);
        }

        public void Add(IEquipmentFailureEventHandler<TArgs> hander)
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

        public void Handle(Device device, TArgs args)
        {
            if (_handlers.Count == 0)
            {
                args.Handled = true;
                return;
            }

            try
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
                            Logger.Warn1(string.Format("{0} 在处理 {1} 时发生异常 {2}，本次事务未提交", this, handler, ex), this, handler);
                        }

                        if (args.Handled == false)
                        {
                            Logger.Warn1(String.Format("{0} 在处理 {1} 时失败，本次事务未提交", handler, args), this, handler);
                            break;
                        }
                    }

                    if (args.Handled)
                    {
                        tsc.Complete();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error1(ex, this);
            }
        }
    }
}
