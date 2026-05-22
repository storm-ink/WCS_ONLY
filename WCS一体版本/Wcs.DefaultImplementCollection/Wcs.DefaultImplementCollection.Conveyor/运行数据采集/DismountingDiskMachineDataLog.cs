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
    public class DismountingDiskMachineDataLog: ReceivedDataLog
    {
        /// <summary>
        /// 拆叠盘机编号
        /// </summary>
        [DisplayName("货位号")]
        public virtual UInt16 PosNo { get; set; }
        /// <summary>
        ///  拨叉1原点
        /// </summary>
        [DisplayName("拨叉1原点")]
        public virtual Boolean Fork1OriginPotocell { get; set; }
        /// <summary>
        ///  拨叉1伸出
        /// </summary>
        [DisplayName("拨叉1伸出")]
        public virtual Boolean Fork1OutPotocell { get; set; }
        /// <summary>
        ///  拨叉2原点
        /// </summary>
        [DisplayName("拨叉2原点")]
        public virtual Boolean Fork2OriginPotocell { get; set; }
        /// <summary>
        ///  拨叉2伸出
        /// </summary>
        [DisplayName("拨叉2伸出")]
        public virtual Boolean Fork2OutPotocell { get; set; }
        /// <summary>
        ///  提升低位光电
        /// </summary>
        [DisplayName("提升低位光电")]
        public virtual Boolean LowPotocell { get; set; }
        /// <summary>
        ///  提升中位光电
        /// </summary>
        [DisplayName("提升中位光电")]
        public virtual Boolean MiddlePotocell { get; set; }
        /// <summary>
        ///  提升高位光电
        /// </summary>
        [DisplayName("提升高位光电")]
        public virtual Boolean TopPotocell { get; set; }
        /// <summary>
        ///  叠满光电
        /// </summary>
        [DisplayName("叠满光电")]
        public virtual Boolean FullPotocell { get; set; }
        /// <summary>
        ///  超高光电
        /// </summary>
        [DisplayName("超高光电")]
        public virtual Boolean HighOverPotocell { get; set; }
        /// <summary>
        ///  拨叉托盘检测光电
        /// </summary>
        [DisplayName("拨叉托盘检测光电")]
        public virtual Boolean ForkCheckPotocell { get; set; }
        /// <summary>
        ///  拨叉第二拖货检测光电
        /// </summary>
        [DisplayName("拨叉第二拖货检测光电")]
        public virtual Boolean ForkSecondCheckPotocell { get; set; }
        /// <summary>
        ///  输送线到位光电
        /// </summary>
        [DisplayName("输送线到位光电")]
        public virtual Boolean ConveyorPotocell { get; set; }
        /// <summary>
        /// 叠满信号
        /// </summary>
        [DisplayName("叠满信号")]
        public virtual Boolean Full { get; set; }
        /// <summary>
        /// 叠满信号
        /// </summary>
        [DisplayName("可拆信号")]
        public virtual Boolean TakeDown { get; set; }
        /// <summary>
        /// 叠满信号
        /// </summary>
        [DisplayName("可叠信号")]
        public virtual Boolean TakeUp { get; set; }
        /// <summary>
        /// 无托盘信号
        /// </summary>
        [DisplayName("无托盘信号")]
        public virtual Boolean Empty { get; set; }

        protected DismountingDiskMachineDataLog()
        {
        }
        public DismountingDiskMachineDataLog(Device device, DismountingDiskMachineNetTransferObject receivedData)
            : base()
        {
            this.PosNo = receivedData.PosNo;
            this.Fork1OriginPotocell = receivedData.Fork1OriginPotocell;
            this.Fork1OutPotocell = receivedData.Fork1OutPotocell;
            this.Fork2OriginPotocell = receivedData.Fork2OriginPotocell;
            this.Fork2OutPotocell = receivedData.Fork2OutPotocell;
            this.LowPotocell = receivedData.LowPotocell;
            this.MiddlePotocell = receivedData.MiddlePotocell;
            this.TopPotocell = receivedData.TopPotocell;
            this.FullPotocell = receivedData.FullPotocell;
            this.HighOverPotocell = receivedData.HighOverPotocell;
            this.ForkCheckPotocell = receivedData.ForkCheckPotocell;
            this.ForkSecondCheckPotocell = receivedData.ForkSecondCheckPotocell;
            this.ConveyorPotocell = receivedData.ConveyorPotocell;
            this.Full = receivedData.Full;
            this.TakeDown = receivedData.TakeDown;
            this.TakeUp = receivedData.TakeUp;
            this.Empty = receivedData.Empty;
        }
    }
}
