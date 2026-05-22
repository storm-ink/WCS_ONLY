using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 占位信号握手变量
    /// </summary>
    public enum OccupiedSignalHandShake
    {
        /// <summary>
        /// 没有占位
        /// </summary>
        [Description("初始状态")]
        Empty = 0,
        /// <summary>
        /// 有占位
        /// 容器到达某特定位置后，电气程序生成该位置的占位信号，并通过设备通迅缓冲区反应到上位机
        /// </summary>
        [Description("有占位")]
        New = 1,
        /// <summary>
        /// 申请清除
        /// 在读到电气生成的占位信号后，需要由上位机发送此握手到设备通迅缓冲区。电气程序在读到该信号后删除当前点位信息
        /// </summary>
        [Description("申请清除")]
        ApplyToDelete = 2,
    }
}
