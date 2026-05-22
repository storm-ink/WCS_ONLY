using System;
using System.Collections.Generic;
using System.Text;

namespace Sineva.WMS.Dto.WCSDto.RequestDto
{
    public class CaptureCompleteRequest : RequestBase
    {
        public RequestInfo Data { get; set; }
        public class RequestInfo
        {
            //任务号
            public string TaskId { get; set; }
            //附加信息
            public Dictionary<string, string> AdditionalAttr { get; set; }
        }
    }
}
