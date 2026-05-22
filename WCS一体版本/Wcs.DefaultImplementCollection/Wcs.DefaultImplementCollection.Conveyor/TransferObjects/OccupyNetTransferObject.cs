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
    /// 输送线光电状态，该对象包含有物理光电设备的实时工作状态信息
    /// 配置
    ///	<collection type="Wcs.DefaultImpls.Conveyor.OccupyNetTransferObject, Wcs.DefaultImpls" blockBytes="4" itemCount="1">
    ///		<property name="ShapeCheckNO" index="0" size="2" type="UInt16" />
    ///		<property name="PhocllUseStatus" index="2" size="1" type="Byte" />
    ///		<property name="FroProPotocell" index="3" size="1" type="Boolean" />
    ///		<property name="FroPosPotocell" index="3.1" size="1" type="Boolean" />
    ///		<property name="FroSloPotocell" index="3.2" size="1" type="Boolean" />
    ///		<property name="AftProPotocell" index="3.3" size="1" type="Boolean" />
    ///		<property name="AftPosPotocell" index="3.4" size="1" type="Boolean" />
    ///		<property name="AftSloPotocell" index="3.5" size="1" type="Boolean" />
    ///		<property name="UpPotocell" index="3.6" size="1" type="Boolean" />
    ///		<property name="DownPotocell" index="3.7" size="1" type="Boolean" />
    ///	</collection>
    /// </summary>
    [System.ComponentModel.DisplayName("光电状态")]
    [JsonObject]
    public class OccupyNetTransferObject : NetTransferObject
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
        /// 光电的使用状态，即此组使用了哪些光电
        /// </summary>
        [System.ComponentModel.DisplayName("光电的使用状态")]
        public UInt16 PhocllUseStatus {get;set;}
        /// <summary>
        /// 指示前保护光电是否被遮挡
        /// </summary>
        [System.ComponentModel.DisplayName("前保护光电")]
        public Boolean FroProPotocell{get;set;}
        /// <summary>
        /// 指示前到位光电是否被遮挡
        /// </summary>
        [System.ComponentModel.DisplayName("前到位")]
        public Boolean FroPosPotocell{get;set;}
        /// <summary>
        /// 指示前减速光电是否被遮挡
        /// </summary>
        [System.ComponentModel.DisplayName("前减速")]        
        public Boolean FroSloPotocell{get;set;}
        /// <summary>
        /// 指示后保护光电是否被遮挡
        /// </summary>
        [System.ComponentModel.DisplayName("后保护")] 
        public Boolean AftProPotocell { get; set; }
        /// <summary>
        /// 指示后到位光电是否被遮挡
        /// </summary>
        [System.ComponentModel.DisplayName("后到位")] 
        public Boolean AftPosPotocell{get;set;}
        /// <summary>
        /// 指示后减速光电是否被遮挡
        /// </summary>
        [System.ComponentModel.DisplayName("后减速")] 
        public Boolean AftSloPotocell{get;set;}
        /// <summary>
        /// 指示后高位光电是否被遮挡
        /// </summary>
        [System.ComponentModel.DisplayName("后高位")] 
        public Boolean UpPotocell { get; set; }
        /// <summary>
        /// 指示后低位光电是否被遮挡
        /// </summary>
        [System.ComponentModel.DisplayName("后低位")] 
        public Boolean DownPotocell { get; set; }
        /// <summary>
        /// 指示后高减速光电是否被遮挡
        /// </summary>
        [System.ComponentModel.DisplayName("高减速")]
        public Boolean UpSloPotocell { get; set; }
        /// <summary>
        /// 指示后低减速光电是否被遮挡
        /// </summary>
        [System.ComponentModel.DisplayName("低减速")]
        public Boolean DownSloPotocell { get; set; }
        [System.ComponentModel.DisplayName("载荷")]
        public Boolean LoadPotocell { get; set; }
        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "PosNo":
                        return this.PosNo;
                    case "PhocllUseStatus":
                        return this.PhocllUseStatus;
                    case "FroProPotocell":
                        return this.FroProPotocell;
                    case "FroPosPotocell":
                        return this.FroPosPotocell;
                    case "FroSloPotocell":
                        return this.FroSloPotocell;
                    case "AftProPotocell":
                        return this.AftProPotocell;
                    case "AftPosPotocell":
                        return this.AftPosPotocell;
                    case "AftSloPotocell":
                        return this.AftSloPotocell;
                    case "UpPotocell":
                        return this.UpPotocell;
                    case "DownPotocell":
                        return this.DownPotocell;
                    case "UpSloPotocell":
                        return this.UpSloPotocell;
                    case "DownSloPotocell":
                        return this.DownSloPotocell;
                    case "LoadPotocell":
                        return this.LoadPotocell;
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
                    case "PhocllUseStatus":
                        this.PhocllUseStatus = Convert.ToUInt16(value);
                        break;
                    case "FroProPotocell":
                        this.FroProPotocell = Convert.ToBoolean(value);
                        break;
                    case "FroPosPotocell":
                        this.FroPosPotocell = Convert.ToBoolean(value);
                        break;
                    case "FroSloPotocell":
                        this.FroSloPotocell = Convert.ToBoolean(value);
                        break;
                    case "AftProPotocell":
                        this.AftProPotocell = Convert.ToBoolean(value);
                        break;
                    case "AftPosPotocell":
                        this.AftPosPotocell = Convert.ToBoolean(value);
                        break;
                    case "AftSloPotocell":
                        this.AftSloPotocell = Convert.ToBoolean(value);
                        break;
                    case "UpPotocell":
                        this.UpPotocell = Convert.ToBoolean(value);
                        break;
                    case "DownPotocell":
                        this.DownPotocell = Convert.ToBoolean(value);
                        break;
                    case "UpSloPotocell":
                        this.UpSloPotocell = Convert.ToBoolean(value);
                        break;
                    case "DownSloPotocell":
                        this.DownSloPotocell = Convert.ToBoolean(value);
                        break;
                    case "LoadPotocell":
                        this.LoadPotocell = Convert.ToBoolean(value);
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }
        public override string ToString()
        {
            return String.Format("光电状态# {0}", this.PosNo);
        }

        public override ReceivedDataLog ToLogData(Device device)
        {
            return new OccupyDataLog(device, this);
        }
    }
}
