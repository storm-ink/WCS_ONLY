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
    /// 活动平台状态
    /// </summary>
    [Description("活动平台状态")]
    [JsonObject]
    public class AlivePlatformNetTransferObject : NetTransferObject
    {
        /// <summary>
        /// 货位号
        /// </summary>
        [Description("货位号")]
        public UInt16 PosNo { get; set; }
        /// <summary>
        /// 活动平台位置
        /// </summary>
        [Description("状态")]
        public HomePosStatus HomePos { get; set; }

        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "PosNo":
                        return this.PosNo;
                    case "HomePos":
                        return this.HomePos;
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
                    case "HomePos":
                        this.HomePos = (HomePosStatus)Convert.ToUInt16(value);
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
