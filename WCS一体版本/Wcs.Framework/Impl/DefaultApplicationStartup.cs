using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Impl
{
    /// <summary>
    /// 一个默认应用程序启动自加载项实现，仅用于演示如何实现此功能
    /// </summary>
    public class DefaultApplicationStartup:ApplicationStartup
    {
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public DefaultApplicationStartup():base()
        {
        }
        /// <summary>
        /// 一个空的执行过程，其中不包含任务代码
        /// </summary>
        /// <param name="application">应用程序对象</param>
        protected override void Proc(IWcsApplication application)
        {
            //to do something
        }
    }
}
