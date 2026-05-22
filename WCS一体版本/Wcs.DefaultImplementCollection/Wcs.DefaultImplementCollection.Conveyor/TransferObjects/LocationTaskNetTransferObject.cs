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
    /// 货位当前任务
    /// 配置
    ///   <collection type="Wcs.DefaultImpls.Conveyor.LocationTaskNetTransferObject, Wcs.DefaultImpls" blockBytes="10" itemCount="1">
    ///     <property name="ShapeCheckNO" index="0" size="2" type="UInt16" />
    ///     <property name="TaskNo" index="2" size="4" type="UInt32" />
    ///     <property name="TUID" index="6" size="2" type="UInt16" />
    ///     <property name="Str_Rcv_X" index="8.0" size="1" type="Boolean" />
    ///     <property name="Fnh_Rcv_X" index="8.1" size="1" type="Boolean" />
    ///     <property name="Rqs_Snt" index="8.2" size="1" type="Boolean" />
    ///     <property name="Rcv_Rdy" index="8.3" size="1" type="Boolean" />
    ///     <property name="Str_Rcv_Y" index="8.4" size="1" type="Boolean" />
    ///     <property name="Fnh_Rcv_Y" index="8.5" size="1" type="Boolean" />
    ///   </collection>
    /// </summary>
    [DisplayName("货位当前任务")]
    [JsonObject]
    public class LocationTaskNetTransferObject : NetTransferObject
    {
        [DisplayName("货位号")]
        public UInt16 PosNo { get; set; }
        [DisplayName("任务号")]
        public UInt32 TaskNo { get; set; }
        [DisplayName("托盘号")]
        public UInt16 TUID { get; set; }
        [DisplayName("X轴开始接收")]
        public Boolean Str_Rcv_X { get; set; }
        [DisplayName("X轴接收完成")]
        public Boolean Fnh_Rcv_X { get; set; }
        [DisplayName("请求传递")]
        public Boolean Rqs_Snt { get; set; }
        [DisplayName("接收Reday")]
        public Boolean Rcv_Rdy { get; set; }
        [DisplayName("Y轴开始接收")]
        public Boolean Str_Rcv_Y { get; set; }
        [DisplayName("Y轴接收完成")]
        public Boolean Fnh_Rcv_Y { get; set; }

        [DisplayName("分路号")]
        public UInt16 NextPosNo { get; set; }
        /// <summary>
        /// 有承载=1，无承载=0
        /// </summary>
        [DisplayName("承载")]
        public UInt16 Load { get; set; }
        /// <summary>
        /// 控制步
        /// </summary>
        [DisplayName("控制步")]
        public UInt16 ControlStep { get; set; }
        
        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "PosNo":
                        return this.PosNo;
                    case "TaskNo":
                        return this.TaskNo;
                    case "TUID":
                        return this.TUID;
                    case "NextPosNo":
                        return this.NextPosNo;
                    case "Load":
                        return this.Load;
                    case "ControlStep":
                        return this.ControlStep;
                    case "Str_Rcv_X":
                        return this.Str_Rcv_X;
                    case "Fnh_Rcv_X":
                        return this.Fnh_Rcv_X;
                    case "Rqs_Snt":
                        return this.Rqs_Snt;
                    case "Rcv_Rdy":
                        return this.Rcv_Rdy;
                    case "Str_Rcv_Y":
                        return this.Str_Rcv_Y;
                    case "Fnh_Rcv_Y":
                        return this.Fnh_Rcv_Y;
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
                    case "TaskNo":
                        this.TaskNo = Convert.ToUInt32(value);
                        break;
                    case "TUID":
                        this.TUID = Convert.ToUInt16(value);
                        break;
                    case "NextPosNo":
                        this.NextPosNo = Convert.ToUInt16(value);
                        break;
                    case "Load":
                        this.Load = Convert.ToUInt16(value);
                        break;
                    case "ControlStep":
                        this.ControlStep = Convert.ToUInt16(value);
                        break;
                    case "Str_Rcv_X":
                        this.Str_Rcv_X = Convert.ToBoolean(value);
                        break;
                    case "Fnh_Rcv_X":
                        this.Fnh_Rcv_X = Convert.ToBoolean(value);
                        break;
                    case "Rqs_Snt":
                        this.Rqs_Snt = Convert.ToBoolean(value);
                        break;
                    case "Rcv_Rdy":
                        this.Rcv_Rdy = Convert.ToBoolean(value);
                        break;
                    case "Str_Rcv_Y":
                        this.Str_Rcv_Y = Convert.ToBoolean(value);
                        break;
                    case "Fnh_Rcv_Y":
                        this.Fnh_Rcv_Y = Convert.ToBoolean(value);
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }

        public override string ToString()
        {
            if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("ConveyorLocationTask", "") == "V1")
                return String.Format("货位任务#{0} 分路号#{1} 承载#{2} 控制步#{3} (在位置 {4} 上)", this.TaskNo, this.NextPosNo, this.Load, this.ControlStep, this.PosNo);
            else
                return String.Format("货位任务#{0}(在位置 {1} 上)", this.TaskNo, this.PosNo);
        }

        public override ReceivedDataLog ToLogData(Device device)
        {
            return new LocationTaskDataLog(device, this);
        }
    }
}
