using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 一个完整的连通路径
    /// </summary>
    public class Net
    {
        /// <summary>
        /// 指示该连通路径所有要经过的 Route
        /// </summary>
        public DeviceRoute[] Routes{get;set;}
        /// <summary>
        /// 路径连通路径的起点
        /// </summary>
        public Location StartLocation
        {
            get
            {
                return Routes.First().StartLocation;
            }
        }
        /// <summary>
        /// 获取连通路径的终点
        /// </summary>
        public Location EndLocation
        {
            get
            {
                return Routes.Last().EndLocation;
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
        public bool CanReach(Location startLocation, Location endLocation, Location currentLocation, Int32? lastMovementRouteId, DeviceRouteType findOptions)
        {
            //路径类型不对
            if (this.Routes.Any(x => (findOptions & x.DeviceRouteType) != findOptions && (x.DeviceRouteType & findOptions) != x.DeviceRouteType))
            {
                return false;
            }

            //允许输送线任务从中途执行
            DeviceRoute startRoute;
            //如果任务还在起点，必须是路径的起点，否则可以允许输送线任务从中途执行
            if ((startLocation.Equals(currentLocation) || currentLocation == null))
            {
                startRoute = this.Routes.FirstOrDefault(x => x.StartLocation.Equals(startLocation));
            }
            else
            {
                if (lastMovementRouteId != null)
                {
                    var lastMovementRoute = this.Routes.SingleOrDefault(x => x.Id == lastMovementRouteId);
                    if (lastMovementRoute == null)
                    {
                        return false;
                    }
                    int index = this.Routes.ToList().IndexOf(lastMovementRoute);
                    if (index == this.Routes.Length - 1)
                    {
                        return false;
                    }

                    startRoute = this.Routes.SingleOrDefault(x => (x.Id == this.Routes[index + 1].Id) && (x.StartLocation.Equals(currentLocation) || (x.AllowStartFromMidway && !x.EndLocation.Equals(currentLocation) && x.Locations.Any(location => location.Equals(currentLocation)))));

                }
                else
                {
                    startRoute = this.Routes.SingleOrDefault(x => x.StartLocation.Equals(currentLocation) || (x.AllowStartFromMidway && !x.EndLocation.Equals(currentLocation) && x.Locations.Any(location => location.Equals(currentLocation))));
                }
            }
            var endRoute = this.Routes.Where(x => x.EndLocation.Equals(endLocation)).SingleOrDefault();
            if (startRoute == null || endRoute == null)
            {
                return false;
            }

            //方向不对
            if (this.Routes.ToList().IndexOf(startRoute) > this.Routes.ToList().IndexOf(endRoute))
            {
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            StringBuilder sb=new StringBuilder();
            foreach (var item in Routes)
	        {
                if(sb.Length>0)
                {
                    sb.Append(" | ");
                }
		        sb.Append(string.Join(",",item.Locations.Select(x=>x.DeviceCode).ToArray()));
	        }
            return String.Format("从 {0} 经过 {1} 到 {2}",
                this.StartLocation, 
                sb.ToString(),
                this.EndLocation);
        }
    }
}
