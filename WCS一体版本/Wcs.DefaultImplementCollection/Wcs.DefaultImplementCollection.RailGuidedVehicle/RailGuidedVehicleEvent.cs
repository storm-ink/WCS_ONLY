using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    /// <summary>
    /// 穿梭车事件
    /// </summary>
    [DataContract]
    public enum RailGuidedVehicleEvent
    {
        /// <summary>无任务</summary>
        [Description("无任务")]
        [EnumMember]
        Initialized = 0,

        /// <summary>接到任务未运行</summary>
        [Description("接到任务未运行")]
        [EnumMember]
        TaskedAndNotRunning = 1,

        /// <summary>行走运行</summary>
        [Description("行走运行")]
        [EnumMember]
        Running = 2,

        /// <summary>到达起始点</summary>
        [Description("到达起始点")]
        [EnumMember]
        ArrivedStartStation = 3,


        /// <summary>到达目的地</summary>
        [Description("到达目的地")]
        [EnumMember]
        ArrivedEndStation = 4,

        /// <summary>执行让道任务</summary>
        [Description("执行让道任务")]
        [EnumMember]
        ClearedTheWay = 5,

        /// <summary>
        /// 自动任务完成
        /// </summary>
        [Description("自动任务完成")]
        [EnumMember]
        AutomaticTaskCompletion = 6,

        /// <summary>
        /// 手动报完成
        /// </summary>
        [Description("手动报完成")]
        [EnumMember]
        TaskCompletionByManual = 7,

        /// <summary>
        /// 交接货超时
        /// </summary>
        [Description("交接货超时")]
        [EnumMember]
        交接货超时 = 8,

        /// <summary>
        /// 远程急停
        /// </summary>
        [Description("远程急停")]
        [EnumMember]
        远程急停 = 9
    }
}
