using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    public class DeviceEventStack:IEnumerable<DeviceEvent>
    {
        List<DeviceEvent> list = new List<DeviceEvent>();
        public void Push<TDevice, TEventArgs>(DeviceEventHandler<TDevice, TEventArgs> handler, TDevice sender, TEventArgs args)
            where TDevice:Device
            where TEventArgs:HandleableEventArgs
        {
            if (handler == null)
            {
                return;
            }
            DeviceEvent receivedEvent = new DeviceEvent((Device arg1, HandleableEventArgs arg2) =>
            {
                using (System.Transactions.TransactionScope tsc = new System.Transactions.TransactionScope())
                {
                    foreach (var dgt in handler.GetInvocationList())
                    {
                        arg2.Handled = false;
                        dgt.Method.Invoke(dgt.Target,new object[]{arg1, arg2});
                        if (arg2.Handled == false)
                        {
                            arg1.Logger.Warn1(String.Format("{0} 在执行时失败，事务回滚", dgt), this, arg2);
                            return;
                        }
                    }

                    tsc.Complete();
                }
                //handler((TDevice)arg1, (TEventArgs)arg2);
            }, sender, args);

            Push(receivedEvent);
        }

        public void Push(DeviceEvent @event)
        {
            list.Add(@event);
        }

        public DeviceEvent Pop()
        {
            return list.FirstOrDefault();
        }

        public IEnumerator<DeviceEvent> GetEnumerator()
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
    public class DeviceEvent
    {
        /// <summary>
        /// 源事件对象
        /// </summary>
        public Action<Device,HandleableEventArgs> Event { get; private set; }
        /// <summary>
        /// 源事件参数
        /// </summary>
        public HandleableEventArgs Args { get; private set; }
        /// <summary>
        /// 源事件的事件源
        /// </summary>
        public Device Sender{get;private set;}

        /// <summary>
        /// 默认构造函数
        /// </summary>
        /// <param name="event">事件</param>
        /// <param name="sender">事件源</param>
        /// <param name="args">事件数据</param>
        public DeviceEvent(Action<Device, HandleableEventArgs> @event, Device sender, HandleableEventArgs args)
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
