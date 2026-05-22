using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    [System.ComponentModel.DisplayName("光电状态")]
    public class OccupyDataLog :ReceivedDataLog
    {
        [DisplayName("货位号")]
        public virtual Int32 PosNo { get; set; }
        [System.ComponentModel.DisplayName("光电的使用状态")]
        public virtual Int32 PhocllUseStatus { get; set; }
        [System.ComponentModel.DisplayName("保护光电")]
        public virtual Boolean FroProPotocell { get; set; }
        [System.ComponentModel.DisplayName("前到位")]
        public virtual Boolean FroPosPotocell { get; set; }
        [System.ComponentModel.DisplayName("前减速")]
        public virtual Boolean FroSloPotocell { get; set; }
        [System.ComponentModel.DisplayName("后保护")]
        public virtual Boolean AftProPotocell { get; set; }
        [System.ComponentModel.DisplayName("后到位")]
        public virtual Boolean AftPosPotocell { get; set; }
        [System.ComponentModel.DisplayName("后减速")]
        public virtual Boolean AftSloPotocell { get; set; }
        [System.ComponentModel.DisplayName("后高位")]
        public virtual Boolean UpPotocell { get; set; }
        [System.ComponentModel.DisplayName("后低位")]
        public virtual Boolean DownPotocell { get; set; }
        [System.ComponentModel.DisplayName("高减速")]
        public Boolean UpSloPotocell { get; set; }
        [System.ComponentModel.DisplayName("低减速")]
        public Boolean DownSloPotocell { get; set; }
        [System.ComponentModel.DisplayName("载荷")]
        public Boolean LoadPotocell { get; set; }
        protected OccupyDataLog():base()
        {
        }
        public OccupyDataLog(Device device, OccupyNetTransferObject receivedData)
            : this()
        {
            this.DeviceName = device.Name;
            this.PosNo = receivedData.PosNo;
            this.PhocllUseStatus = receivedData.PhocllUseStatus;
            this.FroProPotocell = receivedData.FroProPotocell;
            this.FroPosPotocell = receivedData.FroPosPotocell;
            this.FroSloPotocell = receivedData.FroSloPotocell;
            this.AftProPotocell = receivedData.AftProPotocell;
            this.AftPosPotocell = receivedData.AftPosPotocell;
            this.UpSloPotocell = receivedData.UpSloPotocell;
            this.DownSloPotocell = receivedData.DownSloPotocell;
            this.LoadPotocell = receivedData.LoadPotocell;

        }
       
    }
}
