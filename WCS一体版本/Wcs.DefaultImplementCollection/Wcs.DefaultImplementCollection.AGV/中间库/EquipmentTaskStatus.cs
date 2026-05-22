using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.DefaultImplementCollection.AGV
{
    /// <summary>
    /// (1新增;2执行中;3已完成;4错误;5待执行)
    /// </summary>
    public enum EquipmentTaskStatus
    {
        新增 = 1,
        执行中 = 2,
        已完成 = 3,
        错误 = 4,
        待执行 = 5,
        取消 = 6,
        AGV系统主动取消=7    }
}
