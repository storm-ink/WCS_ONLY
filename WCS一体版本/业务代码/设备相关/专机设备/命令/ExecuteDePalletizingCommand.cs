using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs.DefaultImplementCollection.Conveyor;

using Wcs.Framework;

namespace ZHQXC
{

    public class ExecuteDePalletizingCommand : DeviceCommand, IDeviceCommandAdjudicator
    {


        public ExecuteDePalletizingCommand() { }
        public UInt16 PosNo { get; set; }
        public UInt16 HandShake { get; set; }
        public UInt16 Room { get; set; }
        public UInt16 StartLayer { get; set; }
        public UInt16 EndLayer { get; set; }
        public UInt16 IsNG { get; set; }
        public UInt16 IsOver { get; set; }

        public string TaskNo { get; set; }
        public string Box { get; set; }
        public UInt16 DataID { get; set; } = (UInt16)new Random().Next(1, UInt16.MaxValue);
        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "PosNo":
                        return this.PosNo;
                    case "HandShake":
                        return this.HandShake;
                    case "Room":
                        return this.Room;
                    case "StartLayer":
                        return this.StartLayer;
                    case "EndLayer":
                        return this.EndLayer;
                    case "IsNG":
                        return this.IsNG;
                    case "IsOver":
                        return this.IsOver;
                    case "TaskNo":
                        return this.TaskNo;
                    case "Box":
                        return this.Box;
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
                    case "HandShake":
                        this.PosNo = Convert.ToUInt16(HandShake);
                        break;
                    case "Room":
                        this.Room = Convert.ToUInt16(value);
                        break;
                    case "StartLayer":
                        this.StartLayer = Convert.ToUInt16(value);
                        break;
                    case "EndLayer":
                        this.EndLayer = Convert.ToUInt16(value);
                        break;
                    case "IsNG":
                        this.IsNG = Convert.ToUInt16(value);
                        break;
                    case "IsOver":
                        this.IsOver = Convert.ToUInt16(value);
                        break; 
                    case "TaskNo":
                        this.TaskNo = value.ToString().Trim();
                        break;
                    case "Box":
                        this.Box = value.ToString().Trim();
                        break;
                    case "DataID":
                        this.DataID = Convert.ToUInt16(value);
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }

        public bool SendSuccess(TaskableDevice taskableDevice, DeviceCommand command)
        {
            ConveyorDevice conveyorDevice = (ConveyorDevice)taskableDevice;
            var taskBlocks = conveyorDevice.ReadStatus<ReportDePalletizingDataObject>();
            if (taskBlocks.Any(x => x.PosNo == this.PosNo && x.TaskNo == this.TaskNo))
                return true;
            else
                return false;
        }
    }
}
