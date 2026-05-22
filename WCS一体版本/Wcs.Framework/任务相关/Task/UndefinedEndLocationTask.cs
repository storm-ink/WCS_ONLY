using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{    
    /// <summary>
    /// 终点未明的任务。
    /// 说终点未明，其实不是很准备。该类型的任务终点是一个由业务系统限制的位置范围，而且在任务创建完成后应该在这个允许的位置范围内指定一个初始的 EndLocation
    /// 上位机在下发任务时，指定一个以上的可选位置当做任务终点。Wcs将货物按一定的策略送到这些位置中的任意一个即完成。
    /// 此类型的任务在每一步完成后都会重新规划一个新的结束点位置，直到已到达指定位置范围内。
    /// </summary>
    public class UndefinedEndLocationTask:Wcs.Framework.Task
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        protected UndefinedEndLocationTask()
            :base()
        {
            AvailableEndLocations = new Iesi.Collections.Generic.HashedSet<LocationInfo>();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="taskCode">任务号</param>
        /// <param name="startLocation">起始位置</param>
        /// <param name="endLocation">终点位置</param>
        public UndefinedEndLocationTask(String taskCode, LocationInfo startLocation, LocationInfo endLocation)
            : base(taskCode, startLocation,endLocation)
        {
            AvailableEndLocations = new Iesi.Collections.Generic.HashedSet<LocationInfo>();
        }

        /// <summary>
        /// 重新分配新的结束位置。
        /// 该方法应该在每一个逻辑动作完成后调用。
        /// </summary>
        /// <returns></returns>
        protected internal virtual void AssignNewEndLocation()
        {
            var newEndLocation = GetNewEndLocation();
            if (newEndLocation != null && !newEndLocation.Equals(this.EndLocation))
            {
                this.EndLocation = LocationConverter.ToLocationInfo(newEndLocation);
            }
        }
        /// <summary>
        /// 获取一个新的结束位置。
        /// 如果没有新的位置可以分配了，则应返回 null或原结束位置。
        /// </summary>
        /// <returns></returns>
        public virtual Location GetNewEndLocation()
        {
            var loc = this.AvailableEndLocations.FirstOrDefault();
            if (loc == null)
            {
                return null;
            }

            return LocationConverter.ToLocation(loc);
        }

        /// <summary>
        /// 任务的可选结束位置集合
        /// </summary>
        public virtual Iesi.Collections.Generic.ISet<LocationInfo> AvailableEndLocations { get; protected set; }
    }
}
