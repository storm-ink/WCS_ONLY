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
    /// 输送线货位状态对象.
    /// 配置
    ///   <collection type="Wcs.DefaultImpls.Conveyor.LocationNetTransferObject, Wcs.DefaultImpls" blockBytes="4" itemCount="1">
    ///     <property name="ShapeCheckNO" index="0" size="2" type="UInt16" />
    ///     <property name="Status" index="2" size="2" type="UInt16" />
    ///   </collection>
    /// </summary>
    [Description("输送线货位状态")]
    [JsonObject]
    public class LocationNetTransferObject : NetTransferObject
    {
        /// <summary>
        /// 货位号.
        /// </summary>
        /// <value>
        /// 位置在设备中的编码形式.
        /// </value>
        [Description("货位号")]
        public UInt16 PosNo{get;set;}
        /// <summary>
        /// 状态.
        /// </summary>
        [Description("状态")]
        public LocationNetTransferObjectStatus Status { get; set; }

        /// <summary>
        /// 动作-停止
        /// </summary>
        [Description("状态")]
        public bool Stop { get; set; }
        /// <summary>
        /// 动作-向前
        /// </summary>
        [Description("向前")]
        public bool Forword { get; set; }
        /// <summary>
        /// 动作-向后
        /// </summary>
        [Description("向后")]
        public bool Back { get; set; }
        /// <summary>
        /// 动作-正转
        /// </summary>
        [Description("顺时针旋转")]
        public bool 顺时针旋转 { get; set; }
        /// <summary>
        /// 动作-正转
        /// </summary>
        [Description("逆时针旋转")]
        public bool 逆时针旋转 { get; set; }
        /// <summary>
        /// 动作-正转
        /// </summary>
        [Description("上升")]
        public bool UP { get; set; }
        /// <summary>
        /// 动作-正转
        /// </summary>
        [Description("下降")]
        public bool Down { get; set; }
        /// <summary>
        /// 状态.
        /// </summary>
        [Description("角度")]
        public UInt16 Angle { get; set; }

        public override object this[string name]
        {
            get 
            {
                switch (name)
                {
                    case "PosNo":
                        return this.PosNo;
                    case "Status":
                        return this.Status;
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
                    case "Status":
                        this.Status = (LocationNetTransferObjectStatus)Convert.ToInt16(value);
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }

        public override ReceivedDataLog ToLogData(Device device)
        {
            return new LocationDataLog(device, this);
        }
    }
}
