using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    public class ElectricCabinetBlock : NetTransferObject
    {
        /// <summary>
        /// 区域编号
        /// </summary>
        public UInt16 AreaNo { get; set; }
        /// <summary>
        /// 电柜状态
        /// </summary>
        /// <remarks>0:初始化,1:有请求</remarks>
        public UInt32 State { get; set; }
        /// <summary>
        /// 灯状态输入输出
        /// </summary>
        public Byte[] LightIO { get; set; }

        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "AreaNo":
                        return AreaNo;
                    case "State":
                        return State;
                    case "LightIO":
                        return LightIO;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的取值操作", this.GetType(), name));
                }
            }
            set
            {
                switch (name)
                {
                    case "AreaNo":
                        this.AreaNo = Convert.ToUInt16(value);
                        break;
                    case "State":
                        this.State = Convert.ToUInt32(value);
                        break;
                    case "LightIO":
                        this.LightIO = (Byte[])value;
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
                AreaNo = this.AreaNo,
                State = this.State,
                //LightIO = string.Join(" ", this.LightIO)
                LightIO = this.LightIO != null ? string.Join(" ", this.LightIO) : string.Empty

            };
        }
    }
}
