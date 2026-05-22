using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.Crane
{
    public enum CraneTaskStatus
    {
        /// <summary>
        /// 无任务
        /// </summary>
        Empty = 0,
        /// <summary>
        /// 执行中
        /// </summary>
        Running = 1,
        /// <summary>
        /// 完成
        /// </summary>
        Compeleted = 2,
        /// <summary>
        /// 异常完成
        /// </summary>
        ErrorCompeleted = 3,
        /// <summary>
        /// 手动完成
        /// </summary>
        ManualCompeleted = 4,
        /// <summary>
        /// 请求重发
        /// </summary>
        RequestRetry = 5,
        /// <summary>
        /// 盘存有货
        /// </summary>
        InventoryFull = 6,
        /// <summary>
        /// 盘存无货
        /// </summary>
        InventoryEmpry = 7,
        /// <summary>
        /// 扫码盘检完成
        /// </summary>
        ScanOver = 8,
        /// <summary>
        /// 等待执行
        /// </summary>
        Wait = 9,
    }
}
