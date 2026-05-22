using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    public class WriteBarCodeAndWeightCommand : DeviceCommand, Wcs.Framework.IDeviceCommandAdjudicator
    {
        [Description("货位号")]
        public UInt16 PosNo { get; set; }

        [Description("条码")]
        public string BarCode { get; set; }

        [Description("重量")]
        public string Weight { get; set; }
        /// <summary>
        /// 对象在设备数据块中的存储位置
        /// </summary>
        [Description("索引")]
        public UInt16 DB_Index { get; set; }
        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "PosNo":
                        return this.PosNo;
                    case "BarCode":
                        return this.BarCode;
                    case "Weight":
                        return this.Weight;
                    case "DB_Index":
                        return this.DB_Index;
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
                    case "BarCode":
                        this.BarCode = value.ToString();
                        break;
                    case "Weight":
                        this.Weight = value.ToString();
                        break;
                    case "DB_Index":
                        this.DB_Index = Convert.ToUInt16(value);
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }

        public bool SendSuccess(TaskableDevice taskableDevice, DeviceCommand command)
        {
            var device = (ConveyorDevice)taskableDevice;
            var bws = device.ReadStatus<ScannerAndWeightNetTransferObject>();
            if (bws == null)
                return false;

            return bws.Any(x => x.PosNo == this.PosNo && x.BarCode == this.BarCode && x.Weight == this.Weight);
        }
    }
}
