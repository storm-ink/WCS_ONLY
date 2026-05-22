using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    public static class TaskExtentions
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// 为任务设置强制完成信息。该方法应该在强制完成任务的同时调用。
        /// </summary>
        /// <param name="task"></param>
        public static void SetManuallyCompleteInfo(this Task task)
        {
            if (task.Status != TaskStatus.Completed)
            {
                _logger.Warn1(string.Format("正在尝试设置 {0} 的强制完成信息，但其状态当前为 {1}，本次未设置。该方法应该在任务状态更新为完成后再调用。", task, task.Status), null, task);
                return;
            }

            if (task.AdditionalInfo.ContainsKey("强制完成"))
            {
                task.AdditionalInfo["强制完成"] = Wcs.Security.WcsPrincipal.CurrentPrincipal.Identity.Name;

                _logger.Debug1(string.Format("为 {0} 更新强制完成信息 {1}。", task, task.AdditionalInfo["强制完成"]), null, task);
            }
            else
            {
                task.AdditionalInfo.Add("强制完成", Wcs.Security.WcsPrincipal.CurrentPrincipal.Identity.Name);

                _logger.Debug1(string.Format("为 {0} 添加强制完成信息 {1}。", task, task.AdditionalInfo["强制完成"]), null, task);
            }
        }

        /// <summary>
        /// 为任务设置创建原因。该方法应在任务被创建时就调用。
        /// </summary>
        /// <param name="task">指定的任务</param>
        /// <param name="reason">创建原因</param>
        public static void SetCreateReasonInfo(this Task task,String reason)
        {
            if (task.AdditionalInfo.ContainsKey("创建原因"))
            {
                task.AdditionalInfo["创建原因"] = reason;

                _logger.Debug1(string.Format("为 {0} 更新创建原因 {1}。", task, task.AdditionalInfo["创建原因"]), null, task);
            }
            else
            {
                task.AdditionalInfo.Add("创建原因", reason);

                _logger.Debug1(string.Format("为 {0} 添加创建原因 {1}。", task, task.AdditionalInfo["创建原因"]), null, task);
            }
        }
    }
}
