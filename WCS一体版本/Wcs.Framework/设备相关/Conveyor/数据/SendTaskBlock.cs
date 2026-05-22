using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Devices;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 用与给PLC添加新任务的块
    /// </summary>
    public class SendTaskBlock : NetTransferObject
    {
        /// <summary>
        /// 握手协议（为0时说明为地址为空，可以使用）
        /// </summary>
        public HandShake HandShake { get; set; }
        /// <summary>
        /// 任务号
        /// </summary>
        public UInt32 AssignmentID { get; set; }
        /// <summary>
        /// 托盘号
        /// </summary>
        public UInt16 TU_ID { get; set; }
        /// <summary>
        /// 托盘类型
        /// </summary>
        public UInt16 TU_Type { get; set; }
        /// <summary>
        /// 业务数据
        /// </summary>
        public UInt16 IO_Data { get; set; }
        /// <summary>
        /// 路径号
        /// </summary>
        public UInt16 RotingNo { get; set; }
        /// <summary>
        /// 起点位置
        /// </summary>
        public UInt16 StartMotorNo { get; set; }
        /// <summary>
        /// 终点位置
        /// </summary>
        public UInt16 DestinationNo { get; set; }
        /// <summary>
        /// 任务要存放的索引号(在db块中的位置，从1开始)
        /// </summary>
        public UInt16 Index { get; set; }
        /// <summary>
        /// 预留字段
        /// </summary>
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
                    case "Index":
                        this.Index = Convert.ToUInt16(value);
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
            return String.Format("新任务 #{0} 从 {1} 到 {2} 路径为 {3} 分配的索引号为 {4}", this.AssignmentID, this.StartMotorNo, this.DestinationNo, this.RotingNo, this.Index);
        }
    }
}
