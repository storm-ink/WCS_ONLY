using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.Crane
{
    public enum CraneTaskTypes
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// 全自动
        /// </summary>
        Auto = 1,
        /// <summary>
        /// 单步取货
        /// </summary>
        StepPick = 2,
        /// <summary>
        /// 单步放货
        /// </summary>
        StepPut = 3,
        /// <summary>
        /// 行走
        /// </summary>
        StepWalk = 4,
        /// <summary>
        /// 探货盘存
        /// </summary>
        SensorCheckInventory = 5,
        /// <summary>
        /// 扫码盘检
        /// </summary>
        ScanCheckInventory = 6
    }
}
