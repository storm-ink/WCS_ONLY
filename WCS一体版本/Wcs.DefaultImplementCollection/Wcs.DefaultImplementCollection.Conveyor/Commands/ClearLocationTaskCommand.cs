using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 清理货位任务指令
    /// </summary>
    public class ClearLocationTaskCommand : DeviceCommand, IDeviceCommandAdjudicator
    {
        public ClearLocationTaskCommand()
        {
        }

        public ClearLocationTaskCommand(UInt16 posNo,UInt32 taskNo)
        {
            this.PosNo = posNo;
            this.TaskNo = taskNo;
        }

        /// <summary>
        /// 位置编号
        /// </summary>
        public UInt16 PosNo { get; set; }
        /// <summary>
        /// 任务号
        /// </summary>
        public UInt32 TaskNo { get; set; }
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
                    case "TaskNo":
                        return TaskNo;
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
                    case "TaskNo":
                        this.TaskNo = Convert.ToUInt32(value);
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
