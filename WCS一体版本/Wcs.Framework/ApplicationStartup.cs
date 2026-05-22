using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Wcs.Framework.Cfg;

namespace Wcs.Framework
{
    /// <summary>
    /// 应用程序启动时要加载并执行的对象，此对象会在 Configuration 初始化完成之后运行。
    /// </summary>
    [Obsolete("请直接使用 Wcs.Framework.IApplicationStartup 接口。")]
    public abstract class ApplicationStartup : IApplicationStartup
    {
        protected Logger _logger { get; set; }
        public StartupElement StartupElement { get; private set; }
        internal ApplicationStartup()
        {
            this._logger = LogManager.GetCurrentClassLogger();
        }
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public ApplicationStartup(StartupElement element)
        {
            this.StartupElement = element;
            this._logger = LogManager.GetCurrentClassLogger();

            Initialize(element);
        }
        /// <summary>
        /// 运行此自加载项
        /// </summary>
        /// <param name="application">应用程序对象</param>
        public abstract void Run(IWcsApplication application);

        public virtual void Initialize(StartupElement element)
        {

        }
    }
}
