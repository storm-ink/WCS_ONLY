using System.Collections.Generic;

namespace Sineva.WMS.Dto.WCSDto.RequestDto
{
    public class MeasureInfoReportRequest : RequestBase
    {
        public DataInfo Data { get; set; }

        public class DataInfo
        {
            public string PortCode { get; set; }//尺检站点
            public Dictionary<string, string> AdditionalAttr { get; set; } //附加信息 外形检测失败的原因
            public CargoData Info { get; set; }
            public string RequestId { get; set; } //请求ID
            public string Message { get; set; } //详细信息

            public class CargoData
            {
                public string BoxCode { get; set; }
                public string TrayBarcode { get; set; }
                public string isContainer { get; set; }//外形是否有容器
                public bool result { get; set; }//视觉检测结果

            }
        }
    }
}