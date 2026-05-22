using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 任务当前位置改变事件数据
    /// </summary>
    public class TaskCurrentLocationChangedEventArgs : HandleableEventArgs
    {
        /// <summary>
        /// 设备任务号
        /// </summary>
        public Int32 EquipmentTaskId { get; private set; }
        /// <summary>
        /// 当前所在列
        /// </summary>
        public Int32 Column { get; private set; }
        /// <summary>
        /// 当前所在层
        /// </summary>
        public Int32 Level { get; private set; }
        /// <summary>
        /// 当前所在的货架位置（可能为 null）
        /// </summary>
        public RackLocation RackLocation { get; private set; }
        /// <summary>
        /// 默认构造函数
        /// </summary>
        /// <param name="equipmentTaskId">设备任务号</param>
        /// <param name="column">当前所在列</param>
        /// <param name="level">当前所在层</param>
        /// <param name="rackLocation">当前所在的货架位置（可为 null）</param>
        public TaskCurrentLocationChangedEventArgs(Int32 equipmentTaskId,Int32 column,Int32 level, RackLocation rackLocation)
        {
            this.EquipmentTaskId = equipmentTaskId;
            this.Column = column;
            this.Level = level;
            this.RackLocation = rackLocation;
        }
    }
}
