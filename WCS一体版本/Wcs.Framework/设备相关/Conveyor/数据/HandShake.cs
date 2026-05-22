using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 握手状态
    /// </summary>
    public enum HandShake
    { 
        /// <summary>
        /// 初始化时数据
        /// </summary>
        [Description("初始化")]
        Empty = 0,
        /// <summary>
        /// 新任务
        /// </summary>
        [Description("新任务")]
        New = 1,
        /// <summary>
        /// 删除请求
        /// </summary>
        [Description("请求删除")]
        ApplyForDelete = 2,
        /// <summary>
        /// PLC已请求
        /// </summary>
        [Description("设备已读")]
        Readed = 3,
        /// <summary>
        /// WCS 二次确认
        /// </summary>
        [Description("握手完成")]
        SecondConfirm = 4,
        /// <summary>
        /// 请求清空
        /// </summary>
        [Description("请求清空")]
        ApplyForClear = 5,
    }
}
