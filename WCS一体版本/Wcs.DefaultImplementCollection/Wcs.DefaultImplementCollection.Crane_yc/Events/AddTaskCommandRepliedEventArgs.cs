using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 向设备发送新任务命令回复事件数据
    /// </summary>
    public class AddTaskCommandRepliedEventArgs : HandleableEventArgs
    {
        /// <summary>
        /// 设备任务号
        /// </summary>
        public Int32 EquipmentTaskId { get; private set; }
        /// <summary>
        /// 设备对新任务命令的处理结果
        /// </summary>
        public AddTaskCommandReplyTelexTransferObjectResult Result { get; private set; }
        /// <summary>
        /// 默认构造函数
        /// </summary>
        /// <param name="equipmentTaskId">设备任务号</param>
        /// <param name="column">当前所在列</param>
        /// <param name="level">当前所在层</param>
        /// <param name="rackLocation">当前所在的货架位置（可为 null）</param>
        public AddTaskCommandRepliedEventArgs(Int32 equipmentTaskId, AddTaskCommandReplyTelexTransferObjectResult result)
        {
            this.EquipmentTaskId = equipmentTaskId;
            this.Result = result;
        }
    }
}
