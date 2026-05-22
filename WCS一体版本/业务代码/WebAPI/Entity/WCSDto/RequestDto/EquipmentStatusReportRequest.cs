using System;
using System.Collections.Generic;
using System.Text;

namespace Sineva.WMS.Dto.WCSDto.RequestDto
{
    public class EquipmentStatusReportRequest : RequestBase
    {
        public List<DataInfo> Info { get; set; }
        public class DataInfo
        {
            public string DeviceName { get; set; }
            public string DeviceType { get; set; }
            public bool IsOk { get; set; }
            public bool IsJob { get; set; }
            public bool IsAlarm { get; set; }
            public string AlarmDescription { get; set; }
            public string Warehouse { get; set; }
        }
    }
}
