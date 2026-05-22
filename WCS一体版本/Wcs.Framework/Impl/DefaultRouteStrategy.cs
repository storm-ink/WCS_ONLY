using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Devices;

namespace Wcs.Framework.Impl
{
    /// <summary>
    /// 系统提示的默认的路径策略<br />
    /// 以路径的总长度为衡量标准，返回计算数据的分数。路径超长分数越低。
    /// </summary>
    public class DefaultRouteStrategy : RouteStrategy
    {
        /// <summary>
        /// 获取一个 route 在指定 net 内的积分
        /// </summary>
        /// <param name="net">指定的 net</param>
        /// <param name="route">要计算分数的 route 对象</param>
        /// <param name="endLocation">任务要到的终点位置</param>
        /// <returns></returns>
        public override decimal GetRate(Net net, DeviceRoute route,Location endLocation)
        {
            //如果这条路径能直接到达目的地，则按路径长路计算
            if (route.EndLocation.Equals(endLocation))
            {
                return 1m / route.Locations.Length;
            }

            //如果还要经过其它路径，则计算总长度
            int startRouteIndex = net.Routes.ToList().IndexOf(route);
            int endRouteIndex = net.Routes.ToList().IndexOf(net.Routes.Single(x=>x.EndLocation.Equals(endLocation)));
            int totalLocations = net.Routes.Skip(startRouteIndex + 1).Take(endRouteIndex - startRouteIndex).Sum(x => x.Locations.Length);
            return 1m / totalLocations;
        }
    }
}
