using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 输送线光电状态，该对象包含有物理光电设备的实时工作状态信息
    /// </summary>
    [System.ComponentModel.DisplayName("光电状态")]
    public class OccupyStatus : NetTransferObject
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
        public Byte PhocllUseStatus{get;set;}
        /// <summary>
        /// 指示前保护光电是否被遮挡
        /// </summary>
        [System.ComponentModel.DisplayName("保护光电")]
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
        public override object this[string name]
        {
            set
            {
                switch (name)
                {
                    case "PosNo":
                        this.PosNo = Convert.ToUInt16(value);
                        break;
                    case "PhocllUseStatus":
                        this.PhocllUseStatus = Convert.ToByte(value);
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
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }
        public override string ToString()
        {
            return String.Format("光电状态# {0}", this.PosNo);
        }
    }
}
