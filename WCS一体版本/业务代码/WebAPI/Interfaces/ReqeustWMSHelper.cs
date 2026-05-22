using ZHQXC.WebAPI.Entity;
using Newtonsoft.Json;
using NHibernate.Linq;
using Sineva.WMS.Dto.WCSDto.ReplyDto;
using Sineva.WMS.Dto.WCSDto.RequestDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Wcs;
using Wcs.DefaultImplementCollection.Conveyor;
using Wcs.Framework;
using NLog;
using DevExpress.Utils.About;
using static Sineva.WMS.Dto.WCSDto.RequestDto.MeasureInfoReportRequest.DataInfo;

namespace ZHQXC.WebAPI
{
    /// <summary>
    /// 请求WMS帮助类
    /// </summary>
    public static class RequestWMSHelper
    {
        public static string IpPort
        {
            get
            {
                return Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("wmsWebApiAddress", "192.168.141.130:44360/api/wms/wcs");
            }
        }
        static Logger _logger = LogManager.GetCurrentClassLogger();
        public static string PortModeInfo { get; set; }
        static object objLog = new object();
        static ThreadRunningLog WMSWebAPI
        {
            get
            {
                ThreadRunningLog log = new ThreadRunningLog();
                log.Init("WMSWebAPI");
                return log;
            }
        }
        static ThreadRunningLog EquipmentStatusReportLog
        {
            get
            {
                ThreadRunningLog log = new ThreadRunningLog();
                log.Init("设备状态上报日志记录EquipmentStatusReportLog");
                return log;
            }
        }


        //-------------------------------------------------------下料口接口---------------------------------------------------------------------------------------------------------------》》》
        //放入层上报
        //结批上报
        //请求空容器
        //下发不合格信息
        //抓取完成上报

        //放入层上报
        public static bool report_layer_data(ReportLayerDataRequest reportLayerDataRequest, out string msg)
        {
            reportLayerDataRequest.IctId = Guid.NewGuid().ToString("N");
            reportLayerDataRequest.IctDatetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            string url = $"http://{IpPort}/report-layer-data";
            var xmlstr = JsonConvert.SerializeObject(reportLayerDataRequest);
            var replyStr = WebApiRequestHelper.HttpApiWithJsonStr(url, xmlstr);
            var reply = JsonConvert.DeserializeObject<ReplyBase>(replyStr);
            msg = reply.IctMsg;
            return reply.IctResult;
        }

        //结批上报
        public static bool report_batch_closing(ReportBatchClosingRequest reportBatchClosingRequest, out string msg)
        {
            reportBatchClosingRequest.IctId = Guid.NewGuid().ToString("N");

            reportBatchClosingRequest.IctDatetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            string url = $"http://{IpPort}/report-batch-closing";
            var xmlstr = JsonConvert.SerializeObject(reportBatchClosingRequest);
            var replyStr = WebApiRequestHelper.HttpApiWithJsonStr(url, xmlstr);
            var reply = JsonConvert.DeserializeObject<ReplyBase>(replyStr);
            msg = reply.IctMsg;
            return reply.IctResult;
        }
        //专机请求空容器
        public static bool request_empty_container(RequestEmptyContainerRequest requestEmptyContainerRequest, out string msg)
        {

            requestEmptyContainerRequest.IctDatetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            requestEmptyContainerRequest.IctId = Guid.NewGuid().ToString("N");
            string url = $"http://{IpPort}/request-empty-container";
            var xmlstr = JsonConvert.SerializeObject(requestEmptyContainerRequest);
            var replyStr = WebApiRequestHelper.HttpApiWithJsonStr(url, xmlstr);
            var reply = JsonConvert.DeserializeObject<ReplyBase>(replyStr);
            msg = reply.IctMsg;
            return reply.IctResult;
        }

        //抓取完成上报
        public static bool capture_complete(CaptureCompleteRequest captureCompleteRequest, out string msg)
        {
            captureCompleteRequest.IctId = Guid.NewGuid().ToString("N");

            captureCompleteRequest.IctDatetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            string url = $"http://{IpPort}/capture-complete";
            var xmlstr = JsonConvert.SerializeObject(captureCompleteRequest);
            var replyStr = WebApiRequestHelper.HttpApiWithJsonStr(url, xmlstr);
            var reply = JsonConvert.DeserializeObject<ReplyBase>(replyStr);
            msg = reply.IctMsg;
            return reply.IctResult;
        }


        //批次开始上报
        public static bool batch_start(BatchStartRequest batchStartRequest, out string msg)
        {
            batchStartRequest.IctId = Guid.NewGuid().ToString("N");
            batchStartRequest.IctDatetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            string url = $"http://{IpPort}/notify-batch-start";
            var xmlstr = JsonConvert.SerializeObject(batchStartRequest);
            var replyStr = WebApiRequestHelper.HttpApiWithJsonStr(url, xmlstr);
            var reply = JsonConvert.DeserializeObject<ReplyBase>(replyStr);
            msg = reply.IctMsg;
            return reply.IctResult;
        }



        //-------------------------------------------------------立库接口---------------------------------------------------------------------------------------------------------------》》》
        /// <summary>
        /// 空载具出库请求
        /// </summary>
        /// <param name="location"></param>
        /// <param name="msg"></param>
        /// <param name="requestId"></param>
        /// <param name="requestType"></param>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static bool BoxSupplyRequest(this string location, out string msg, string requestId, string requestType, out string Data)
        {

            PalletSupplyRequest palletSupplyRequest = new PalletSupplyRequest();
            var dataInfo = new PalletSupplyRequest.DataInfo();
            dataInfo.RequestId = location;//RequestId 编码
            dataInfo.PortCode = location;//RequestId 编码
            palletSupplyRequest.IctId = Guid.NewGuid().ToString("N");
            palletSupplyRequest.IctDatetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            palletSupplyRequest.Data = dataInfo;
            string url = $"http://{IpPort}/BoxSupply";
            var xmlstr = JsonConvert.SerializeObject(palletSupplyRequest);
            var replyStr = WebApiRequestHelper.HttpApiWithJsonStr(url, xmlstr);
            var reply = JsonConvert.DeserializeObject<ReplyBase>(replyStr);
            msg = reply.IctMsg == null ? "" : reply.IctMsg;
            Data = reply.AdditionalAttrStr;//所有的数据都在附加属性中
            return reply.IctResult;
        }

        /// <summary>
        /// 任务状态上报
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="taskState"></param>
        /// <param name="msg"></param>
        /// <param name="Data"></param>
        /// <param name="additionalInfo"></param>
        /// <returns></returns>
        public static bool EquipmentTaskStatusChangeReport(string taskId, WMSTaskStatus taskState, out string msg, Dictionary<string, string> additionalInfo = null)
        {
            EquipTaskStatusChangeReportRequest equipTaskStatusChangeReportRequest = new EquipTaskStatusChangeReportRequest();
            equipTaskStatusChangeReportRequest.IctId = Guid.NewGuid().ToString("N");
            equipTaskStatusChangeReportRequest.IctDatetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            equipTaskStatusChangeReportRequest.Data = new EquipTaskStatusChangeReportRequest.RequestData();
            equipTaskStatusChangeReportRequest.Data.TaskId = taskId;
            equipTaskStatusChangeReportRequest.Data.ReportTaskStatus = taskState.ToString();
            if (additionalInfo != null && additionalInfo.Count() > 0)
                equipTaskStatusChangeReportRequest.Data.AdditionalAttr = additionalInfo;

            string url = $"http://{IpPort}/EquipmentTaskStatusChangeReport";
            var xmlstr = JsonConvert.SerializeObject(equipTaskStatusChangeReportRequest);
            var replyStr = WebApiRequestHelper.HttpApiWithJsonStr(url, xmlstr);
            Wcs.Framework.MessageBoard.AbstractMessageBoard.Instance.Add(
                    new Wcs.Framework.MessageBoard.Messages.TipMessage(Wcs.Framework.MessageBoard.MessageLevel.Info,
                    $"向WMS请求",
                    $"返回Json：{replyStr}",
                    null));
            var reply = JsonConvert.DeserializeObject<ReplyBase>(replyStr);
            msg = reply.IctMsg == null ? "" : reply.IctMsg;
            return reply.IctResult;
        }

        /// <summary>
        /// 尺检后，请求WMS入库
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static bool ScanCodeRequest(this RequestBlock request, string portcode, string barcode, string TrayBarcode, string isContainer, bool result, out string msg)
        {
            MeasureInfoReportRequest measureInfoReportRequest = new MeasureInfoReportRequest();
            measureInfoReportRequest.IctId = Guid.NewGuid().ToString("N");
            measureInfoReportRequest.IctDatetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            measureInfoReportRequest.Data = new MeasureInfoReportRequest.DataInfo();
            measureInfoReportRequest.Data.PortCode = portcode;
            measureInfoReportRequest.Data.RequestId = request.RequestID.ToString();
            measureInfoReportRequest.Data.Info = new MeasureInfoReportRequest.DataInfo.CargoData();
            measureInfoReportRequest.Data.Info.BoxCode = barcode;
            measureInfoReportRequest.Data.Info.TrayBarcode = TrayBarcode;
            measureInfoReportRequest.Data.Info.isContainer = isContainer;
            measureInfoReportRequest.Data.Info.result = result;
            string url = $"http://{IpPort}/ScanCodeReporting";
            var xmlstr = JsonConvert.SerializeObject(measureInfoReportRequest);
            var replyStr = WebApiRequestHelper.HttpApiWithJsonStr(url, xmlstr);
            var reply = JsonConvert.DeserializeObject<ReplyBase>(replyStr);
            msg = reply.IctMsg == null ? "" : reply.IctMsg;
            return reply.IctResult;
        }

    }

    public enum AlarmStaus
    {
        happen = 0,
        resolve = 1
    }

    public enum WMSTaskStatus
    {
        /// <summary>
        /// 开始执行
        /// </summary>
        start,
        /// <summary>
        /// 任务取消
        /// </summary>
        cancel,
        /// <summary>
        /// 执行完成
        /// </summary>
        finish,
        /// <summary>
        /// 盘点到站
        /// </summary>
        tacking_act,
        /// <summary>
        /// 托盘解绑
        /// </summary>
        disbond
    }
}
