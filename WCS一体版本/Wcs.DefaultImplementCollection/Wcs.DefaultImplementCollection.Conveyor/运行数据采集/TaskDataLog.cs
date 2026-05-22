using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    [System.ComponentModel.DisplayName("任务块")]
    public class TaskDataLog :ReceivedDataLog
    {
        [System.ComponentModel.DisplayName("握手变量")]
        public virtual TaskHandShakes HandShake { get; set; }
        [System.ComponentModel.DisplayName("任务号")]
        public virtual Int32 AssignmentID { get; set; }
        [System.ComponentModel.DisplayName("托盘号")]
        public virtual Int32 TU_ID { get; set; }
        [System.ComponentModel.DisplayName("托盘类型")]
        public virtual Int32 TU_Type { get; set; }
        [System.ComponentModel.DisplayName("业务数据")]
        public virtual Int32 IO_Data { get; set; }
        [System.ComponentModel.DisplayName("路径号")]
        public virtual Int32 RotingNo { get; set; }
        [System.ComponentModel.DisplayName("起点位置")]
        public virtual Int32 StartMotorNo { get; set; }
        [System.ComponentModel.DisplayName("终点位置")]
        public virtual Int32 DestinationNo { get; set; }
        [System.ComponentModel.DisplayName("任务状态")]
        public virtual TaskNetTransferObjectStatus TaskStatus { get; set; }
        [System.ComponentModel.DisplayName("随机数")]
        public virtual Int32 Data_ID { get; set; }
        protected TaskDataLog():base()
        {
        }
        public TaskDataLog(Device device, TaskNetTransferObject receivedData)
            : this()
        {
            this.DeviceName = device.Name;
            this.HandShake = receivedData.HandShake;
            this.AssignmentID = Convert.ToInt32(receivedData.AssignmentID);
            this.TU_ID = receivedData.TUID;
            this.IO_Data = receivedData.IO_Data;
            this.RotingNo = receivedData.RotingNo;
            this.StartMotorNo = receivedData.StartMotorNo;
            this.DestinationNo = receivedData.DestinationNo;
            this.TaskStatus = receivedData.TaskStatus;
            this.Data_ID =Convert.ToInt32(receivedData.Data_ID);
        }
       
    }
}
