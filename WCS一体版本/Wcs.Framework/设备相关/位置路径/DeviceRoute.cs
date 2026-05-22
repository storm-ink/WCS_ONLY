using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 路径（具体表现为一个 LogicMovement 对象）
    /// </summary>
    public class DeviceRoute
    {
        public DeviceRoute(Int32 id,Int32 no,Boolean enabled,Boolean allowStartFromMidway,
            DeviceRouteDirection direction,DeviceRouteType deviceRouteType,
            Location[] location,RouteStrategy strategy,Device device)
        {
            this.Id = id;
            this.No = no;
            this.Enabled = enabled;
            this.AllowStartFromMidway = allowStartFromMidway;
            this.Direction = direction;
            this.DeviceRouteType = deviceRouteType;
            this.Locations = location;
            this.Strategy = strategy;
            this.Device = device;
        }
        /// <summary>
        /// 路径 Id
        /// </summary>
        public Int32 Id { get; private set; }

        /// <summary>
        /// 路径号
        /// </summary>
        public Int32 No { get; private set; }

        /// <summary>
        /// 是否启用（默认为启用）
        /// </summary>
        public bool Enabled { get; private set; }

        /// <summary>
        /// 路径运行方向
        /// </summary>
        public DeviceRouteDirection Direction { get; private set; }

        /// <summary>
        /// 路径类型
        /// </summary>
        public DeviceRouteType DeviceRouteType { get; private set; }

        /// <summary>
        /// 此路径经过的所有货位
        /// </summary>
        public Location[] Locations { get; private set; }

        /// <summary>
        /// 邻接集合
        /// </summary>
        public DeviceRoute[] Adjoins { get; private set; }

        
        /// <summary>
        /// Route 的适用选择器
        /// </summary>
        public RouteStrategy Strategy { get; private set; }

        /// <summary>
        /// 所属设备
        /// </summary>
        public Device Device { get; private set; }

        /// <summary>
        /// 是否允许从路径的非起点位置开始执行
        /// </summary>
        public Boolean AllowStartFromMidway { get; private set; }

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
        /// 对应的动作逻辑动作类型
        /// </summary>
        public Type[] LogicMovementTypes { get; set; }
        /// <summary>
        /// 设备路径转换为逻辑动作时使用的逻辑动作类型选择器
        /// </summary>
        public IDeviceRouteToLogicMovementSelector LogicMovementSelector { get; set; }

        /// <summary>
        /// 从本路径对象创建一个逻辑动作
        /// </summary>
        /// <returns></returns>
        public LogicMovement CreateLogicMovement()
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
                return LogicMovementSelector.ToLogicMovement(this);
            }
        }


        /// <summary>
        /// 获取两个路径的交集
        /// </summary>
        /// <param name="route1">路径1</param>
        /// <param name="route2">路径2</param>
        /// <returns></returns>
        public static Location[] Intersect(DeviceRoute route1, DeviceRoute route2)
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

        /// <summary>
        /// 在指定的网络里，此路径的到达指点位置的积分数
        /// </summary>
        /// <param name="net"></param>
        /// <param name="endLocation"></param>
        /// <returns></returns>
        public Decimal GetRate(Net net, Location endLocation)
        {
            return this.Strategy.GetRate(net, this, EndLocation);
        }
        /// <summary>
        /// 创建一个当前对象的副本。
        /// </summary>
        /// <returns>
        public DeviceRoute Clone()
        {
            DeviceRoute newObject = new DeviceRoute(
                this.Id, this.No, this.Enabled, this.AllowStartFromMidway,
                this.Direction, this.DeviceRouteType, 
                this.Locations, this.Strategy, this.Device
                );

            return newObject;
        }
    }
}
