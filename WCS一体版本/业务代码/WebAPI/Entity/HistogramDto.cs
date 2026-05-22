using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZHQXC.WebAPI.Entity
{
    /// <summary>
    /// 直线图
    /// </summary>
    public class HistogramDto
    {
        /// <summary>
        /// X
        /// </summary>
        [JsonProperty("x")]
        public string XValue { get; set; }
        /// <summary>
        /// Y1
        /// </summary>
        [JsonProperty("y1")]
        public int Y1Value { get; set; }
        /// <summary>
        /// Y2
        /// </summary>
        [JsonProperty("y2")]
        public int Y2Value { get; set; }
        /// <summary>
        /// 占比值
        /// </summary>
        [JsonProperty("percent")]
        public int PercentValue { get; set; }
    }
}
