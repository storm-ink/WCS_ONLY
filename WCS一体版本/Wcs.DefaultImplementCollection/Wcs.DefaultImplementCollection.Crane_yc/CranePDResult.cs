using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    public enum CranePDResult
    {
        /// <summary>
        /// 获取盘点结果失败
        /// </summary>
        _getResultError = 0,
        /// <summary>
        /// 盘点有货
        /// </summary>
        _getResultOK = 1,
        /// <summary>
        /// 盘点无货
        /// </summary>
        _getResultFailed=2
    }
}
