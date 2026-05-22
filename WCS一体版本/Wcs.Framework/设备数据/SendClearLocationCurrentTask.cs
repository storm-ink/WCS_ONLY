using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 清除货位当前任务的申请
    /// </summary>
    public class SendClearLocationCurrentTask : NetTransferObject
    {
        /// <summary>
        /// 货位号.
        /// </summary>
        /// <value>
        /// 位置在设备中的编码形式.
        /// </value>
        [Description("货位号")]
        public UInt16 PosNo { get; set; }
        public UInt32 TaskNo { get; set; }
        public UInt16 TUID { get; set; }
        public UInt16 Index { get; set; }
        public override object this[string name]
        {
            set
            {
                switch (name)
                {
                    case "PosNo":
                        this.PosNo = Convert.ToUInt16(value);
                        break;
                    case "TaskNo":
                        this.TaskNo = Convert.ToUInt32(value);
                        break;
                    case "TUID":
                        this.TUID = Convert.ToUInt16(value);
                        break;
                    case "Index":
                        this.Index = Convert.ToUInt16(value);
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }

        public override string ToString()
        {
            return String.Format("货位任务清除指令#{0}(在位置 {1}，索引 {2})", this.TaskNo, this.PosNo,this.Index);
        }
    }
}
