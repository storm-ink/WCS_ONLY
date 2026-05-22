using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 表示一个清除货位任务的命令
    /// </summary>
    [Description("远程控制命令")]
    public class RemoteControlCommand : DeviceCommand, Wcs.Framework.IDeviceCommandAdjudicator
    {
        /// <summary>
        /// 托盘号
        /// </summary>
        [System.ComponentModel.DisplayName("区域号")]
        public UInt16 AreaNo { get; set; }

        /// <summary>
        /// 业务数据
        /// </summary>
        [System.ComponentModel.DisplayName("启动")]
        public bool Start { get; set; }
        /// <summary>
        /// 路径号
        /// </summary>
        [System.ComponentModel.DisplayName("停止")]
        public bool Stop { get; set; }
        /// <summary>
        /// 起点位置
        /// </summary>
        [System.ComponentModel.DisplayName("复位")]
        public bool Reset { get; set; }
        /// <summary>
        /// 随机数
        /// </summary>
        [System.ComponentModel.DisplayName("随机数")]
        public UInt16 DateID { get; set; }


        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "AreaNo":
                        return this.AreaNo;
                    case "Start":
                        return this.Start;
                    case "Stop":
                        return this.Stop;
                    case "Reset":
                        return this.Reset;
                    case "DateID":
                        return this.DateID;
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
                    case "Start":
                        this.Start = Convert.ToBoolean(value);
                        break;
                    case "Stop":
                        this.Stop = Convert.ToBoolean(value);
                        break;
                    case "Reset":
                        this.Reset = Convert.ToBoolean(value);
                        break;
                    case "DateID":
                        this.DateID = Convert.ToUInt16(value);
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }

        public bool SendSuccess(TaskableDevice taskableDevice, DeviceCommand command)
        {
            ConveyorDevice conveyorDevice = (ConveyorDevice)taskableDevice;
            RemoteControlCommand _cmd = (RemoteControlCommand)command;

            if (conveyorDevice.ReadStatus<RemoteControlNetTransferObject>() == null)
                return false;
            if (conveyorDevice.ReadStatus<RemoteControlNetTransferObject>().Length == 0)
                return false;
            var transfer = conveyorDevice.ReadStatus<RemoteControlNetTransferObject>().FirstOrDefault(x => x.AreaNo == _cmd.AreaNo);
            if (transfer == null)
                return false;
            if (_cmd.Start)
                return transfer.Start && _cmd.DateID == transfer.DateID;
            if (_cmd.Stop)
                return transfer.Stop && _cmd.DateID == transfer.DateID;
            if (_cmd.Reset)
                return transfer.Reset && _cmd.DateID == transfer.DateID;

            return transfer.Start == _cmd.Start && transfer.Stop == _cmd.Stop && transfer.Reset == _cmd.Reset && _cmd.DateID == transfer.DateID;
        }
    }
}
