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
    /// 输送线占位信号
    /// 此信号通常是在容器到达或经过某些特定的输送线位置时，由电气控制生成。
    /// 一般用在入库时入库指令的生成上
    /// 配置
    ///   <collection type="Wcs.DefaultImpls.Conveyor.HoldSignalNetTransferObject, Wcs.DefaultImpls" blockBytes="14" itemCount="1">
    ///     <property name="ShapeCheckNO" index="0" size="2" type="UInt16" />
    ///     <property name="HandShake" index="2" size="2" type="UInt16" />
    ///     <property name="AssignmentID" index="4" size="4" type="UInt32" />
    ///     <property name="TU_ID" index="8" size="2" type="UInt16" />
    ///     <property name="TU_Type" index="10" size="2" type="UInt16" />
    ///     <property name="IO_Data" index="12" size="2" type="UInt16" />
    ///   </collection>
    /// </summary>
    [DisplayName("复杂占位信号")]
    [JsonObject]
    public class HoldSignalNetTransferObject : NetTransferObject
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
        public HoldSignalNetTransferObjectHandShake HandShake { get; set; }
       
        /// <summary>
        /// 获取或设置占位信号对应的业务数据
        /// </summary>
        [DisplayName("业务数据")]
        public UInt16 IO_Data{get;set;}
        /// <summary>
        /// 获取或设置占位信号对应的随机数
        /// </summary>
        [DisplayName("随机数")]
        public UInt16 Data_ID { get; set; }
        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "PosNo":
                        return this.PosNo;
                    case "HandShake":
                        return this.HandShake;
                    case "IO_Data":
                        return this.IO_Data;
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
                    case "PosNo":
                        this.PosNo = Convert.ToUInt16(value);
                        break;
                    case "HandShake":
                        this.HandShake = (HoldSignalNetTransferObjectHandShake)Convert.ToInt16(value);
                        break;
                    case "IO_Data":
                        this.IO_Data = Convert.ToUInt16(value);
                        break;
                    case "Data_ID":
                        this.Data_ID = Convert.ToUInt16(value);
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }

        public override string ToString()
        {
            return String.Format("占位信号# {0} 握手 {1} IO_DATA {2} DATA_ID {3}", this.PosNo, HandShake.GetDescription(), this.IO_Data, this.Data_ID);
        }

        public override ReceivedDataLog ToLogData(Device device)
        {
            return new HoldSignalDataLog(device, this);
        }
    }
}
