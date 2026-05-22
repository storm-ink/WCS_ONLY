using System;
using System.Collections.Generic;
using System.Text;

namespace Sineva.WMS.Dto.WCSDto.RequestDto
{
    public class EquipmentTaskChangeRequest: RequestBase
    {
        public RequestData Data { get; set; }
        public class RequestData
        {
            public string TaskId { get; set; }
            /// <summary>
            /// cancel,目前只开放取消
            /// </summary>
            public string RequestTaskStatus { get; set; }
            public string TaskDestination { get; set; }
            public int TaskPriority { get; set; }
        }
    }
}
