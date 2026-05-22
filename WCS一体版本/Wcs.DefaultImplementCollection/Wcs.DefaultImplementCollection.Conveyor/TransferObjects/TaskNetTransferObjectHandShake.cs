using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 握手状态
    /// </summary>
    public enum TaskHandShakes
    {
        /// <summary>
        /// 初始化时数据
        /// </summary>
        [Description("初始化")]
        Empty = 0,
        /// <summary>
        /// 新任务
        /// </summary>
        [Description("新任务")]
        New = 1,
        /// <summary>
        /// 删除请求
        /// </summary>
        [Description("请求删除")]
        ApplyForDelete = 2
    }
}
