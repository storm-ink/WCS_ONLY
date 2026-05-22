using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 任务块
    /// 配置
    ///   <collection type="Wcs.DefaultImpls.Conveyor.TaskNetTransferObject, Wcs.DefaultImpls" blockBytes="28" itemCount="1">
    ///     <property name="HandShake" index="0" size="2" type="UInt16" />
    ///     <property name="AssignmentID" index="2" size="4" type="UInt32" />
    ///     <property name="TUID" index="6" size="2" type="UInt16" />
    ///     <property name="TU_Type" index="8" size="2" type="UInt16" />
    ///     <property name="IO_Data" index="10" size="2" type="UInt16" />
    ///     <property name="RotingNo" index="12" size="2" type="UInt16" />
    ///     <property name="StartMotorNo" index="14" size="2" type="UInt16" />
    ///     <property name="DestinationNo" index="16" size="2" type="UInt16" />
    ///     <property name="TaskStatus" index="18" size="2" type="UInt16" />
    ///     <property name="ReadTask" index="20" size="4" type="UInt32" />
    ///     <property name="Spare" index="24" size="4" type="UInt32" />
    ///   </collection>
    /// </summary>
    [System.ComponentModel.DisplayName("远程控制")]
    [JsonObject]
    public class RemoteControlNetTransferObject : NetTransferObject
    {
        /// <summary>
        /// 托盘号
        /// </summary>
        [System.ComponentModel.DisplayName("区域号")]
        public UInt16 AreaNo { get; set; }
       
        /// <summary>
        /// 业务数据
        /// </summary>
        [System.ComponentModel.DisplayName("启动")]
        public bool Start { get; set; }
        /// <summary>
        /// 路径号
        /// </summary>
        [System.ComponentModel.DisplayName("停止")]
        public bool Stop { get; set; }
        /// <summary>
        /// 起点位置
        /// </summary>
        [System.ComponentModel.DisplayName("复位")]
        public bool Reset { get; set; }
        /// <summary>
        /// 随机数
        /// </summary>
        [System.ComponentModel.DisplayName("随机数")]
        public UInt16 DateID { get; set; }


        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "AreaNo":
                        return this.AreaNo;
                    case "Start":
                        return this.Start;
                    case "Stop":
                        return this.Stop;
                    case "Reset":
                        return this.Reset;
                    case "DateID":
                        return this.DateID;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的取值操作", this.GetType(), name));
                }
            }
            set
            {
                switch (name)
                {
                    case "AreaNo":
                        this.AreaNo = Convert.ToUInt16(value);
                        break;
                    case "Start":
                        this.Start = Convert.ToBoolean(value);
                        break;
                    case "Stop":
                        this.Stop = Convert.ToBoolean(value);
                        break;
                    case "Reset":
                        this.Reset = Convert.ToBoolean(value);
                        break;
                    case "DateID":
                        this.DateID = Convert.ToUInt16(value);
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }

        public override string ToString()
        {
            return String.Format("远程控制模块#{0}", this.AreaNo);
        }

        public override ReceivedDataLog ToLogData(Device device)
        {
            return null;
        }
    }
}
