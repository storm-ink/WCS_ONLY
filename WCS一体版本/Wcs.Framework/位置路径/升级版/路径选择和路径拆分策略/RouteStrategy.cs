using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 路径策略，计算一个路径在某个连通里返回的积分数
    /// </summary>
    public abstract class RouteStrategy
    {
        /// <summary>
        /// 获取一个 route 在指定 net 内的积分
        /// </summary>
        /// <param name="net">指定的 net</param>
        /// <param name="route">要计算分数的 route 对象</param>
        /// <param name="task">任务对象</param>
        /// <param name="startLocation">业务结束位置</param>
        /// <param name="endLocation">业务结束位置</param>
        /// <returns></returns>
        public abstract Decimal GetRate(Net net, Route route,Task task, Location startLocation,Location endLocation);
    }
}
