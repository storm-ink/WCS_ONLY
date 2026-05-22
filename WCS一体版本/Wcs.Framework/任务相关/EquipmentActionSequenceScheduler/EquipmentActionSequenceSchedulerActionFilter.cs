using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using NLog;

namespace Wcs.Framework
{
    /// <summary>
    /// 序列动作过滤器，用来在发送任务时对任务进行判断，显示是否可以发送
    /// </summary>
    public abstract class EquipmentActionSequenceSchedulerActionFilter
    {
        public Logger Logger { get; protected set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        public EquipmentActionSequenceSchedulerActionFilter() {
            Logger = NLog.LogManager.GetCurrentClassLogger();
        }
        /// <summary>
        /// 判断指定的动作是否可以发送
        /// </summary>
        /// <param name="equipmentActionSequenceScheduler"></param>
        /// <param name="action">要发送的动作</param>
        /// <returns>返回 true 表示动作可以发送；false 表示动作不可以发送。</returns>
        public abstract Boolean CanSend(EquipmentActionSequenceScheduler equipmentActionSequenceScheduler,EquipmentAction action);
    }
}
