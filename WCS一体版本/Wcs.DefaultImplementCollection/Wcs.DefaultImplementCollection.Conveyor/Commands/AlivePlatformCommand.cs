using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 活动平台原点设置命令
    /// </summary>
    public class AlivePlatformCommand : DeviceCommand, IDeviceCommandAdjudicator
    {
        public AlivePlatformCommand(UInt16 PosNo, HomePosStatus homePos, UInt16 dataid = 0) : this()
        {
            this.PosNo = PosNo;
            this.HomePos = homePos;
            if (dataid == 0)
                this.DataID = (UInt16)new Random().Next(0, UInt16.MaxValue);
            else
                DataID = dataid;
        }
        public AlivePlatformCommand() : base()
        {

        }
        /// <summary>
        /// 货位号
        /// </summary>
        public UInt16 PosNo { get; set; }
        /// <summary>
        /// 活动平台位置
        /// </summary>
        public HomePosStatus HomePos { get; set; }
        /// <summary>
        /// 随机数
        /// </summary>
        public UInt16 DataID { get; set; }

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

                    case "PosNo":
                        this.PosNo = Convert.ToUInt16(value);
                        break;
                    case "HomePos":
                        this.HomePos = (HomePosStatus)Convert.ToUInt16(value);
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
            return $"设置活动货位{PosNo} 位置 {HomePos}({(int)HomePos}) 随机数为 {DataID}";
        }

        public bool SendSuccess(TaskableDevice taskableDevice, DeviceCommand command)
        {
            var conveyor = (ConveyorDevice)taskableDevice;
            var alivePlatforms = conveyor.ReadStatus<AlivePlatformNetTransferObject>();
            if (alivePlatforms == null || alivePlatforms.Length == 0)
                return false;

            var cmd = (AlivePlatformCommand)command;
            var alivePlatform = alivePlatforms.FirstOrDefault(x => x.PosNo == cmd.PosNo && x.HomePos == cmd.HomePos);
            if (alivePlatform == null)
                return false;
            return true;
        }
    }
}
