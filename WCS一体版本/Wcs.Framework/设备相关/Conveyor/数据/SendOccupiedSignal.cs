using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 发送给设备的占位信息
    /// </summary>
    public class SendOccupiedSignal : NetTransferObject
    {
        public UInt16 PosNo{get;set;}
        public OccupiedSignalHandShake HandShake{get;set;}
        public UInt32 AssignmentID{get;set;}
        public UInt16 TU_ID{get;set;}
        public UInt16 TU_Type{get;set;}
        public UInt16 IO_Data{get;set;}
        public UInt16 Index { get; set; }
        public override object this[string name]
        {
            set
            {
                switch (name)
                {
                    case "PosNo":
                        this.PosNo = Convert.ToUInt16(value);
                        break;
                    case "HandShake":
                        this.HandShake = (OccupiedSignalHandShake)Convert.ToInt32(value);
                        break;
                    case "AssignmentID":
                        this.AssignmentID = Convert.ToUInt32(value);
                        break;
                    case "TU_ID":
                        this.TU_ID = Convert.ToUInt16(value);
                        break;
                    case "TU_Type":
                        this.TU_Type = Convert.ToUInt16(value);
                        break;
                    case "IO_Data":
                        this.IO_Data = Convert.ToUInt16(value);
                        break;
                    case "Index":
                        this.Index = Convert.ToUInt16(value);
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }
        public override string ToString()
        {
            return String.Format("占位信号发送块# {0} 握手 {1}", this.PosNo, HandShake.GetDescription());
        }
    }
}
