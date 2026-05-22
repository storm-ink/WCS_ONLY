using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.Crane
{
    /// <summary>
    /// 货叉动作，表示货叉可进行的动作
    /// </summary>
    [Flags]
    public enum ForkAction
    {
        /// <summary>
        /// 禁止任何货叉东西
        /// </summary>
        None = 0,
        /// <summary>
        /// 取
        /// </summary>
        Pickup = 1,
        /// <summary>
        /// 放
        /// </summary>
        Putdown = 2,
        /// <summary>
        /// 取放
        /// </summary>
        PickAndPut=3
    }
}
