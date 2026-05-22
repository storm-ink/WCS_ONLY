using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
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
        /// <param name="endLocation">任务要到的终点位置</param>
        /// <returns></returns>
        public abstract Decimal GetRate(Net net, DeviceRoute route, Location endLocation);
    }
}
