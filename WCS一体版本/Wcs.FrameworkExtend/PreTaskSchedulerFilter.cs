using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using NLog;
using Wcs.Framework;

namespace Wcs.FrameworkExtend
{
    /// <summary>
    /// 预任务下发过滤器<br />
    /// 用来在发送预任务时对任务进行判断，显示是否可以执行
    /// </summary>
    public abstract class PreTaskSchedulerFilter
    {
        public Logger Logger { get; protected set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        public PreTaskSchedulerFilter()
        {
            Logger = NLog.LogManager.GetCurrentClassLogger();
        }
        /// <summary>
        /// 判断指定的预任务是否可以发送
        /// </summary>
        /// <param name="preTask">预任务</param>
        /// <returns>返回 true 表示动作可以发送；false 表示动作不可以发送。</returns>
        public abstract ActionSchedulerFilterResult Filter(PreTask preTask);
    }
}
