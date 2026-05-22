using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    public class DBLengthNetTransferObject : NetTransferObject
    {
        /// <summary>
        /// 拆叠盘机编号
        /// </summary>
        [DisplayName("组数")]
        public UInt16 ItemCount { get; set; }

        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "ItemCount":
                        return this.ItemCount;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的取值操作", this.GetType(), name));
                }
            }
            set
            {
                switch (name)
                {
                    case "ItemCount":
                        this.ItemCount = Convert.ToUInt16(value);
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }

        public override Framework.ReceivedDataLog ToLogData(Framework.Device device)
        {
            return null;
        }
    }
}
