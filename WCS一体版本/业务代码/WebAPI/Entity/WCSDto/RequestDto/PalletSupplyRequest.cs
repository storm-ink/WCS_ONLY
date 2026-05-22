using System.Collections.Generic;

namespace Sineva.WMS.Dto.WCSDto.RequestDto
{
    public class PalletSupplyRequest : RequestBase
    {
        public DataInfo Data { get; set; }
        public class DataInfo
        {
            public string PortCode { get; set; } //补充站点
            public Dictionary<string, string> AdditionalAttr { get; set; } = new Dictionary<string, string>();//附加信息

            public string RequestId { get; set; }

        }
    }
}