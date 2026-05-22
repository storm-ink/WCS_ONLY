using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    public class TaskBlock : NetTransferObject
    {
        public TaskHandShakes HandShake { get; set; }
        public UInt32 TaskNo { get; set; }
        public UInt16 RotingNo { get; set; }
        public UInt16 From { get; set; }
        public UInt16 To { get; set; }
        public TaskBlockTaskStatus TaskState { get; set; }
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
                    case "RotingNo":
                        return this.RotingNo;
                    case "From":
                        return this.From;
                    case "To":
                        return this.To;
                    case "TaskState":
                        return this.TaskState;
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
                    case "RotingNo":
                        this.RotingNo = Convert.ToUInt16(value);
                        break;
                    case "From":
                        this.From = Convert.ToUInt16(value);
                        break;
                    case "To":
                        this.To = Convert.ToUInt16(value);
                        break;
                    case "TaskState":
                        this.TaskState = (TaskBlockTaskStatus)Convert.ToInt16(value);
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
