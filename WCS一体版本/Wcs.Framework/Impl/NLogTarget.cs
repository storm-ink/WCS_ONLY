using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace Wcs.Framework.Impl
{
    /// <summary>
    /// NLog 日志输出目标<br />
    /// 将 Wcs 内的日志数据转接到 NLog 中处理，由 NLog 提供相关的管理操作。
    /// </summary>
    public class NLogTarget:LogTarget
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="targetSelection">配置节点</param>
        public NLogTarget(System.Xml.XmlNode targetSelection):base(targetSelection)
        {
            
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">日志输入目标名称</param>
        public NLogTarget(string name):base(null)
        {
            this.Name = name;
        }
        /// <summary>
        /// 输出
        /// </summary>
        /// <param name="logger">日志对象</param>
        /// <param name="logEntry">日志条目</param>
        public override void Output(Logger logger, LogEntry logEntry)
        {
            LogEventInfo eventInfo;
            switch (logEntry.Type)
            {
                case LogType.Info:
                    eventInfo=new LogEventInfo(LogLevel.Info,this.Name,logEntry.Content);
                    break;
                case LogType.Warning:
                    eventInfo = new LogEventInfo(LogLevel.Warn, this.Name, logEntry.Content);
                    break;
                case LogType.Error:
                    eventInfo = LogEventInfo.Create(LogLevel.Error, this.Name, logEntry.Content, logEntry.Exception);
                    break;
                case LogType.Fatal:
                    eventInfo = LogEventInfo.Create(LogLevel.Fatal, this.Name, logEntry.Content, logEntry.Exception);
                    break;
                case LogType.Debug:
                    eventInfo = new LogEventInfo(LogLevel.Debug, this.Name, logEntry.Content);
                    break;
                case LogType.Trace:
                    eventInfo = new LogEventInfo(LogLevel.Trace, this.Name, logEntry.Content);
                    break;
                default:
                    eventInfo = new LogEventInfo(LogLevel.Info, this.Name, logEntry.Content);
                    break;
            }
            eventInfo.Properties.Add("sender", logEntry.Source);
            eventInfo.Properties.Add("data", logEntry.ReferenceDataString);
            NLog.LogManager.GetLogger(eventInfo.LoggerName).Log(eventInfo);
        }
    }
}
