using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework
{
    /// <summary>
    /// Wms通知提供程序
    /// </summary>
    public interface IWmsNotifyProvider
    {
        /// <summary>
        /// 任务变为正在执行时发生
        /// </summary>
        /// <param name="task"></param>
        void OnExecuting(Task task);
        /// <summary>
        /// 任务变为暂停时发生
        /// </summary>
        /// <param name="task"></param>
        void OnSuspend(Task task);
        /// <summary>
        /// 任务变为已取消时发生
        /// </summary>
        /// <param name="task"></param>
        void OnCancelled(Task task);
        /// <summary>
        /// 任务被删除时发生
        /// </summary>
        /// <param name="task"></param>
        void OnDeleted(Task task);
        /// <summary>
        /// 任务已完成时发生
        /// </summary>
        /// <param name="task"></param>
        void OnCompleted(Task task);
        /// <summary>
        /// 设备请求时发生
        /// </summary>
        /// <param name="request"></param>
        void OnRequest(Request request);
    }
}
