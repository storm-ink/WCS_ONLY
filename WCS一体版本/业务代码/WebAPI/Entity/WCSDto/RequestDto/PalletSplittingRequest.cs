using System;
using System.Collections.Generic;
using System.Text;

namespace Sineva.WMS.Dto.WCSDto.RequestDto
{
    public class PalletSplittingRequest : RequestBase
    {
        public DataInfo Data { get; set; }
        public class DataInfo
        {
            public string RequestId { get; set; }
            public string PortCode { get; set; } //站点
            public string PalletCode { get; set; } //母托
            public string TaskId { get; set; } //设备任务ID
            public Dictionary<string, string> AdditionalAttr { get; set; }//附加信息

        }
    }
}
