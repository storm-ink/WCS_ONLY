using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    public class TaskCompletedEventArgs : HandleableEventArgs
    {
        /// <summary>
        /// 设备任务号
        /// </summary>
        public Int32 EquipmentTaskId { get; private set; }
        public TaskCompletedEventArgs(Int32 equipmentTaskId)
        {
            this.EquipmentTaskId = equipmentTaskId;
        }
    }
}
