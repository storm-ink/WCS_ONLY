using System;
using System.Collections.Generic;
using System.Text;

namespace Sineva.WMS.Dto.WCSDto.RequestDto
{
    public class IssueNonCompliantRequest : RequestBase
    {
        public DataInfo Data { get; set; }
        public class DataInfo
        {
            public string TaskId { get; set; }//任务号：任务号唯一
            public string LocationCode { get; set; }//工位
            public string BoxCode { get; set; } //箱号
            public int Chamber { get; set; }
            public int StartLayer { get; set; }
            public int EndLayer { get; set; }
            public int IsNG { get; set; }
            public string IsFinish { get; set; }

           
            public Dictionary<string, string> AdditionalAttr { get; set; } //附加信息
        }
    }
}
