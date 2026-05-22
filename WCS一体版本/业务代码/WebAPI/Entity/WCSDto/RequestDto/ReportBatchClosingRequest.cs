using System;
using System.Collections.Generic;
using System.Text;

namespace Sineva.WMS.Dto.WCSDto.RequestDto
{
    public class ReportBatchClosingRequest : RequestBase
    {
        public RequestInfo Data { get; set; }
        public class RequestInfo
        {
            //工位
            public string LocationCode { get; set; }
            //批次号
            public string BatchNumber { get; set; }
            //数量：数量可以为0，范围：0-36
            public int Quantity { get; set; }
            //结批：0：未结批，1：结批
            public int BatchClosing { get; set; }
            //附加信息
            public Dictionary<string, string> AdditionalAttr { get; set; }
        }
    }
}
