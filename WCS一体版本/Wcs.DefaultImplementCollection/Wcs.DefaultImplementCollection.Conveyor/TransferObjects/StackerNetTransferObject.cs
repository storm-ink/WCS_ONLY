using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 叠盘机状态
    /// </summary>
    [System.ComponentModel.DisplayName("叠盘机状态")]
    [JsonObject]
    public class StackerNetTransferObject : NetTransferObject
    {
        /// <summary>
        /// 叠盘机编号
        /// </summary>
        [System.ComponentModel.DisplayName("叠盘机编号")]
        public UInt16 StackerNo { get; set; }
        /// <summary>
        /// 叠盘数量
        /// </summary>
        [System.ComponentModel.DisplayName("叠盘数量")]
        public UInt16 Stacker_Actual_Number { get; set; }
        /// <summary>
        /// 高位检测
        /// </summary>
        [System.ComponentModel.DisplayName("高位检测")]
        public Boolean High_Position_Phocll { get; set; }
        /// <summary>
        /// 中间位检测
        /// </summary>
        [System.ComponentModel.DisplayName("中间位检测")]
        public Boolean Mid_Position_Phocll { get; set; }
        /// <summary>
        /// 低位检测
        /// </summary>
        [System.ComponentModel.DisplayName("低位检测")]
        public Boolean Low_Position_Phocll { get; set; }
        /// <summary>
        /// 叠满到位检测
        /// </summary>
        [System.ComponentModel.DisplayName("叠满到位检测")]
        public Boolean Pallet_Full_Phocll { get; set; }
        /// <summary>
        /// 货叉伸出到位检测-左侧
        /// </summary>
        [System.ComponentModel.DisplayName("货叉伸出到位检测-左侧")]
        public Boolean Fork_Extend_Phocll_L { get; set; }
        /// <summary>
        /// 货叉伸出到位检测-右侧
        /// </summary>
        [System.ComponentModel.DisplayName("货叉伸出到位检测-右侧")]
        public Boolean Fork_Extend_Phocll_R { get; set; }
        /// <summary>
        /// 货叉缩回到位检测-左侧
        /// </summary>
        [System.ComponentModel.DisplayName("货叉缩回到位检测-左侧")]
        public Boolean Fork_Retract_Phocll_L { get; set; }
        /// <summary>
        /// 货叉缩回到位检测-右侧
        /// </summary>
        [System.ComponentModel.DisplayName("货叉缩回到位检测-右侧")]
        public Boolean Fork_Retract_Phocll_R { get; set; }
        /// <summary>
        /// 数据块配置错误
        /// </summary>
        [System.ComponentModel.DisplayName("数据块配置错误")]
        public Boolean Error_Configure { get; set; }
        /// <summary>
        /// 升降电机断开
        /// </summary>
        [System.ComponentModel.DisplayName("升降电机断开")]
        public Boolean LiftingMotor_Breaker { get; set; }
        /// <summary>
        /// 升降电机电路保护器断开
        /// </summary>
        [System.ComponentModel.DisplayName("升降电机电路保护器断开")]
        public Boolean LiftingMotor_VF_Alarm { get; set; }
        /// <summary>
        /// 升降电机变频器故障
        /// </summary>
        [System.ComponentModel.DisplayName("升降电机变频器故障")]
        public Boolean LiftingMotor_Contactor { get; set; }
        /// <summary>
        /// 升降电机运行故障
        /// </summary>
        [System.ComponentModel.DisplayName("升降电机运行故障")]
        public Boolean LiftingMotor_Braker { get; set; }
        /// <summary>
        /// 升降电机抱闸接触器故障
        /// </summary>
        [System.ComponentModel.DisplayName("升降电机抱闸接触器故障")]
        public Boolean ForkMotor_Breaker { get; set; }
        /// <summary>
        /// 货叉电机电路保护器断开
        /// </summary>
        [System.ComponentModel.DisplayName("货叉电机电路保护器断开")]
        public Boolean ForkMotor_Extend_Contactor { get; set; }
        /// <summary>
        /// 货叉电机伸出接触器故障
        /// </summary>
        [System.ComponentModel.DisplayName("货叉电机伸出接触器故障")]
        public Boolean ForkMotor_Retract_Contactor { get; set; }
        /// <summary>
        /// 货叉电机缩回接触器故障
        /// </summary>
        [System.ComponentModel.DisplayName("货叉电机缩回接触器故障")]
        public Boolean ForkMotor_Braker { get; set; }
        /// <summary>
        /// 急停故障
        /// </summary>
        [System.ComponentModel.DisplayName("急停故障")]
        public Boolean EStop { get; set; }
        /// <summary>
        /// 安全门故障
        /// </summary>
        [System.ComponentModel.DisplayName("安全门故障")]
        public Boolean SafeDoor { get; set; }
        /// <summary>
        /// 升降电机下降至低位超时
        /// </summary>
        [System.ComponentModel.DisplayName("升降电机下降至低位超时")]
        public Boolean RunOvertime_Low { get; set; }
        /// <summary>
        /// 升降电机下降至中间位超时
        /// </summary>
        [System.ComponentModel.DisplayName("升降电机下降至中间位超时")]
        public Boolean RunOvertime_Mid { get; set; }
        /// <summary>
        /// 升降电机上升至高位超时
        /// </summary>
        [System.ComponentModel.DisplayName("升降电机上升至高位超时")]
        public Boolean RunOvertime_High { get; set; }
        /// <summary>
        /// 货叉电机伸出超时
        /// </summary>
        [System.ComponentModel.DisplayName("货叉电机伸出超时")]
        public Boolean RunOvertime_Extend { get; set; }
        /// <summary>
        /// 货叉电机缩回超时
        /// </summary>
        [System.ComponentModel.DisplayName("货叉电机缩回超时")]
        public Boolean RunOvertime_Retract { get; set; }
        /// <summary>
        ///满料传感器检测异常
        /// </summary>
        [System.ComponentModel.DisplayName("满料传感器检测异常")]
        public Boolean RunOvertime_Full { get; set; }
        /// <summary>
        ///初始条件未满足
        /// </summary>
        [System.ComponentModel.DisplayName("初始条件未满足")]
        public Boolean Condition_OK { get; set; }
        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "StackerNo":
                        return this.StackerNo;
                    case "Stacker_Actual_Number":
                        return this.Stacker_Actual_Number;
                    case "High_Position_Phocll":
                        return this.High_Position_Phocll;
                    case "Mid_Position_Phocll":
                        return this.Mid_Position_Phocll;
                    case "Low_Position_Phocll":
                        return this.Low_Position_Phocll;
                    case "Pallet_Full_Phocll":
                        return this.Pallet_Full_Phocll;
                    case "Fork_Extend_Phocll_L":
                        return this.Fork_Extend_Phocll_L;
                    case "Fork_Extend_Phocll_R":
                        return this.Fork_Extend_Phocll_R;
                    case "Fork_Retract_Phocll_L":
                        return this.Fork_Retract_Phocll_L;
                    case "Fork_Retract_Phocll_R":
                        return this.Fork_Retract_Phocll_R;
                    case "Error_Configure":
                        return this.Error_Configure;
                    case "LiftingMotor_Breaker":
                        return this.LiftingMotor_Breaker;
                    case "LiftingMotor_VF_Alarm":
                        return this.LiftingMotor_VF_Alarm;
                    case "LiftingMotor_Contactor":
                        return this.LiftingMotor_Contactor;
                    case "LiftingMotor_Braker":
                        return this.LiftingMotor_Braker;
                    case "ForkMotor_Breaker":
                        return this.ForkMotor_Breaker;
                    case "ForkMotor_Extend_Contactor":
                        return this.ForkMotor_Extend_Contactor;
                    case "ForkMotor_Retract_Contactor":
                        return this.ForkMotor_Retract_Contactor;
                    case "ForkMotor_Braker":
                        return this.ForkMotor_Braker;
                    case "EStop":
                        return this.EStop;
                    case "SafeDoor":
                        return this.SafeDoor;
                    case "RunOvertime_Low":
                        return this.RunOvertime_Low;
                    case "RunOvertime_Mid":
                        return this.RunOvertime_Mid;
                    case "RunOvertime_High":
                        return this.RunOvertime_High;
                    case "RunOvertime_Extend":
                        return this.RunOvertime_Extend;
                    case "RunOvertime_Retract":
                        return this.RunOvertime_Retract;
                    case "RunOvertime_Full":
                        return this.RunOvertime_Full;
                    case "Condition_OK":
                        return this.Condition_OK;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的取值操作", this.GetType(), name));
                }
            }
            set
            {
                switch (name)
                {

                    case "StackerNo":
                        this.StackerNo = Convert.ToUInt16(value);
                        break;
                    case "Stacker_Actual_Number":
                        this.Stacker_Actual_Number = Convert.ToUInt16(value);
                        break;
                    case "High_Position_Phocll":
                        this.High_Position_Phocll = Convert.ToBoolean(value);
                        break;
                    case "Mid_Position_Phocll":
                        this.Mid_Position_Phocll = Convert.ToBoolean(value);
                        break;
                    case "Low_Position_Phocll":
                        this.Low_Position_Phocll = Convert.ToBoolean(value);
                        break;
                    case "Pallet_Full_Phocll":
                        this.Pallet_Full_Phocll = Convert.ToBoolean(value);
                        break;
                    case "Fork_Extend_Phocll_L":
                        this.Fork_Extend_Phocll_L = Convert.ToBoolean(value);
                        break;
                    case "Fork_Extend_Phocll_R":
                        this.Fork_Extend_Phocll_R = Convert.ToBoolean(value);
                        break;
                    case "Fork_Retract_Phocll_L":
                        this.Fork_Retract_Phocll_L = Convert.ToBoolean(value);
                        break;
                    case "Fork_Retract_Phocll_R":
                        this.Fork_Retract_Phocll_R = Convert.ToBoolean(value);
                        break;
                    case "Error_Configure":
                        this.Error_Configure = Convert.ToBoolean(value);
                        break;
                    case "LiftingMotor_Breaker":
                        this.LiftingMotor_Breaker = Convert.ToBoolean(value);
                        break;
                    case "LiftingMotor_VF_Alarm":
                        this.LiftingMotor_VF_Alarm = Convert.ToBoolean(value);
                        break;
                    case "LiftingMotor_Contactor":
                        this.LiftingMotor_Contactor = Convert.ToBoolean(value);
                        break;
                    case "LiftingMotor_Braker":
                        this.LiftingMotor_Braker = Convert.ToBoolean(value);
                        break;
                    case "ForkMotor_Breaker":
                        this.ForkMotor_Breaker = Convert.ToBoolean(value);
                        break;
                    case "ForkMotor_Extend_Contactor":
                        this.ForkMotor_Extend_Contactor = Convert.ToBoolean(value);
                        break;
                    case "ForkMotor_Retract_Contactor":
                        this.ForkMotor_Retract_Contactor = Convert.ToBoolean(value);
                        break;
                    case "ForkMotor_Braker":
                        this.ForkMotor_Braker = Convert.ToBoolean(value);
                        break;
                    case "EStop":
                        this.EStop = Convert.ToBoolean(value);
                        break;
                    case "SafeDoor":
                        this.SafeDoor = Convert.ToBoolean(value);
                        break;
                    case "RunOvertime_Low":
                        this.RunOvertime_Low = Convert.ToBoolean(value);
                        break;
                    case "RunOvertime_Mid":
                        this.RunOvertime_Mid = Convert.ToBoolean(value);
                        break;
                    case "RunOvertime_High":
                        this.RunOvertime_High = Convert.ToBoolean(value);
                        break;
                    case "RunOvertime_Extend":
                        this.RunOvertime_Extend = Convert.ToBoolean(value);
                        break;
                    case "RunOvertime_Retract":
                        this.RunOvertime_Retract = Convert.ToBoolean(value);
                        break;
                    case "RunOvertime_Full":
                        this.RunOvertime_Full = Convert.ToBoolean(value);
                        break;
                    case "Condition_OK":
                        this.Condition_OK = Convert.ToBoolean(value);
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }
        public override string ToString()
        {
            return String.Format("{0}叠盘机编号{1} 外形检测状态{2} 高位检测为 {3} 中间位检测为{4} 低位检测为 {5} 叠满到位检测" +
                "为 {6}货叉伸出到位检测—左侧为{6}", this.GetType(), this.StackerNo, this.Stacker_Actual_Number, this.High_Position_Phocll, this.Mid_Position_Phocll, this.Low_Position_Phocll, this.Pallet_Full_Phocll);
        }
        public override ReceivedDataLog ToLogData(Device device)
        {
            return new StackerDataLog(device, this);
        }
    }
}
