using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZHQXC.WebAPI.Entity
{
    public class MessageDto
    {
        /// <summary>
        /// 索引
        /// </summary>
        [JsonProperty("index")]
        public int Index { get; set; }
        /// <summary>
        /// 文本
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
