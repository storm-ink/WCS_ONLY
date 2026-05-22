using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Scanner
{
    /// <summary>
    /// 条码扫描结果
    /// </summary>
    [DisplayName("条码扫描结果")]
    public class ScanerTransferObject : NetTransferObject
    {
        /// <summary>
        /// 条码编号，默认为所在货位号
        /// </summary>
        [DisplayName("条码编号")]
        public UInt16 ScanerNo { get; set; }

        /// <summary>
        /// 条码结果：1=成功；2=失败
        /// </summary>
        [DisplayName("条码结果")]
        public UInt16 ScanResult { get; set; }

        /// <summary>
        /// 条码所在货位已读条码结果
        /// </summary>
        [DisplayName("已读条码结果")]
        public UInt16 ConvReaded { get; set; }


        public override object this[string name]
        {
            get
            {
                switch (name)
                { 
                    case "ScanerNo" :
                        return this.ScanerNo;
                    case "ScanResult" :
                        return this.ScanResult;
                    case "ConvReaded" :
                        return this.ConvReaded;
                    default :
                        throw new NotSupportedException(String.Format("无法识别的属性名 {0}", name));
                }
            }
            set
            {
                switch (name)
                { 
                    case "ScanerNo" :
                        this.ScanerNo = Convert.ToUInt16(value);
                        break;
                    case "ScanResult" :
                        this.ScanResult = Convert.ToUInt16(value);
                        break;
                    case "ConvReaded" :
                        this.ConvReaded = Convert.ToUInt16(value);
                        break;
                    default :
                        throw new NotSupportedException(String.Format("无法识别属性名 {0}",name));
                }
            }
        }

       
    }
}
