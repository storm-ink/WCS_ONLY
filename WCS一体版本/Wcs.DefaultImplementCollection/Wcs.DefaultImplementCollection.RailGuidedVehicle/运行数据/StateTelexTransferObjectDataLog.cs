using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{

    /// <summary>
    /// 表示一个穿梭车状态回报文 数据日志
    /// </summary>
    public class StateTelexTransferObjectDataLog : ReceivedDataLog
    {
        /// <summary>
        /// 位置值
        /// </summary>
        public virtual Int32 Position { get; protected set; }

        /// <summary>
        /// 当前站台号
        /// </summary>
        public virtual Int32 CurrentStation { get; protected set; }

        /// <summary>
        /// 指示穿梭车当前是否在站点位置
        /// </summary>
        public virtual Boolean AtStation { get; protected set; }

        /// <summary>
        /// 错误码（默认为 0）
        /// </summary>
        public virtual string ErrorCode { get; protected set; }


        /// <summary>
        /// 穿梭车状态
        /// </summary>
        public virtual RailGuidedVehicleStatus State { get; protected set; }

        /// <summary>
        /// 穿梭车事件
        /// </summary>
        public virtual RailGuidedVehicleEvent Event { get; protected set; }


        /// <summary>
        /// 当前任务号
        /// </summary>
        public virtual String TaskId { get; protected set; }

        /// <summary>
        /// 不知道什么玩意 干什么用的
        /// </summary>
        public virtual Int32 ContainerCode { get; protected set; }

        /// <summary>
        /// 起点
        /// </summary>
        public virtual Int32 FromStation { get; protected set; }

        /// <summary>
        /// 目的的
        /// </summary>
        public virtual Int32 ToStation { get; protected set; }

        /// <summary>
        /// 任务模式
        /// </summary>
        public virtual RailGuidedVehicleTaskMode TaskMode { get; protected set; }
        /// <summary>
        /// 原始报文信息
        /// </summary>
        public virtual String Telex { get; protected set; }
        protected StateTelexTransferObjectDataLog()
            : base()
        {

        }
        public StateTelexTransferObjectDataLog(Device device, StateTelexTransferObject receivedData)
            : this()
        {
            this.AtStation = receivedData.AtStation;
            this.ContainerCode = Convert.ToInt32(receivedData.ContainerCode);
            this.CreatedAt = DateTime.Now;
            this.CurrentStation = receivedData.CurrentStation;
            this.DataLevel = ReceivedDataLogLevel.Normal;
            this.DeviceName = device.Name;
            this.ErrorCode = receivedData.ErrorCode;
            this.Event = receivedData.Event;
            this.FromStation = receivedData.FromStation;
            this.Position = Convert.ToInt32(receivedData.Position);
            this.State = receivedData.State;
            this.TaskId = receivedData.TaskId;
            this.TaskMode = receivedData.TaskMode;
            this.Telex = receivedData.ToTelex();
            this.ToStation = receivedData.ToStation;
        }

    }
}
