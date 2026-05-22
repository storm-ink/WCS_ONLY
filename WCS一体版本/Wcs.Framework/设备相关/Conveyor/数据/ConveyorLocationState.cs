using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
namespace Wcs.Framework.Devices.Conveyor
{
    /// <summary>
    /// 输送线货位状态对象.
    /// </summary>
    [Description("输送线货位状态")]
    public class ConveyorLocationState : NetTransferObject
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
        public ConveyorLocationStatus Status { get; set; }

        public override object this[string name]
        {
            set 
            {
                switch (name)
                {
                    case "PosNo":
                        this.PosNo = Convert.ToUInt16(value);
                        break;
                    case "Status":
                        this.Status = (ConveyorLocationStatus)Convert.ToInt32(value);
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }
    }
}
