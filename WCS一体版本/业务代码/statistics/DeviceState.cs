using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZHQXC.Statistics
{
    public partial class DeviceState
    {
        public DeviceState()
        {
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Device { get; set; }
        /// <summary>
        /// 车辆状态
        /// </summary>
        public DeviceStatus LastState { get; set; }
        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime LastTimespan { get; set; }
        /// <summary>
        /// 车辆状态
        /// </summary>
        public DeviceStatus State { get; set; }
        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timespan { get; set; }

        /// <summary>
        /// 持续时间（秒）
        /// </summary>
        public double TotalSeconds { get; set; }
        /// <summary>
        /// 备注0
        /// </summary>
        public string Mark0 { get; set; } = "";
        /// <summary>
        /// 备注1
        /// </summary>
        public string Mark1 { get; set; } = "";
        /// <summary>
        /// 备注2
        /// </summary>
        public string Mark2 { get; set; } = "";

        public object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "Device":
                        return this.Device;
                    case "LastState":
                        return this.LastState;
                    case "LastTimespan":
                        return this.LastTimespan;
                    case "State":
                        return this.State;
                    case "Timespan":
                        return this.Timespan;
                    case "TotalSeconds":
                        return this.TotalSeconds;
                    case "Mark0":
                        return this.Mark0;
                    case "Mark1":
                        return this.Mark1;
                    case "Mark2":
                        return this.Mark2;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的取值操作", this.GetType(), name));
                }
            }
            set
            {
                switch (name)
                {
                    case "Device":
                        this.Device = value.ToString();
                        break;
                    case "LastState":
                        if (int.TryParse(value.ToString(), out int lastState))
                            this.LastState = (DeviceStatus)lastState;
                        else
                            this.LastState = (DeviceStatus)Enum.Parse(typeof(DeviceStatus), value.ToString());
                        break;
                    case "LastTimespan":
                        this.LastTimespan = Convert.ToDateTime(value);
                        break;
                    case "State":
                        if (int.TryParse(value.ToString(), out int state))
                            this.State = (DeviceStatus)state;
                        else
                            this.State = (DeviceStatus)Enum.Parse(typeof(DeviceStatus), value.ToString());
                        break;
                    case "Timespan":
                        this.Timespan = Convert.ToDateTime(value);
                        break;
                    case "TotalSeconds":
                        this.TotalSeconds = Convert.ToDouble(value);
                        break;
                    case "Mark0":
                        this.Mark0 = value.ToString();
                        break;
                    case "Mark1":
                        this.Mark1 = value.ToString();
                        break;
                    case "Mark2":
                        this.Mark2 = value.ToString();
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }

        }
    }

    /// <summary>
    /// 设备状态
    /// </summary>
    public enum DeviceStatus
    {
        /// <summary>
        /// 开始统计
        /// </summary>
        Start,
        /// <summary>
        /// 结束统计
        /// </summary>
        End,
        /// <summary>
        /// 空闲
        /// </summary>
        IDLE,
        /// <summary>
        /// 等待
        /// </summary>
        WAIT,
        /// <summary>
        /// 行走
        /// </summary>
        MOVING,
        /// <summary>
        /// 移栽
        /// </summary>
        PICKORPUT,
        /// <summary>
        /// 充电
        /// </summary>
        CHARGING,
        /// <summary>
        /// 报警
        /// </summary>
        Alarm,
        /// <summary>
        /// 未连接
        /// </summary>
        DISCONNECTED,
        /// <summary>
        /// 未收到数据
        /// </summary>
        NORECEIVEDDATA,
        /// <summary>
        /// 手动或者离线
        /// </summary>
        MANUALOROFFLINE,
        /// <summary>
        /// 故障恢复
        /// </summary>
        REPAIR,
        /// <summary>
        /// 异常状态码
        /// 非ALARM状态
        /// </summary>
        Abnormal,
        /// <summary>
        /// 未就绪
        /// </summary>
        NotReady
    }
}
