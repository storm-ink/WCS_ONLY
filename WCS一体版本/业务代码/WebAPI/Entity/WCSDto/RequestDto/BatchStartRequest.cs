using System;
using System.Collections.Generic;
using System.Text;

namespace Sineva.WMS.Dto.WCSDto.RequestDto
{
    public class BatchStartRequest : RequestBase
    {
        public class DataInfo
        {
            public string LocationCode { get; set; } 
            public string BatchNumber { get; set; } 
        }
        public DataInfo Data { get; set; }
    }
}
