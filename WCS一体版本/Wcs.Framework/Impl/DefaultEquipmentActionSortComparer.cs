using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Impl
{
    /// <summary>
    /// 默认的物理动作排序对比器，此处所有的对象将相等。
    /// </summary>
    public class DefaultEquipmentActionSortComparer : EquipmentActionSortComparer
    {
        public override int Compare(IGrouping<EquipmentActionGroup, EquipmentAction> x, IGrouping<EquipmentActionGroup, EquipmentAction> y)
        {
            return 0;
        }
    }
}
