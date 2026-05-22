using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 堆垛机状态
    /// </summary>
    public enum CraneStatus
    {
        /// <summary>
        /// 初始化
        /// </summary>
        [Description("初始化")]
        Initialized,

        /// <summary>
        /// 回原点
        /// </summary>
        [Description("初始化")]
        MovingToOrigin,

        /// <summary>
        /// 待命
        /// </summary>
        [Description("待命")]
        Waiting,

        /// <summary>
        /// 运行中
        /// </summary>
        [Description("初始化")]
        Running,

        /// <summary>
        /// 取货
        /// </summary>
        [Description("取货")]
        Loading,

        /// <summary>
        /// 放货
        /// </summary>
        [Description("放货")]
        Unloading,

        /// <summary>
        /// 报警
        /// </summary>
        [Description("报警")]
        Error,

       /// <summary>
       /// 手动操作
        /// </summary>
        [Description("手动操作")]
       Manual
    }
}
