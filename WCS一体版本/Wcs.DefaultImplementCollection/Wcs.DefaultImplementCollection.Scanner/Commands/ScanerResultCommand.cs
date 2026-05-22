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
    public class ScanerResultCommand : DeviceCommand
    {
        public ScanerResultCommand()
        { 
        
        }

        public ScanerResultCommand(UInt16 scanerNo, UInt16 handshake, UInt16 scanerResult, UInt16 db803_Index, UInt32 data_Id)
        {
            ScanerNo = scanerNo;
            this.ScanResult = scanerResult;

            this.DB803_Index = db803_Index;
            this.Data_Id = data_Id;
        }

        /// <summary>
        /// 握手，1=新条码结果，2=删除PLC暂存条码结果
        /// </summary>
        [DisplayName("编号")]
        public UInt16 ScanerNo{ get; set; }

        /// <summary>
        /// 握手，1=新条码结果，2=删除PLC暂存条码结果
        /// </summary>
        [DisplayName("握手")]
        public UInt16 Handshake { get; set; }
         
        /// </summary>
        [DisplayName("条码结果")]
        public UInt16 ScanResult { get; set; }

        /// <summary>
        /// 索引
        /// </summary>
        [DisplayName("索引")]
        public UInt16 DB803_Index { get; set; }
        /// <summary>
        /// 随机数 wcs每次下发数据都会使用不同的Data_ID
        /// </summary>
        [DisplayName("随机数")]
        public UInt32 Data_Id { get; set; }


        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "ScanerNo":
                        return this.ScanerNo;
                    case "Handshake":
                        return this.Handshake;
                    case "ScanResult":
                        return this.ScanResult;
                    case "DB803_Index":
                        return this.DB803_Index;
                    case "Data_Id":
                        return this.Data_Id;
                    default:
                        throw new NotSupportedException(String.Format("无法识别的属性名 {0}", name));
                }
            }
            set
            {
                switch (name)
                {
                    case "ScanerNo":
                        this.ScanerNo = Convert.ToUInt16(value);
                        break;
                    case "Handshake":
                        this.Handshake = Convert.ToUInt16(value);
                        break;
                    case "ScanResult":
                        this.ScanResult = Convert.ToUInt16(value);
                        break;
                    case "DB803_Index":
                        this.DB803_Index = Convert.ToUInt16(value);
                        break;
                    case "Data_Id":
                        this.DB803_Index = Convert.ToUInt16(value);
                        break;
                    default:
                        throw new NotSupportedException(String.Format("无法识别属性名 {0}", name));
                }
            }
        }
    }
}
