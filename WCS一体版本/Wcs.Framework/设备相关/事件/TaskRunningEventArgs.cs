using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    public class TaskRunningEventArgs : HandleableEventArgs
    {
        /// <summary>
        /// 设备任务号
        /// </summary>
        public Int32 EquipmentTaskId { get; private set; }
        public TaskRunningEventArgs(Int32 equipmentTaskId)
        {
            this.EquipmentTaskId = equipmentTaskId;
        }
    }
}
