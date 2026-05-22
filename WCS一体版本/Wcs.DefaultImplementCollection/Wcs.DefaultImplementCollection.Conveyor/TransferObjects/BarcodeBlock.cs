using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 扫码块
    /// 一般建议如下：
    /// 1.清理-PLC按需自主清理，①离位后清理 ②重新触发后清理
    /// </summary>
    public class BarcodeBlock : NetTransferObject
    {
        /// <summary>
        /// 货位号
        /// </summary>
        public UInt16 PosNo { get; set; }
        /// <summary>
        /// 握手
        /// </summary>
        /// <remarks>0:初始化,1:有请求</remarks>
        public string Barcode { get; set; }
        public string TrayBarcode { get; set; }



        public UInt16 IsContainer { get; set; }

        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "PosNo":
                        return PosNo;
                    case "Barcode":
                        return Barcode;
                    case "IsContainer":
                        return IsContainer;
                    case "TrayBarcode":
                        return TrayBarcode;
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
                    case "Barcode":
                        this.Barcode = value.ToString().Trim();
                        break;
                    case "IsContainer":
                        this.IsContainer = Convert.ToUInt16(value);
                        break;
                    case "TrayBarcode":
                        this.TrayBarcode = value.ToString().Trim();
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
