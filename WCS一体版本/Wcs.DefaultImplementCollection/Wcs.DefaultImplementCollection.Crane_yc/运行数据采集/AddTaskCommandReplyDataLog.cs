using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 发送 <see cref="T:AddTaskCommand"/> 命令给堆垛机后，堆垛机的应答状态报文数据日志
    /// </summary>
    public class AddTaskCommandReplyDataLog : ReceivedDataLog
    {
        /// <summary>
        /// 设备处理结果
        /// </summary>
        public virtual AddTaskCommandReplyTelexTransferObjectResult Result { get;  set; }
        /// <summary>
        /// 任务号
        /// </summary>
        public virtual String TaskId { get; set; }
        /// <summary>
        /// 原始报文信息
        /// </summary>
        public virtual String Telex { get; set; }
        protected AddTaskCommandReplyDataLog()
            : base()
        {
        }
        public AddTaskCommandReplyDataLog(Device device, AddTaskCommandReplyTelexTransferObject receivedData)
            : this()
        {
            this.DeviceName = device.Name;
            this.Result = receivedData.Result;
            this.TaskId = receivedData.TaskId;
            this.Telex = receivedData.ToTelex().ToString();
        }
    }
}
