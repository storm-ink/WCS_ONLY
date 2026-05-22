using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 动作序列排序对比器
    /// </summary>
    public abstract class EquipmentActionSortComparer : IComparer<IGrouping<EquipmentActionGroup, EquipmentAction>>
    {
        public EquipmentActionSortComparer()
        {
        }
        /// <summary>
        /// 具体的排序对比方法
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>
        /// -1:x 小于 y
        ///  0:x 等于 y
        ///  1:x 大于 y
        /// </returns>
        public abstract int Compare(IGrouping<EquipmentActionGroup, EquipmentAction> x, IGrouping<EquipmentActionGroup, EquipmentAction> y);
    }
}
