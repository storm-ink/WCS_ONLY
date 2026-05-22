using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 路径（具体表现为一个 LogicMovement 对象）
    /// </summary>
    public class Route
    {
        ReadOnlyDictionary<String, List<Int32>> _locationIndexs;
        Location[] m_locations;
        String[] _reachedLocations;
        public Route(Int32 id,Int32 no,Boolean enabled,Boolean allowStartFromMidway,
            RouteDirection direction,RouteType deviceRouteType,
            Location[] location,RouteStrategy strategy, TaskableDevice device, Type[] logicMovementTypes, LogicMovementSelector logicMovementSelector)
        {
            this.Id = id;
            this.No = no;
            this.Enabled = enabled;
            this.AllowStartFromMidway = allowStartFromMidway;
            this.Direction = direction;
            this.RouteType = deviceRouteType;
            this.Locations = location;
            this.Strategy = strategy;
            this.Device = device;
            this.LogicMovementTypes = logicMovementTypes;
            this.LogicMovementSelector = logicMovementSelector;

            refreshPoints();

            _reachedLocations = _locationIndexs.Select(x => x.Key).ToArray();

        }

        private Route(Int32 id, Int32 no, Boolean enabled, Boolean allowStartFromMidway,
            RouteDirection direction, RouteType deviceRouteType,
            Location[] locations, RouteStrategy strategy, TaskableDevice device, Type[] logicMovementTypes, LogicMovementSelector logicMovementSelector, ReadOnlyDictionary<String, List<Int32>> locationIndexs, String[] reachedLocations)
        {
            this.Id = id;
            this.No = no;
            this.Enabled = enabled;
            this.AllowStartFromMidway = allowStartFromMidway;
            this.Direction = direction;
            this.RouteType = deviceRouteType;
            this.m_locations = locations;
            this.Strategy = strategy;
            this.Device = device;
            this.LogicMovementTypes = logicMovementTypes;
            this.LogicMovementSelector = logicMovementSelector;

            this._locationIndexs = locationIndexs;

            _reachedLocations = reachedLocations;

        }

        /// <summary>
        /// 获取该路径可以经过的所有位置可转换编码集合
        /// </summary>
        public String[] ReachedLocations
        {
            get
            {
                return _reachedLocations;
            }
        }

        public Boolean IsCallAt(Location loc)
        {
            var hash = loc.ToConvertibleCode();
            return _locationIndexs.ContainsKey(hash);
        }

        /// <summary>
        /// 邻接集合
        /// </summary>
        public Route[] Adjoins { get; private set; }

        /// <summary>
        /// 是否允许从路径的非起点位置开始执行
        /// </summary>
        public Boolean AllowStartFromMidway { get; private set; }

        /// <summary>
        /// 所属设备
        /// </summary>
        public TaskableDevice Device { get; private set; }

        /// <summary>
        /// 路径运行方向
        /// </summary>
        public RouteDirection Direction { get; private set; }

        /// <summary>
        /// 是否启用（默认为启用）
        /// </summary>
        public bool Enabled { get; private set; }

        /// <summary>
        /// 终点位置
        /// </summary>
        public Location EndLocation
        {
            get
            {
                return Locations.Last();
            }
        }

        /// <summary>
        /// 路径 Id
        /// </summary>
        public Int32 Id { get; private set; }

        /// <summary>
        /// 此路径经过的所有货位
        /// </summary>
        public Location[] Locations
        {
            get
            {
                return m_locations;
            }
            private set
            {
                m_locations = value;

                refreshPoints();
            }
        }

        /// <summary>
        /// 设备路径转换为逻辑动作时使用的逻辑动作类型选择器
        /// </summary>
        public LogicMovementSelector LogicMovementSelector { get; private set; }

        /// <summary>
        /// 对应的动作逻辑动作类型
        /// </summary>
        public Type[] LogicMovementTypes { get; private set; }

        /// <summary>
        /// 路径号
        /// </summary>
        public Int32 No { get; private set; }
        /// <summary>
        /// 路径类型
        /// </summary>
        public RouteType RouteType { get; private set; }
        /// <summary>
        /// 起点位置
        /// </summary>
        public Location StartLocation
        {
            get
            {
                return Locations.First();
            }
        }

        /// <summary>
        /// Route 的适用选择器
        /// </summary>
        public RouteStrategy Strategy { get; private set; }
        /// <summary>
        /// 获取两个路径的交集
        /// </summary>
        /// <param name="route1">路径1</param>
        /// <param name="route2">路径2</param>
        /// <returns></returns>
        public static Location[] Intersect(Route route1, Route route2)
        {
            if (route1 == null)
            {
                throw new ArgumentNullException("route1");
            }

            if (route1 == null)
            {
                throw new ArgumentNullException("route2");
            }

            var intersect = route1
                .Locations
                .Intersect(
                    route2
                    .Locations
                );

            return intersect.ToArray();
        }

        public void AddAdjoins(IEnumerable<Route> routes)
        {
            if (routes == null)
            {
                throw new ArgumentNullException("routes");
            }

            if (this.Adjoins == null)
            {
                this.Adjoins = routes.ToArray();
            }
            else
            {
                //取交集
                var intersect = this.Adjoins.Intersect(routes);
                //过滤交集（已存在于连通集合）
                var validAdjoins = routes.Except(intersect);
                //合并新的连通集合集合到原连通集合
                this.Adjoins = this.Adjoins.Concat(validAdjoins).ToArray();
            }
        }

        /// <summary>
        /// 创建一个当前对象的副本。
        /// </summary>
        public Route Clone()
        {
            Location[] copyLocations = new Location[m_locations.Length];
            Array.Copy(m_locations, copyLocations, copyLocations.Length);

            Route newObject = new Route(
                this.Id, this.No, this.Enabled,
                this.AllowStartFromMidway,
                this.Direction, this.RouteType,
                copyLocations,
                this.Strategy, this.Device,
                this.LogicMovementTypes, 
                this.LogicMovementSelector,
                this._locationIndexs,
                this._reachedLocations
                );

            return newObject;
        }

        /// <summary>
        /// 从本路径对象创建一个逻辑动作
        /// </summary>
        /// <returns></returns>
        public LogicMovement CreateLogicMovement(Task task)
        {
            if (LogicMovementSelector == null)
            {
                var result = this.LogicMovementTypes[0].Assembly.CreateInstance(
                    LogicMovementTypes[0].FullName
                    , false
                    , System.Reflection.BindingFlags.CreateInstance
                    , null
                    , new object[] { this.Device, this.Id, this.StartLocation, this.EndLocation }
                    , null
                    , null);

                return (LogicMovement)result;
            }
            else
            {
                return LogicMovementSelector.ToLogicMovement(this,task);
            }
        }
        /// <summary>
        /// 在指定的网络里，此路径的到达指点位置的积分数
        /// </summary>
        /// <param name="net">连通网络</param>
        /// <param name="startLocation">业务开始位置</param>
        /// <param name="endLocation">业务结束位置</param>
        /// <returns></returns>
        public Decimal GetRate(Net net, Task task, Location startLocation, Location endLocation)
        {
            return this.Strategy.GetRate(net, this,task,startLocation, endLocation);
        }
        public override string ToString()
        {
            return string.Format("{0} Route#{1}", this.Device,this.Id);
        }
        /// <summary>
        /// 获取指定的位置在当前路径中的索引位置
        /// </summary>
        /// <param name="loc">要查找的货位</param>
        /// <returns>返回一个数组，原因是同一位置可能存在此路径的多个索引位置。这种情况大多发生在路径中包含通配符位置的时候。在未找到索引位置时将返回只包含 -1 一个元素的数组。</returns>
        public Int32[] LocationIndexOf(Location loc)
        {
            if (!_locationIndexs.ContainsKey(loc.ToConvertibleCode()))
            {
                return new Int32[]{-1};
            }

            return _locationIndexs[loc.ToConvertibleCode()].ToArray();
        }

        void addPoints(Dictionary<string, List<Int32>> container, Location loc,Int32 index)
        {
            if (container.ContainsKey(loc.ToConvertibleCode()))
            {
                container[loc.ToConvertibleCode()].Add(index);
                return;
            }

            if (loc is ILocationWildcard)
            {
                var lw = (ILocationWildcard)loc;
                foreach (var item in lw.GetMatchedLocations())
                {
                    addPoints(container, item,index);
                }
            }
            else
            {
                container.Add(loc.ToConvertibleCode(), new List<Int32>(){index});
                foreach (var item in loc.Synonymous)
                {
                    addPoints(container, item,index);
                }
            }
        }

        void refreshPoints()
        {
            Dictionary<string, List<Int32>> container = new Dictionary<string, List<int>>();
            for (int i = 0; i < this.Locations.Length; i++)
            {
                addPoints(container, this.Locations[i], i);
            }

            _locationIndexs = new ReadOnlyDictionary<string, List<Int32>>(container);
        }
    }
}
