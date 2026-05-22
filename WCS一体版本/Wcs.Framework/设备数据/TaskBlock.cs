using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Devices;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 任务块
    /// </summary>
    [System.ComponentModel.DisplayName("任务块")]
    public class TaskBlock : NetTransferObject
    {
        /// <summary>
        /// 握手协议（为0时说明为地址为空，可以使用）
        /// </summary>
        [System.ComponentModel.DisplayName("握手变量")]
        public HandShake HandShake { get; set; }
        /// <summary>
        /// 任务号
        /// </summary>
        [System.ComponentModel.DisplayName("任务号")]
        public UInt32 AssignmentID { get; set; }
        /// <summary>
        /// 托盘号
        /// </summary>
        [System.ComponentModel.DisplayName("托盘号")]
        public UInt16 TU_ID { get; set; }
        /// <summary>
        /// 托盘类型
        /// </summary>
        [System.ComponentModel.DisplayName("托盘类型")]
        public UInt16 TU_Type { get; set; }
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
        public TaskStatus TaskStatus { get; set; }
        /// <summary>
        /// PLC读取任务后会填一下这个任务号，现在没用上好像
        /// </summary>
        [System.ComponentModel.DisplayName("读取号")]
        public UInt32 ReadTask { get; set; }
        /// <summary>
        /// 预留字段
        /// </summary>
        [System.ComponentModel.DisplayName("预留")]
        public UInt32 Spare { get; set; }

        public override object this[string name]
        {
            set
            {
                switch (name)
                {
                    case "HandShake":
                        this.HandShake = (HandShake)Convert.ToInt32(value);
                        break;
                    case "AssignmentID":
                        this.AssignmentID = Convert.ToUInt32(value);
                        break;
                    case "TU_ID":
                        this.TU_ID = Convert.ToUInt16(value);
                        break;
                    case "TU_Type":
                        this.TU_Type = Convert.ToUInt16(value);
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
                        this.TaskStatus = (TaskStatus)Convert.ToInt32(value);
                        break;
                    case "ReadTask":
                        this.ReadTask = Convert.ToUInt32(value);
                        break;
                    case "Spare":
                        this.Spare = Convert.ToUInt32(value);
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
    }
}
