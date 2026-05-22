using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 堆垛机事件
    /// </summary>
    public enum CraneEvent
    {
        /// <summary>
        /// 已初始化
        /// </summary>
        [System.ComponentModel.Description("已初始化")]
        Initialized = 0,

        /// <summary>
        /// 开始运行
        /// </summary>
        [System.ComponentModel.Description("开始运行")]
        BeginRunning = 1,

        /// <summary>
        /// 开始取货
        /// </summary>
        [System.ComponentModel.Description("开始取货")]
        Loading = 2,

        /// <summary>
        /// 取货完成
        /// </summary>
        [System.ComponentModel.Description("取货完成")]
        Loaded = 3,

        /// <summary>
        /// 正在放货
        /// </summary>
        [System.ComponentModel.Description("正在放货")]
        Unloading = 4,

        /// <summary>
        /// 放货完成
        /// </summary>
        [System.ComponentModel.Description("放货完成")]
        Unloaded = 5,

        /// <summary>
        /// 完成
        /// </summary>
        [System.ComponentModel.Description("完成")]
        Completed = 6,

        /// <summary>
        /// 急停
        /// </summary>
        [System.ComponentModel.Description("急停")]
        EmergencyStopped = 7,

        /// <summary>
        /// 出错完成
        /// </summary>
        [System.ComponentModel.Description("出错完成")]
        CompletedWithError = 8,

        /// <summary>
        /// 回原点
        /// </summary>
        [System.ComponentModel.Description("回原点")]
        MovingToOrigin = 9
    }
}
