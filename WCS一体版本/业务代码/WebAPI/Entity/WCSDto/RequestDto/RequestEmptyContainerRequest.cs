using System;
using System.Collections.Generic;
using System.Text;

namespace Sineva.WMS.Dto.WCSDto.RequestDto
{
    public class RequestEmptyContainerRequest : RequestBase
    {
        public RequestInfo Data { get; set; }
        public class RequestInfo
        {
            public string LocationCode { get; set; } //回收站点
            public string RequestId { get; set; } //回收站点

            public Dictionary<string, string> AdditionalAttr { get; set; }//附加信息
        }
    }
}
