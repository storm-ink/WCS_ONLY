using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Devices;
using System.ComponentModel;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 货位当前任务
    /// </summary>
    [DisplayName("货位当前任务")]
    public class LocationCurrentTask : NetTransferObject
    {
        [DisplayName("货位号")]
        public UInt16 PosNo { get; set; }
        [DisplayName("任务号")]
        public UInt32 TaskNo { get; set; }
        [DisplayName("托盘号")]
        public UInt16 TUID{get;set;}
        public Boolean Str_Rcv_X{get;set;}
        public Boolean Fnh_Rcv_X{get;set;}
        public Boolean Rqs_Snt{get;set;}
        public Boolean Rcv_Rdy{get;set;}
        public Boolean Str_Rcv_Y{get;set;}
        public Boolean Fnh_Rcv_Y{get;set;}
        public override object this[string name]
        {
            set
            {
                switch (name)
                {
                    case "PosNo":
                        this.PosNo = Convert.ToUInt16(value);
                        break;
                    case "TaskNo":
                        this.TaskNo = Convert.ToUInt32(value);
                        break;
                    case "TUID":
                        this.TUID = Convert.ToUInt16(value);
                        break;
                    case "Str_Rcv_X":
                        this.Str_Rcv_X = Convert.ToBoolean(value);
                        break;
                    case "Fnh_Rcv_X":
                        this.Fnh_Rcv_X = Convert.ToBoolean(value);
                        break;
                    case "Rqs_Snt":
                        this.Rqs_Snt = Convert.ToBoolean(value);
                        break;
                    case "Rcv_Rdy":
                        this.Rcv_Rdy = Convert.ToBoolean(value);
                        break;
                    case "Str_Rcv_Y":
                        this.Str_Rcv_Y = Convert.ToBoolean(value);
                        break;
                    case "Fnh_Rcv_Y":
                        this.Fnh_Rcv_Y = Convert.ToBoolean(value);
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }

        public override string ToString()
        {
            return String.Format("货位任务#{0}(在位置 {1} 上)", this.TaskNo,this.PosNo);
        }
    }
}
