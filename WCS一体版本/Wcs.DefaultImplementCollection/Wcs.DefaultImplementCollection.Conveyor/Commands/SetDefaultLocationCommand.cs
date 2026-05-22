using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 设置升降/转台输送机待机位
    /// </summary>
    public class SetDefaultLocationCommand : DeviceCommand, IDeviceCommandAdjudicator
    {
        public SetDefaultLocationCommand()
        {
        }

        public SetDefaultLocationCommand(UInt16 posNo,UInt32 deviceType)
        {
            this.PosNo = posNo;
            this.DeviceType = deviceType;
        }

        /// <summary>
        /// 位置编号
        /// </summary>
        public UInt16 PosNo { get; set; }
        /// <summary>
        /// 设备类型
        /// </summary>
        /// <remarks>0-未知设备，1-提升机，2旋转机，3.。。。依次类推</remarks>
        /// <remarks>默认0，如果有值那么此值必然来自LocationInfoBlock-DeviceType</remarks>
        public UInt32 DeviceType { get; set; }
        /// <summary>
        /// 待机位
        /// </summary>
        /// <remarks>
        /// 具体项目具体定义
        /// 具体定义根据设备类型来定：
        /// 提升机：0-未设置，1-1层，2-2层。。。依次类推
        /// 旋转机：0-未设置，0-0°，1-1°。。。依次类推，一般来说旋转机只有0 90 270 360取值
        /// 顶升机：0-未设置，1-顶起，2-落下。。。
        /// </remarks>
        public UInt32 HomePos { get; set; }
        /// <summary>
        /// 随机数
        /// </summary>
        /// <remarks>有效数据识别序列号，从1开始，供PLC防止重复执行缓存块命令使用</remarks>
        public UInt16 DataID { get; set; } = (UInt16)new Random().Next(1, UInt16.MaxValue);

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
                    case "DataID":
                        return DataID;
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
                        this.DeviceType = Convert.ToUInt32(value);
                        break;
                    case "HomePos":
                        this.HomePos = Convert.ToUInt32(value);
                        break;
                    case "DataID":
                        this.DataID = Convert.ToUInt16(value);
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

        public bool SendSuccess(TaskableDevice taskableDevice, DeviceCommand command)
        {
            throw new NotImplementedException();
        }
    }
}
