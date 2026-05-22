using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 输送线位置状态
    /// </summary>
    public enum ConveyorLocationStatus
    {
        /// <summary>
        /// 初始化
        /// </summary>
        [Description("初始化")]
        Initialize = 0,
        /// <summary>
        /// 报警
        /// </summary>
        [Description("报警")]
        Warning = 1,
        /// <summary>
        /// 离线
        /// </summary>
        [Description("离线")]
        Offline = 2,
        /// <summary>
        /// 手动
        /// </summary>
        [Description("手动")]
        Manual = 3,
        /// <summary>
        /// 停止
        /// </summary>
        [Description("停止")]
        Stopped = 4,
        /// <summary>
        /// 运行
        /// </summary>
        [Description("运行")]
        Running = 5
    }
}
