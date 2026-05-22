using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NHibernate.Linq;
using System.Transactions;
using Wcs;

namespace Wcs.Framework.EventBus
{
    public static class EventBusEventPublisher
    {
        static Logger _logger;
        static List<IEvent> _eventsQueue;
        static Thread _thread;
        static EventBusEventPublisher()
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _logger.Trace1("初始化事件总线发布程序...",null);
            List<IEvent> events;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                events = unitOfWork
                    .session
                    .Query<IEvent>()
                    .ToList();

                unitOfWork.Commit();
            }

            _eventsQueue = new List<IEvent>(events);

            _logger.Info1(string.Format("加载了 {0} 个待触发事件",_eventsQueue.Count), null);
            _logger.Trace1("启动发布线程...", null);
            _thread = new Thread(thread_proc);
            _thread.Name = "事件总线";
            _thread.IsBackground = true;
            _thread.StartAndManaged();

            _logger.Info1("发布线程启动成功", null);
            _logger.Info1("事件总线发布程序初始化成功", null);
            
        }

        public static void Initialization()
        {

        }
        
        public static Boolean Publish<TEvent>(IEventHandler<TEvent> eventHandler, TEvent evnt)
            where TEvent : class,IEvent
        {
            Boolean result = true;
            try
            {
                using (TransactionScope tsc = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    result = Send(eventHandler, evnt);

                    tsc.Complete();
                }
            }
            catch (Exception ex)
            {
                _logger.WarnException(String.Format("{0}事件处理程序{1}处理失败", evnt, eventHandler), ex);
                _logger.Error1(ex, null);
            }

            return result;
        }

        static Boolean Send<TEvent>(IEventHandler<TEvent> eventHandler, TEvent evnt)
            where TEvent:class,IEvent
        {
            if (!evnt.Preservable && eventHandler.Hold)
            {
                throw new NotSupportedPreservableException<TEvent>(evnt, eventHandler);
            }

            Boolean success;
            try
            {
                eventHandler.Handle(evnt);
                success = true;
            }
            catch (Exception ex)
            {
                success = false;
                _logger.Error1(ex, null);
            }

            if (!success)
            {
                _logger.Warn1(string.Format("{0} 的 {1} 处理失败", eventHandler, evnt.EventKey), null);
            }
            else
            {
                if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<Boolean>("eventBusEventPublisher-logger-enabled", false))
                {
                    _logger.Debug1(string.Format("{0} 的 {1} 处理成功", eventHandler, evnt.EventKey), null);
                }
            }

            return success;
        }

        /// <summary>
        /// 将失败的事件进行持久货操作
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="evnt"></param>
        internal static void Preserve<TEvent>(TEvent evnt)
            where TEvent:class,IEvent
        {
            if (evnt.Preservable)
            {
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                {
                    var obj = unitOfWork.session.Get(evnt.GetType(), evnt.EventKey);

                    if (obj != null)
                    {
                        _logger.Debug1(string.Format("数据库中已存在类型为 {0}，EventKey 值为 {1} 的事件", evnt.GetType().Name, evnt.EventKey), null);

                    }
                    else
                    {

                        unitOfWork.session.Save(evnt);
                    }

                    unitOfWork.Commit();

                }
                if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<Boolean>("eventBusEventPublisher-logger-enabled", false))
                {
                    _logger.Trace1(string.Format("事件 {0}#{1} 保存成功", evnt, evnt.EventKey), null, evnt);
                }
            }
            else
            {
                _logger.Warn1(string.Format("事件 {0}#{1} 不支持持久化，已丢弃", evnt, evnt.EventKey), null, evnt);
            }
        }

        /// <summary>
        /// 将成功的事件从持久化集合中移除
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="evnt"></param>
        internal static void UnPreserve<TEvent>(TEvent evnt)
            where TEvent : class,IEvent
        {
            if (evnt.Preservable)
            {
                using (System.Transactions.TransactionScope ts = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                    {
                        var obj = unitOfWork.session.Get(evnt.GetType(), evnt.EventKey);
                        if (obj != null)
                        {
                            unitOfWork.session.Delete(obj);
                        }

                        unitOfWork.Commit();
                    }

                    ts.Complete();
                }

                if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<Boolean>("eventBusEventPublisher-logger-enabled", false))
                {
                    _logger.Trace1(string.Format("事件 {0}#{1} 删除成功", evnt, evnt.EventKey), null, evnt);
                }
            }
        }

        /// <summary>
        /// 从延迟事件队列中移除指定的事件
        /// </summary>
        /// <param name="evnt"></param>
        public static void Pop(IEvent evnt)
        {
            try
            {
                if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<Boolean>("eventBusEventPublisher-logger-enabled", false))
                {
                    _logger.Debug1(string.Format("准备将 {0} 从持久化中移除...", evnt.EventKey), null);
                }

                UnPreserve(evnt);

                if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<Boolean>("eventBusEventPublisher-logger-enabled", false))
                {
                    _logger.Debug1("移除成功", null);
                }

                var existsEvent = _eventsQueue.FirstOrDefault(x => x.EventKey == evnt.EventKey);
                if (existsEvent != null)
                {
                    _eventsQueue.Remove(existsEvent);
                    if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<Boolean>("eventBusEventPublisher-logger-enabled", false))
                    {
                        _logger.Debug1(string.Format("已将 {0} 从队列中移除.", evnt.EventKey), null);
                    }
                }
                else
                {
                    //_logger.Debug1(string.Format("未在序列中找到 {0}，未进行任何移除操作.", evnt.EventKey), null);
                }
            }
            catch (Exception ex)
            {
                _logger.Error1(new Exception(string.Format("在删除迟延事件#{0}时，尝试将其取消持久化时发生异常。（仍将执行从队列中移除的操作）", evnt.EventKey), ex), null);
            }
        }

        public static void Push(IEvent evnt)
        {
            if (_eventsQueue.Any(x => x.EventKey == evnt.EventKey))
            {
                _logger.Warn1(string.Format("{0} 已存在于队列中，不再添加", evnt.EventKey), null);
            }
            else
            {
                _eventsQueue.Add(evnt);

                if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<Boolean>("eventBusEventPublisher-logger-enabled", false))
                {
                    _logger.Debug1(string.Format("已将 {0} 添加到队列尾部", evnt.EventKey), null);
                }
            }

            if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<Boolean>("eventBusEventPublisher-logger-enabled", false))
            {
                _logger.Debug1(string.Format("准备将 {0} 持久化...", evnt.EventKey), null);
            }

            Preserve(evnt);

            if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<Boolean>("eventBusEventPublisher-logger-enabled", false))
            {
                _logger.Debug1("持久化成功", null);
            }
        }

        static object _eventsQueueLocker = new object();
        static void thread_proc(Object state)
        {
            while (true)
            {
                System.Threading.Thread.Sleep(15 * 1000);

                String firstEventKey = null;
                try
                {
                    while (_eventsQueue.Count > 0 && firstEventKey != _eventsQueue.First().EventKey)
                    {
                        IEvent evnt = _eventsQueue.First();
                        if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<Boolean>("eventBusEventPublisher-logger-enabled", false))
                        {
                            _logger.Debug1(string.Format("从队列顶部取出 {0} 准备处理...", evnt.EventKey), null);

                            _logger.Debug1(string.Format("将 {0} 从队列中移除", evnt.EventKey), null);
                        }
                        _eventsQueue.Remove(evnt);
                        if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<Boolean>("eventBusEventPublisher-logger-enabled", false))
                        {
                            _logger.Debug1(string.Format("将 {0} 添加到队列尾部", evnt.EventKey), null);
                        }
                        _eventsQueue.Add(evnt);
                        if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<Boolean>("eventBusEventPublisher-logger-enabled", false))
                        {
                            _logger.Debug1("开始触发事件...", null);
                        }
                        EventBus.Instance.Publish(evnt);
                        if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<Boolean>("eventBusEventPublisher-logger-enabled", false))
                        {
                            _logger.Debug1("事件触发结束.", null);
                        }
                        if (firstEventKey == null)
                        {
                            firstEventKey = evnt.EventKey;
                        }
                        if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<Boolean>("eventBusEventPublisher-logger-enabled", false))
                        {
                            _logger.Debug1("处理结束.", null);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, null);
                }
            }
        }
    }
}
