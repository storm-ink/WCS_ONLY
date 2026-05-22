using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
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
        /// <param name="endLocation">任务要起点位置</param>
        /// <param name="endLocation">任务要到的终点位置</param>
        /// <returns></returns>
        public override decimal GetRate(Net net, Route route, Task task, Location startLocation, Location endLocation)
        {
            if (route.Enabled == false)
            {
                return 0m;
            }

            //如果这条路径能直接到达目的地，则按路径长路计算
            if (route.EndLocation.Equals(endLocation))
            {
                return 1m / route.Locations.Length;
            }

            //如果还要经过其它路径，则计算总长度
            int startRouteIndex = net.Routes.ToList().FindIndex(x => x.Id == route.Id);
            var endRoute = net.Routes.Skip(startRouteIndex) //跳过起始路径 
                             .FirstOrDefault(x =>
                                 (
                                        x.StartLocation.Equals(x.EndLocation) //起点和终点一样（场景：堆垛机内部连通，*@C001,*@C001）
                                        || !x.StartLocation.Equals(endLocation)//终点不能是路径的起点
                                  )
                                 &&
                                 (
                                    (x.AllowStartFromMidway && x.Locations.Any(loc => loc.Equals(endLocation))) //允许从中途执行的，终点应该是路径中的任意一个位置
                                 || (!x.AllowStartFromMidway && x.EndLocation.Equals(endLocation)) //不允许从中途执行的，终点应该是路径中终点位置
                                 )
                             );

            if (endRoute == null)
            {
                return 1m / route.Locations.Length;
            }

            int endRouteIndex = net.Routes.ToList().FindIndex(x => x.Id == endRoute.Id);

            int totalLocations = net.Routes.Skip(startRouteIndex)
                .Take(endRouteIndex - startRouteIndex)
                .Sum(x => x.Locations.Length);
                //+ route.Locations.Length
                //+ endRoute.Locations.Length;

            var splitCount = endRouteIndex - startRouteIndex;

            if (splitCount <= 0)
            {
                splitCount = 1;
            }

            return 1m / (totalLocations + splitCount*1.5m);

            
        }
    }
}
