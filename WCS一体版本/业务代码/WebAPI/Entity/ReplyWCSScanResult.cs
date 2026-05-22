using Sineva.WMS.Dto.WCSDto.RequestDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZHQXC.WebAPI.Entity
{
    /// <summary>
    /// WMS入库扫码后，回复WCS扫码结果 1053/1054/1055/1045/1037
    /// </summary>
    class ReplyWCSScanResult : RequestBase
    {

        public ReplyWCSScanResultDataInfo Data { get; set; }


       


    }

    public class ReplyWCSScanResultDataInfo
    {
        /// <summary>
        /// 站点号
        /// </summary>
        public string PortCode { get; set; }
        /// <summary>
        /// 条码值
        /// </summary>
        public string BarCode { get; set; }
        /// <summary>
        /// 1-0K;2-NG;
        /// </summary>
        public string Result { get; set; }
        public Dictionary<string, string> AdditionalAttr { get; set; } = new Dictionary<string, string>();

    }
}
