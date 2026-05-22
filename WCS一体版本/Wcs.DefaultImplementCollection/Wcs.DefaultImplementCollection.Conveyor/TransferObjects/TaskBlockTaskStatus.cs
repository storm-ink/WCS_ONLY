using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 任务状态
    /// </summary>
    public enum TaskBlockTaskStatus
    {
        /// <summary>
        /// 初始化时数据
        /// </summary>
        [Description("初始化")]
        Empty = 0,
        /// <summary>
        /// 完成
        /// </summary>
        [Description("已完成")]
        Finished = 1,
        /// <summary>
        /// 执行中
        /// </summary>
        [Description("执行中")]
        Running = 2,
        /// <summary>
        /// 等待执行
        /// </summary>
        [Description("等待执行")]
        Waiting = 3,
        /// <summary>
        /// 发生错误
        /// </summary>
        [Description("发生错误")]
        Error = 4,
        /// <summary>
        /// 取消
        /// </summary>
        [Description("已取消")]
        Cancelled = 5
    }
}
