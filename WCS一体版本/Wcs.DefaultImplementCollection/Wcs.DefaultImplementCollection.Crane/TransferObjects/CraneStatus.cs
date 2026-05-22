using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.Crane
{
    public enum CraneStatus
    {
        /// <summary>
        /// 初始化
        /// </summary>
        Initilization = 0,
        /// <summary>
        /// 待命
        /// </summary>
        Watting = 1,
        /// <summary>
        /// 运行
        /// </summary>
        Running = 2,
        /// <summary>
        /// 报警停机
        /// </summary>
        AlarmDown = 3,
        /// <summary>
        /// 远程急停
        /// </summary>
        RemoteEmergency = 4,
        /// <summary>
        /// 取消远程急停
        /// </summary>
        CancelRemoteEmergency = 5
    }
}
