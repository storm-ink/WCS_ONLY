using ZHQXC.WebAPI.Entity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHibernate.Linq;
using NLog;
using Sineva.WMS.Dto.WCSDto.ReplyDto;
using Sineva.WMS.Dto.WCSDto.RequestDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;
using System.Web.Http.Results;
using Wcs;
using Wcs.DefaultImplementCollection.Business;
using Wcs.DefaultImplementCollection.Conveyor;
using Wcs.Framework;
using Wcs.FrameworkExtend;

namespace ZHQXC.WebAPI.Controller
{
    [RoutePrefix("API/WCSForWMSWebApi")]
    public class WCSForWMSWebApiController : ApiController
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();
        static ThreadRunningLog WMSToWCSAPILogs
        {
            get
            {
                ThreadRunningLog log = new ThreadRunningLog();
                log.Init("WMSToWCSAPILogs");
                return log;
            }
        }

        //-------------------------------------------------------立库---------------------------------------------------------------------------------------------------------------》》》

        /// <summary>
        /// 3.WMS任务下发
        /// </summary>
        /// <returns></returns>
        [Route("EquipmentTaskRequest")]
        [HttpPost]
        public ReplyBase EquipmentTaskRequest([FromBody] JObject msg)
        {
            ReplyBase replyBase = new ReplyBase();
            AssignEquipTaskRequest assignEquipTaskRequest;
            PreTask preTask;
            try
            {
                WMSToWCSAPILogs.Log($"WMS-->WCS 收到WMS调用 EquipmentTaskRequest 接口，接收数据：{msg.ToString()}");
                assignEquipTaskRequest = JsonConvert.DeserializeObject<AssignEquipTaskRequest>(msg.ToString());
                replyBase.IctId = assignEquipTaskRequest.IctId;
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                replyBase.IctResult = false;
                replyBase.IctCode = "400";
                replyBase.IctMsg = ex.Message;
                replyBase.IctDatetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                replyBase.Data = ex.Message;
                WMSToWCSAPILogs.Log($"WMS-->WCS WMS调用WCS EquipmentTaskRequest 接口WCS回复：{JsonConvert.SerializeObject(replyBase)}，WMS传入数据解析发生异常,异常消息：\r\n {ex}");//zhj-WMS日志记录
                return replyBase;
            }

            try
            {
                ConvertToPreTask(assignEquipTaskRequest, out preTask);

                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    if (unitOfWork.session.Query<PreTask>().Any(x => x.TaskCode == preTask.TaskCode))
                    {
                        unitOfWork.Commit();
                        _logger.Info1($"WMS任务下发成功,任务已存在，接收数据：{msg.ToString()}", this);
                        replyBase.IctDatetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                        replyBase.IctMsg = "成功";
                        replyBase.IctResult = true;
                        return replyBase;
                    }
                    //WCS主动请求的任务都带REQEUSTID，如果这个重复则直接报错
                    var additionalInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(preTask.AdditionalInfo);
                    if (additionalInfo.ContainsKey("REQUEST") && unitOfWork.session.Query<Task>().Any(x => x.AdditionalInfo.ContainsKey("REQUEST") && x.AdditionalInfo["REQUEST"] == preTask.TaskCode))
                    {
                        unitOfWork.Commit();
                        throw new ArgumentException($"请求ID {additionalInfo["REQUEST"]} 重复");
                    }
                    unitOfWork.session.Save(preTask);
                    unitOfWork.Commit();
                }
                Wcs.Framework.EventBus.EventBus.Instance.Publish<Wcs.FrameworkExtend.Events.PreTaskAddedEvent>(new Wcs.FrameworkExtend.Events.PreTaskAddedEvent(preTask));
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                replyBase.IctResult = false;
                replyBase.IctCode = "500";
                replyBase.IctMsg = "操作执行异常（执行请求失败）";
                replyBase.IctDatetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                replyBase.Data = ex.Message;
                WMSToWCSAPILogs.Log($"WMS-->WCS WMS调用WCS EquipmentTaskRequest 接口WCS回复：{JsonConvert.SerializeObject(replyBase)}，WCS处理时发生异常,异常消息：\r\n {ex}");
                return replyBase;
            }
            _logger.Info1($"WMS任务下发成功，接收数据：{msg.ToString()}", this);
            replyBase.IctDatetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            replyBase.IctMsg = "成功";
            replyBase.IctResult = true;
            WMSToWCSAPILogs.Log($"WMS-->WCS WMS调用WCS EquipmentTaskRequest 接口WCS回复：{JsonConvert.SerializeObject(replyBase)}");
            return replyBase;
        }
       
        //-------------------------------------------------------下料口---------------------------------------------------------------------------------------------------------------》》》
        [Route("issue_non_compliant")]
        [HttpPost]
        public ReplyBase issue_non_compliant([FromBody] JObject msg)
        {
            ReplyBase replyBase = new ReplyBase();
            IssueNonCompliantRequest issueNonCompliantRequest;
            PreTask preTask;
            try
            {
                WMSToWCSAPILogs.Log($"WMS-->WCS 收到WMS调用 EquipmentTaskRequest 接口，接收数据：{msg.ToString()}");
                issueNonCompliantRequest = JsonConvert.DeserializeObject<IssueNonCompliantRequest>(msg.ToString());
                replyBase.IctId = issueNonCompliantRequest.IctId;
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                replyBase.IctResult = false;
                replyBase.IctCode = "400";
                replyBase.IctMsg = ex.Message;
                replyBase.IctDatetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                replyBase.Data = ex.Message;
                WMSToWCSAPILogs.Log($"WMS-->WCS WMS调用WCS EquipmentTaskRequest 接口WCS回复：{JsonConvert.SerializeObject(replyBase)}，WMS传入数据解析发生异常,异常消息：\r\n {ex}");//zhj-WMS日志记录
                return replyBase;
            }
            if (issueNonCompliantRequest.Data.LocationCode == "00-001-1023"
                || issueNonCompliantRequest.Data.LocationCode == "00-001-1024"
                ||issueNonCompliantRequest.Data.LocationCode == "00-001-1025"
                || issueNonCompliantRequest.Data.LocationCode == "00-001-1026"
                || issueNonCompliantRequest.Data.LocationCode == "00-001-1027"
                || issueNonCompliantRequest.Data.LocationCode == "00-001-1028"
                )
            {

                //待发送给专机PLC的命令
                ExecuteDePalletizingCommand executeDePalletizingCommand = new ExecuteDePalletizingCommand()
                {
                    PosNo = Convert.ToUInt16(issueNonCompliantRequest.Data.LocationCode.Substring(issueNonCompliantRequest.Data.LocationCode.Length - 4)),
                    HandShake = 1,
                    Room = Convert.ToUInt16(issueNonCompliantRequest.Data.Chamber),
                    StartLayer = Convert.ToUInt16(issueNonCompliantRequest.Data.StartLayer),
                    EndLayer = Convert.ToUInt16(issueNonCompliantRequest.Data.EndLayer),
                    IsNG = Convert.ToUInt16((issueNonCompliantRequest.Data.IsNG) == 0 ? 1 : 2),
                    IsOver = Convert.ToUInt16(issueNonCompliantRequest.Data.IsFinish == "true" ? 2 : 1),
                    TaskNo = issueNonCompliantRequest.Data.TaskId,
                    Box = issueNonCompliantRequest.Data.BoxCode
                };

                
                var conveyorName = "";
                //判断给三个专机中的哪一个
                if (issueNonCompliantRequest.Data.LocationCode == "00-001-1023"
                || issueNonCompliantRequest.Data.LocationCode == "00-001-1024")
                {
                    conveyorName = "专机CV(A)";
                }
                else if (issueNonCompliantRequest.Data.LocationCode == "00-001-1025"
                || issueNonCompliantRequest.Data.LocationCode == "00-001-1026")
                {
                    conveyorName = "专机CV(B)";
                }
                else if(issueNonCompliantRequest.Data.LocationCode == "00-001-1027"
                    || issueNonCompliantRequest.Data.LocationCode == "00-001-1028")
                {
                    conveyorName = "专机CV(C)";
                }
                var conveyor = DeviceConverter.ToDevice<ConveyorDevice>(conveyorName);
                try
                {
                    conveyor.Write<ExecuteDePalletizingCommand>(executeDePalletizingCommand, executeDePalletizingCommand.SendSuccess);
                }
                catch (Exception ex)
                {
                   
                }
            }
            else {
                replyBase.IctResult = false;
                replyBase.IctCode = "400";
                replyBase.IctMsg = $"WMS下发拣选的货位为空，下发信息为{JsonConvert.SerializeObject(issueNonCompliantRequest)} ";
                replyBase.IctDatetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                replyBase.Data = "";
                WMSToWCSAPILogs.Log($"WMS-->WCS WMS调用WCS EquipmentTaskRequest 接口WCS回复：{JsonConvert.SerializeObject(replyBase)}，WMS传入数据异常,货位信息为空");//zhj-WMS日志记录
                return replyBase;
            }

            _logger.Info1($"WMS任务下发成功，接收数据：{msg.ToString()}", this);
            replyBase.IctDatetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            replyBase.IctMsg = "成功";
            replyBase.IctResult = true;
            WMSToWCSAPILogs.Log($"WMS-->WCS WMS调用WCS EquipmentTaskRequest 接口WCS回复：{JsonConvert.SerializeObject(replyBase)}");
            return replyBase;
        }


        [Route("box_arrive_info")]
        [HttpPost]
        public ReplyBase box_arrive_info([FromBody] JObject msg)
        {
            ReplyBase replyBase = new ReplyBase();
            BoxArriveInfoReportReply boxArriveInfoReportReply;
            try
            {
                WMSToWCSAPILogs.Log($"WMS-->WCS 收到WMS调用 EquipmentTaskRequest 接口，接收数据：{msg.ToString()}");
                boxArriveInfoReportReply = JsonConvert.DeserializeObject<BoxArriveInfoReportReply>(msg.ToString());
                replyBase.IctId = boxArriveInfoReportReply.IctId;
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                replyBase.IctResult = false;
                replyBase.IctCode = "400";
                replyBase.IctMsg = ex.Message;
                replyBase.IctDatetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                replyBase.Data = ex.Message;
                WMSToWCSAPILogs.Log($"WMS-->WCS WMS调用WCS EquipmentTaskRequest 接口WCS回复：{JsonConvert.SerializeObject(replyBase)}，WMS传入数据解析发生异常,异常消息：\r\n {ex}");//zhj-WMS日志记录
                return replyBase;
            }
            //待发送给专机PLC的命令
            BoxInfomationCommand boxInfomationCommand = new BoxInfomationCommand()
            {
                PosNo = Convert.ToUInt16(boxArriveInfoReportReply.Data.LocationCode.Substring(boxArriveInfoReportReply.Data.LocationCode.Length - 4)),
                Room1Floot = Convert.ToUInt16(boxArriveInfoReportReply.Data.Chamber1Layer),
                Room2Floot = Convert.ToUInt16(boxArriveInfoReportReply.Data.Chamber2Layer),
                Room3Floot = Convert.ToUInt16(boxArriveInfoReportReply.Data.Chamber3Layer),
                Room4Floot = Convert.ToUInt16(boxArriveInfoReportReply.Data.Chamber4Layer),
                Box = boxArriveInfoReportReply.Data.Boxcode,
                DataID = 34
            };
            if (boxArriveInfoReportReply.Data.LocationCode == "00-001-1023"|| boxArriveInfoReportReply.Data.LocationCode == "00-001-1024")
            {

                
                var conveyor = DeviceConverter.ToDevice<ConveyorDevice>("专机CV(A)");
                try
                {
                    conveyor.Write<BoxInfomationCommand>(boxInfomationCommand, boxInfomationCommand.SendSuccess);
                }
                catch (Exception ex)
                {

                }

            }
            else if(boxArriveInfoReportReply.Data.LocationCode == "00-001-1025" || boxArriveInfoReportReply.Data.LocationCode == "00-001-1026")
            {
                var conveyor = DeviceConverter.ToDevice<ConveyorDevice>("专机CV(B)");
                try
                {
                    conveyor.Write<BoxInfomationCommand>(boxInfomationCommand, boxInfomationCommand.SendSuccess);
                }
                catch (Exception ex)
                {

                }
            }
            else if (boxArriveInfoReportReply.Data.LocationCode == "00-001-1027" || boxArriveInfoReportReply.Data.LocationCode == "00-001-1028")
            {
                var conveyor = DeviceConverter.ToDevice<ConveyorDevice>("专机CV(C)");
                try
                {
                    conveyor.Write<BoxInfomationCommand>(boxInfomationCommand, boxInfomationCommand.SendSuccess);
                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                replyBase.IctResult = false;
                replyBase.IctCode = "400";
                replyBase.IctMsg = $"WMS下发拣选的货位为空，下发信息为{JsonConvert.SerializeObject(boxArriveInfoReportReply)} ";
                replyBase.IctDatetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                replyBase.Data = "";
                WMSToWCSAPILogs.Log($"WMS-->WCS WMS调用WCS EquipmentTaskRequest 接口WCS回复：{JsonConvert.SerializeObject(replyBase)}，WMS传入数据异常,货位信息为空");//zhj-WMS日志记录
                return replyBase;
            }
            

            _logger.Info1($"WMS任务下发成功，接收数据：{msg.ToString()}", this);
            replyBase.IctDatetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            replyBase.IctMsg = "成功";
            replyBase.IctResult = true;
            WMSToWCSAPILogs.Log($"WMS-->WCS WMS调用WCS EquipmentTaskRequest 接口WCS回复：{JsonConvert.SerializeObject(replyBase)}");
            return replyBase;
        }



        private bool ConvertToPreTask(AssignEquipTaskRequest assignEquipTaskRequest, out PreTask preTask)
        {
            var taskCode = assignEquipTaskRequest.Data.TaskId;

            if (string.IsNullOrWhiteSpace(assignEquipTaskRequest.Data.Site.Source))
                throw new InvalidOperationException($"起点字段 Source 值无效，不可为空！");
            var starts = Wcs.Framework.Cfg.WcsConfiguration
                        .Instance
                        .LocationCollection
                        .Locations
                        .Where(x => x.UserCode == assignEquipTaskRequest.Data.Site.Source);

            if (starts == null)
                throw new InvalidOperationException($"起点字段 Source 值 {assignEquipTaskRequest.Data.Site.Source} 无效");
            if (starts.Count() > 1)
                throw new InvalidOperationException($"起点字段 Source 值 {assignEquipTaskRequest.Data.Site.Source} 无效，匹配到多个位置{string.Join(",", starts.Select(x => x.UserCode))}");
            var start = starts.First();

            if (string.IsNullOrWhiteSpace(assignEquipTaskRequest.Data.Site.Destination))
                throw new InvalidOperationException($"终点字段 Destination 值无效，不可为空！");
            var ends = Wcs.Framework.Cfg.WcsConfiguration
                        .Instance
                        .LocationCollection
                        .Locations
                        .Where(x => x.UserCode == assignEquipTaskRequest.Data.Site.Destination);

            if (ends == null)
                throw new InvalidOperationException($"终点字段 Destination 值 {assignEquipTaskRequest.Data.Site.Destination} 无效");
            if (ends.Count() > 1)
                throw new InvalidOperationException($"终点字段 Destination 值 {assignEquipTaskRequest.Data.Site.Destination} 无效，匹配到多个位置{string.Join(",", ends.Select(x => x.UserCode))}");
            var end = ends.First();

            var ableArrived = RouteHelper.AbleArrived(start, end);
            if (!ableArrived)
                throw new InvalidOperationException($"{start} 到 {end} 无法连通");

            var taskType = assignEquipTaskRequest.Data.TaskMode;
            var priority = assignEquipTaskRequest.Data.Priority;
            preTask = new PreTask(taskCode, LocationConverter.ToLocationInfo(start), LocationConverter.ToLocationInfo(end));
            preTask.TaskType = taskType;
            preTask.Priority = priority;
            if (!string.IsNullOrWhiteSpace(assignEquipTaskRequest.Data.PalletCode))
                preTask.ContainerCodes = JsonConvert.SerializeObject(new List<string>() { assignEquipTaskRequest.Data.PalletCode });
            preTask.Source = TaskSource.Wms;
            Dictionary<string, string> additionalAttr = new Dictionary<string, string>();
            if (assignEquipTaskRequest.Data.AdditionalAttr != null && assignEquipTaskRequest.Data.AdditionalAttr.Count() > 0)
                additionalAttr = assignEquipTaskRequest.Data.AdditionalAttr;

            if (!string.IsNullOrWhiteSpace(assignEquipTaskRequest.Data.RequestId))
                additionalAttr.Add("REQUEST", assignEquipTaskRequest.Data.RequestId);
            preTask.AdditionalInfo = JsonConvert.SerializeObject(additionalAttr);
            return true;
        }

    }

}
