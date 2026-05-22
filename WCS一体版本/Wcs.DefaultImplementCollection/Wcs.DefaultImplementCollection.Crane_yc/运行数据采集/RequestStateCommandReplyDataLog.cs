using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 表示一个堆垛机状态回报报文数据日志
    /// </summary>
    public class RequestStateCommandReplyDataLog : ReceivedDataLog
    {
        /// <summary>
        /// 堆垛机状态
        /// </summary>
        public virtual CraneStatus State { get; set; }
        /// <summary>
        /// 当前所在列
        /// </summary>
        public virtual Int32 Column { get; set; }
        /// <summary>
        /// 当前所在层
        /// </summary>
        public virtual Int32 Level { get; set; }
        /// <summary>
        /// 货叉水平位置状态
        /// </summary>
        public virtual ForkHorizontalPosition ForkHorizontalPosition { get; set; }
        /// <summary>
        /// 货叉上下位置状态
        /// </summary>
        public virtual ForkVerticalPosition ForkVerticalPosition { get; set; }
        /// <summary>
        /// 指示堆垛机当前是否在站点位置
        /// </summary>
        public virtual Boolean AtStation { get; set; }
        /// <summary>
        /// 错误码（默认为 空）
        /// </summary>
        public virtual string ErrorCode { get; set; }
        /// <summary>
        /// 堆垛机事件
        /// </summary>
        public virtual CraneEvent Event { get; set; }
        /// <summary>
        /// 当前任务号
        /// </summary>
        public virtual String TaskId { get; set; }
        /// <summary>
        /// 原始报文信息
        /// </summary>
        public virtual String Telex { get; set; }
        protected RequestStateCommandReplyDataLog()
            : base()
        {

        }
        public RequestStateCommandReplyDataLog(Device device, RequestStateCommandReplyTelexTransferObject receivedData)
            : this()
        {
            this.AtStation = receivedData.AtStation;
            this.Column = receivedData.Column;
            this.DeviceName = device.Name;
            this.ErrorCode = receivedData.ErrorCode;
            this.Event = receivedData.Event;
            this.ForkHorizontalPosition = receivedData.ForkHorizontalPosition;
            this.ForkVerticalPosition = receivedData.ForkVerticalPosition;
            this.Level = receivedData.Level;
            this.State = receivedData.State;
            this.TaskId = receivedData.TaskId;
            this.Telex = receivedData.ToTelex().ToString();
        }
    }
}
