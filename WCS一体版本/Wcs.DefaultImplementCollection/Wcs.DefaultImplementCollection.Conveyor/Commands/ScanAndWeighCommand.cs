using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;
namespace Wcs.DefaultImplementCollection.Conveyor
{
    public class ScanAndWeighCommand : DeviceCommand, IDeviceCommandAdjudicator
    {
       
        public ScanAndWeighCommand(UInt16 PosNo, Byte ScanningStatus, Byte WeighingStatus, UInt16 Data_ID):this()
        {

            this.PosNo = PosNo;
            this.ScanningStatus = ScanningStatus;
            this.WeighingStatus = WeighingStatus;
            this.Data_ID = Data_ID;
        }
        public ScanAndWeighCommand() : base()
        {

        }
        /// <summary>
        /// 货位号
        /// </summary>
        public UInt16  PosNo { get; set; }
        /// <summary>
        /// 扫码状态
        /// 0初始值 1合格 2不合格
        /// </summary>
        public Byte ScanningStatus { get; set; }

        /// <summary>
        /// 称重状态
        /// 0初始值 1合格 2不合格
        /// </summary>
        public Byte WeighingStatus { get; set; }
        /// <summary>
        /// 随机数
        /// </summary>
        public UInt16 Data_ID { get; set; }
      
        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "PosNo":
                        return this.PosNo;
                    case "ScanningStatus":
                        return this.ScanningStatus;
                    case "WeighingStatus":
                        return this.WeighingStatus;
                    case "Data_ID":
                        return this.Data_ID;
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
                    case "ScanningStatus":
                        this.ScanningStatus = Convert.ToByte(value);
                        break;
                    case "WeighingStatus":
                        this.WeighingStatus = Convert.ToByte(value);
                        break;
                    case "Data_ID":
                        this.Data_ID = Convert.ToUInt16(value);
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }

        public override string ToString()
        {
            return String.Format("{0} 货位{1} 扫码状态 {2} 称重状态 {3} 随机数为 {4}", this.GetType(), this.PosNo, this.ScanningStatus, this.WeighingStatus, this.Data_ID);
        }

        public bool SendSuccess(TaskableDevice taskableDevice, DeviceCommand command)
        {
            var conveyor = (ConveyorDevice)taskableDevice;
            var swResults = conveyor.ReadStatus<ScanningAndWeighingTransferObject>();
            if (swResults == null || swResults.Length == 0)
                return false;

            var cmd = (ScanAndWeighCommand)command;
            var alivePlatform = swResults.FirstOrDefault(x => x.PosNo == cmd.PosNo && x.ScanningStatus == cmd.ScanningStatus && x.WeighingStatus == cmd.WeighingStatus);
            if (alivePlatform == null)
                return false;
            return true;
        }
    }
}
