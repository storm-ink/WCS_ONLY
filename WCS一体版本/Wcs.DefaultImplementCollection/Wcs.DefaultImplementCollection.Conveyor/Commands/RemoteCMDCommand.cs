using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 远程控制指令
    /// </summary>
    public class RemoteCMDCommand : DeviceCommand, IDeviceCommandAdjudicator
    {
        public RemoteCMDCommand()
        {
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="areaNo">区域编号</param>
        /// <param name="emergency">急停</param>
        /// <param name="mute">消息</param>
        /// <param name="stop">停止</param>
        /// <param name="reset">复位</param>
        /// <param name="start">开始</param>
        public RemoteCMDCommand(UInt16 areaNo, bool emergency, bool mute, bool stop, bool reset, bool start)
        {
            this.AreaNo = areaNo;
            this.Emergency = emergency;
            this.Mute = mute;
            this.Stop = stop;
            this.Reset = reset;
            this.Start = start;
        }

        /// <summary>
        /// 控制柜编号
        /// </summary>
        public UInt16 AreaNo { get; set; }

        /// <summary>
        /// 急停
        /// </summary>
        public bool Emergency { get; set; }

        /// <summary>
        /// 消音
        /// </summary>
        public bool Mute { get; set; }

        /// <summary>
        /// 停止
        /// </summary>
        public bool Stop { get; set; }

        /// <summary>
        /// 复位
        /// </summary>
        public bool Reset { get; set; }

        /// <summary>
        /// 启动
        /// </summary>
        public bool Start { get; set; }

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
                    case "AreaNo":
                        return AreaNo;
                    case "Emergency":
                        return Emergency;
                    case "Mute":
                        return Mute;
                    case "Stop":
                        return Stop;
                    case "Reset":
                        return Reset;
                    case "Start":
                        return Start;
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
                    case "AreaNo":
                        this.AreaNo = Convert.ToUInt16(value);
                        break;
                    case "Emergency":
                        this.Emergency = Convert.ToBoolean(value);
                        break;
                    case "Mute":
                        this.Mute = Convert.ToBoolean(value);
                        break;
                    case "Stop":
                        this.Stop = Convert.ToBoolean(value);
                        break;
                    case "Reset":
                        this.Reset = Convert.ToBoolean(value);
                        break;
                    case "Start":
                        this.Start = Convert.ToBoolean(value);
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
