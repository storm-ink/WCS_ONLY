using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 一个完整的连通路径
    /// </summary>
    public class Net
    {
        ReadOnlyDictionary<String, Location> _points;
        static ReadOnlyDictionary<String, Location> _locationDictionary;

        static Int32 nnid = 0;

        Route[] routes;
        /// <summary>
        /// 默认构造函数
        /// </summary>
        /// <param name="routes">连通网络包含的有向的临接边集合</param>
        public Net(Route[] routes,Cfg.WcsConfiguration cfg)
        {
            if (_locationDictionary == null)
            {
                _locationDictionary = cfg.LocationCollection.AsDictionary();
            }

            this.routes = routes;

            //foreach (var item in routes.SelectMany(x => x.Locations))
            //{
            //    addPoints(source, item);
            //}

            //Dictionary<string, Location> source = new Dictionary<string, Location>();

            //foreach (var route in routes)
            //{

            //    foreach (var item in route.ReachedLocations)
            //    {
            //        if (source.ContainsKey(item))
            //        {
            //            continue;
            //        }
            //        source.Add(item, _locationDictionary[item]);
            //    }
            //}

            //_points = new ReadOnlyDictionary<string, Location>(source);
        }

        /// <summary>
        /// 获取连通路径的终点
        /// </summary>
        public Location EndLocation
        {
            get
            {
                return routes[routes.Length - 1].EndLocation;
            }
        }

        object _pointsLocker = new object();
        /// <summary>
        /// 获取此连通网络经过的所有点（货位）
        /// </summary>
        [Obsolete("该属性将永远返回一个空字典，如果需要判断是否停靠某位置，请使用IsCallAt方法")]
        public ReadOnlyDictionary<String, Location> Points
        {
            get
            {
                //lock (_pointsLocker)
                //{
                //    if (_points == null || _points.Count == 0)
                //    {
                //        Dictionary<string, Location> source = new Dictionary<string, Location>();

                //        foreach (var route in routes)
                //        {

                //            foreach (var item in route.ReachedLocations)
                //            {
                //                if (source.ContainsKey(item))
                //                {
                //                    continue;
                //                }
                //                source.Add(item, _locationDictionary[item]);
                //            }
                //        }

                //        _points = new ReadOnlyDictionary<string, Location>(source);
                //    }
                //}

                return _points;
            }
        }

        /// <summary>
        /// 返回一个值，指示该连通是否经过或依靠指定的位置
        /// </summary>
        /// <param name="loc">指定的位置</param>
        /// <returns></returns>
        public Boolean IsCallAt(Location loc)
        {
            return this.routes.Any(x => x.IsCallAt(loc));
        }

        /// <summary>
        /// 指示该连通路径所有要经过的 Route
        /// </summary>
        public Route[] Routes
        {
            get
            {
                return routes;
            }
            private set
            {
                routes = value;

                Dictionary<string, Location> source = new Dictionary<string, Location>();
                foreach (var item in routes.SelectMany(x => x.Locations))
                {
                    addPoints(source, item);
                }

                _points = new ReadOnlyDictionary<string, Location>(source);
            }
        }

        /// <summary>
        /// 路径连通路径的起点
        /// </summary>
        public Location StartLocation
        {
            get
            {
                return routes[0].StartLocation;
            }
        }

        /// <summary>
        /// 指示两个位置是否连通.
        /// </summary>
        /// <param name="startLocation">        起点位置. </param>
        /// <param name="endLocation">          结束位置. </param>
        /// <param name="currentLocation">      货物所在当前位置. </param>
        /// <param name="lastMovementRouteId">  任务中最后一个逻辑动作的路径 id.通常用在判断一个任务的下任务能用的路径时使用 </param>
        /// <param name="findOptions">          可用的搜索路径类型. </param>
        public bool CanReach(Location startLocation, Location endLocation, Location currentLocation, Int32? lastMovementRouteId, RouteType findOptions)
        {
            //路径类型不对
            if (this.routes.Any(x => (findOptions & x.RouteType) != findOptions && (x.RouteType & findOptions) != x.RouteType))
            {
                return false;
            }

            Location _currLoc = currentLocation ?? startLocation;

            var startRoute = findStartRoute(this, _currLoc);

            if (startRoute == null)
            {
                return false;
            }

            //如果结束位置和开始位置在同一路径当中
            //var startLocIndex = startRoute.Locations.IndexOf(_currLoc);
            var startLocIndex = startRoute.LocationIndexOf(_currLoc).Min();
            if (startRoute.Locations.Skip(startLocIndex + 1).Any(loc => loc.Equals(endLocation)))
            {
                return true;
            }

            Int32 toLocationIndex;
            var endRoute = findEndRoute(this, this.Routes.IndexOf(startRoute), endLocation,startLocIndex, out toLocationIndex);

            //如果开始路径和结束路径一样，说明只需要运行一条任务就能从起点到达终点，此时应该判断起点是否在终点前面（方向）
            if (startRoute.Equals(endRoute))
            {
                //开始位置和结束位置相等的场景：*@C001,*@C001
                return startLocIndex < toLocationIndex;
                //return startLocIndex <= endRoute.Locations.IndexOf(endLocation);
            }

            if (endRoute != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in routes)
            {
                if (sb.Length > 0)
                {
                    sb.Append(" | ");
                }
                sb.Append(string.Join(",", item.Locations.Select(x => x.DeviceCode).ToArray()));
            }
            return String.Format("从 {0} 经过 {1} 到 {2}",
                routes.First(),
                sb.ToString(),
                routes.Last());
        }

        void addPoints(Dictionary<string, Location> container, Location loc)
        {
            if (container.ContainsKey(loc.ToConvertibleCode()))
            {
                return;
            }

            if (loc is ILocationWildcard)
            {
                var lw = (ILocationWildcard)loc;
                foreach (var item in lw.GetMatchedLocations())
                {
                    addPoints(container, item);
                }
            }
            else
            {
                container.Add(loc.ToConvertibleCode(), loc);
                foreach (var item in loc.Synonymous)
                {
                    addPoints(container, item);
                }
            }
        }
        Route findEndRoute(Net net, Int32 startRouteIndex, Location to,Int32 startLocIndex,out Int32 toLocationIndex)
        {
            toLocationIndex = -1;

            List<Route> results = new List<Route>();
            foreach (var route in net.Routes.Skip(startRouteIndex))
            {
                toLocationIndex = route.LocationIndexOf(to).Min();
                if (toLocationIndex < 0)
                {
                    continue;
                }

                //找到的终点位置是路径的起点位置，则再从后向前找一次
                if (toLocationIndex == 0)
                {
                    toLocationIndex = route.LocationIndexOf(to).Max();
                }

                if (toLocationIndex==0 && route.StartLocation == route.EndLocation && route.StartLocation is ILocationWildcard && route.EndLocation is ILocationWildcard)
                {
                    toLocationIndex = route.Locations.Length - 1;
                }

                //if (toLocationIndex == startLocIndex && net.Routes[startRouteIndex] == route)
                //{
                //    //如果是通配符路径，直接把终点位置弄到最后
                //    if (route.StartLocation.Equals(route.Locations[startLocIndex])
                //        && route.StartLocation.Equals(to)
                //        && route.EndLocation.Equals(route.Locations[startLocIndex])
                //        && route.EndLocation.Equals(to)
                //        )
                //    {
                //        toLocationIndex = route.Locations.Length - 1;
                //    }
                //    else
                //    {
                //        continue;
                //    }
                //}

                //经过两次查找还是不符合要求
                if (toLocationIndex == 0 || toLocationIndex == startRouteIndex)
                {
                    continue;
                }

                //不允许中途执行，但结束位置又在路径中途的
                if (toLocationIndex != route.Locations.Length - 1 && route.AllowStartFromMidway == false)
                {
                    continue;
                }

                results.Add(route);
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

            return results.FirstOrDefault();
        }

        Route findStartRoute(Net net, Location currentLocation)
        {
            List<Route> results = new List<Route>();
            foreach (var route in net.Routes)
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

            return results.FirstOrDefault();
        }
    }
}
