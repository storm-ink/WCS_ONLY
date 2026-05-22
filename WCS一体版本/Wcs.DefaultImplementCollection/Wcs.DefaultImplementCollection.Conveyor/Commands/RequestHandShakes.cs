using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    public enum RequestHandShakes
    {
        /// <summary>
        /// 初始化时数据
        /// </summary>
        [Description("初始化")]
        Empty = 0,
        /// <summary>
        /// 新任务
        /// </summary>
        [Description("新请求")]
        New = 1,
        /// <summary>
        /// 删除请求
        /// </summary>
        [Description("删除请求")]
        ApplyForDelete = 2
    }
}
