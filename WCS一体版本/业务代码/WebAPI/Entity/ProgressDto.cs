using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZHQXC.WebAPI.Entity
{
    /// <summary>
    /// 百分比信息
    /// </summary>
    public class ProgressDto
    {
        /// <summary>
        /// 名称
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        [JsonProperty("value")]
        public int Value { get; set; }
        /// <summary>
        /// 占比
        /// </summary>
        [JsonProperty("percent")]
        public int Percent { get; set; }
    }
}
