using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 输送线占位信号
    /// 此信号通常是在容器到达或经过某些特定的输送线位置时，由电气控制生成。
    /// 一般用在入库时入库指令的生成上
    /// </summary>
    [DisplayName("占位信号")]
    public class OccupiedSignal : NetTransferObject
    {
        /// <summary>
        /// 获取或设置占位信号对应的货位号（信号源）
        /// </summary>
        /// <value>
        /// 货位对应的设备编码形式.即输送线位置对象 DeviceCode 值.
        /// </value>
        [DisplayName("货位号")]
        public UInt16 PosNo { get; set; }

        /// <summary>
        ///  获取或设置占位信号的握手变量值
        /// </summary>
        /// <value>
        /// 占位信号握手变量
        /// </value>
        [DisplayName("握手")]
        public OccupiedSignalHandShake HandShake { get; set; }
        /// <summary>
        /// 获取或设置占位信号对应的任务号
        /// </summary>
        [DisplayName("任务号")]
        public UInt32 AssignmentID { get; set; }
        /// <summary>
        /// 获取或设置占位信号对应的托盘号
        /// </summary>
        [DisplayName("托盘号")]
        public UInt16 TU_ID { get; set; }
        /// <summary>
        /// 获取或设置占位信号对应的托盘类型
        /// </summary>
        [DisplayName("托盘类型")]
        public UInt16 TU_Type { get; set; }
        /// <summary>
        /// 获取或设置占位信号对应的业务数据
        /// </summary>
        [DisplayName("业务数据")]
        public UInt16 IO_Data{get;set;}
        public override object this[string name]
        {
            set
            {
                switch (name)
                {
                    case "PosNo":
                        this.PosNo = Convert.ToUInt16(value);
                        break;
                    case "HandShake":
                        this.HandShake = (OccupiedSignalHandShake)Convert.ToInt32(value);
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
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }

        public override string ToString()
        {
            return String.Format("占位信号# {0} 握手 {1}", this.PosNo,HandShake.GetDescription());
        }
    }
}
