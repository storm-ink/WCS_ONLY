using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 堆垛机左右位置
    /// </summary>
    public enum CraneForkHorizontalLocation
    {
        /// <summary>
        /// 左排
        /// </summary>
        [Description("左排")]
        Left = 1,

        /// <summary>
        /// 右排
        /// </summary>
        [Description("右排")]
        Right = 2
    }
}
