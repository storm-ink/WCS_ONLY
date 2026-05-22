using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 0-未知，1-待机，2-无货行走，3-有货行走，4-取货，5-放货，6-等待取货，7-等待放货
    /// </summary>
    public enum VehicleEvents
    {
        /// <summary>
        /// 初始化时数据
        /// </summary>
        [Description("未知")]
        Unknown = 0,
        /// <summary>
        /// 报警
        /// </summary>
        [Description("待机")]
        Waitting = 1,
        /// <summary>
        /// 离线
        /// </summary>
        [Description("无货行走")]
        EmptyMoving = 2,
        /// <summary>
        /// 自动（非异常状态）
        /// </summary>
        [Description("有货行走")]
        NonEmptyMoving = 3,
        /// <summary>
        /// 离线
        /// </summary>
        [Description("取货")]
        Picking = 4,
        /// <summary>
        /// 离线
        /// </summary>
        [Description("放货")]
        Putting = 5,
        /// <summary>
        /// 离线
        /// </summary>
        [Description("等待取货")]
        PickWaitting = 6,
        /// <summary>
        /// 离线
        /// </summary>
        [Description("等待放货")]
        PutWaitting = 7
    }
}
