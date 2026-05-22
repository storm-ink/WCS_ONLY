using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sineva.WMS.Dto.WCSDto.RequestDto
{
    class PortModeChangeRequest : RequestBase
    {
        public RequestData Data { get; set; }
        public class RequestData
        {
            public string RequestId { get; set; }
            public string portMode { get; set; }
            public bool isEmpty { get; set; }
        }
    }
}


