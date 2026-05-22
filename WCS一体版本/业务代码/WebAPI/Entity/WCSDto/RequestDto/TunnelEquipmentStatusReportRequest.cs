using System;
using System.Collections.Generic;
using System.Text;

namespace Sineva.WMS.Dto.WCSDto.RequestDto
{
    public class TunnelEquipmentStatusReportRequest : RequestBase
    {
        public List<DataInfo> Info { get; set; }
        public class DataInfo
        {
            public string TunnelNo { get; set; }
            public string TunnelStatus { get; set; }
        }
    }
}
