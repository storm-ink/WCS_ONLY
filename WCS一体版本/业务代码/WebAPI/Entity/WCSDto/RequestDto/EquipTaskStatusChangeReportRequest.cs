using System;
using System.Collections.Generic;
using System.Text;

namespace Sineva.WMS.Dto.WCSDto.RequestDto
{
    public class EquipTaskStatusChangeReportRequest: RequestBase
    {
        public RequestData Data { get; set; }
        public class RequestData
        {
            public string TaskId { get; set; }
            public string ReportTaskStatus { get; set; }
            public Dictionary<string,string> AdditionalAttr { get; set; }
        }
    }
}
