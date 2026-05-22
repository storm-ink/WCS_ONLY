using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    public class ReportResultInfoBlock : NetTransferObject
    {
        /// <summary>
        /// 货位编号
        /// </summary>
        public UInt16 PosNo { get; set; }
        /// <summary>
        /// 结果
        /// </summary>
        /// <remarks>
        /// 建议：0-未知，1成功，2失败
        /// </remarks>
        public UInt16 Result { get; set; }
        /// <summary>
        /// 中文描述
        /// </summary>
        /// <remarks>
        /// 自定义，每个块根据需要定义
        /// </remarks>
        public UInt16 Description { get; set; }
        /// <summary>
        /// 随机数
        /// </summary>
        /// <remarks>
        /// 随机数，有效数据识别序列号，WCS发送什么PLC转存什么即可
        /// </remarks>
        public UInt16 DataID { get; set; }

        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "PosNo":
                        return PosNo;
                    case "Result":
                        return Result;
                    case "Description":
                        return Description;
                    case "DataID":
                        return DataID;
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
                    case "Result":
                        this.Result = Convert.ToUInt16(value);
                        break;
                    case "Description":
                        this.Description = Convert.ToUInt16(value);
                        break;
                    case "DataID":
                        this.DataID = Convert.ToUInt16(value);
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
