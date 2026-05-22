using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NLog;
using System.Windows.Forms;

namespace Wcs.Framework
{
    public class DeviceEventQueue : System.Collections.Concurrent.ConcurrentQueue<DeviceEventInvocation> //Queue<DeviceEventInvocation>
    {
        //System.Collections.Concurrent.ConcurrentQueue<DeviceEventInvocation>
        static Logger _logger = LogManager.GetCurrentClassLogger();
        System.Threading.Thread _thread;
        public Device Device { get; private set; }
        public Logger Logger
        {
            get
            {
                return _logger;
            }
        }

        public DeviceEventQueue(Device device)
        {
            this.Device = device;
            _thread = new Thread(DispatchEvent);
            _thread.Name = this.ToString();
            _thread.IsBackground = true;
            _thread.StartAndManaged(this);
        }

        public Boolean IsRunning { get; private set; }

        /// <summary>
        /// 向事件队列中添加一个新的元素
        /// </summary>
        /// <typeparam name="TDevice">泛型参数：事件源类型</typeparam>
        /// <typeparam name="TEventArgs">泛型参数：事件数据</typeparam>
        /// <param name="handler">处理程序</param>
        /// <param name="sender">事件源</param>
        /// <param name="args">事件数据</param>
        public void Enqueue<TDevice, TEventArgs>(DeviceEventHandler<TDevice, TEventArgs> handler, TDevice sender, TEventArgs args)
            where TDevice : Device
            where TEventArgs : HandleableEventArgs
        {
            try
            {
                if (handler == null)
                {
                    _logger.Warn1("handler 为空，未向设备事件池添加任何事件。", this, args);
                    return;
                }

                var invocationList = handler.GetInvocationList();
                if (invocationList == null || invocationList.Length == 0)
                {
                    _logger.Warn1(string.Format("{0} 的调用列表为空，未向设备事件池添加任何事件。", handler), this, args);
                    return;
                }

                foreach (var dgt in invocationList)
                {
                    DeviceEventInvocation item = new DeviceEventInvocation(dgt, sender, args);
                    Enqueue(item);
                }
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
            }
        }

        /// <summary>
        /// 向事件队列中添加一个新的元素
        /// </summary>
        /// <param name="item">要添加到队列中的新元素</param>
        public new void Enqueue(DeviceEventInvocation item)
        {
            //lock (this)
            //{
            //    if (!this.Any(x => x.Args == item.Args
            //         && x.Delegate == item.Delegate
            //         && x.Sender == item.Sender))
            //    {
            base.Enqueue(item);
            //    }

            //}
            if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<Boolean>("deviceEventQueue-logger-enabled", false))
            {
                //lock (this)
                //{
                _logger.Debug1(String.Format("{0} 添加到事件队列。当前元素总量：{1}", item, this.Count), this, item.Args);
                //}
            }

        }

        public new void Dequeue()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 事件队列处理主过程
        /// </summary>
        /// <param name="state">状态对象</param>
        void DispatchEvent(Object state)
        {
            _logger.Info1("事件派发器开始工作...", this);
            while (true)
            {
                //lock (this)
                //{
                try
                {
                    Thread.Sleep(1);

                    if (this.Count == 0)
                    {
                        Thread.Sleep(50);
                        continue;
                    }

                    DeviceEventInvocation evt;
                    if (!base.TryDequeue(out evt))
                    {
                        _logger.Warn1("取出的evt,TryDequeue 失败。", this);
                        continue;
                    }
                    //var evt = base.Dequeue();
                    if (evt == null)
                    {
                        _logger.Warn1("取出的evt为null。", this);
                        continue;
                    }

                    //异步处理
                    if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<bool>("设备事件.异步处理"))
                    {
                        System.Threading.ThreadPool.QueueUserWorkItem(stat =>
                        {
                            Event_Handle((DeviceEventInvocation)stat);
                        }, evt);
                    }
                    else
                    {
                        //同步处理
                        Event_Handle(evt);
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.Error1(ex, this);
                }
                //}
            }

            _logger.Info1("事件派发器已停止。", this);
        }

        void Event_Handle(DeviceEventInvocation evt)
        {
            //同一事件处理程序同步执行
            lock (evt.Args)
            {
                try
                {
                    using (System.Transactions.TransactionScope tsc = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.RequiresNew))
                    {
                        evt.Fire();
                        if (evt.Args.Handled)
                        {
                            tsc.Complete();
                        }
                        else
                        {
                            this.Logger.Warn1(string.Format("{0} 未处理成功。", evt), this, evt.Args);
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.Error1(ex, this, evt);
                    evt.Args.Handled = false;
                }

                if (!evt.Args.Handled)
                {
                    this.Enqueue(evt);
                }
                else
                {
                    if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<Boolean>("deviceEventQueue-logger-enabled", false))
                    {
                        _logger.Debug1(String.Format("{0} 派发成功。", evt), this, evt.Args);
                    }

                    if (evt.Args.LazyEvents != null && evt.Args.LazyEvents.Length > 0)
                    {
                        if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<Boolean>("deviceEventQueue-logger-enabled", false))
                        {
                            _logger.Debug1(String.Format("{0} 处理完成后包含 {1} 个 LazyEvent，准备引发...", evt, evt.Args.LazyEvents.Length), this, evt.Args);
                        }

                        //EventBus.EventBus.Instance.Publish(evt.Args.LazyEvents);
                        //异步处理
                        if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<bool>("设备事件.异步处理"))
                        {
                            evt.Args.FireLazyEvents(true);
                        }
                        else
                        {
                            evt.Args.FireLazyEvents(false);
                        }

                        if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<Boolean>("deviceEventQueue-logger-enabled", false))
                        {
                            _logger.Debug1(String.Format("{0} 的 LazyEvents 已引发。", evt), this, evt.Args);
                        }
                    }
                }

                //恢复处理标志(多播广播委托中，每个委托方法都会执行一次fire，并且分布在独立的事务中。
                //所以在前一个委托方法处理结束后必须恢复 args 的 handled 标志)
                //evt.Args.Handled = false;
                evt.Args.Reset();
            }
        }

        public override string ToString()
        {
            return String.Format("{0}的事件派发器", Device);
        }
    }

    /// <summary>
    /// 事件绑定的具体处理方法<br />
    /// 通常从按照调用顺序返回此多路广播委托的调用列表中取得
    /// </summary>
    public class DeviceEventInvocation
    {
        /// <summary>
        /// 事件绑定的处理方法<br />
        /// 通常从按照调用顺序返回此多路广播委托的调用列表中取得
        /// </summary>
        public Delegate Delegate { get; private set; }
        public Device Sender { get; private set; }
        public HandleableEventArgs Args { get; private set; }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        /// <param name="dgt">事件绑定的处理方法,通常从按照调用顺序返回此多路广播委托的调用列表中取得</param>
        /// <param name="paramaters">参数</param>
        public DeviceEventInvocation(Delegate dgt, Device sender, HandleableEventArgs args)
        {
            this.Delegate = dgt;
            this.Sender = sender;
            this.Args = args;
        }

        /// <summary>
        /// 引发事件
        /// </summary>
        public void Fire()
        {
            this.Args.Handled = false;
            this.Delegate.DynamicInvoke(this.Sender, this.Args);
        }

        public override string ToString()
        {
            return this.Delegate.Method.ToString();
        }
    }
}
