using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    public class ShapeCheckTransferObjectLogData : ReceivedDataLog
    {
        /// <summary>
        /// 外形检测编号，默认为所在输送机编号
        /// </summary>
        [DisplayName("编号")]
        public virtual UInt16 PosNo { get; set; }
        /// <summary>
        /// 外形检测结果，PLC写：成功写1，失败写2，离位后写0
        /// </summary>
        [DisplayName("检测结果")]
        public virtual ShapeCheckResults ShapeCheckResult { get; set; }
        /// <summary>
        /// 长度 默认值0，由低向高从1开始依次取值
        /// </summary>
        [DisplayName("长度")]
        public virtual UInt16 ShapeType { get; set; }
       
        /// <summary>
        /// 左超差
        /// </summary>
        [DisplayName("左超差")]
        public virtual Boolean Left_Over { get; set; }
        /// <summary>
        /// 右超差
        /// </summary>
        [DisplayName("右超差")]
        public virtual Boolean Right_Over { get; set; }
        /// <summary>
        /// 前超差
        /// </summary>
        [DisplayName("前超差")]
        public virtual Boolean Front_Over { get; set; }
        /// <summary>
        /// 后超差
        /// </summary>
        [DisplayName("后超差")]
        public virtual Boolean Back_Over { get; set; }
        /// <summary>
        /// 超高
        /// </summary>
        [DisplayName("超高")]
        public virtual Boolean Too_High { get; set; }
      

        protected ShapeCheckTransferObjectLogData()
            : base()
        {
            this.CreatedAt = System.DateTime.Now;
        }

        public ShapeCheckTransferObjectLogData(Device device, ShapeCheckTransferObject receivedData)
            : this()
        {
            this.DataLevel = ReceivedDataLogLevel.Normal;
            this.DeviceName = device.Name;
            this.ShapeCheckResult = receivedData.ShapeStatus;
            this.ShapeType = receivedData.ShapeType;
            this.Left_Over = receivedData.Left_Over;
            this.Right_Over = receivedData.Right_Over;
            this.Front_Over = receivedData.Front_Over;
            this.Back_Over = receivedData.Back_Over;
            this.Too_High = receivedData.High_Over;
        }
    }
}
