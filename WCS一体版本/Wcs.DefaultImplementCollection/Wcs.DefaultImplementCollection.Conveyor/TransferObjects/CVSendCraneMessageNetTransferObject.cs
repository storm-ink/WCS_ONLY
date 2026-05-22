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
    /// RGV发给CV的信号
    ///   <collection type="Wcs.DefaultImpls.Conveyor.CVSendCraneMessageNetTransferObject, Wcs.DefaultImpls" blockBytes="6" itemCount="1">
    ///     <property name="PosNo" index="0" size="2" type="UInt16" />
    ///     <property name="DeviceNo" index="2" size="2" type="UInt16" />
    ///     <property name="Alarm_Not_PosNo" index="4.0" size="1" type="Boolean" />
    ///     <property name="Alarm_Device_Req" index="4.1" size="1" type="Boolean" />
    ///     <property name="Alarm_Device_MotorRun" index="4.2" size="1" type="Boolean" />
    ///     <property name="Alarm_Device_Rcv" index="4.3" size="1" type="Boolean" />
    ///     <property name="Alarm_Device_Fnh" index="4.4" size="1" type="Boolean" />
    ///   </collection>
    /// </summary>
    [Description("CV发给Crane的信号")]
    [JsonObject]
    public class CVSendCraneMessageNetTransferObject : NetTransferObject
    {
        /// <summary>
        /// 货位编号.
        /// </summary>
        [Description("货位编号")]
        public UInt16 PosNo { get; set; }
        /// <summary>
        /// 设备编号.
        /// </summary>
        [Description("设备编号")]
        public UInt16 DeviceNo { get; set; }
        /// <summary>
        /// 无对应货位站口.
        /// </summary>
        [Description("无对应货位站口")]
        public bool Alarm_Not_PosNo { get; set; }
        /// <summary>
        /// 等待设备申请传递信号超时.
        /// </summary>
        [Description("等待设备申请传递信号超时")]
        public bool Alarm_Device_Req { get; set; }
        /// <summary>
        /// 等待设备运行信号超时.
        /// </summary>
        [Description("等待设备运行信号超时")]
        public bool Alarm_Device_MotorRun { get; set; }
        /// <summary>
        /// 等待设备可以接收信号超时.
        /// </summary>
        [Description("等待设备可以接收信号超时")]
        public bool Alarm_Device_Rcv { get; set; }
        /// <summary>
        /// 等待设备接收完成信号超时.
        /// </summary>
        [Description("等待设备接收完成信号超时")]
        public bool Alarm_Device_Fnh { get; set; }

        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "PosNo":
                        return this.PosNo;
                    case "DeviceNo":
                        return this.DeviceNo;
                    case "Alarm_Not_PosNo":
                        return this.Alarm_Not_PosNo;
                    case "Alarm_Device_Req":
                        return this.Alarm_Device_Req;
                    case "Alarm_Device_MotorRun":
                        return this.Alarm_Device_MotorRun;
                    case "Alarm_Device_Rcv":
                        return this.Alarm_Device_Rcv;
                    case "Alarm_Device_Fnh":
                        return this.Alarm_Device_Fnh;
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
                    case "DeviceNo":
                        this.DeviceNo = Convert.ToUInt16(value);
                        break;
                    case "Alarm_Not_PosNo":
                        this.Alarm_Not_PosNo = Convert.ToBoolean(value);
                        break;
                    case "Alarm_Device_Req":
                        this.Alarm_Device_Req = Convert.ToBoolean(value);
                        break;
                    case "Alarm_Device_MotorRun":
                        this.Alarm_Device_MotorRun = Convert.ToBoolean(value);
                        break;
                    case "Alarm_Device_Rcv":
                        this.Alarm_Device_Rcv = Convert.ToBoolean(value);
                        break;
                    case "Alarm_Device_Fnh":
                        this.Alarm_Device_Fnh = Convert.ToBoolean(value);
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }

        public override ReceivedDataLog ToLogData(Device device)
        {
            return null;
        }
    }
}
