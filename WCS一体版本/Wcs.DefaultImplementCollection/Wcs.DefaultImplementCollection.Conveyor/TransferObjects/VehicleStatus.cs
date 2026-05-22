using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 0-未知，1-手动，2-离线，3-自动，4-报警
    /// </summary>
    public enum VehicleStatus
    {
        /// <summary>
        /// 初始化时数据
        /// </summary>
        [Description("未知")]
        Unknown = 0,
        /// <summary>
        /// 报警
        /// </summary>
        [Description("手动")]
        Manual = 1,
        /// <summary>
        /// 离线
        /// </summary>
        [Description("离线")]
        Offline = 2,
        /// <summary>
        /// 自动（非异常状态）
        /// </summary>
        [Description("自动")]
        Waitting = 3,
        /// <summary>
        /// 离线
        /// </summary>
        [Description("报警")]
        Alarming = 4,
    }
}
