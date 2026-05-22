using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 报警状态.
    /// </summary>
    [DisplayName("报警状态")]
    public class AlarmDataLog : ReceivedDataLog
    {
        [DisplayName("货位号")]
        public virtual Int32 PosNo { get; set; }
        [DisplayName("是否手动")]
        public virtual Boolean Manual { get; set; }
        [DisplayName("离线（隔离开关断开）")]
        public virtual Boolean Isolator { get; set; }
        [DisplayName("电路保护器断开（断路器断开）")]
        public virtual Boolean Breaker { get; set; }
        [DisplayName("光电异常")]
        public virtual Boolean Photocell { get; set; }
        [DisplayName("运行超时")]
        public virtual Boolean RunOvertime { get; set; }
        [DisplayName("占位超时")]
        public virtual Boolean OccupyOvertime { get; set; }
        [DisplayName("有任务无货")]
        public virtual Boolean TaskNoGoods { get; set; }
        [DisplayName("使用状态")]
        public virtual Int32 MotorUseStatus { get; set; }
        [DisplayName("X轴电机变频器故障")]
        public virtual Boolean X_MotorVAF { get; set; }
        [DisplayName("Y轴电机变频器故障")]
        public virtual Boolean Y_MotorVAF { get; set; }
        [DisplayName("X轴电机正反转接触器故障")]
        public virtual Boolean X_MotorContactor { get; set; }
        [DisplayName("X轴电机抱闸接触器故障")]
        public virtual Boolean X_MotorBraker { get; set; }
        [DisplayName("Y轴电机正反转接触器故障")]
        public virtual Boolean Y_MotorContactor { get; set; }
        [DisplayName("Y轴电机抱闸接触器故障")]
        public virtual  Boolean Y_MotorBraker { get; set; }
        [DisplayName("顶升电机正反转接触器故障")]
        public virtual Boolean Lift_MotorContactor { get; set; }
        [DisplayName("顶升电机抱闸接触器故障")]
        public virtual Boolean Lift_MotorBraker { get; set; }
        [DisplayName("预留")]
        public virtual Int32 Spare { get; set; }
        protected AlarmDataLog()
        {
        }
        public AlarmDataLog(Device device, AlarmNetTransferObject receivedData)
            : base()
        {
            this.DeviceName = device.Name;
            this.PosNo = receivedData.PosNo;
            this.Manual = receivedData.Manual;
            this.Isolator = receivedData.Isolator;
            this.Breaker = receivedData.Breaker;
            this.Photocell = receivedData.Photocell;
            this.RunOvertime = receivedData.RunOvertime;
            this.OccupyOvertime = receivedData.OccupyOvertime;
            this.TaskNoGoods = receivedData.TaskNoGoods;
            this.MotorUseStatus = receivedData.MotorUseStatus;
            this.X_MotorVAF = receivedData.X_MotorVAF;
            this.Y_MotorVAF = receivedData.Y_MotorVAF;
            this.X_MotorContactor = receivedData.X_MotorContactor;
            this.X_MotorBraker = receivedData.X_MotorBraker;
            this.Y_MotorContactor = receivedData.Y_MotorContactor;
            this.Y_MotorBraker = receivedData.Y_MotorBraker;
            this.Lift_MotorContactor = receivedData.Lift_MotorContactor;
            this.Lift_MotorBraker = receivedData.Lift_MotorBraker;
            this.Spare = receivedData.Spare;
        }
    }
}
