using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 安全门状态
    /// </summary>
    [Description("安全门状态")]
    [JsonObject]
    public class SafeDoorNetTransferObject : NetTransferObject
    {
        /// <summary>
        /// 安全门编号
        /// </summary>
        [Description("安全门编号")]
        public UInt16 SafeDoorNo { get; set; }
        /// <summary>
        /// 状态. true-关闭 false-打开
        /// </summary>
        [Description("状态")]
        public bool SafeDoorStatus { get; set; }

        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "SafeDoorNo":
                        return this.SafeDoorNo;
                    case "SafeDoorStatus":
                        return this.SafeDoorStatus;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的取值操作", this.GetType(), name));
                }
            }
            set
            {
                switch (name)
                {
                    case "SafeDoorNo":
                        this.SafeDoorNo = Convert.ToUInt16(value);
                        break;
                    case "SafeDoorStatus":
                        this.SafeDoorStatus = Convert.ToBoolean(value);
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }

        public override ReceivedDataLog ToLogData(Device device)
        {
            return null;
        }
    }
}
