using Sineva.WMS.Dto.WCSDto.RequestDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZHQXC.WebAPI.Entity
{
    /// <summary>
    /// 入库WMS手动扫码后，WCS根据PLC占位申请WMS下发入库任务
    /// </summary>
    public class RequestUpTask: RequestBase
    {
       
        public RequestUpTaskDataInfo Data { get; set; }

    }

    public class RequestUpTaskDataInfo
    {
        /// <summary>
        /// 站点号
        /// </summary>
        public string PortCode { get; set; }

        /// <summary>
        /// 请求入库标志1-0K;2-NG
        /// </summary>
        public string Result { get; set; }
        /// <summary>
        /// 信号唯一标识
        /// </summary>
        public string KeyId { get; set; }
        /// <summary>
        /// 附加属性
        /// </summary>
        public Dictionary<string, string> AdditionalAttr { get; set; } = new Dictionary<string, string>();

    }
}
