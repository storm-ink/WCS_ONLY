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
    ///   <collection type="Wcs.DefaultImpls.Conveyor.RGVSendCVMessageNetTransferObject, Wcs.DefaultImpls" blockBytes="6" itemCount="1">
    ///     <property name="DeviceNo" index="0" size="2" type="UInt16" />
    ///     <property name="Error_Configuration" index="2.0" size="1" type="Boolean" />
    ///     <property name="Error_Communication" index="2.1" size="1" type="Boolean" />
    ///     <property name="Error_DataBlock" index="2.2" size="1" type="Boolean" />
    ///   </collection>
    /// </summary>
    [Description("RGV发给CV的信号")]
    [JsonObject]
    public class RGVSendCVMessageNetTransferObject : NetTransferObject
    {
        /// <summary>
        /// 设备编号.
        /// </summary>
        [Description("设备编号")]
        public UInt16 DeviceNo { get; set; }
        /// <summary>
        /// 配置错误
        /// </summary>
        [Description("配置错误")]
        public bool Error_Configuration { get; set; }
        /// <summary>
        /// 通讯故障
        /// </summary>
        [Description("通讯故障")]
        public bool Error_Communication { get; set; }
        /// <summary>
        /// 数据块故障
        /// </summary>
        [Description("数据块故障")]
        public bool Error_DataBlock { get; set; }

        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "DeviceNo":
                        return this.DeviceNo;
                    case "Error_Configuration":
                        return this.Error_Configuration;
                    case "Error_Communication":
                        return this.Error_Communication;
                    case "Error_DataBlock":
                        return this.Error_DataBlock;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的取值操作", this.GetType(), name));
                }
            }
            set
            {
                switch (name)
                {
                    case "DeviceNo":
                        this.DeviceNo = Convert.ToUInt16(value);
                        break;
                    case "Error_Configuration":
                        this.Error_Configuration = Convert.ToBoolean(value);
                        break;
                    case "Error_Communication":
                        this.Error_Communication = Convert.ToBoolean(value);
                        break;
                    case "Error_DataBlock":
                        this.Error_DataBlock = Convert.ToBoolean(value);
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
