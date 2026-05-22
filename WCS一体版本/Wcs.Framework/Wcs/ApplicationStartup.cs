using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 应用程序启动时要加载并执行的对象，此对象会在 Configuration 初始化完成之后运行。
    /// </summary>
    public abstract class ApplicationStartup
    {
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public ApplicationStartup()
        {
        }
        /// <summary>
        /// 具体要执行的过程
        /// </summary>
        /// <param name="application">应用程序对象</param>
        protected abstract void Proc(IWcsApplication application);
        /// <summary>
        /// 运行此自加载项
        /// </summary>
        /// <param name="application">应用程序对象</param>
        public void Run(IWcsApplication application)
        {
            try
            {
                Proc(application);
                application.Logger.Info(string.Format("在 {0} 启动后加载了自启动项 {1}", application, this), this);
            }
            catch (Exception ex)
            {
                application.Logger.Exception(ex.Message, this, ex);
            }
        }
    }
}
