using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 表示一个与占位相关的命令
    /// </summary>
    public class HoldSignalCommand : DeviceCommand
    {
        /// <summary>
        /// 编码时不应该使用该构造函数
        /// </summary>
        public HoldSignalCommand():base()
        {

        }
        public HoldSignalCommand(UInt16 posNo, UInt16 io_data, UInt16 index,UInt16 dataId)
        {
            this.PosNo = posNo;
            this.IO_Data = io_data;
            this.DB1023_Index = index;
            this.Data_Id = dataId;
        }
        public UInt16 PosNo{get;set;}
        /// <summary>
        /// 握手变量
        /// </summary>
        /// <remarks>子类应该重写该方法</remarks>
        public virtual HoldSignalNetTransferObjectHandShake HandShake { get; set; }
      
        public UInt16 IO_Data{get;set;}
        public UInt16 DB1023_Index { get; set; }
        public UInt32 Data_Id { get; set; }
        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "PosNo":
                        return this.PosNo;
                    case "IO_Data":
                        return this.IO_Data;
                    case "DB1023_Index":
                        return this.DB1023_Index;
                    case "HandShake":
                        return this.HandShake;
                    case "Data_Id":
                        return this.Data_Id;
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
                    case "IO_Data":
                        this.IO_Data = Convert.ToUInt16(value);
                        break;
                    case "DB1023_Index":
                        this.DB1023_Index = Convert.ToUInt16(value);
                        break;
                    case "HandShake":
                        //throw new InvalidOperationException("不允许对 HandShake 属性进行赋值操作");
                        this.HandShake = (HoldSignalNetTransferObjectHandShake)Convert.ToInt16(value);
                        break;
                    case "Data_Id":
                        this.Data_Id = Convert.ToUInt16(value);
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }
        public override string ToString()
        {
            return String.Format("{0},货位{1}握手{2}",this.GetType(), this.PosNo, HandShake.GetDescription());
        }
    }
}
