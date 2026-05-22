using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.Crane
{
    public enum CmdTypes
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown=0,
        /// <summary>
        /// 新任务
        /// </summary>
        NewTask = 1,
        /// <summary>
        /// 清除任务
        /// </summary>
        ClearTask = 2,
        /// <summary>
        /// 急停
        /// </summary>
        Emergency = 3,
        /// <summary>
        /// 取消急停
        /// </summary>
        CancelEmergency = 4
    }
}
