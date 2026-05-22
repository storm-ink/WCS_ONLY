using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    //0:初始化,1:报警,2:离线,3:手动,4:待命,5:运行
    public enum LocationStatus
    {
        /// <summary>
        /// 初始化时数据
        /// </summary>
        [Description("初始化")]
        Empty = 0,
        /// <summary>
        /// 报警
        /// </summary>
        [Description("报警")]
        Alarming = 1,
        /// <summary>
        /// 离线
        /// </summary>
        [Description("离线")]
        Offline = 2,
        /// <summary>
        /// 手动
        /// </summary>
        [Description("手动")]
        Manual = 3,
        /// <summary>
        /// 待命
        /// </summary>
        [Description("待命")]
        Waitting = 4,
        /// <summary>
        /// 运行
        /// </summary>
        [Description("运行")]
        Running = 5,
        /// <summary>
        /// 运行
        /// </summary>
        [Description("无货运行")]
        UnloadedRunning = 6,
        /// <summary>
        /// 运行
        /// </summary>
        [Description("有货运行")]
        LoadedRunning = 7,
    }
}
