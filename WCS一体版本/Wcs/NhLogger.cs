/*
 * ================================================
 * 创建人：王建军
 * 创建日期：2012年11月中旬
 * 备注：
 * 
 * 修改人：
 * 修改日期：
 * 备注：
 * ================================================
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NLog;
using System.Configuration;
using System.Collections.Specialized;

namespace Wcs
{
    /// <summary>
    /// NHibernate 默认使用 log4net，NhLogger 为通过 NLog 实现的 NHibernate 日志类。
    /// </summary>
    public class NhLogger : IInternalLogger
    {
        private readonly NLog.Logger _logger;

        /// <summary>
        /// 初始化此类的新实例。
        /// </summary>
        /// <param name="logger"></param>
        public NhLogger(NLog.Logger logger)
        {
            _logger = logger;
        }

        #region Properties

        /// <summary>
        /// 获取一个值，指示 Debug 级别是否已启用。
        /// </summary>
        public bool IsDebugEnabled
        {
            get
            {
                return _logger.IsDebugEnabled;
            }
        }

        /// <summary>
        /// 获取一个值，指示 Error 级别是否已启用。
        /// </summary>
        public bool IsErrorEnabled
        {
            get
            {
                return _logger.IsErrorEnabled;
            }
        }

        /// <summary>
        /// 获取一个值，指示 Fatal 级别是否已启用。
        /// </summary>

        public bool IsFatalEnabled
        {
            get
            {
                return _logger.IsFatalEnabled;
            }
        }

        /// <summary>
        /// 获取一个值，指示 Info 级别是否已启用。
        /// </summary>
        public bool IsInfoEnabled
        {
            get
            {
                return _logger.IsInfoEnabled;
            }
        }

        /// <summary>
        /// 获取一个值，指示 Warn 级别是否已启用。
        /// </summary>
        public bool IsWarnEnabled
        {
            get
            {
                return _logger.IsWarnEnabled;
            }
        }



        #endregion

        #region IInternalLogger Methods

        /// <summary>
        /// 输出 debug 级别的消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public void Debug(object message, Exception exception)
        {
            _logger.DebugException(message.ToString(), exception);
        }

        /// <summary>
        /// 输出 debug 级别的消息
        /// </summary>
        /// <param name="message"></param>
        public void Debug(object message)
        {
            _logger.Debug(message.ToString());
        }

        /// <summary>
        /// 输出 debug 级别的消息
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void DebugFormat(string format, params object[] args)
        {
            _logger.Debug(String.Format(format, args));
        }

        /// <summary>
        /// 输出 Error 级别的消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public void Error(object message, Exception exception)
        {
            _logger.ErrorException(message.ToString(), exception);
        }

        /// <summary>
        /// 输出 Error 级别的消息
        /// </summary>
        /// <param name="message"></param>
        public void Error(object message)
        {
            _logger.Error(message.ToString());
        }

        /// <summary>
        /// 输出 Error 级别的消息
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void ErrorFormat(string format, params object[] args)
        {
            _logger.Error(String.Format(format, args));
        }

        /// <summary>
        /// 输出 Fatal 级别的消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public void Fatal(object message, Exception exception)
        {
            _logger.FatalException(message.ToString(), exception);
        }

        /// <summary>
        /// 输出 Fatal 级别的消息
        /// </summary>
        /// <param name="message"></param>
        public void Fatal(object message)
        {
            _logger.Fatal(message.ToString());
        }

        /// <summary>
        /// 输出 Info 级别的消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public void Info(object message, Exception exception)
        {
            _logger.InfoException(message.ToString(), exception);
        }

        /// <summary>
        /// 输出 Info 级别的消息
        /// </summary>
        /// <param name="message"></param>
        public void Info(object message)
        {
            _logger.Info(message.ToString());
        }

        /// <summary>
        /// 输出 Info 级别的消息
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void InfoFormat(string format, params object[] args)
        {
            _logger.Info(String.Format(format, args));
        }

        /// <summary>
        /// 输出 Warn 级别的消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public void Warn(object message, Exception exception)
        {
            _logger.WarnException(message.ToString(), exception);
        }

        /// <summary>
        /// 输出 Warn 级别的消息
        /// </summary>
        /// <param name="message"></param>
        public void Warn(object message)
        {
            _logger.Warn(message.ToString());
        }

        /// <summary>
        /// 输出 Warn 级别的消息
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void WarnFormat(string format, params object[] args)
        {
            _logger.Warn(String.Format(format, args));
        }

        #endregion


    }
}
