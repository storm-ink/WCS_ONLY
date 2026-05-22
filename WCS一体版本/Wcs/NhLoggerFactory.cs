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

namespace Wcs
{
    /// <summary>
    /// 生产 NhLogger 的工厂。
    /// </summary>
    public class NhLoggerFactory : ILoggerFactory
    {

        /// <summary>
        /// 初始化此类的新实例。
        /// </summary>
        public NhLoggerFactory()
        {
        }

        /// <summary>
        /// 指定 type 返回 logger。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IInternalLogger LoggerFor(System.Type type)
        {
            return this.LoggerFor(type.ToString());
        }

        /// <summary>
        /// 指定 keyName 返回 logger。
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public IInternalLogger LoggerFor(string keyName)
        {
            NLog.Logger logger = NLog.LogManager.GetLogger(keyName);
            return new NhLogger(logger);
        }
    }
}
