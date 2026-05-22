using System;
using System.Collections.Generic;
using System.Text;

namespace Sineva.WMS.Dto.WCSDto.RequestDto
{
    public class BoxArriveInfoReportReply : RequestBase
    {
        public class DataInfo
        {
      
            public string LocationCode { get; set; }
            public string Boxcode { get; set; }//栈板编码
            public string Chamber1Layer { get; set; }//栈板编码
            public string Chamber2Layer { get; set; }//栈板编码
            public string Chamber3Layer { get; set; }//栈板编码
            public string Chamber4Layer { get; set; }//栈板编码

            public Dictionary<string, string> AdditionalAttr { get; set; } //附加信息
        }
        public DataInfo Data { get; set; }
    }
}
