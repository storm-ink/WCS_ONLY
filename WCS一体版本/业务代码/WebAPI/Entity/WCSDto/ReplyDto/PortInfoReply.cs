using System;
using System.Collections.Generic;
using System.Text;

namespace Sineva.WMS.Dto.WCSDto.ReplyDto
{
    public class PortInfoReply: ReplyBase
    {
        public List<PortInfo> Info { get; set; }

        public class PortInfo
        {
            public string PortCode { get; set; }
            public string PortStatus { get; set; }
            public string PortIOMode { get; set; }
            public string PortSameGroup { get; set; }
        }
 
    }
}
