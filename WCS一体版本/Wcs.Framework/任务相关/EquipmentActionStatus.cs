using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Wcs.Framework
{
    /// <summary>
    /// 设备动作状态
    /// </summary>
    public enum EquipmentActionStatus
    {
        /// <summary>
        /// 新动作，等待执行
        /// </summary>
        [Description("等待执行")]
        New,
        /// <summary>
        /// 正在执行中
        /// </summary>
        [Description("执行中")]
        Executing,
        /// <summary>
        /// 暂停中
        /// </summary>
        [Description("已暂停")]
        Suspend,
        /// <summary>
        /// 执行中发生错误
        /// </summary>
        [Description("发生错误")]
        Error,
        /// <summary>
        /// 被取消
        /// </summary>
        [Description("已取消")]
        Cancelled,
        /// <summary>
        /// 已完成
        /// </summary>
        [Description("已完成")]
        Completed
    }
}
