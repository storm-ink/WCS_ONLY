using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZHQXC.WebAPI.Entity
{
    /// <summary>
    /// 面板
    /// </summary>
    public class TableDto
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
        public string Value { get; set; }
        /// <summary>
        /// 颜色
        /// </summary>
        [JsonProperty("color")]
        public string Color { get; set; }
    }
}
