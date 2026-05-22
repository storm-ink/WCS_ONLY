using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework.Cfg;

namespace Wcs.Framework
{
    /// <summary>
    /// Wms通知提供程序
    /// </summary>
    public class WmsNotifyProvider
    {
        /// <summary>
        /// 获取所有通知对象（有可能为 null）
        /// </summary>
        public static readonly WmsNotify[] Notifys = null;
        /// <summary>
        /// 日志对象
        /// </summary>
        static Logger logger = null;
        /// <summary>
        /// 静态构造函数
        /// </summary>
        static WmsNotifyProvider()
        {
            lock (typeof(WmsNotifyProvider))
            {
                if (Notifys != null)
                {
                    return;
                }

                if (!Configuration.Initialized)
                {
                    throw new Exception("尝试在配置未初始化前访问 Configuration 对象");
                }

                List<WmsNotify> result = new List<WmsNotify>();
                XmlNode node = Configuration.GetSelection("wmsNotifys");
                if (node != null)
                {
                    LogTarget logTarget = null;
                    string logTargetName = node.Attributes["logTarget"] == null ? "" : node.Attributes["logTarget"].Value;
                    if (!string.IsNullOrWhiteSpace(logTargetName))
                    {
                        logTarget = Framework.Cfg.Configuration.LoggerTargets.SingleOrDefault(x => string.Equals(x.Name, logTargetName, StringComparison.CurrentCultureIgnoreCase));
                    }
                    logger = new Logger(null, logTarget);

                    foreach (XmlNode item in node.SelectNodes("notify"))
                    {
                        if (item.Attributes["type"] == null || String.IsNullOrWhiteSpace(item.Attributes["type"].Value))
                        {
                            throw new Exception("请先指定 notify 节点 type 属性");
                        }

                        String typeName = item.Attributes["type"].Value;
                        Type type = Type.GetType(typeName);
                        if (type == null)
                        {
                            throw new Exception("未找到类型 " + typeName);
                        }

                        WmsNotify notice = (WmsNotify)type.Assembly.CreateInstance(type.FullName, false, System.Reflection.BindingFlags.CreateInstance, null, new object[] { logger }, null, null);
                        result.Add(notice);
                    }
                }

                Notifys = result.ToArray();
            }
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        private WmsNotifyProvider() { }
        /// <summary>
        /// 获取日志对象
        /// </summary>
        public static Logger Logger
        {
            get
            {
                return logger;
            }
        }
        /// <summary>
        /// 任务变为正在执行时发生
        /// </summary>
        /// <param name="task"></param>
        public static void OnExecuting(Task task)
        {
            foreach (var notity in Notifys)
            {
                try
                {
                    notity.OnExecuting(task);
                    Logger.Info(String.Format("{0} on executing {1}", notity, task), notity, task);
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex.ToString(), null, ex);
                }
            }
        }
        /// <summary>
        /// 任务变为暂停时发生
        /// </summary>
        /// <param name="task"></param>
        public static void OnSuspend(Task task)
        {
            foreach (var notity in Notifys)
            {
                try
                {
                    notity.OnSuspend(task);
                    Logger.Info(String.Format("{0} on suspend {1}", notity, task), notity, task);
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex.ToString(), null, ex);
                }
            }
        }
        /// <summary>
        /// 任务变为已取消时发生
        /// </summary>
        /// <param name="task"></param>
        public static void OnCancelled(Task task)
        {
            foreach (var notity in Notifys)
            {
                try
                {
                    notity.OnCancelled(task);
                    Logger.Info(String.Format("{0} on cancelled {1}", notity, task), notity, task);
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex.ToString(), null, ex);
                }
            }
        }
        /// <summary>
        /// 任务被删除时发生
        /// </summary>
        /// <param name="task"></param>
        public static void OnDeleted(Task task)
        {
            foreach (var notity in Notifys)
            {
                try
                {
                    notity.OnDeleted(task);
                    Logger.Info(String.Format("{0} on deleted {1}", notity, task), notity, task);
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex.ToString(), null, ex);
                }
            }
        }
        /// <summary>
        /// 任务已完成时调用
        /// </summary>
        /// <param name="task"></param>
        public static void OnCompleted(Task task)
        {
            foreach (var notity in Notifys)
            {
                try
                {
                    notity.OnCompleted(task);
                    Logger.Info(String.Format("{0} on completed {1}", notity, task), notity, task);
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex.ToString(), null, ex);
                }
            }
        }
        /// <summary>
        /// 有请求时调用
        /// </summary>
        /// <param name="request">请求对象</param>
        public static void OnRequest(Request request)
        {
            foreach (var notity in Notifys)
            {
                try
                {
                    notity.OnRequest(request);
                    Logger.Info(String.Format("{0} on request {1}", notity, request), notity, request);
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex.ToString(), null, ex);
                }
            }
        }        
    }
}
