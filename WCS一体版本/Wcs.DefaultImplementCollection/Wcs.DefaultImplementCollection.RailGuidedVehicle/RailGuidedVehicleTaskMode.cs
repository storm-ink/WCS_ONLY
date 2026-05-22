using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{

    /// <summary>
    /// 任务模式
    /// </summary>
    [DataContract]
    public enum RailGuidedVehicleTaskMode
    {
        /// <summary>无任务类型</summary>
        [Description("无任务类型")]
        [EnumMember]
        None = 0,

        /// <summary>表示HB任务
        /// 全自动任务
        /// </summary>
        [Description("全自动任务")]
        [EnumMember]
        AutomaticTask = 1,

        /// <summary>取货任务</summary>
        [Description("取货任务")]
        [EnumMember]
        Picking = 2,

        /// <summary>放货任务</summary>
        [Description("放货任务")]
        [EnumMember]
        Putting = 3,


        /// <summary>有货行走</summary>
        [Description("有货行走")]
        [EnumMember]
        WalkWithGoods = 4,

        /// <summary>无货行走</summary>
        [Description("无货行走")]
        [EnumMember]
        Walk = 5,

    }
}
