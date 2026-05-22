using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// Wms 状态通知
    /// </summary>
    public abstract class WmsNotify
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logger">日志对象</param>
        public WmsNotify(Logger logger)
        {
            this.Logger = logger;
        }
        /// <summary>
        /// 任务变为正在执行时发生
        /// </summary>
        /// <param name="task"></param>
        public abstract void OnExecuting(Task task);
        /// <summary>
        /// 任务变为暂停时发生
        /// </summary>
        /// <param name="task"></param>
        public abstract void OnSuspend(Task task);
        /// <summary>
        /// 任务变为已取消时发生
        /// </summary>
        /// <param name="task"></param>
        public abstract void OnCancelled(Task task);
        /// <summary>
        /// 任务被删除时发生
        /// </summary>
        /// <param name="task"></param>
        public abstract void OnDeleted(Task task);
        /// <summary>
        /// 任务已完成时发生
        /// </summary>
        /// <param name="task"></param>
        public abstract void OnCompleted(Task task);
        /// <summary>
        /// 设备请求时发生
        /// </summary>
        /// <param name="request"></param>
        public abstract void OnRequest(Request request);
        /// <summary>
        /// 获取或设置日志
        /// </summary>
        public virtual Logger Logger { get; protected set; }
        /// <summary>
        /// 获取通知别名
        /// </summary>
        public abstract String Name { get;  }
    }
}
