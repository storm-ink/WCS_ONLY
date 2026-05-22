using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Wcs.Framework.Cfg;
using System.Xml;

namespace Wcs.Framework
{
    /// <summary>
    /// 延迟通知处理程序
    /// </summary>
    public class DelayedWmsNotifyProcessor
    {
        static DelayedWmsNotifyProcessor _instance;
        public static DelayedWmsNotifyProcessor GetInstance()
        {
            lock (typeof(DelayedWmsNotifyProcessor))
            {
                if (_instance == null)
                {
                    _instance = new DelayedWmsNotifyProcessor();
                }
            }

            return _instance;
        }
        public Boolean Initialized { get; private set; }
        public Logger Logger { get; private set; }
        List<DelayedWmsNotify> delayedWmsNotifies;
        /// <summary>
        /// 获取当前延迟队列中的通知集合
        /// </summary>
        public DelayedWmsNotify[] DelayedWmsNotifies
        {
            get
            {
                return delayedWmsNotifies.ToArray();
            }
        }
        Thread thread;
        private DelayedWmsNotifyProcessor()
        {
            if (!Configuration.Initialized)
            {
                throw new Exception("尝试在配置未初始化前访问 Configuration 对象");
            }

            List<WmsNotify> result = new List<WmsNotify>();
            XmlNode node = Configuration.GetSelection("delayedWmsNotifyProcessor");
            if (node != null)
            {
                LogTarget logTarget = null;
                string logTargetName = node.Attributes["logTarget"] == null ? "" : node.Attributes["logTarget"].Value;
                if (!string.IsNullOrWhiteSpace(logTargetName))
                {
                    logTarget = Framework.Cfg.Configuration.LoggerTargets.SingleOrDefault(x => string.Equals(x.Name, logTargetName, StringComparison.CurrentCultureIgnoreCase));
                }
                this.Logger = new Logger(this, logTarget);
            }
            else
            {
                this.Logger = new Logger(this, null);
            }
        }
        /// <summary>
        /// 初始化对象 <br />
        /// 这包括：<br />
        /// 1、从数据库内加载所有被延迟的通知<br />
        /// 2、启动延迟通知处理进程
        /// </summary>
        /// <exception cref="Exception">    如果尝试初始化状态已为 Initialized 的对象，将抛出异常</exception>
        public void Initialize()
        {
            if (Initialized)
            {
                throw new Exception(string.Format("正在尝试重复初始化 {0}", this));
            }

            lock (typeof(DelayedWmsNotifyProcessor))
            {
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    delayedWmsNotifies = Repository
                        .Query<DelayedWmsNotify>(unitOfWork)
                        .ToList();
                    unitOfWork.Commit();
                }

            }

            thread = new Thread(internalProcess);
            thread.IsBackground = true;
            thread.Priority = ThreadPriority.Lowest;
            thread.Start();
        }

        private void internalAdd(DelayedWmsNotify delayedWmsNotify)
        {
            lock (delayedWmsNotify)
            {
                if (delayedWmsNotifies.Any(x => x.Id == delayedWmsNotify.Id))
                {
                    Logger.Warning(string.Format("{0} 已存在于延迟列表", delayedWmsNotify), this, delayedWmsNotify);
                    return;
                }

                delayedWmsNotifies.Add(delayedWmsNotify);
                Logger.Info(string.Format("{0} 被添加到了延迟列表", delayedWmsNotify), this, delayedWmsNotify);
            }
        }

        private void internalRemove(DelayedWmsNotify delayedWmsNotify)
        {
            lock (delayedWmsNotify)
            {
                var obj = delayedWmsNotifies.SingleOrDefault(x => x.Id == delayedWmsNotify.Id);
                if (obj != null)
                {
                    delayedWmsNotifies.Remove(obj);
                    Logger.Info(string.Format("{0} 被移出延迟列表", delayedWmsNotify), this, delayedWmsNotify);
                }
            }
        }

        private void internalProcess()
        {
            while (true)
            {
                //30 秒处理一次延迟通知
                Thread.Sleep(1000 * 30);
                if (delayedWmsNotifies == null || delayedWmsNotifies.Count == 0)
                {
                    continue;
                }

                try
                {
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                    {
                        for (int i = 0; i < delayedWmsNotifies.Count; i++)
                        {
                            var delayedWmsNotify = delayedWmsNotifies.ElementAtOrDefault(i);
                            if (delayedWmsNotify != null)
                            {
                                //验证通知源是否存在，不存在则移除
                                dynamic context = null;
                                if (delayedWmsNotify.IsRequest())
                                {
                                    context = Repository.Query<Request>(unitOfWork, x => x.Id == delayedWmsNotify.NotifyObjectId).SingleOrDefault();
                                }
                                else if (delayedWmsNotify.IsTask())
                                {
                                    context = Repository.Query<Task>(unitOfWork, x => x.Id == delayedWmsNotify.NotifyObjectId).SingleOrDefault();
                                }

                                if (context == null)
                                {
                                    RemoveDelayedWmsNotify(delayedWmsNotify.NotifyTypeFullName, delayedWmsNotify.NotifyMethod, delayedWmsNotify.NotifyObjectType,delayedWmsNotify.NotifyObjectId);
                                    break;
                                }

                                delayedWmsNotify.Notify(unitOfWork);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Info(string.Format("延迟处理进程发生错误: {0}", ex), this, ex);
                }
            }
        }

        /// <summary>
        /// 延迟指定通知特定方法
        /// </summary>
        /// <param name="notify">   通知类型 </param>
        /// <param name="method">   通知方法 </param>
        /// <param name="context">  通知上下文数据 </param>
        public void Delay(WmsNotify notify, String method, dynamic context)
        {
            try
            {
                lock (typeof(DelayedWmsNotifyProcessor))
                {
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                    {
                        string objectType = context.GetType().FullName;
                        Int32 objectId = context.Id;

                        //验证原始通知对像是否还存在，如果不存在了，必须取消已存在的延时通过记录
                        var getMethod = unitOfWork.session.GetType().GetMethod("Get", new Type[] { typeof(object) }).MakeGenericMethod(new Type[] { context.GetType() });
                        dynamic contextNowStatus = getMethod.Invoke(unitOfWork.session, new object[] { context.Id });
                        if (contextNowStatus == null)
                        {
                            RemoveDelayedWmsNotify(notify, method, context);
                            return;
                        }

                        DelayedWmsNotify delayedWmsNotify = Repository
                            .Query<DelayedWmsNotify>(unitOfWork, x =>
                                x.NotifyTypeFullName == notify.GetType().FullName
                                && x.NotifyMethod == method
                                && x.NotifyObjectType == objectType
                                && x.NotifyObjectId == objectId
                            ).SingleOrDefault();
                        if (delayedWmsNotify != null)
                        {
                            return;
                        }

                        delayedWmsNotify = new DelayedWmsNotify();
                        delayedWmsNotify.NotifyTypeFullName = notify.GetType().FullName;
                        delayedWmsNotify.NotifyMethod = method;
                        delayedWmsNotify.NotifyObjectType = context.GetType().FullName;
                        delayedWmsNotify.NotifyObjectId = context.Id;

                        Repository.Save(unitOfWork,delayedWmsNotify);
                        unitOfWork.Commit();

                        internalAdd(delayedWmsNotify);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Info(string.Format("延迟 {0}.{1},上下文 {2} 失败,{3}", notify, method, context, ex), this, ex);
            }
        }

        /// <summary>
        /// 删除指定的被延迟的通知对象（如果有的话）
        /// </summary>
        /// <param name="notify">   通知类型. </param>
        /// <param name="method">   通知方法. </param>
        /// <param name="context">  通知上下文数据. </param>
        public void RemoveDelayedWmsNotify(WmsNotify notify, String method, dynamic context)
        {
            RemoveDelayedWmsNotify(notify.GetType().FullName, method, context.GetType().FullName, context.Id);
        }

        /// <summary>
        /// 删除指定的被延迟的通知对象（如果有的话）
        /// </summary>
        /// <param name="notifyTypeFullName">   通知类型完整名. </param>
        /// <param name="method">   通知方法. </param>
        /// <param name="contextType">  通知上下文数据类型. </param>
        /// <param name="contextId">    通知上下文数据Id</param>
        public void RemoveDelayedWmsNotify(String notifyTypeFullName, String method, String contextType, Int32 contextId)
        {
            try
            {
                lock (typeof(DelayedWmsNotifyProcessor))
                {
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                    {
                        DelayedWmsNotify delayedWmsNotify = Repository
                            .Query<DelayedWmsNotify>(unitOfWork, x =>
                                x.NotifyTypeFullName == notifyTypeFullName
                                && x.NotifyMethod == method
                                && x.NotifyObjectType == contextType
                                && x.NotifyObjectId == contextId
                            ).SingleOrDefault();

                        if (delayedWmsNotify != null)
                        {
                            unitOfWork.session.Delete(delayedWmsNotify);
                            internalRemove(delayedWmsNotify);
                        }

                        unitOfWork.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Info(string.Format("移出延迟 {0}.{1},上下文 {2}({3}) 失败,{4}", notifyTypeFullName, method, contextType,contextId, ex), this, ex);
            }
        }
    }
}
