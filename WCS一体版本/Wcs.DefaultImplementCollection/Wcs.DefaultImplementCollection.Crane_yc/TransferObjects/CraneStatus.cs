using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 堆垛机状态
    /// </summary>
    [DataContract]
    public enum CraneStatus
    {
        /// <summary>
        /// 初始化
        /// </summary> 
        [Description("初始化")]
        [EnumMember]
        Initialized = 0,

        /// <summary>
        /// 回原点
        /// </summary>
        [Description("回原点")]
        [EnumMember]
        BackToTheOrigin=1,

        /// <summary>
        /// 无货待命
        /// </summary>
        [Description("无货待命")]
        [EnumMember]
        无货待命=2,

        /// <summary>
        /// 有货待命
        /// </summary>
        [Description("有货待命")]
        [EnumMember]
        有货待命=3,

        /// <summary>
        /// 无货运行
        /// </summary>
        [Description("无货运行")]
        [EnumMember]
        无货运行=4,

        /// <summary>
        /// 有货运行
        /// </summary>
        [Description("有货运行")]
        [EnumMember]
        有货运行=5,

        /// <summary>
        /// 取货
        /// </summary>
        [Description("取货")]
        [EnumMember]
        Pickup=6,

        /// <summary>
        /// 放货
        /// </summary>
        [Description("放货")]
        [EnumMember]
        Putin=7,

        /// <summary>
        /// 报警停机
        /// </summary>
        [Description("报警停机")]
        [EnumMember]
        AlarmAndShutdown=8,

        /// <summary>
        /// 报警复位
        /// </summary>
        [Description("报警复位")]
        [EnumMember]
        ResetAlarm=9,

        /// <summary>
        /// ???
        /// </summary>
        [Description("???")]
#warning 这个状态需要确认
        [EnumMember]
        奇怪的状态=10,

        /// <summary>
        /// 未连接
        /// </summary>
        [Description("脱机")]
        [EnumMember]
        Disconnected = 11,

        /// <summary>
        /// 手动操作
        /// </summary>
        [Description("手动操作")]
        [EnumMember]
        ManualMode=12,
        /// <summary>
        /// 左探有货(非对称货格盘点专用)
        /// </summary>
        [Description("左探有货")]
        [EnumMember]
        左探有货 = 13,
        /// <summary>
        /// 左探有货(非对称货格盘点专用)
        /// </summary>
        [Description("左探无货")]
        [EnumMember]
        左探无货 = 14,
        /// <summary>
        /// 右探有货(非对称货格盘点专用)
        /// </summary>
        [Description("右探有货")]
        [EnumMember]
        右探有货 = 15,
        /// <summary>
        /// 左探有货(非对称货格盘点专用)
        /// </summary>
        [Description("右探无货")]
        [EnumMember]
        右探无货 = 16,
        /// <summary>
        /// 左探有货右探有货(对称货格盘点专用)
        /// </summary>
        [Description("左有右有")]
        [EnumMember]
        左有右有 = 17,
        /// <summary>
        /// 左探有货(对称货格盘点专用)
        /// </summary>
        [Description("左有右无")]
        [EnumMember]
        左有右无 = 18,
        /// <summary>
        /// 右探有货(对称货格盘点专用)
        /// </summary>
        [Description("左无右有")]
        [EnumMember]
        左无右有 = 19,
        /// <summary>
        /// 左探有货(对称货格盘点专用)
        /// </summary>
        [Description("左无右无")]
        [EnumMember]
        左无右无 = 20
    }
}
