using Newtonsoft.Json;
using System.Collections.Generic;
using System.Dynamic;

namespace Sineva.WMS.Dto.WCSDto.ReplyDto
{
    public class ReplyBase
    {
        public string IctId { get; set; } //交互ID
        public string IctDatetime { get; set; } //回复时间（YYYY/MM/DD HH:MI:SS）
        public bool IctResult { get; set; } //交互结果
        public string IctCode { get; set; } = "200"; //结果编码
        public string IctMsg { get; set; } //交互结果描述 可空
        public string Data { get; set; }
        public Dictionary<string, string> AdditionalAttr { get; set; }
        public string AdditionalAttrStr
        {
            get
            {
                dynamic obj = new ExpandoObject();
                //obj["WMSTSWCS"] = "00:00:00";
                if (AdditionalAttr != null)
                {
                    foreach (var key in AdditionalAttr.Keys)
                    {
                        try
                        {
                            obj[key] = AdditionalAttr[key];
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
                return JsonConvert.SerializeObject(obj);
            }
        }
    }
}