using System;
using System.Collections.Generic;
using System.Text;

namespace Sineva.WMS.Dto.WCSDto.RequestDto
{
    public class AlarmReportRequest: RequestBase
    {
        public RequestInfo Data { get; set; }
        public class RequestInfo
        {
            public string AlarmHost { get; set; }
            public string AlarmCode { get; set; }
            public string AlarmDecp { get; set; }
            public string AlarmStatus { get; set; }

            public string AlarmId { get; set;}
        }
    }
}
