using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 结果消息指令
    /// </summary>
    public class ResultInfoCommand : DeviceCommand, IDeviceCommandAdjudicator
    {
        public ResultInfoCommand()
        {
        }

        public ResultInfoCommand(UInt16 posNo)
        {
            this.PosNo = posNo;
        }

        /// <summary>
        /// 位置编号
        /// </summary>
        public UInt16 PosNo { get; set; }

        /// <summary>
        /// 操作结果
        /// </summary>
        /// <remarks>按需商讨设定该字段值（参考：0-未知，1成功，2失败）</remarks>
        public UInt16 Result { get; set; }
        /// <summary>
        /// 中文描述
        /// </summary>
        /// <remarks>按需商讨设定该字段值，理论上来说不同的值对应不同的显示结果</remarks>
        public UInt16 Description { get; set; }
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
                    case "Result":
                        return Result;
                    case "Description":
                        return Description;
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
                    case "Result":
                        this.Result = Convert.ToUInt16(value);
                        break;
                    case "Description":
                        this.Description = Convert.ToUInt16(value);
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
            ConveyorDevice conveyor = (ConveyorDevice)taskableDevice;
            ResultInfoCommand cmd = (ResultInfoCommand)command;
            ReportResultInfoBlock rrib = conveyor.ReadStatus<ReportResultInfoBlock>().FirstOrDefault(x => x.PosNo == cmd.PosNo);

            return rrib != null && rrib.DataID == cmd.DataID;
        }
    }
}
