using System;
using System.Collections.Generic;
using System.Text;

namespace Sineva.WMS.Dto.WCSDto.RequestDto
{
    public class PalletRecycleRequest : RequestBase
    {
        public DataInfo Data { get; set; }
        public class DataInfo
        {
            public string PortCode { get; set; } //回收站点
            public Dictionary<string,string> AdditionalAttr { get; set; }//附加信息
            public string RequestId { get; set; }

            public string RequestType { get; set; }
        }
    }
}
