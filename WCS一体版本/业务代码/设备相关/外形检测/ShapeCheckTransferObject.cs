using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs;
using Wcs.Framework;

namespace BOE
{
    /// <summary>
    /// 外形检测
    /// </summary>
    [DisplayName("外形检测")]
    public class ShapeCheckTransferObject : NetTransferObject
    {
        /// <summary>
        /// 外形检测编号，默认为所在输送机编号
        /// </summary>
        [DisplayName("编号")]
        public virtual UInt16 ShapeCheckNO { get; set; }
        /// <summary>
        /// 外形检测结果，PLC写：成功写1，失败写2，离位后写0
        /// </summary>
        [DisplayName("检测结果")]
        public virtual UInt16 ShapeStatus { get; set; }

        /// 检测长度，默认值0，由低向高从1开始依次取值
        /// </summary>
        [DisplayName("检测长度")]
        public virtual UInt16 ShapeLength { get; set; }
        /// <summary>
        /// 检测高度，默认值0，由低向高从1开始依次取值
        /// </summary>
        [DisplayName("检测宽度")]
        public virtual UInt16 ShapeWidth { get; set; } /// <summary>
        /// <summary>
        /// 检测高度，默认值0，由低向高从1开始依次取值
        /// </summary>
        [DisplayName("检测高度")]
        public virtual UInt16 ShapeHeight { get; set; }
        /// <summary>

        /// <summary>
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
        /// <summary>
        /// 箱底检测
        /// </summary>
        [DisplayName("箱底检测")]
        public virtual Boolean BottomCheck { get; set; }
        /// <summary>
        /// 是否有货
        /// </summary>
        [DisplayName("是否有货")]
        public virtual Boolean HaveGoods { get; set; }

        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "ShapeCheckNO":
                        return this.ShapeCheckNO;
                    case "ShapeStatus":
                        return this.ShapeStatus;
                    case "ShapeLength":
                        return this.ShapeLength;
                    case "ShapeWidth":
                        return this.ShapeWidth;
                    case "ShapeHeight":
                        return this.ShapeHeight;
                    case "Left_Over":
                        return this.Left_Over;
                    case "Right_Over":
                        return this.Right_Over;
                    case "Front_Over":
                        return this.Front_Over;
                    case "Back_Over":
                        return this.Back_Over;
                    case "Too_High":
                        return this.Too_High;
                    case "BottomCheck":
                        return this.BottomCheck;
                    case "HaveGoods":
                        return this.HaveGoods;
                    default:
                        throw new NotSupportedException(String.Format("无法识别属性名 {0}", name));
                }
            }
            set
            {
                switch (name)
                {
                    case "ShapeCheckNO":
                        this.ShapeCheckNO = Convert.ToUInt16(value);
                        break;
                    case "ShapeStatus":
                        this.ShapeStatus = Convert.ToUInt16(value);
                        break;
                    case "ShapeLength":
                        this.ShapeLength = Convert.ToUInt16(value);
                        break;
                    case "ShapeWidth":
                        this.ShapeWidth = Convert.ToUInt16(value);
                        break;
                    case "ShapeHeight":
                        this.ShapeHeight = Convert.ToUInt16(value);
                        break;
                    case "Left_Over":
                        this.Left_Over = Convert.ToBoolean(value);
                        break;
                    case "Right_Over":
                        this.Right_Over = Convert.ToBoolean(value);
                        break;
                    case "Front_Over":
                        this.Front_Over = Convert.ToBoolean(value);
                        break;
                    case "Back_Over":
                        this.Back_Over = Convert.ToBoolean(value);
                        break;
                    case "Too_High":
                        this.Too_High = Convert.ToBoolean(value);
                        break;
                    case "BottomCheck":
                        this.BottomCheck = Convert.ToBoolean(value);
                        break;
                    case "HaveGoods":
                        this.HaveGoods = Convert.ToBoolean(value);
                        break;
                    default:
                        throw new NotSupportedException(String.Format("无法识别属性名 {0}", name));
                }
            }
        }

        public String[] Warnings
        {
            get
            {
                List<string> result = new List<string>();
                //if (ShapeStatus == 2)
                //{
                //    result.Add("检测失败");
                //}
                if (Left_Over)
                {
                    result.Add("左超差");
                }
                if (Right_Over)
                {
                    result.Add("右超差");
                }
                if (Too_High)
                {
                    result.Add("高超差");
                }
                if (Back_Over)
                {
                    result.Add("后超差");
                }
                if (Front_Over)
                {
                    result.Add("前超差");
                }
                if (BottomCheck)
                {
                    result.Add("箱底检测失败");
                }
                return result.ToArray();
            }
        }

        public Dictionary<String, String> GetResult
        {
            get
            {
                Dictionary<String, String> result = new Dictionary<string, string>();
                if (ShapeStatus == 1 || ShapeStatus == 2)
                {
                    result.Add("外形检测", ShapeStatus == 1 ? "成功" : "失败");
                    result.Add("检测结果", ShapeStatus.ToString());
                    result.Add("检测高度", ShapeHeight.ToString());
                    result.Add("检测宽度", ShapeWidth.ToString());
                    result.Add("检测长度", ShapeLength.ToString());
                    result.Add("左超差", Left_Over.ToString().ToLower());
                    result.Add("右超差", Right_Over.ToString().ToLower());
                    result.Add("前超差", Front_Over.ToString().ToLower());
                    result.Add("后超差", Back_Over.ToString().ToLower());
                    result.Add("超高", Too_High.ToString().ToLower());
                    result.Add("箱底检测失败", BottomCheck.ToString());
                    result.Add("是否有货", HaveGoods.ToString());
                }
                return result;
            }
        }

        public override ReceivedDataLog ToLogData(Device device)
        {
            return new ProfileMeasurementResultNetTransferObjectLogData(device, this);
            //return null;
        }
    }
}
