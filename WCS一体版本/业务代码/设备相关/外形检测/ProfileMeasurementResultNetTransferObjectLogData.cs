using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace BOE
{
    public class ProfileMeasurementResultNetTransferObjectLogData : ReceivedDataLog
    {
        /// <summary>
        /// 外形检测编号，默认为所在输送机编号
        /// </summary>
        public virtual UInt16 PosNo { get; set; }
        /// <summary>
        /// 外形检测结果，PLC写：成功写1，失败写2，离位后写0
        /// </summary>
        public virtual UInt16 Result { get; set; }
        /// <summary>
        /// 高度检测结果
        /// </summary>
        public virtual UInt16 Height { get; set; }
        /// <summary>
        /// 左超差
        /// </summary>
        public virtual Boolean LeftOver { get; set; }
        /// <summary>
        /// 右超差
        /// </summary>
        public virtual Boolean RightOver { get; set; }
        /// <summary>
        /// 前超差
        /// </summary>
        public virtual Boolean FrontOver { get; set; }
        /// <summary>
        /// 后超差
        /// </summary>
        public virtual Boolean BackOver { get; set; }
        /// <summary>
        /// 超高
        /// </summary>
        public virtual Boolean SupereLevation { get; set; }
        /// <summary>
        /// 箱底检测
        /// </summary>
        public virtual Boolean BottomCheck { get; set; }
        /// <summary>
        /// 货物检测
        /// </summary>
        public virtual Boolean HaveGoods { get; set; }

        protected ProfileMeasurementResultNetTransferObjectLogData()
            : base()
        {
            this.CreatedAt = System.DateTime.Now;
        }

        public ProfileMeasurementResultNetTransferObjectLogData(Device device, ShapeCheckTransferObject receivedData)
            : this()
        {
            this.DeviceName = device.Name;
            this.BackOver = receivedData.Back_Over;
            this.DataLevel = ReceivedDataLogLevel.Normal;
            this.FrontOver = receivedData.Front_Over;
            this.LeftOver = receivedData.Left_Over;
            this.PosNo = receivedData.ShapeCheckNO;
            this.Result = receivedData.ShapeStatus;
            this.RightOver = receivedData.Right_Over;
            this.SupereLevation = receivedData.Too_High;
            this.Height = receivedData.ShapeHeight;
            this.BottomCheck = receivedData.BottomCheck;
            this.HaveGoods = receivedData.HaveGoods;
        }
    }
}
