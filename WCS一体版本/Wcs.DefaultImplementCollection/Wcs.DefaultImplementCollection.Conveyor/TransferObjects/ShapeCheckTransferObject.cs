using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 外形检测
    /// 配置
    /// <collection type="Wcs.DefaultImpls.Conveyor.ShapeCheckTransferObject, Wcs.DefaultImpls" blockBytes="8" itemCount="1">
    ///     <property name="ShapeCheckNO" index="0" size="2" type="UInt16" />
    ///     <property name="ShapeStatus" index="2" size="2" type="UInt16" />
    ///      <property name="ShapeType" index="4" size="2" type="UInt16" />
    ///     <property name="Left_Over" index="6.0" size="1" type="Boolean" />
    ///     <property name="Right_Over" index="6.1" size="1" type="Boolean" />
    ///     <property name="Front_Over" index="6.2" size="1" type="Boolean" />
    ///     <property name="Back_Over"  index="6.3" size="1" type="Boolean" />
    ///     <property name="High_Over"  index="6.4" size="1" type="Boolean" />
    /// </collection>
    /// </summary>
    [DisplayName("外形检测")]
    [JsonObject]
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
        public virtual ShapeCheckResults ShapeStatus { get; set; }
        /// <summary>
        /// 货型默认0 有效值从1开始依次递增 此值只有适配业务才有意义
        /// </summary>
        [DisplayName("货型")]
        public virtual UInt16 ShapeType{ get; set; }
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
        public virtual Boolean High_Over { get; set; }
        /// <summary>
        /// 初始状态异常-前定位光电
        /// </summary>
        [DisplayName("初始状态异常-前定位光电")]
        public virtual Boolean Error_CheckFront_Phocll { get; set; }
        /// <summary>
        /// 初始状态异常-后定位光电
        /// </summary>
        [DisplayName("初始状态异常-后定位光电")]
        public virtual Boolean Error_CheckBack_Phocll { get; set; }
        /// <summary>
        /// 初始状态异常-高度检测光电1
        /// </summary>
        [DisplayName("初始状态异常-高度检测光电1")]
        public virtual Boolean Error_CheckHeight_Phocll_1 { get; set; }
        /// <summary>
        /// 初始状态异常-高度检测光电2
        /// </summary>
        [DisplayName("初始状态异常-高度检测光电2")]
        public virtual Boolean Error_CheckHeight_Phocll_2 { get; set; }
        /// <summary>
        /// 初始状态异常-高度检测光电3
        /// </summary>
        [DisplayName("初始状态异常-高度检测光电3")]
        public virtual Boolean Error_CheckHeight_Phocll_3 { get; set; }
        /// <summary>
        /// 初始状态异常-超高检测光电
        /// </summary>
        [DisplayName("初始状态异常-超高检测光电")]
        public virtual Boolean Error_OverHeight_Phocll { get; set; }
        /// <summary>
        /// 初始状态异常-超前检测光电
        /// </summary>
        [DisplayName("初始状态异常-超前检测光电")]
        public virtual Boolean Error_OverLength_Front_Phocll { get; set; }
        /// <summary>
        /// 初始状态异常-超后检测光电
        /// </summary>
        [DisplayName("初始状态异常-超后检测光电")]
        public virtual Boolean Error_OverLength_Back_Phocll { get; set; }
        /// <summary>
        /// 初始状态异常-左超差检测光电
        /// </summary>
        [DisplayName("初始状态异常-左超差检测光电")]
        public virtual Boolean Error_OverLeft_Phocll { get; set; }
        /// <summary>
        /// 初始状态异常-右超差检测光电
        /// </summary>
        [DisplayName("初始状态异常-右超差检测光电")]
        public virtual Boolean Error_OverRight_Phocll { get; set; }


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
                    case "ShapeType":
                        return this.ShapeType;
                    case "Left_Over":
                        return this.Left_Over;
                    case "Right_Over":
                        return this.Right_Over;
                    case "Front_Over":
                        return this.Front_Over;
                    case "Back_Over":
                        return this.Back_Over;
                    case "High_Over":
                        return this.High_Over;
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
                        this.ShapeStatus = (ShapeCheckResults)Convert.ToUInt16(value);
                        break;
                    case "ShapeType":
                        this.ShapeType = Convert.ToUInt16(value);
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
                    case "High_Over":
                        this.High_Over = Convert.ToBoolean(value);
                        break;
                    default:
                        throw new NotSupportedException(String.Format("无法识别属性名 {0}", name));
                }
            }
        }

        /// <summary>
        /// 警告信息
        /// </summary>
        public String[] Warnings
        {
            get
            {
                List<string> result = new List<string>();
                if (ShapeStatus == ShapeCheckResults.UnKnow)
                    result.Add("检测结果未知");
                if (ShapeStatus == ShapeCheckResults.Error)
                    result.Add("检测失败");
                if (Left_Over)
                    result.Add("左超差");
                if (Right_Over)
                    result.Add("右超差");
                if (High_Over)
                    result.Add("高超差");
                if (Back_Over)
                    result.Add("后超差");
                if (Front_Over)
                    result.Add("前超差");
                return result.ToArray();
            }
        }

        /// <summary>
        /// 外形检测结果是否有效
        /// true 有效 false 无效
        /// </summary>
        public Boolean IsEffective
        {
            get
            {
                return ShapeStatus != ShapeCheckResults.UnKnow;
            }
        }
        /// <summary>
        /// 外形检测信息
        /// </summary>
        public Dictionary<String, String> Information
        {
            get
            {
                Dictionary<String, String> result = new Dictionary<string, string>();
                foreach (var item in this.GetType().GetProperties())
                {
                    if (item.Name != item.GetDisplayName())
                        result.Add(item.GetDisplayName(), item.GetValue(this, null).ToString());
                }
                return result;
            }
        }

        public override ReceivedDataLog ToLogData(Device device)
        {
            return new ShapeCheckTransferObjectLogData(device, this);
        }

        public Dictionary<String, String> GetResult
        {
            get
            {
                Dictionary<String, String> result = new Dictionary<string, string>();
                if (ShapeStatus == ShapeCheckResults.OK || ShapeStatus == ShapeCheckResults.Error)
                {
                    foreach (var item in this.GetType().GetProperties())
                    {
                        if (item.Name != item.GetDisplayName())
                            result.Add(item.GetDisplayName(), item.GetValue(this, null).ToString());
                    }
                }
                return result;
            }
        }
    }
}
