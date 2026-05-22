using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 设备路径类型<br />
    /// 某些时候，不同的业务类型可能会需要不同的路径。比如盘点。<br />
    /// 盘点的时候出库和入库时使用的路径可能不允许使用正常出入库的路径，所以路径必须有一个标识指示它属于哪一种。<br />
    /// 任务在下发时会自动根据业务类型来处理哪些类型的连通路径是可用的。
    /// </summary>
    [Flags]
    public enum DeviceRouteType
    {
        /// <summary>
        /// 常规路径
        /// </summary>
        Normal = 1,
        /// <summary>
        /// 盘点路径（仅用于盘点作业）
        /// </summary>
        Counting = 2
    }
}
