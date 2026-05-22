using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 穿梭车状态上报块
    /// </summary>
    public class VehicleBlock : NetTransferObject
    {
        /// <summary>
        /// 车号
        /// </summary>
        public UInt16 VehicleNo { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public VehicleStatus State { get; set; }
        /// <summary>
        /// 事件
        /// </summary>
        public VehicleEvents Event { get; set; }
        /// <summary>
        /// 条码值/激光测距值
        /// </summary>
        public UInt32 Position { get; set; }
        /// <summary>
        /// 报警信息
        /// </summary>
        public Byte[] Alarms { get; set; }

        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "VehicleNo":
                        return VehicleNo;
                    case "State":
                        return State;
                    case "Event":
                        return Event;
                    case "Position":
                        return Position;
                    case "Alarms":
                        return Alarms;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的取值操作", this.GetType(), name));
                }
            }
            set
            {
                switch (name)
                {
                    case "VehicleNo":
                        this.VehicleNo = Convert.ToUInt16(value);
                        break;
                    case "State":
                        this.State = (VehicleStatus)Convert.ToInt16(value);
                        break;
                    case "Event":
                        this.Event = (VehicleEvents)Convert.ToInt16(value);
                        break;
                    case "Position":
                        this.Position = Convert.ToUInt32(value);
                        break;
                    case "Alarms":
                        this.Alarms = (Byte[])value;
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

        public override object GetDataGridViewShow()
        {
            return new
            {
                VehicleNo = this.VehicleNo,
                State = this.State.GetDescription(),
                Event = this.Event.GetDescription(),
                Position = this.Position,
                //Alarms = string.Join(" ", this.Alarms)
                Alarms = this.Alarms != null ? string.Join(" ", this.Alarms) : string.Empty

            };
        }

        public List<string> GetErrorList()
        {
            var alarmVersion = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("VehicleBlockAlarmVersion", "UInt16Version");
            string path = null;
            if (alarmVersion == "BitVersion")
            {
                path = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("VehicleBlockAlarmMappingPath", "");
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                    throw new InvalidOperationException("报警版本 BitVersion 时，未配置 VehicleBlockAlarmMappingPath");
            }
            return GeneralMappingHelper.GetErrorCodeList(this.Alarms.ToList(), this.VehicleNo.ToString(), alarmVersion, null, path);
        }
    }
}
