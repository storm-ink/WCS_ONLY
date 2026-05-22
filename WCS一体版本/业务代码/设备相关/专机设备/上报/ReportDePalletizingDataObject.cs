using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace ZHQXC
{
    public class ReportDePalletizingDataObject : NetTransferObject
    {
        public UInt16 PosNo { get; set; }
        public UInt16 Room { get; set; }
        public UInt16 StartLayer { get; set; }
        public UInt16 EndLayer { get; set; }
        public UInt16 IsNG { get; set; }
        public UInt16 IsOver { get; set; }

        public String TaskNo { get; set; }
        public String Box { get; set; }
        public UInt16 TaskStatus { get; set; }
      


        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "PosNo":
                        return this.PosNo;
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
                    case "TaskStatus":
                        return this.TaskStatus;
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
                    case "TaskStatus":
                        this.TaskStatus = Convert.ToUInt16(value);
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
    }


}
