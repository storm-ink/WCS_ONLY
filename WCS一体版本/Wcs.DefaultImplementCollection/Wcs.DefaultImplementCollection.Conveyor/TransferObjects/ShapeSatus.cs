using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    public enum ShapeSatus
    {
        /// <summary>
        /// 初始化时数据
        /// </summary>
        [Description("初始化")]
        Empty = 0,
        /// <summary>
        /// 报警
        /// </summary>
        [Description("成功")]
        OK = 1,
        /// <summary>
        /// 离线
        /// </summary>
        [Description("失败")]
        Fail = 2
    }
}
