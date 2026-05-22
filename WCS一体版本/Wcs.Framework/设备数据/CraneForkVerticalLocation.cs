using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 堆垛机货叉垂直位置
    /// </summary>
    public enum CraneForkVerticalLocation
    {
        /// <summary>
        /// 中位
        /// </summary>
        [System.ComponentModel.Description("中位")]
        Middle = 0,
        /// <summary>
        /// 高位
        /// </summary>
        [System.ComponentModel.Description("高位")]
        Top,
        /// <summary>
        /// 低位
        /// </summary>
        [System.ComponentModel.Description("低位")]
        Bottom
    }
}
