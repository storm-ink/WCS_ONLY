using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using NLog;

namespace Wcs.Framework
{
    /// <summary>
    /// 调度程序动作过滤器<br />
    /// 用来在发送任务时对任务进行判断，显示是否可以发送
    /// </summary>
    public abstract class EquipmentActionSchedulerFilter
    {
        public Logger Logger { get; protected set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        public EquipmentActionSchedulerFilter() {
            Logger = NLog.LogManager.GetCurrentClassLogger();
        }
        /// <summary>
        /// 判断指定的动作是否可以发送
        /// </summary>
        /// <param name="equipmentActionScheduler">调度程序</param>
        /// <param name="action">要发送的动作</param>
        /// <returns>返回 true 表示动作可以发送；false 表示动作不可以发送。</returns>
        public abstract Boolean CanSend(EquipmentActionScheduler equipmentActionScheduler, EquipmentAction action);
    }
}
