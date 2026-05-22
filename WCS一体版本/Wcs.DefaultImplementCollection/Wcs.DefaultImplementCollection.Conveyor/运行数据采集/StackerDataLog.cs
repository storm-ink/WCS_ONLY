using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    [System.ComponentModel.DisplayName("叠盘机状态")]
     public class StackerDataLog : ReceivedDataLog
    {
        protected StackerDataLog()
        {
        }
        [System.ComponentModel.DisplayName("叠盘机编号")]
        public UInt16 StackerNo { get; set; }

        [System.ComponentModel.DisplayName("外形检测状态")]
        public UInt16 Stacker_Actual_Number { get; set; }
      
        [System.ComponentModel.DisplayName("高位检测")]
        public Boolean High_Position_Phocll { get; set; }
        
        [System.ComponentModel.DisplayName("中间位检测")]
        public Boolean Mid_Position_Phocll { get; set; }
      
        [System.ComponentModel.DisplayName("低位检测")]
        public Boolean Low_Position_Phocll { get; set; }
      
        [System.ComponentModel.DisplayName("叠满到位检测")]
        public Boolean Pallet_Full_Phocll { get; set; }
       
        [System.ComponentModel.DisplayName("货叉伸出到位检测-左侧")]
        public Boolean Fork_Extend_Phocll_L { get; set; }
       
        [System.ComponentModel.DisplayName("货叉伸出到位检测-右侧")]
        public Boolean Fork_Extend_Phocll_R { get; set; }
       
        [System.ComponentModel.DisplayName("货叉缩回到位检测-左侧")]
        public Boolean Fork_Retract_Phocll_L { get; set; }
       
        [System.ComponentModel.DisplayName("货叉缩回到位检测-右侧")]
        public Boolean Fork_Retract_Phocll_R { get; set; }
      
        [System.ComponentModel.DisplayName("数据块配置错误")]
        public Boolean Error_Configure { get; set; }
       
        [System.ComponentModel.DisplayName("升降电机断开")]
        public Boolean LiftingMotor_Breaker { get; set; }
       
        [System.ComponentModel.DisplayName("升降电机电路保护器断开")]
        public Boolean LiftingMotor_VF_Alarm { get; set; }
      
        [System.ComponentModel.DisplayName("升降电机变频器故障")]
        public Boolean LiftingMotor_Contactor { get; set; }
      
        [System.ComponentModel.DisplayName("升降电机运行故障")]
        public Boolean LiftingMotor_Braker { get; set; }
      
        [System.ComponentModel.DisplayName("升降电机抱闸接触器故障")]
        public Boolean ForkMotor_Breaker { get; set; }
       
        [System.ComponentModel.DisplayName("货叉电机电路保护器断开")]
        public Boolean ForkMotor_Extend_Contactor { get; set; }
      
        [System.ComponentModel.DisplayName("货叉电机伸出接触器故障")]
        public Boolean ForkMotor_Retract_Contactor { get; set; }
       
        [System.ComponentModel.DisplayName("货叉电机缩回接触器故障")]
        public Boolean ForkMotor_Braker { get; set; }
      
        [System.ComponentModel.DisplayName("急停故障")]
        public Boolean EStop { get; set; }
       
        [System.ComponentModel.DisplayName("安全门故障")]
        public Boolean SafeDoor { get; set; }
      
        [System.ComponentModel.DisplayName("升降电机下降至低位超时")]
        public Boolean RunOvertime_Low { get; set; }
       
        [System.ComponentModel.DisplayName("升降电机下降至中间位超时")]
        public Boolean RunOvertime_Mid { get; set; }
      
        [System.ComponentModel.DisplayName("升降电机上升至高位超时")]
        public Boolean RunOvertime_High { get; set; }
      
        [System.ComponentModel.DisplayName("货叉电机伸出超时")]
        public Boolean RunOvertime_Extend { get; set; }
      
        [System.ComponentModel.DisplayName("货叉电机缩回超时")]
        public Boolean RunOvertime_Retract { get; set; }
       
        [System.ComponentModel.DisplayName("满料传感器检测异常")]
        public Boolean RunOvertime_Full { get; set; }
       
        [System.ComponentModel.DisplayName("初始条件未满足")]
        public Boolean Condition_OK { get; set; }
        public StackerDataLog(Device device, StackerNetTransferObject receivedData) : base()
        {
            this.StackerNo = StackerNo;
            this.Stacker_Actual_Number = Stacker_Actual_Number;
            this.High_Position_Phocll = High_Position_Phocll;
            this.Mid_Position_Phocll = Mid_Position_Phocll;
            this.Low_Position_Phocll = Low_Position_Phocll;
            this.Pallet_Full_Phocll = Pallet_Full_Phocll;
            this.Fork_Extend_Phocll_L = Fork_Extend_Phocll_L;
            this.Fork_Extend_Phocll_R = Fork_Extend_Phocll_R;
            this.Fork_Retract_Phocll_L = Fork_Retract_Phocll_L;
            this.Fork_Retract_Phocll_R = Fork_Retract_Phocll_R;
            this.Error_Configure = Error_Configure;
            this.LiftingMotor_Breaker = LiftingMotor_Breaker;
            this.LiftingMotor_VF_Alarm = LiftingMotor_VF_Alarm;
            this.LiftingMotor_Contactor = LiftingMotor_Contactor;
            this.LiftingMotor_Braker = LiftingMotor_Braker;
            this.ForkMotor_Breaker = ForkMotor_Breaker;
            this.ForkMotor_Extend_Contactor = ForkMotor_Extend_Contactor;
            this.ForkMotor_Retract_Contactor = ForkMotor_Retract_Contactor;
            this.ForkMotor_Braker = ForkMotor_Braker;
            this.EStop = EStop;
            this.SafeDoor = SafeDoor;
            this.RunOvertime_Low = RunOvertime_Low;
            this.RunOvertime_Mid = RunOvertime_Mid;
            this.RunOvertime_High = RunOvertime_High;
            this.RunOvertime_Extend = RunOvertime_Extend;
            this.RunOvertime_Retract = RunOvertime_Retract;
            this.RunOvertime_Full = RunOvertime_Full;
            this.Condition_OK = Condition_OK;
        }
    }
}
