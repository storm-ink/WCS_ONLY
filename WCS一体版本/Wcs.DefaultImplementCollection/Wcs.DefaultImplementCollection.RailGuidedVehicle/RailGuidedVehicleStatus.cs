using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    /// <summary>
    /// 穿梭车状态
    /// </summary>
    [DataContract]
    public enum RailGuidedVehicleStatus
    {
        /// <summary>
        /// 初始化
        /// </summary>
        [Description("初始化")]
        [EnumMember]
        初始化 = 0,

        /// <summary>
        /// 手动模式
        /// </summary>
        [Description("手动模式")]
        [EnumMember]
        手动模式 = 1,

        /// <summary>
        /// 无货待命
        /// </summary>
        [Description("无货待命")]
        [EnumMember]
        无货待命 = 2,

        /// <summary>
        /// 有货待命
        /// </summary>
        [Description("有货待命")]
        [EnumMember]
        有货待命 = 3,

        /// <summary>
        /// 无货运行
        /// </summary>
        [Description("无货运行")]
        [EnumMember]
        无货运行 = 4,

        /// <summary>
        /// 有货运行
        /// </summary>
        [Description("有货运行")]
        [EnumMember]
        有货运行 = 5,

        /// <summary>
        /// 输送线运行
        /// </summary>
        [Description("接货中")]
        [EnumMember]
        接货中 = 6,


        /// <summary>
        /// 停止
        /// </summary>
        [Description("卸货中")]
        [EnumMember]
        卸货中 = 7,

        /// <summary>
        /// 报警停机
        /// </summary>
        [Description("报警停机")]
        [EnumMember]
        报警停机 = 8,

        /// <summary>
        /// 有货有任务
        /// </summary>
        [Description("有货有任务")]
        [EnumMember]
        有货有任务 = 9,
        /// <summary>
        /// 无货有任务
        /// </summary>
        [Description("无货有任务")]
        [EnumMember]
        A = 10,
        /// <summary>
        /// 手动清任务
        /// </summary>
        [Description("手动清任务")]
        [EnumMember]
        B = 11

        ///// <summary>
        ///// 取货失败
        ///// </summary>
        //[Description("取货失败")]
        //[EnumMember]
        //取货失败 = 10,

        ///// <summary>
        ///// 放货失败
        ///// </summary>
        //[Description("放货失败")]
        //[EnumMember]
        //放货失败 = 11,
    }
}
