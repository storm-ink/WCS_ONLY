using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 安全信息上报块
    /// </summary>
    public class SafeBlock : NetTransferObject
    {
        /// <summary>
        /// 安全编号
        /// </summary>
        /// <remarks>
        /// 安全编号，所有安全设备统一唯一编号
        /// </remarks>
        public UInt16 SafeNo { get; set; }
        /// <summary>
        /// 安全类型
        /// </summary>
        /// <remarks>0-未知安全设备，1-安全门，2-安全光栅，3.。。。依次类推</remarks>
        public UInt16 SafeType { get; set; }
        /// <summary>
        /// 安全状态
        /// </summary>
        /// <remarks>
        /// 具体定义根据安全类型来定：
        /// 安全光栅：0-未知，1-正常，2-屏蔽， 3-触发
        /// 安全门：0-未知，1-打开，2-关闭
        /// 其余待补充
        /// </remarks>
        public UInt16 SafeStatus { get; set; }

        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "SafeNo":
                        return SafeNo;
                    case "SafeType":
                        return SafeType;
                    case "SafeStatus":
                        return SafeStatus;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的取值操作", this.GetType(), name));
                }
            }
            set
            {
                switch (name)
                {
                    case "SafeNo":
                        this.SafeNo = Convert.ToUInt16(value);
                        break;
                    case "SafeType":
                        this.SafeType = Convert.ToUInt16(value);
                        break;
                    case "SafeStatus":
                        this.SafeStatus = Convert.ToUInt16(value);
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
