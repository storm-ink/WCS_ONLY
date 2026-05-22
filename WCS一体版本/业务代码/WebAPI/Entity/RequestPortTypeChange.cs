using Sineva.WMS.Dto.WCSDto.RequestDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZHQXC.WebAPI.Entity
{
    /// <summary>
    /// port口出入库模式切换
    /// </summary>
    public class RequestPortTypeChange:RequestBase
    {
        public RequestPortTypeChangeDataInfo Data { get; set; }
      
    }
    public class RequestPortTypeChangeDataInfo
    {
        /// <summary>
        /// 站点号
        /// </summary>
        public string PortCode { get; set; }
        /// <summary>
        /// 站点模式：1-入库；2-出库
        /// </summary>
        public string PortType { get; set; }
        /// <summary>
        /// 附加属性
        /// </summary>
        public Dictionary<string, string> AdditionalAttr { get; set; }
        /// <summary>
        /// 站点状态 online:在线  /offline：不在线
        /// </summary>
        public string PortStatus { get; set; }
    }
}
