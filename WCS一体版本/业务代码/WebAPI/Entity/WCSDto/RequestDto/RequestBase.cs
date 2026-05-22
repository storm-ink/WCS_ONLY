namespace Sineva.WMS.Dto.WCSDto.RequestDto
{
    public class RequestBase
    {
        public string IctId { get; set; } //交互ID
        public string IctDatetime { get; set; } //发送时间（YYYY/MM/DD HH:MI:SS）
        public string WarehouseCode { get; set; }//仓库编码 可为空
    }
}