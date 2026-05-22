using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZHQXC.WebAPI
{
    /// <summary>
    /// 返回值
    /// </summary>
    public class ResponseDto
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        [JsonProperty("success")]
        public bool Success { get; set; } = true;
        /// <summary>
        /// 错误码
        /// </summary>
        [JsonProperty("status")]
        public int Status { get; set; } = 0;
        /// <summary>
        /// 错误信息
        /// </summary>
        [JsonProperty("error")]
        public string Error { get; set; } = "操作成功";
        /// <summary>
        /// 数据
        /// </summary>
        [JsonProperty("data")]
        public object Data { get; set; }
    }
}
