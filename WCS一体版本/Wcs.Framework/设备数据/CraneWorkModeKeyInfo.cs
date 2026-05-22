using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Devices;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 堆垛机远程控制盒信息
    /// </summary>
    [DisplayName("堆垛机远程控制盒信息")]
    [DataContract]
    public class CraneWorkModeKeyInfo : NetTransferObject,IComparer<CraneWorkModeKeyInfo>
    {
        /// <summary>
        /// 堆垛机编号
        /// </summary>
        [DisplayName("堆垛机号")]
        [DataMember]
        public UInt16 CraneNo { get; set; }
        /// <summary>
        /// 手动按钮是否被按下
        /// </summary>
        [DataMember]
        [DisplayName("是否手动")]
        public Boolean IsManual { get; set; }
        /// <summary>
        /// 急停按钮是否被按下
        /// </summary>
        [DataMember]
        [DisplayName("是否急停")]
        public Boolean IsBrake { get; set; }
        public override object this[string name]
        {
            set
            {
                switch (name)
                {
                    case "CraneNo":
                        this.CraneNo = Convert.ToUInt16(value);
                        break;
                    case "IsManual":
                        this.IsManual = Convert.ToBoolean(value);
                        break;
                    case "IsBrake":
                        this.IsBrake = Convert.ToBoolean(value);
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }
        public override string ToString()
        {
            return String.Format("手动:{0}，急停:{1}", this.IsManual, this.IsBrake);
        }

        public int Compare(CraneWorkModeKeyInfo x, CraneWorkModeKeyInfo y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x == null && y != null)
            {
                return -1;
            }

            if (x != null && y == null)
            {
                return 1;
            }

            if (x.IsBrake!=y.IsBrake || x.IsManual!=y.IsManual)
            {
                return 1;
            }

            return 0;
        }
    }
}
