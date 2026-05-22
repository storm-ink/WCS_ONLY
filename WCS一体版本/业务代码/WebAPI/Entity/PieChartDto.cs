using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZHQXC.WebAPI.Entity
{
    /// <summary>
    /// 饼图
    /// </summary>
    public class PieChartDto
    {
        /// <summary>
        /// 名称
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        /// <summary>
        /// 数值
        /// </summary>
        [JsonProperty("value")]
        public int Value { get; set; }
    }
}
