using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 实时时间上报块
    /// </summary>
    public class DateTimeBlock : NetTransferObject
    {
        /// <summary>
        /// 年
        /// </summary>
        public UInt16 Year { get; set; }
        /// <summary>
        /// 月
        /// </summary>
        public UInt16 Month { get; set; }
        /// <summary>
        /// 日
        /// </summary>
        public UInt16 Day { get; set; }
        /// <summary>
        /// 时
        /// </summary>
        public UInt16 Hour { get; set; }
        /// <summary>
        /// 分
        /// </summary>
        public UInt16 Minuter { get; set; }
        /// <summary>
        /// 秒
        /// </summary>
        public UInt16 Second { get; set; }

        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "Year":
                        return Year;
                    case "Month":
                        return Month;
                    case "Day":
                        return Day;
                    case "Hour":
                        return Hour;
                    case "Minuter":
                        return Minuter;
                    case "Second":
                        return Second;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的取值操作", this.GetType(), name));
                }
            }
            set
            {
                switch (name)
                {
                    case "Year":
                        this.Year = Convert.ToUInt16(value);
                        break;
                    case "Month":
                        this.Month = Convert.ToUInt16(value);
                        break;
                    case "Day":
                        this.Day = Convert.ToUInt16(value);
                        break;
                    case "Hour":
                        this.Hour = Convert.ToUInt16(value);
                        break;
                    case "Minuter":
                        this.Minuter = Convert.ToUInt16(value);
                        break;
                    case "Second":
                        this.Second = Convert.ToUInt16(value);
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
            return new { DateTime = $"{Year}-{Month}-{Day} {Hour}:{Minuter}:{Second}" };
        }
    }
}
