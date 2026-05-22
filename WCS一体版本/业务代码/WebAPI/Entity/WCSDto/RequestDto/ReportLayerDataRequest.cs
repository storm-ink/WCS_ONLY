using System;
using System.Collections.Generic;
using System.Text;

namespace Sineva.WMS.Dto.WCSDto.RequestDto
{
    public class ReportLayerDataRequest : RequestBase
    {
        public List<RequestInfo> Data { get; set; }
        public class RequestInfo
        {
            //工位
            public string LocationCode { get; set; }
            //批次号
            string _batchNumber;
            public string BatchNumber
            {
                get => _batchNumber;
                set => _batchNumber = TrimAfterNull(value);
            }
            //数量
            public int Quantity { get; set; }
            //腔室
            public int Chamber { get; set; }
            //层
            public int Layer { get; set; }
            //是否混批
            public int IsMixedBatch { get; set; }
            //附加信息
            public Dictionary<string, string> AdditionalAttr { get; set; }
        }
        static string TrimAfterNull(string src)
        {
            if (string.IsNullOrEmpty(src)) return src;
            int idx = src.IndexOf('\0');
            return idx < 0 ? src : src.Substring(0, idx);
        }
    }
}
