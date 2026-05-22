using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Cfg;
using System.Collections;
using System.Reflection;
namespace Wcs.Framework
{
    /// <summary>
    /// 路径拆分助手类
    /// </summary>
    public static class PathHelper
    {
        static PropertyInfo _SetLocationsPropertyMethod;
        static PathHelper()
        {
            _SetLocationsPropertyMethod = typeof(Route).GetProperty("Locations");
        }
        /// <summary>
        /// 查找指定起点到终点的所有连通路径
        /// </summary>
        /// <param name="task">任务</param>
        /// <param name="from">起点</param>
        /// <param name="to">终点</param>
        /// <param name="currentLocation">当前位置,如果指定该值将从此位置开始搜索路径</param>
        /// <param name="findOptions">搜索路径的类型选项</param>
        /// <returns>返回经过排序的路径集合(按优先级降序)</returns>
        public static Dictionary<Net, Route> FindNextPath(Task task, Location from, Location to, Location currentLocation, Int32? lastMovementRouteId, RouteType findOptions)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            var nets = WcsConfiguration.Instance.Nets
                //.Where(net => net.Points.ContainsKey((currentLocation ?? from).ToConvertibleCode()) && net.Points.ContainsKey(to.ToConvertibleCode()))
                .Where(net=>net.IsCallAt((currentLocation ?? from)) && net.IsCallAt(to))
                .Where(net => net.CanReach(from, to, currentLocation, lastMovementRouteId, findOptions))
                .ToList();

            sw.Stop();
            Console.WriteLine("===================Start==================");
            Console.WriteLine("from {0} to {1},find {2} nets,used {3} milliseconds.", from, to, nets.Count, sw.ElapsedMilliseconds);
            
            sw.Restart();
            Dictionary<Net, Route> result = new Dictionary<Net, Route>();

            //本次路径的起点位置
            Location routeStartLocation = currentLocation ?? from;
            
            foreach (var net in nets)
            {
                var endRoute = getEndRoute(net, to);
                if (endRoute == null)
                {
                    continue;
                }

                var endRouteIndex = net.Routes.IndexOf(endRoute);
                var startRoute = getStartRoute(endRoute, net, routeStartLocation, to);
                if (startRoute == null)
                {
                    continue;
                }

                var startRouteIndex = net.Routes.IndexOf(startRoute);

                Route newRoute = startRoute.Clone();

                //如果已存在相同的路径返回
                if (result.Any(x =>
                    x.Value.Id == newRoute.Id
                    && x.Value.StartLocation.UserCode == newRoute.StartLocation.UserCode
                    && x.Value.EndLocation.UserCode == newRoute.EndLocation.UserCode
                    ))
                {
                    continue;
                }

                //先修正通配符的问题
                if (newRoute.EndLocation is ILocationWildcard || newRoute.StartLocation is ILocationWildcard)
                {
                    if (newRoute.StartLocation is ILocationWildcard)
                    {
                        newRoute.Locations[0] = routeStartLocation;
                    }

                    if (newRoute.EndLocation is ILocationWildcard)
                    {
                        if (newRoute.Locations.Length == 2)
                        {

                            if (startRoute == endRoute)
                            {
                                newRoute.Locations[newRoute.Locations.Length - 1] = to;
                            }
                            else
                            {
                                if (net.Routes[startRouteIndex + 1].StartLocation is ILocationWildcard)
                                {
                                    throw new Exception(string.Format("{0} 和 {1} 无法连接，原因是系统无法转换第一个路径的结束位置于第二个路径的开始位置实际货位", 
                                        startRoute, 
                                        net.Routes[startRouteIndex + 1]));
                                }
                                newRoute.Locations[newRoute.Locations.Length - 1] = net.Routes[startRouteIndex + 1].StartLocation;
                            }
                        }
                    }
                }

                //修正最后一条路径时，结束点在路径中途时的任务终点问题
                if (endRouteIndex == startRouteIndex)
                {
                    List<Location> locations = new List<Location>(newRoute.Locations);
                    //从路径的未节向前刻度（结束位置）
                    while (!to.Equals(locations[locations.Count-1]))
                    {
                        locations.RemoveAt(locations.Count - 1);
                    }

                    while (!routeStartLocation.Equals(locations[0]))
                    {
                        locations.RemoveAt(0);
                    }

                    _SetLocationsPropertyMethod.SetValue(newRoute, locations.ToArray(), null);
                }
                else
                {
                    //修正从中途开始执行时路径的起点问题
                    List<Location> locations = new List<Location>(newRoute.Locations);
                    while (!routeStartLocation.Equals(locations[0]))
                    {
                        locations.RemoveAt(0);
                    }

                    _SetLocationsPropertyMethod.SetValue(newRoute, locations.ToArray(), null);
                }

                //如果已存在相同的路径返回
                if (result.Any(x =>
                    x.Value.Id == newRoute.Id
                    && x.Value.StartLocation.UserCode == newRoute.StartLocation.UserCode
                    && x.Value.EndLocation.UserCode == newRoute.EndLocation.UserCode
                    ))
                {
                    continue;
                }

                result.Add(net, newRoute);
                //if (newRoute.EndLocation is ILocationWildcard || newRoute.StartLocation is ILocationWildcard)
                //{
                //    if (newRoute.StartLocation is ILocationWildcard)
                //    {
                //        newRoute.Locations[0] = routeStartLocation;
                //    }

                //    if (newRoute.EndLocation is ILocationWildcard)
                //    {
                //        newRoute.Locations[newRoute.Locations.Length - 1] = to;
                //    }

                //    result.Add(net, newRoute);
                //}
                //else
                //{
                //    result.Add(net, newRoute);
                //}
            }

            sw.Stop();
            Console.WriteLine("find routes used {0} milliseconds.", sw.ElapsedMilliseconds);
            sw.Restart();
            result = result.OrderByDescending(x => x.Value.GetRate(x.Key, task, from, to))
                .ToDictionary(x => x.Key, x => x.Value);
            sw.Stop();
            Console.WriteLine("order by used {0} milliseconds.", sw.ElapsedMilliseconds);
            Console.WriteLine("===================End==================");
            sw = null;
            return result;
        }

        static Route getEndRoute(Net net, Location to)
        {
            List<Route> results = new List<Route>();
            foreach (var route in net.Routes)
            {
                var toLocationIndex = route.LocationIndexOf(to).Min();
                if (toLocationIndex < 0)
                {
                    continue;
                }

                //找到的终点位置是路径的起点位置，则再从后向前找一次
                if (toLocationIndex == 0 )
                {
                    toLocationIndex = route.LocationIndexOf(to).Max();
                }

                if (toLocationIndex == 0 && route.StartLocation==route.EndLocation && route.StartLocation is ILocationWildcard && route.EndLocation is ILocationWildcard)
                {
                    toLocationIndex = route.Locations.Length - 1;
                }

                //经过两次查找还是不符合要求
                if (toLocationIndex == 0)
                {
                    continue;
                }

                //不允许中途执行，但结束位置又在路径中途的
                if (toLocationIndex != route.Locations.Length - 1 && route.AllowStartFromMidway == false)
                {
                    continue;
                }

                results.Add(route);

                //if (!route.Locations.Any(loc => loc.Equals(to)))
                //{
                //    continue;
                //}

                //if (
                //    //路径的起点不是结束位置
                //    !route.StartLocation.Equals(to)
                //    ||
                //    (   //起点和终点一样，并且只有两个货位，且都是通配符，并且两位置相同（场景：堆垛机内部连通，*@C001,*@C001）
                //        route.Locations.Length == 2
                //        && route.StartLocation is ILocationWildcard
                //        && route.EndLocation is ILocationWildcard
                //        && route.StartLocation == route.EndLocation
                //    ))
                //{
                //    results.Add(route);
                //}
            }

#warning ？？？？
            //return results.LastOrDefault();

            return results.FirstOrDefault();
        }

        static Route getStartRoute(Route endRoute, Net net, Location currentLocation,Location toLocation)
        {
            var endRouteIndex = net.Routes.IndexOf(endRoute);
            var startRouteRange = net.Routes.Take(endRouteIndex + 1);
            List<Route> results = new List<Route>();
            foreach (var route in startRouteRange)
            {
                var currentLocationIndex = route.LocationIndexOf(currentLocation).Min();
                if (currentLocationIndex < 0)
                {
                    continue;
                }

                //找到的起点位置是路径的结束位置
                if (currentLocationIndex == route.Locations.Length - 1)
                {
                    continue;
                }

                //不允许中途执行，但当前位置又在路径中途的
                if (currentLocationIndex > 0 && route.AllowStartFromMidway == false)
                {
                    continue;
                }

                results.Add(route);

                //if (!route.Locations.Any(loc => loc.Equals(currentLocation)))
                //{
                //    continue;
                //}
                
                //if (
                //    //路径的终点不是当前位置
                //    !route.EndLocation.Equals(currentLocation)
                //    ||
                //    (   //起点和终点一样，并且只有两个货位，且都是通配符（场景：堆垛机内部连通，*@C001,*@C001）
                //        route.Locations.Length == 2
                //        && route.StartLocation is ILocationWildcard
                //        && route.EndLocation is ILocationWildcard
                //        && route.StartLocation == route.EndLocation
                //    ))
                //{
                //    results.Add(route);
                //}
            }

            foreach (var startRoute in results.ToArray())
            {
                if (startRoute == endRoute)
                {
                    if (startRoute.StartLocation == startRoute.EndLocation
                        && startRoute.StartLocation is ILocationWildcard
                        && startRoute.EndLocation is ILocationWildcard)
                    {

                    }
                    else
                    {
                        if (startRoute.LocationIndexOf(currentLocation).Min() >= endRoute.LocationIndexOf(toLocation).Max())
                        {
                            results.Remove(startRoute);
                        }
                    }
                }
            }

            return results.LastOrDefault();
        }

        /// <summary>
        /// 获取从起点到终点的整个运行轨迹(经过的所有货位)
        /// </summary>
        /// <param name="fromLocation">起点位置</param>
        /// <param name="toLocation">终点位置</param>
        /// <returns></returns>
        public static Location[] GetTrajectories(Location fromLocation, Location toLocation, RouteType findOptions)
        {
            List<Location> trajectories = new List<Location>();
            var routes = FindNextPath(null, fromLocation, toLocation, fromLocation, null, findOptions);
            
            while (routes.Count != 0)
            {
                var r=routes.First();
                trajectories.AddRange(r.Value.Locations);

                if (r.Value.EndLocation.Equals(toLocation))
                {
                    break;
                }

                routes = FindNextPath(null, r.Value.EndLocation, toLocation, r.Value.EndLocation, null, findOptions);
            }

            return trajectories.ToArray();
        }
    }
}
