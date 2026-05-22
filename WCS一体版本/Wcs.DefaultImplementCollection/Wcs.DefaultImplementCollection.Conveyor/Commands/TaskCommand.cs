using Newtonsoft.Json;
using System;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 任务指令
    /// </summary>
    public class TaskCommand : DeviceCommand, IDeviceCommandAdjudicator
    {
        /// <summary>
        /// 编码时不应该使用该构造函数
        /// </summary>
        public TaskCommand()
        {

        }
        public TaskCommand(TaskHandShakes handShake, UInt32 taskNo, string tuid, UInt16[] ioDatas, UInt16 rotingNo, UInt16 from, UInt16 to, UInt16 dbIndex)
        {
            this.HandShake = handShake;
            this.TaskNo = taskNo;
            this.TUID = tuid;
            this.IODatas = ioDatas;
            this.RotingNo = rotingNo;
            this.From = from;
            this.To = to;
            this.DBIndex = dbIndex;
        }
        /// <summary>
        /// 握手协议（为0时说明为地址为空，可以使用）
        /// </summary>
        /// <remarks>子类应该重写该方法</remarks>
        /// <remarks>目前基类缩减为1和2两个取值，后期需要再重写扩增</remarks>
        public virtual TaskHandShakes HandShake { get; set; }
        /// <summary>
        /// 任务号
        /// </summary>
        /// <remarks>任务号由Wcs本地生成，由1开始累加。最大值9999999（7位）</remarks>
        public UInt32 TaskNo { get; set; }
        /// <summary>
        /// 托盘号
        /// </summary>
        /// <remarks>托盘号，ASCII码对应的值，不足20位在字符串前或者后统一增加空格（ASCII码值32）填充，默认值全空格（ASCII码值32），WCS发给PLC存储并在PLC触摸屏上展示使用，无需反馈给WCS</remarks>
        public string TUID { get; set; } = "                    ";
        /// <summary>
        /// 业务数据
        /// </summary>
        /// <remarks>功能每个项目自定义，功能字节供PLC执行任务过程中逻辑处理使用，无需反馈给WCS，默认值为0</remarks>
        public UInt16[] IODatas { get; set; } = new UInt16[10];
        /// <summary>
        /// 路径号
        /// </summary>
        /// <remarks>指示该任务要使用哪一条电气路径输送货物</remarks>
        public UInt16 RotingNo { get; set; }
        /// <summary>
        /// 任务起点
        /// </summary>
        /// <remarks>指示该任务在指定电气路径中需要从哪一节货位开始运行</remarks>
        public UInt16 From { get; set; }
        /// <summary>
        /// 任务终点
        /// </summary>
        /// <remarks>指示该任务在指定电气路径中需要在哪一节货位停止运行</remarks>
        public UInt16 To { get; set; }
        /// <summary>
        /// 任务要存放的索引号(在db块中的位置，从1开始)
        /// </summary>
        /// <remarks>主任务DB索引号，从1开始</remarks>
        public UInt16 DBIndex { get; set; }
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
                    case "HandShake":
                        return this.HandShake;
                    case "TaskNo":
                        return this.TaskNo;
                    case "TUID":
                        return this.TUID;
                    case "IODatas":
                        return this.IODatas;
                    case "RotingNo":
                        return this.RotingNo;
                    case "From":
                        return this.From;
                    case "To":
                        return this.To;
                    case "DBIndex":
                        return this.DBIndex;
                    case "DataID":
                        return this.DataID;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的取值操作", this.GetType(), name));
                }
            }
            set
            {
                switch (name)
                {
                    case "HandShake":
                        this.HandShake = (TaskHandShakes)Convert.ToInt16(value);
                        break;
                    case "TaskNo":
                        this.TaskNo = Convert.ToUInt32(value);
                        break;
                    case "TUID":
                        this.TUID = value.ToString().Trim();
                        break;
                    case "IODatas":
                        this.IODatas = (UInt16[])value;
                        break;
                    case "RotingNo":
                        this.RotingNo = Convert.ToUInt16(value);
                        break;
                    case "From":
                        this.From = Convert.ToUInt16(value);
                        break;
                    case "To":
                        this.To = Convert.ToUInt16(value);
                        break;
                    case "DBIndex":
                        this.DBIndex = Convert.ToUInt16(value);
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
            //return $"TaskComamd:HandShake={HandShake.ToString()}({(int)HandShake}),TaskID={TaskID},TUID={IODatas},IODatas={string.Join("-", IODatas)},RotingNo={RotingNo},StartMotorNo={StartMotorNo},DestinationNo={DestinationNo},DBIndex={DBIndex},DataID={DataID}";
            return JsonConvert.SerializeObject(this);
        }

        public bool SendSuccess(TaskableDevice taskableDevice, DeviceCommand command)
        {
            ConveyorDevice conveyorDevice = (ConveyorDevice)taskableDevice;
            TaskCommand _cmd = (TaskCommand)command;
            var taskBlocks = conveyorDevice.ReadStatus<TaskBlock>();
            if (_cmd.HandShake == TaskHandShakes.New)
            {
                if (taskBlocks == null || taskBlocks.Length == 0)
                    return false;

                if (taskBlocks.Length < _cmd.DBIndex)
                    return false;

                return taskBlocks[_cmd.DBIndex - 1].TaskNo == _cmd.TaskNo;
            }
            else if (_cmd.HandShake == TaskHandShakes.ApplyForDelete)
            {
                return taskBlocks != null &&
                    taskBlocks[_cmd.DBIndex - 1].TaskNo != _cmd.TaskNo;
            }
            else
                throw new NotImplementedException();
        }
    }
}
