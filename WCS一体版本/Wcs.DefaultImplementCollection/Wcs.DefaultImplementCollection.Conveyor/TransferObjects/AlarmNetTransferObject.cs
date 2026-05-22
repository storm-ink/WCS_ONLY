using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Wcs.Framework;
using Newtonsoft.Json;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 报警状态.
    /// 配置
    ///   <collection type="Wcs.DefaultImpls.Conveyor.AlarmNetTransferObject, Wcs.DefaultImpls" blockBytes="6" itemCount="20">
    //        <property name="ShapeCheckNO" index="0" size="2" type="UInt16" />
    //        <property name="Manual" index="2" size="1" type="Boolean" />
    //        <property name="Isolator" index="2.1" size="1" type="Boolean" />
    //        <property name="Breaker" index="2.2" size="1" type="Boolean" />
    //        <property name="Photocell" index="2.3" size="1" type="Boolean" />
    //        <property name="RunOvertime" index="2.4" size="1" type="Boolean" />
    //        <property name="OccupyOvertime" index="2.5" size="1" type="Boolean" />
    //        <property name="TaskNoGoods" index="2.6" size="1" type="Boolean" />
    //        <property name="MotorUseStatus" index="3" size="1" type="Byte" />
    //        <property name="X_MotorVAF" index="4" size="1" type="Boolean" />
    //        <property name="Y_MotorVAF" index="4.1" size="1" type="Boolean" />
    //        <property name="X_MotorContactor" index="4.2" size="1" type="Boolean" />
    //        <property name="X_MotorBraker" index="4.3" size="1" type="Boolean" />
    //        <property name="Y_MotorContactor" index="4.4" size="1" type="Boolean" />
    //        <property name="Y_MotorBraker" index="4.5" size="1" type="Boolean" />
    //        <property name="Lift_MotorContactor" index="4.6" size="1" type="Boolean" />
    //        <property name="Lift_MotorBraker" index="4.7" size="1" type="Boolean" />
    //        <property name="Spare" index="5" size="1" type="Byte" />
    //      </collection>
    /// </summary>
    [DisplayName("报警状态")]
    [JsonObject]
    public class AlarmNetTransferObject : NetTransferObject
    {
        [DisplayName("货位号")]
        public UInt16 PosNo { get; set; }
        [DisplayName("手动")]
        public Boolean Manual { get; set; }
        [DisplayName("离线（隔离开关断开）")]
        public Boolean Isolator{get;set;}
        [DisplayName("断路器断开")]
        public Boolean Breaker { get; set; }
        [DisplayName("光电异常")]
        public Boolean Photocell { get; set; }
        [DisplayName("运行超时")]
        public Boolean RunOvertime { get; set; }
        [DisplayName("占位超时")]
        public Boolean OccupyOvertime { get; set; }
        [DisplayName("有任务无货")]
        public Boolean TaskNoGoods { get; set; }
        [DisplayName("使用状态")]
        public Byte MotorUseStatus{get;set;}
        [DisplayName("X轴电机变频器故障")]
        public Boolean X_MotorVAF { get; set; }
        [DisplayName("Y轴电机变频器故障")]
        public Boolean Y_MotorVAF { get; set; }
        [DisplayName("X轴电机正反转接触器故障")]
        public Boolean X_MotorContactor { get; set; }
        [DisplayName("X轴电机抱闸接触器故障")]
        public Boolean X_MotorBraker { get; set; }
        [DisplayName("Y轴电机正反转接触器故障")]
        public Boolean Y_MotorContactor { get; set; }
        [DisplayName("Y轴电机抱闸接触器故障")]
        public Boolean Y_MotorBraker { get; set; }
        [DisplayName("顶升电机正反转接触器故障")]
        public Boolean Lift_MotorContactor { get; set; }
        [DisplayName("顶升电机抱闸接触器故障")]
        public Boolean Lift_MotorBraker { get; set; }
        [DisplayName("预留")]
        public Byte Spare { get; set; }
        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "PosNo":
                        return this.PosNo;
                    case "Manual":
                        return this.Manual;
                    case "Isolator":
                        return this.Isolator;
                    case "Breaker":
                        return this.Breaker;
                    case "Photocell":
                        return this.Photocell;
                    case "RunOvertime":
                        return this.RunOvertime;
                    case "OccupyOvertime":
                        return this.OccupyOvertime;
                    case "TaskNoGoods":
                        return this.TaskNoGoods;
                    case "MotorUseStatus":
                        return this.MotorUseStatus;
                    case "X_MotorVAF":
                        return this.X_MotorVAF;
                    case "Y_MotorVAF":
                        return this.Y_MotorVAF;
                    case "X_MotorContactor":
                        return this.X_MotorContactor;
                    case "X_MotorBraker":
                        return this.X_MotorBraker;
                    case "Y_MotorContactor":
                        return this.Y_MotorContactor;
                    case "Y_MotorBraker":
                        return this.Y_MotorBraker;
                    case "Lift_MotorContactor":
                        return this.Lift_MotorContactor;
                    case "Lift_MotorBraker":
                        return this.Lift_MotorBraker;
                    case "Spare":
                        return this.Spare;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的取值操作", this.GetType(), name));
                }
            }
            set
            {
                switch (name)
                {
                    case "PosNo":
                        this.PosNo = Convert.ToUInt16(value);
                        break;
                    case "Manual":
                        this.Manual = Convert.ToBoolean(value);
                        break;
                    case "Isolator":
                        this.Isolator = Convert.ToBoolean(value);
                        break;
                    case "Breaker":
                        this.Breaker = Convert.ToBoolean(value);
                        break;
                    case "Photocell":
                        this.Photocell = Convert.ToBoolean(value);
                        break;
                    case "RunOvertime":
                        this.RunOvertime = Convert.ToBoolean(value);
                        break;
                    case "OccupyOvertime":
                        this.OccupyOvertime = Convert.ToBoolean(value);
                        break;
                    case "TaskNoGoods":
                        this.TaskNoGoods = Convert.ToBoolean(value);
                        break;
                    case "MotorUseStatus":
                        this.MotorUseStatus = Convert.ToByte(value);
                        break;
                    case "X_MotorVAF":
                        this.X_MotorVAF = Convert.ToBoolean(value);
                        break;
                    case "Y_MotorVAF":
                        this.Y_MotorVAF = Convert.ToBoolean(value);
                        break;
                    case "X_MotorContactor":
                        this.X_MotorContactor = Convert.ToBoolean(value);
                        break;
                    case "X_MotorBraker":
                        this.X_MotorBraker = Convert.ToBoolean(value);
                        break;
                    case "Y_MotorContactor":
                        this.Y_MotorContactor = Convert.ToBoolean(value);
                        break;
                    case "Y_MotorBraker":
                        this.Y_MotorBraker = Convert.ToBoolean(value);
                        break;
                    case "Lift_MotorContactor":
                        this.Lift_MotorContactor = Convert.ToBoolean(value);
                        break;
                    case "Lift_MotorBraker":
                        this.Lift_MotorBraker = Convert.ToBoolean(value);
                        break;
                    case "Spare":
                        this.Spare = Convert.ToByte(value);
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }

        /// <summary>
        /// 获取报警信息.
        /// </summary>
        public String[] ToAlarms()
        {
            List<String> result = new List<string>();
            Type type=this.GetType();
            foreach (var property in type.GetProperties().Where(x=>x.PropertyType==typeof(Boolean)))
            {
                if ((Boolean)property.GetValue(this, null))
                {
                    result.Add(property.GetDisplayName());
                }
            }

            return result.ToArray();
        }

        public override ReceivedDataLog ToLogData(Device device)
        {
            return new AlarmDataLog(device, this);
        }
    }
}
