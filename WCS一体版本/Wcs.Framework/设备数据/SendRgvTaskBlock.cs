using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 发送给设备的穿梭车任务信息
    /// </summary>
    public class SendRgvTaskBlock : NetTransferObject
    {
        public UInt16 SC_No{get;set;}
        public UInt32 Chain1_TaskNo { get; set; }
        public UInt16 FromStation1{get;set;}
        public UInt16 ToStation1{get;set;}
        public UInt16 Chain1_FirstAction{get;set;}
        public UInt16 Chain1_SecondAction{get;set;}
        public UInt32 Chain2_TaskNo { get; set; }
        public UInt16 FromStation2{get;set;}
        public UInt16 ToStation2{get;set;}
        public UInt16 Chain2_FirstAction{get;set;}
        public UInt16 Chain2_SecondAction{get;set;}
        public UInt16 Command{get;set;}
        public UInt16 ClearTaskCommand{get;set;}
        public UInt32 Received{get;set;}
        public override object this[string name]
        {
            set
            {
                switch (name)
                {
                    case "SC_No":
                        this.SC_No = Convert.ToUInt16(value);
                        break;
                    case "Chain1_TaskNo":
                        this.Chain1_TaskNo = Convert.ToUInt32(value);
                        break;
                    case "FromStation1":
                        this.FromStation1 = Convert.ToUInt16(value);
                        break;
                    case "ToStation1":
                        this.ToStation1 = Convert.ToUInt16(value);
                        break;
                    case "Chain1_FirstAction":
                        this.Chain1_FirstAction = Convert.ToUInt16(value);
                        break;
                    case "Chain1_SecondAction":
                        this.Chain1_SecondAction = Convert.ToUInt16(value);
                        break;
                    case "Chain2_TaskNo":
                        this.Chain2_TaskNo = Convert.ToUInt32(value);
                        break;
                    case "FromStation2":
                        this.FromStation2 = Convert.ToUInt16(value);
                        break;
                    case "ToStation2":
                        this.ToStation1 = Convert.ToUInt16(value);
                        break;
                    case "Chain2_FirstAction":
                        this.Chain2_FirstAction = Convert.ToUInt16(value);
                        break;
                    case "Chain2_SecondAction":
                        this.Chain2_SecondAction = Convert.ToUInt16(value);
                        break;
                    case "Command":
                        this.Command = Convert.ToUInt16(value);
                        break;
                    case "ClearTaskCommand":
                        this.ClearTaskCommand = Convert.ToUInt16(value);
                        break;
                    case "Received":
                        this.Received = Convert.ToUInt32(value);
                        break;
                }
            }
        }
    }
}
