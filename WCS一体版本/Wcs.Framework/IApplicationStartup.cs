using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Cfg;

namespace Wcs.Framework
{
    /// <summary>
    /// 表示继续此接口的对象是一个启动程序。此对象会在 Configuration 初始化完成之后运行。
    /// </summary>
    public interface IApplicationStartup
    {
        /// <summary>
        /// 初始化启动对象
        /// </summary>
        /// <param name="element">配置节点</param>
        void Initialize(StartupElement element);
        /// <summary>
        /// 运行此自加载项
        /// </summary>
        /// <param name="application">应用程序对象</param>
        void Run(IWcsApplication application);
    }
}
