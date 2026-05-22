using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace ZHQXC
{
    public class SpecialAircraftEquipmentDateResultObject : NetTransferObject
    {
   
        public UInt16 PosNo { get; set; }
        public UInt16 IsFull { get; set; }
        public UInt16 TaskType { get; set; }
        public String TrayBarcode { get; set; }
        public UInt16 DataID { get; set; }
        public UInt16 PosNoStatus { get; set; }
        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "PosNo":
                        return this.PosNo;
                    case "IsFull":
                        return this.IsFull;
                    case "TaskType":
                        return this.TaskType;
                    case "TrayBarcode":
                        return this.TrayBarcode;
                    case "DataID":
                        return this.DataID;
                    case "PosNoStatus":
                        return this.PosNoStatus;
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
                    case "IsFull":
                        this.IsFull = Convert.ToUInt16(value);
                        break;
                    case "TaskType":
                        this.TaskType = Convert.ToUInt16(value);
                        break;
                    case "TrayBarcode":
                        this.TrayBarcode = value.ToString().Trim();
                        break;
                    case "DataID":
                        this.DataID = Convert.ToUInt16(value);
                        break;
                    case "PosNoStatus":
                        this.PosNoStatus = Convert.ToUInt16(value);
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
