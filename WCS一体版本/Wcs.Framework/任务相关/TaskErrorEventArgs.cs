using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
{
    public class TaskErrorEventArgs:EventArgs
    {
        /// <summary>
        /// 设备任务号
        /// </summary>
        public Int32 EquipmentTaskId{get;private set;}
        /// <summary>
        /// 错误编码
        /// </summary>
        public String ErrorCode { get; private set; }
        /// <summary>
        /// 错误描述
        /// </summary>
        public String ErrorDescription { get; private set; }
        /// <summary>
        /// 指示此事件是否已被处理
        /// </summary>
        public Boolean Handled { get; set; }
        public TaskErrorEventArgs(Int32 equipmentTaskId,String code, String description)
        {
            this.EquipmentTaskId = equipmentTaskId;
            this.ErrorCode = code;
            this.ErrorDescription = description;
            this.Handled = false;
        }
    }
}
