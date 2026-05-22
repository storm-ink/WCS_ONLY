using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.Framework
{
    public class TaskCurrentLocationChangedEventArgs : HandleableEventArgs
    {
        /// <summary>
        /// 设备任务号
        /// </summary>
        public Int32 EquipmentTaskId { get; private set; }
        /// <summary>
        /// 当前位置
        /// </summary>
        public Location CurrentLocation { get; private set; }
        public TaskCurrentLocationChangedEventArgs(Int32 equipmentTaskId, Location currentLocation)
        {
            this.EquipmentTaskId = equipmentTaskId;
            this.CurrentLocation = currentLocation;
        }
    }
}
