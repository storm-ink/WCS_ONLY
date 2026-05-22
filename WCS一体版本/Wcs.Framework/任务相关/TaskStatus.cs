using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 任务状态
    /// </summary>
    public enum TaskStatus
    {
        /// <summary>
        /// 新任务
        /// </summary>
        [System.ComponentModel.Description("新任务")]
        New,
        /// <summary>
        /// 已发送
        /// </summary>
        [System.ComponentModel.Description("已发送")]
        Sent,
        /// <summary>
        /// 正在运行
        /// </summary>
        [System.ComponentModel.Description("执行中")]
        Executing,
        /// <summary>
        /// 暂停
        /// </summary>
        [System.ComponentModel.Description("已暂停")]
        Suspend,
        /// <summary>
        /// 发生错误
        /// </summary>
        [System.ComponentModel.Description("发生错误")]
        Error,
        /// <summary>
        /// 被取消
        /// </summary>
        [System.ComponentModel.Description("已取消")]
        Cancelled,
        /// <summary>
        /// 已完成
        /// </summary>
        [System.ComponentModel.Description("已完成")]
        Completed,
    }
}
