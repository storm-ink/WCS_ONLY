using System;
using System.Collections.Generic;
using System.Text;

namespace Sineva.WMS.Dto.WCSDto.RequestDto
{
    public class AssignEquipTaskRequest : RequestBase
    {
        public class DataInfo
        {
            public string RequestId { get; set; }//尺检请求ID 和上架关联
            public class SiteInfo
            {
                public string Source { get; set; } //起点
                public string Destination { get; set; } //终点

            }
            public string PalletCode { get; set; }//栈板编码
            public SiteInfo Site { get; set; }
            public string TaskId { get; set; } //任务号
            public string TaskMode { get; set; } //任务模式
            public int Priority { get; set; } //优先级
            public Dictionary<string,string> AdditionalAttr { get; set; } //附加信息
        }
        public DataInfo Data { get; set; }
    }
}
