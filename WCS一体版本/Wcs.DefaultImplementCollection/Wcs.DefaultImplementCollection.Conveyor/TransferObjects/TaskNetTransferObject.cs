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
    [System.ComponentModel.DisplayName("任务块")]
    [JsonObject]
    public class TaskNetTransferObject : NetTransferObject
    {
        /// <summary>
        /// 握手协议（为0时说明为地址为空，可以使用）
        /// </summary>
        [System.ComponentModel.DisplayName("握手变量")]
        public TaskHandShakes HandShake { get; set; }
        /// <summary>
        /// 任务号
        /// </summary>
        [System.ComponentModel.DisplayName("任务号")]
        public UInt32 AssignmentID { get; set; }
        /// <summary>
        /// 托盘号
        /// </summary>
        [System.ComponentModel.DisplayName("托盘号")]
        public UInt16 TUID { get; set; }
       
        /// <summary>
        /// 业务数据
        /// </summary>
        [System.ComponentModel.DisplayName("业务数据")]
        public UInt16 IO_Data { get; set; }
        /// <summary>
        /// 路径号
        /// </summary>
        [System.ComponentModel.DisplayName("路径号")]
        public UInt16 RotingNo { get; set; }
        /// <summary>
        /// 起点位置
        /// </summary>
        [System.ComponentModel.DisplayName("起点位置")]
        public UInt16 StartMotorNo { get; set; }
        /// <summary>
        /// 终点位置
        /// </summary>
        [System.ComponentModel.DisplayName("终点位置")]
        public UInt16 DestinationNo { get; set; }
        /// <summary>
        /// 任务状态
        /// </summary>
        [System.ComponentModel.DisplayName("任务状态")]
        public TaskNetTransferObjectStatus TaskStatus { get; set; }
        /// <summary>
        /// PLC读取任务后会填一下这个任务号，现在没用上好像
        /// </summary>
        [System.ComponentModel.DisplayName("随机数")]
        public UInt32 Data_ID { get; set; }
   

        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "HandShake":
                        return this.HandShake;
                    case "AssignmentID":
                        return this.AssignmentID;
                    case "TUID":
                        return this.TUID;
                    case "IO_Data":
                        return this.IO_Data;
                    case "RotingNo":
                        return this.RotingNo;
                    case "StartMotorNo":
                        return this.StartMotorNo;
                    case "DestinationNo":
                        return this.DestinationNo;
                    case "TaskStatus":
                        return this.TaskStatus;
                    case "Data_ID":
                        return this.Data_ID;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的取值操作", this.GetType(), name));
                }
            }
            set
            {
                switch (name)
                {
                    case "HandShake":
                        this.HandShake = (TaskHandShakes)Convert.ToInt32(value);
                        break;
                    case "AssignmentID":
                        this.AssignmentID = Convert.ToUInt32(value);
                        break;
                    case "TUID":
                        this.TUID = Convert.ToUInt16(value);
                        break;
                    case "IO_Data":
                        this.IO_Data = Convert.ToUInt16(value);
                        break;
                    case "RotingNo":
                        this.RotingNo = Convert.ToUInt16(value);
                        break;
                    case "StartMotorNo":
                        this.StartMotorNo = Convert.ToUInt16(value);
                        break;
                    case "DestinationNo":
                        this.DestinationNo = Convert.ToUInt16(value);
                        break;
                    case "TaskStatus":
                        this.TaskStatus = (TaskNetTransferObjectStatus)Convert.ToInt16(value);
                        break;
                    case "Data_ID":
                        this.Data_ID = Convert.ToUInt32(value);
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }

        public override string ToString()
        {
            return String.Format("设备任务#{0}(从 {1} 到 {2}，路径为 {3})", this.AssignmentID, this.StartMotorNo, this.DestinationNo,this.RotingNo);
        }

        public override object GetDataGridViewShow()
        {
            return new
            {
                HandShake = this.HandShake.ToString(),
                AssignmentID = this.AssignmentID,
                TUID = this.TUID,
                IO_Data = this.IO_Data,
                RotingNo = this.RotingNo,
                StartMotorNo = this.StartMotorNo,
                DestinationNo = this.DestinationNo,
                TaskStatus = this.TaskStatus.ToString(),
                Data_ID = this.Data_ID
            };
        }

        public override ReceivedDataLog ToLogData(Device device)
        {
            return new TaskDataLog(device, this);
        }
    }
}
