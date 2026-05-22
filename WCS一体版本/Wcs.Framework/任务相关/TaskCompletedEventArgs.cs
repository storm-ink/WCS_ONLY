using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
{
    public class TaskCompletedEventArgs:EventArgs
    {
        /// <summary>
        /// 设备任务号
        /// </summary>
        public Int32 EquipmentTaskId { get; private set; }
        /// <summary>
        /// 指示此事件是否已被处理
        /// </summary>
        public Boolean Handled { get; set; }
        public TaskCompletedEventArgs(Int32 equipmentTaskId)
        {
            this.EquipmentTaskId = equipmentTaskId;
            this.Handled = false;
        }
    }
}
