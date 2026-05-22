using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 货叉方向<br />
    /// 指示堆垛机执行动作时伸叉的方向（相对于对堆机本身）
    /// </summary>
    public enum ForkDirection
    {
        /// <summary>
        /// 左排
        /// </summary>
        Left=1,
        /// <summary>
        /// 右排
        /// </summary>
        Right=2,
        /// <summary>
        /// 左排2(双伸位)
        /// </summary>
        Left2 = 3,
        /// <summary>
        /// 右排2(双伸位)
        /// </summary>
        Right2 = 4,
    }
}
