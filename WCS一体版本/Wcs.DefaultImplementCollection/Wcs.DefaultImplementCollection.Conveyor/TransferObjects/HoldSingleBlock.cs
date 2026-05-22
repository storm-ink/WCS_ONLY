using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    public class HoldSingleBlock : NetTransferObject
    {
        /// <summary>
        /// 货位号
        /// </summary>
        public UInt16 PosNo { get; set; }
        /// <summary>
        /// 握手
        /// </summary>
        /// <remarks>0:初始化,1:有请求</remarks>
        public RequestHandShakes HandShake { get; set; }

        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "PosNo":
                        return PosNo;
                    case "HandShake":
                        return HandShake;
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
                        this.HandShake = (RequestHandShakes)Convert.ToInt16(value);
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
