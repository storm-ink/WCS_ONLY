using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 升降/转台输送机待机位+出入库口模式上报块
    /// </summary>
    public class DefaultLocationBlock : NetTransferObject
    {
        /// <summary>
        /// 货位号
        /// </summary>
        public UInt16 PosNo { get; set; }
        /// <summary>
        /// 设备类型
        /// </summary>
        /// <remarks>0-未知设备，1-提升机，2旋转机，3.PortIO，4。。。依次类推</remarks>
        public UInt16 DeviceType { get; set; }
        /// <summary>
        /// 待机位
        /// </summary>
        /// <remarks>
        /// 具体定义根据设备类型来定：
        /// 提升机：0-未设置，1-1层，2-2层。。。依次类推
        /// 旋转机：0-未设置，0-0°，1-1°。。。依次类推，一般来说旋转机只有0 90 270 360取值
        /// POrtIO：0未设置，1-入库，2-出库
        /// </remarks>
        public UInt16 HomePos { get; set; }
        public UInt16 Request { get; set; }

        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "PosNo":
                        return PosNo;
                    case "DeviceType":
                        return DeviceType;
                    case "HomePos":
                        return HomePos;
                    case "Request":
                        return Request;
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
                    case "DeviceType":
                        this.DeviceType = Convert.ToUInt16(value);
                        break;
                    case "HomePos":
                        this.HomePos = Convert.ToUInt16(value);
                        break;
                    case "Request":
                        this.Request = Convert.ToUInt16(value);
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
