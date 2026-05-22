using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 拆叠盘机信号
    /// </summary>
    [DisplayName("拆叠盘机信号")]
    public class DismountingDiskMachineNetTransferObject : NetTransferObject
    {
        /// <summary>
        /// 拆叠盘机编号
        /// </summary>
        [DisplayName("货位号")]
        public UInt16 PosNo { get; set; }
        /// <summary>
        ///  拨叉1原点
        /// </summary>
        [DisplayName("拨叉1原点")]
        public Boolean Fork1OriginPotocell { get; set; }
        /// <summary>
        ///  拨叉1伸出
        /// </summary>
        [DisplayName("拨叉1伸出")]
        public Boolean Fork1OutPotocell { get; set; }
        /// <summary>
        ///  拨叉2原点
        /// </summary>
        [DisplayName("拨叉2原点")]
        public Boolean Fork2OriginPotocell { get; set; }
        /// <summary>
        ///  拨叉2伸出
        /// </summary>
        [DisplayName("拨叉2伸出")]
        public Boolean Fork2OutPotocell { get; set; }
        /// <summary>
        ///  提升低位光电
        /// </summary>
        [DisplayName("提升低位光电")]
        public Boolean LowPotocell { get; set; }
        /// <summary>
        ///  提升中位光电
        /// </summary>
        [DisplayName("提升中位光电")]
        public Boolean MiddlePotocell { get; set; }
        /// <summary>
        ///  提升高位光电
        /// </summary>
        [DisplayName("提升高位光电")]
        public Boolean TopPotocell { get; set; }
        /// <summary>
        ///  叠满光电
        /// </summary>
        [DisplayName("叠满光电")]
        public Boolean FullPotocell { get; set; }
        /// <summary>
        ///  超高光电
        /// </summary>
        [DisplayName("超高光电")]
        public Boolean HighOverPotocell { get; set; }
        /// <summary>
        ///  拨叉托盘检测光电
        /// </summary>
        [DisplayName("拨叉托盘检测光电")]
        public Boolean ForkCheckPotocell { get; set; }
        /// <summary>
        ///  拨叉第二拖货检测光电
        /// </summary>
        [DisplayName("拨叉第二拖货检测光电")]
        public Boolean ForkSecondCheckPotocell { get; set; }
        /// <summary>
        ///  输送线到位光电
        /// </summary>
        [DisplayName("输送线到位光电")]
        public Boolean ConveyorPotocell { get; set; }
        /// <summary>
        /// 叠满信号
        /// </summary>
        [DisplayName("叠满信号")]
        public Boolean Full { get; set; }
        /// <summary>
        /// 可拆信号
        /// </summary>
        [DisplayName("可拆信号")]
        public Boolean TakeDown { get; set; }
        /// <summary>
        /// 可叠信号
        /// </summary>
        [DisplayName("可叠信号")]
        public Boolean TakeUp { get; set; }
        /// <summary>
        /// 无托盘信号
        /// </summary>
        [DisplayName("无托盘信号")]
        public Boolean Empty { get; set; }
        /// <summary>
        /// 托盘数量
        /// </summary>
        [DisplayName("托盘数量")]
        public UInt16 Count { get; set; }

        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "PosNo":
                        return this.PosNo;
                    case "Fork1OriginPotocell":
                        return this.Fork1OriginPotocell;
                    case "Fork1OutPotocell":
                        return this.Fork1OutPotocell;
                    case "Fork2OriginPotocell":
                        return this.Fork2OriginPotocell;
                    case "Fork2OutPotocell":
                        return this.Fork2OutPotocell;
                    case "LowPotocell":
                        return this.LowPotocell;
                    case "MiddlePotocell":
                        return this.MiddlePotocell;
                    case "TopPotocell":
                        return this.TopPotocell;
                    case "FullPotocell":
                        return this.FullPotocell;
                    case "HighOverPotocell":
                        return this.HighOverPotocell;
                    case "ForkCheckPotocell":
                        return this.ForkCheckPotocell;
                    case "ForkSecondCheckPotocell":
                        return this.ForkSecondCheckPotocell;
                    case "ConveyorPotocell":
                        return this.ConveyorPotocell;
                    case "Full":
                        return this.Full;
                    case "TakeDown":
                        return this.TakeDown;
                    case "TakeUp":
                        return this.TakeUp;
                    case "Empty":
                        return this.Empty;
                    case "Count":
                        return this.Count;
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
                    case "Fork1OriginPotocell":
                        this.Fork1OriginPotocell = Convert.ToBoolean(value);
                        break;
                    case "Fork1OutPotocell":
                        this.Fork1OutPotocell = Convert.ToBoolean(value);
                        break;
                    case "Fork2OriginPotocell":
                        this.Fork2OriginPotocell = Convert.ToBoolean(value);
                        break;
                    case "Fork2OutPotocell":
                        this.Fork2OutPotocell = Convert.ToBoolean(value);
                        break;
                    case "LowPotocell":
                        this.LowPotocell = Convert.ToBoolean(value);
                        break;
                    case "MiddlePotocell":
                        this.MiddlePotocell = Convert.ToBoolean(value);
                        break;
                    case "TopPotocell":
                        this.TopPotocell = Convert.ToBoolean(value);
                        break;
                    case "FullPotocell":
                        this.FullPotocell = Convert.ToBoolean(value);
                        break;
                    case "HighOverPotocell":
                        this.HighOverPotocell = Convert.ToBoolean(value);
                        break;
                    case "ForkCheckPotocell":
                        this.ForkCheckPotocell = Convert.ToBoolean(value);
                        break;
                    case "ForkSecondCheckPotocell":
                        this.ForkSecondCheckPotocell = Convert.ToBoolean(value);
                        break;
                    case "ConveyorPotocell":
                        this.ConveyorPotocell = Convert.ToBoolean(value);
                        break;
                    case "Full":
                        this.Full = Convert.ToBoolean(value);
                        break;
                    case "TakeDown":
                        this.TakeDown = Convert.ToBoolean(value);
                        break;
                    case "TakeUp":
                        this.TakeUp = Convert.ToBoolean(value);
                        break;
                    case "Empty":
                        this.Empty = Convert.ToBoolean(value);
                        break;
                    case "Count":
                        this.Count = Convert.ToUInt16(value);
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }

        public override Framework.ReceivedDataLog ToLogData(Framework.Device device)
        {
            return new DismountingDiskMachineDataLog(device, this);
        }
    }
}
