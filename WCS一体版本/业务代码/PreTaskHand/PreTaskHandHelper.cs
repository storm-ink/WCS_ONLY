using ZHQXC.WebAPI;
using Newtonsoft.Json;
using NHibernate.Hql.Ast;
using NHibernate.Linq;
using NHibernate.Mapping;
using NLog;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using Sineva.WMS.Dto.WCSDto.ReplyDto;
using Sineva.WMS.Dto.WCSDto.RequestDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Wcs;
using Wcs.DefaultImplementCollection.Business;
using Wcs.Framework;
using Wcs.Framework.Cfg;
using Wcs.Framework.Events;
using Wcs.FrameworkExtend;

namespace ZHQXC.PreTaskHand
{
    /// <summary>
    /// 预任务处理Helper
    /// </summary>
    public static class PreTaskHandHelper
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();

        public static int interval = 250;

        public static List<Wcs.FrameworkExtend.PreTask> PreTasks = new List<Wcs.FrameworkExtend.PreTask>();
        public static List<Task> Tasks = new List<Task>();
        public static Dictionary<string, PreTaskCancelResult> WaitCancelPreTasks = new Dictionary<string, PreTaskCancelResult>();
        public static List<Task> CompeletedTasks = new List<Task>();
        public static List<ReportInfo> PreTaskReportList = new List<ReportInfo>();
        public static Dictionary<string, ActionSchedulerFilterResult> lastPreTaskSchedulerFilterResult = new Dictionary<string, ActionSchedulerFilterResult>();

        #region 待取消处理
        public static bool PushWaitCancelPreTaskList(string taskcode)
        {
            lock (WaitCancelPreTasks)
            {
                if (!WaitCancelPreTasks.ContainsKey(taskcode))
                    WaitCancelPreTasks.Add(taskcode, new PreTaskCancelResult() { CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, CancleResult = CancelResults.WaitingCancel });
                return true;
            }
        }

        public static void PopWaitCancelPreTaskList(string taskcode)
        {
            lock (WaitCancelPreTasks)
            {
                WaitCancelPreTasks.Remove(taskcode);
            }
        }

        public static void UpdateCancelPreTaskList(string taskcode, CancelResults cancelResult)
        {
            lock (WaitCancelPreTasks)
            {
                if (WaitCancelPreTasks.ContainsKey(taskcode) && WaitCancelPreTasks[taskcode] != null && WaitCancelPreTasks[taskcode].CancleResult == cancelResult)
                {
                    WaitCancelPreTasks[taskcode].UpdatedAt = DateTime.Now;
                    WaitCancelPreTasks[taskcode].CancleResult = cancelResult;
                }
            }
        }
        #endregion
        #region 待预处理列表 完成同步、取消同步、下发同步
        public static bool PushPreTaskList(Wcs.FrameworkExtend.PreTask preTask)
        {
            lock (PreTasks)
            {
                if (!PreTasks.Any(x => x.TaskCode == preTask.TaskCode))
                {
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                    {
                        if (!unitOfWork.session.Query<Wcs.FrameworkExtend.PreTask>().Any(x => x.TaskCode == preTask.TaskCode))
                            unitOfWork.session.Save(preTask);
                        unitOfWork.Commit();
                    }
                    PreTasks.Add(preTask);
                }

                return true;
            }
        }

        public static void UpdatePreTaskList(Wcs.FrameworkExtend.PreTask pretask)
        {
            lock (PreTasks)
            {
                var index = PreTasks.FindIndex(x => x.TaskCode == pretask.TaskCode);
                if (index != -1)
                    PreTasks[index] = pretask;
                else
                    PreTasks.Add(pretask);

                if (pretask.Source == TaskSource.Wms)
                {
                    if (pretask.Status == TaskStatus.Executing)
                        PushWaitReportPreTaskList(new ReportInfo() { PreTask = pretask, CreatedAt = DateTime.Now, MessageType = MessageTypes.StartingMessage });
                }

                if (pretask.Status == TaskStatus.Cancelled || pretask.Status == TaskStatus.Completed)
                    PushWaitReportPreTaskList(new ReportInfo() { PreTask = pretask, CreatedAt = DateTime.Now, MessageType = MessageTypes.CompeletedMessage });
            }
        }

        public static bool PopPreTaskList(string taskCode)
        {
            lock (PreTasks)
            {
                var preTask = PreTasks.FirstOrDefault(x => x.TaskCode == taskCode);
                if (preTask != null)
                {
                    PushDeletePreTaskList(preTask);
                    PreTasks.Remove(preTask);
                }

                return true;
            }
        }

        public static PreTask GetPreTaskByTaskCode(string taskCode)
        {
            lock (PreTasks)
            {
                return PreTasks.FirstOrDefault(x => x.TaskCode == taskCode);
            }
        }
        #endregion
        #region 等待完成处理列表
        public static bool PushCompeletedTaskList(Task task)
        {
            lock (CompeletedTasks)
            {
                if (!CompeletedTasks.Any(x => x.TaskCode == task.TaskCode))
                    CompeletedTasks.Add(task);

                return true;
            }
        }

        public static bool PopCompeletedTaskList(Task task)
        {
            lock (CompeletedTasks)
            {
                var tsk = CompeletedTasks.FirstOrDefault(x => x.TaskCode == task.TaskCode);
                if (task != null)
                {
                  
                    CompeletedTasks.Remove(tsk);
                    BusinessHelper.PushDeleteTaskList(task);
                }
                return true;
            }
        }
        #endregion
        #region 待上报
        public static bool PushWaitReportPreTaskList(ReportInfo reportInfo)
        {
            lock (PreTaskReportList)
            {
                if (!PreTaskReportList.Any(x => x.PreTask.TaskCode == reportInfo.PreTask.TaskCode && x.MessageType == reportInfo.MessageType))
                {
                    if (reportInfo.MessageType == MessageTypes.CompeletedMessage)
                    {
                        var ris = PreTaskReportList.Where(x => x.PreTask.TaskCode == reportInfo.PreTask.TaskCode && x.MessageType != MessageTypes.CompeletedMessage);
                        //可能存在问题
                        //for ()
                        //{

                        //}
                        if (ris.Count() > 0)
                            ris.ForEach(x => PreTaskReportList.Remove(x));
                    }
                    PreTaskReportList.Add(reportInfo);
                }
                return true;
            }
        }

        public static void PopWaitReportPreTaskList(ReportInfo reportInfo)
        {
            lock (PreTaskReportList)
            {
                var ri = PreTaskReportList.FirstOrDefault(x => x.PreTask.TaskCode == reportInfo.PreTask.TaskCode && x.MessageType == reportInfo.MessageType);
                if (ri != null)
                    PreTaskReportList.Remove(ri);
            }
        }

        public static void UpdateReportPreTaskList(string taskcode, CancelResults cancelResult)
        {
            lock (PreTaskReportList)
            {
                if (WaitCancelPreTasks.ContainsKey(taskcode) && WaitCancelPreTasks[taskcode] != null && WaitCancelPreTasks[taskcode].CancleResult == cancelResult)
                {
                    WaitCancelPreTasks[taskcode].UpdatedAt = DateTime.Now;
                    WaitCancelPreTasks[taskcode].CancleResult = cancelResult;
                }
            }
        }
        #endregion
        #region PreTaskHand 完成处理、取消处理、下发处理
        /// <summary>
        /// 预任务处理程序
        /// 1.完成同步
        /// 2.预任务拦截（上位取消任务，已下发的任务不可取消）
        /// 3.预任务下发
        /// </summary>
        public static void PreTaskHandProc()
        {
            List<PreTask> preTasks = new List<PreTask>();
            while (true)
            {
                System.Threading.Thread.Sleep(interval);
                try
                {
                    //同步执行中的任务信息
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                    {
                        Tasks = unitOfWork.session.Query<Task>().ToList();
                        unitOfWork.Commit();
                    }

                    //完成处理
                    if (CompeletedTasks.Count() > 0)
                        CompeletedHand();

                    //取消拦截处理
                    if (WaitCancelPreTasks.Count() > 0)
                        UpSystemCancelHand();

                    //下发处理
                    SendHand();
                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, typeof(BusinessHelper));
                }
            }
        }
        /// <summary>
        /// 完成处理
        /// </summary>
        private static void CompeletedHand()
        {
            //同步数据
            Task[] taskCloned;
            lock (CompeletedTasks)
            {
                taskCloned = CompeletedTasks.ToArray();
            }

            foreach (var item in taskCloned)
            {
                PreTask preTask;
                lock (PreTasks)
                {
                    preTask = PreTasks.FirstOrDefault(x => x.TaskCode == item.TaskCode);
                }
                if (preTask == null)
                    PopCompeletedTaskList(item);
                if (preTask.Status == item.Status)
                    PopCompeletedTaskList(item);

                var taskcode = item.TaskCode;
                string containercodes = JsonConvert.SerializeObject(new List<string>());
                string additionalInfo = JsonConvert.SerializeObject(new List<string>());
                if (item.ContainerCodes != null)
                    containercodes = JsonConvert.SerializeObject(item.ContainerCodes.ToList());
                if (item.AdditionalInfo != null)
                    additionalInfo = JsonConvert.SerializeObject(item.AdditionalInfo.ToDictionary(x => x.Key, y => y.Value));


                PreTask pretask;
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    pretask = unitOfWork.session.Query<PreTask>().FirstOrDefault(x => x.TaskCode == taskcode);
                    if (pretask.Status != item.Status)
                    {
                        pretask.Status = item.Status;
                        pretask.FinishedAt = item.FinishedAt;
                        pretask.Description = item.Description;
                        pretask.ContainerCodes = containercodes;
                        pretask.AdditionalInfo = additionalInfo;
                        unitOfWork.session.SaveOrUpdate(pretask);
                    }
                    unitOfWork.Commit();
                }
                UpdatePreTaskList(pretask);
                PopCompeletedTaskList(item);
            }
        }

        /// <summary>
        /// 下发处理
        /// </summary>
        private static void SendHand()
        {
            PreTask[] preTasksCloned;
            lock (PreTasks)
            {
                lock (Tasks)
                {
                    var taskCodes = Tasks.Select(x => x.TaskCode).ToArray();
                   // preTasksCloned = PreTasks.Where(x => !taskCodes.Contains(x.TaskCode) || x.Status == TaskStatus.New).OrderBy(x => x.Priority).ToArray();
                    preTasksCloned = PreTasks.Where(x => !taskCodes.Contains(x.TaskCode) && x.Status == TaskStatus.New).OrderBy(x => x.Priority).ToArray();
                }
            }

            foreach (var item in preTasksCloned)//PreTaskSchedulerFilter
            {
                ActionSchedulerFilterResult result = new ActionSchedulerFilterResult(false, "");
                if (Wcs.FrameworkExtend.Cfg.WcsConfiguration.Instance !=null)
                {
                    foreach (var filter in Wcs.FrameworkExtend.Cfg.WcsConfiguration.Instance.PreTaskSchedulerHandlerElement.PreTaskSchedulerFilterHandlers)
                    {
                        try
                        {
                            result = filter.Filter(item);
                            if (result.Defeated)
                            {
                                if (PreTaskSchedulerFilterHelper.lastPreTaskSchedulerFilterResult.ContainsKey(item.TaskCode))
                                {
                                    PreTaskSchedulerFilterHelper.lastPreTaskSchedulerFilterResult[item.TaskCode] = result;
                                }
                                else
                                {
                                    PreTaskSchedulerFilterHelper.lastPreTaskSchedulerFilterResult.Add(item.TaskCode, result);
                                }

                                Console.WriteLine($"预任务 {item}（{item.StartLocation.UserCode}->{item.EndLocation.UserCode}） 被预任务任务下发过滤器 {filter.GetType()} 因 {result.Reason} 过滤，暂时无法下发");
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            result = new ActionSchedulerFilterResult(true, $"预任务 {item}（{item.StartLocation.UserCode}->{item.EndLocation.UserCode}）在任务下发过滤器 {filter.GetType()} 处理时发生异常，异常消息{ex}");
                        }
                    }

                }

                if (result.Defeated)
                {
                    if (PreTaskSchedulerFilterHelper.lastPreTaskSchedulerFilterResult.ContainsKey(item.TaskCode))
                    {
                        PreTaskSchedulerFilterHelper.lastPreTaskSchedulerFilterResult[item.TaskCode] = result;
                    }
                    else
                    {
                        PreTaskSchedulerFilterHelper.lastPreTaskSchedulerFilterResult.Add(item.TaskCode, result);
                    }
                    continue;
                }
                else
                {
                    if (PreTaskSchedulerFilterHelper.lastPreTaskSchedulerFilterResult.ContainsKey(item.TaskCode))
                    {
                        PreTaskSchedulerFilterHelper.lastPreTaskSchedulerFilterResult.Remove(item.TaskCode);
                    }
                   
                }
                //if (result.Defeated)
                //{
                //    lock (lastPreTaskSchedulerFilterResult)
                //    {
                //        if (lastPreTaskSchedulerFilterResult.ContainsKey(item.TaskCode))
                //            lastPreTaskSchedulerFilterResult[item.TaskCode] = result;
                //        else
                //            lastPreTaskSchedulerFilterResult.Add(item.TaskCode, result);
                //    }
                //    continue;
                //}
                //else
                //{
                //    lock (lastPreTaskSchedulerFilterResult)
                //    {
                //        lastPreTaskSchedulerFilterResult.Remove(item.TaskCode);
                //    }
                //}

                if (item.Status == TaskStatus.New)
                {
                    PreTaskConvertToTask(item, out Task task);
                    if (task != null && !Tasks.Any(x => x.TaskCode == task.TaskCode))
                    {
                        PreTask preTask;
                        TaskAddedEvent taskAddedEvent = null;
                        using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                        {
                            preTask = unitOfWork.session.Get<PreTask>(item.Id);
                            if (preTask != null)
                            {
                                if (preTask.Status == TaskStatus.New)
                                {
                                    preTask.Status = TaskStatus.Executing;
                                    preTask.StartedAt = DateTime.Now;
                                    unitOfWork.session.SaveOrUpdate(preTask);
                                    if (!unitOfWork.session.Query<Task>().Any(x => x.TaskCode == task.TaskCode))
                                    {
                                        unitOfWork.session.Save(task);
                                        taskAddedEvent = new TaskAddedEvent(task);
                                    }
                                }
                            }
                            unitOfWork.Commit();
                        }
                        UpdatePreTaskList(preTask);
                        if (taskAddedEvent != null)
                        {
                            Wcs.Framework.EventBus.EventBus.Instance.Publish<TaskAddedEvent>(new TaskAddedEvent(task));
                            lock (Tasks)
                            {
                                Tasks.Add(task);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 取消处理
        /// </summary>
        private static void UpSystemCancelHand()
        {
            List<string> waitCancelList = new List<string>();
            lock (WaitCancelPreTasks)
            {
                if (WaitCancelPreTasks.Any(x => x.Value == null || (x.Value != null && x.Value.CancleResult != CancelResults.WaitingCancel)))
                    WaitCancelPreTasks = WaitCancelPreTasks
                                        .Where(x => x.Value != null)
                                        .Where(x => x.Value != null && DateTime.Now.Subtract(x.Value.UpdatedAt).TotalMilliseconds > 60000)
                                        .ToDictionary(x => x.Key, x => x.Value);

                waitCancelList = WaitCancelPreTasks.Where(x => x.Value != null && x.Value.CancleResult == CancelResults.WaitingCancel).Select(x => x.Key).ToList();
            }
            foreach (var item in waitCancelList)
            {
                var pretask = PreTasks.FirstOrDefault(x => x.TaskCode == item);
                if (pretask == null)
                    UpdateCancelPreTaskList(item, CancelResults.NoTask);
                else if (pretask.Status == TaskStatus.Cancelled)
                    UpdateCancelPreTaskList(item, CancelResults.Canceled);
                else if (pretask.Status != TaskStatus.New)
                    UpdateCancelPreTaskList(item, CancelResults.Unable);
                else
                {
                    CancelResults result = CancelResults.WaitingCancel;
                    PreTask preTsk;
                    bool update = false;
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                    {
                        preTsk = unitOfWork.session.Get<PreTask>(pretask.Id);
                        if (preTsk == null)
                            result = CancelResults.NoTask;
                        else if (preTsk.Status == TaskStatus.New)
                        {
                            preTsk.Status = TaskStatus.Cancelled;
                            preTsk.StartedAt = DateTime.Now;
                            preTsk.FinishedAt = DateTime.Now;
                            result = CancelResults.Canceled;
                            update = true;
                        }
                        else if (preTsk.Status == TaskStatus.Cancelled)
                            result = CancelResults.Canceled;
                        else
                            result = CancelResults.Unable;
                        unitOfWork.Commit();
                    }

                    if (result != CancelResults.WaitingCancel)
                    {
                        if (preTsk == null)
                            PopPreTaskList(pretask.TaskCode);
                        else
                        {
                            if (update)
                                UpdatePreTaskList(preTsk);
                        }

                        UpdateCancelPreTaskList(item, result);
                    }
                }
            }
        }

        public static void PreTaskConvertToTask(PreTask preTask, out Task task)
        {
            var taskCode = preTask.TaskCode;
            var start = preTask.StartLocation;
            var end = preTask.EndLocation;
            var taskType = preTask.TaskType;
            var priority = preTask.Priority;
            task = new Task(taskCode, start, end);
            task.TaskType = taskType;
            task.Priority = priority;
            var containerCodes = JsonConvert.DeserializeObject<List<string>>(preTask.ContainerCodes);
            if (containerCodes != null)
            {
                foreach (var containerCode in containerCodes)
                {
                    task.ContainerCodes.Add(containerCode);
                }
            }
            task.Source = preTask.Source;
            task.AdditionalInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(preTask.AdditionalInfo);
        }
        #endregion
        #region 上报WMS
        public static void ReprotWMS()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(1000);

                    ReportInfo[] reportInfosCloned;
                    lock (PreTaskReportList)
                    {
                        reportInfosCloned = PreTaskReportList.Where(x => x.ReplyBase == null || (x.ReplyBase != null && !x.ReplyBase.IctResult)).ToArray();
                    }

                    foreach (var item in reportInfosCloned)
                    {
                        if (item.PreTask.Source != TaskSource.Wms)
                        {
                            PopPreTaskList(item.PreTask.TaskCode);
                            PushDeletePreTaskList(item.PreTask);
                            continue;
                        }

                        if (item.LastReportedAt != null && DateTime.Now.Subtract((DateTime)item.LastReportedAt).TotalMilliseconds < 10000)
                            continue;

                        //if (string.IsNullOrWhiteSpace(item.ReportJson))
                        //    SetReportJson(item);
                        string msg;
                        var additionaryInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(item.PreTask.AdditionalInfo);
                        WMSTaskStatus wmsTaskState = WMSTaskStatus.start;
                        switch (item.MessageType)
                        {
                            case MessageTypes.StartingMessage:
                                wmsTaskState = WMSTaskStatus.start;
                                break;
                            case MessageTypes.PreUnbildMessage:
                                wmsTaskState = WMSTaskStatus.disbond;
                                break;
                            case MessageTypes.CompeletedMessage:
                                if (item.PreTask.Status == TaskStatus.Cancelled)
                                    wmsTaskState = WMSTaskStatus.cancel;
                                else if (item.PreTask.Status == TaskStatus.Completed)
                                    wmsTaskState = WMSTaskStatus.finish;
                                else
                                    continue;
                                break;
                            default:
                                continue;
                        }

                        try
                        {
                            //item.LastReportedAt = DateTime.Now;
                            //item.ReplyBase = RequestWMSHelper.EquipmentTaskStatusChangeReport(item.PreTask.TaskCode, wmsTaskState, out msg, additionaryInfo);
                            //if (item.ReplyBase.IctResult)
                            //{
                            //    PopWaitReportPreTaskList(item);
                            //    if (item.MessageType == MessageTypes.CompeletedMessage)
                            //    {
                            //        PopPreTaskList(item.PreTask.TaskCode);
                            //        PushDeletePreTaskList(item.PreTask);
                            //    }
                            //}
                        }
                        catch (Exception ex)
                        {
                            _logger.Error1(ex, typeof(PreTaskHandHelper));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, typeof(PreTaskHandHelper));
                }
            }
        }

        private static void SetReportJson(ReportInfo item)
        {
            switch (item.MessageType)
            {
                case MessageTypes.StartingMessage:
                    break;
                case MessageTypes.PreUnbildMessage:
                    break;
                case MessageTypes.CompeletedMessage:

                    break;
                default:
                    break;
            }
        }
        #endregion
        #region 预任务归档处理
        /// <summary>
        /// 预任务待归档列表
        /// </summary>
        public static List<PreTask> _waitDeletePreTasks = new List<PreTask>();

        /// <summary>
        /// 是否已经存在于归档列表中
        /// </summary>
        /// <param name="PreTask"></param>
        /// <returns></returns>
        public static Boolean ExistArchiveProcPreTaskList(PreTask PreTask)
        {
            return _waitDeletePreTasks.Any(x => x.TaskCode == PreTask.TaskCode);
        }

        /// <summary>
        /// 将预任务添加到归档列表
        /// </summary>
        /// <param name="PreTask"></param>
        public static void PushDeletePreTaskList(PreTask PreTask)
        {
            lock (_waitDeletePreTasks)
            {
                if (_waitDeletePreTasks.Any(x => x.TaskCode == PreTask.TaskCode))
                {
                    _logger.Warn1(string.Format("{0}已存在于预任务归档队列当中", PreTask), typeof(BusinessHelper), PreTask);
                    return;
                }

                _waitDeletePreTasks.Add(PreTask);

                _logger.Debug1(string.Format("{0}被加入预任务归档队列", PreTask), typeof(BusinessHelper), PreTask);
            }
        }

        /// <summary>
        /// 将预任务从归档列表中移除
        /// </summary>
        /// <param name="PreTask"></param>
        public static void PopDeletePreTaskList(PreTask PreTask)
        {
            lock (_waitDeletePreTasks)
            {
                var item = _waitDeletePreTasks.FirstOrDefault(x => x.TaskCode == PreTask.TaskCode);
                if (item == null)
                {
                    _logger.Warn1(string.Format("{0}已被从归档队列移除", item), typeof(BusinessHelper), item);
                    return;
                }

                _waitDeletePreTasks.Remove(item);

                _logger.Debug1(string.Format("{0}被移出归档队列", item), typeof(BusinessHelper), item);
            }
        }

        /// <summary>
        /// 预任务归档线程
        /// </summary>
        public static void ArchiveProc()
        {
            //延时归档列表
            List<PreTask> delayedArchivePreTaskList = new List<PreTask>();
            while (true)
            {
                System.Threading.Thread.Sleep(500);
                try
                {
                    PreTask[] PreTasksCloned;
                    lock (_waitDeletePreTasks)
                    {
                        PreTasksCloned = _waitDeletePreTasks.ToArray();
                    }

                    foreach (var item in PreTasksCloned)
                    {
                        try
                        {
                            _logger.Trace1(string.Format("准备将{0}预任务{1}归档...", item.TaskType, item), typeof(BusinessHelper), item);

                            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                            {
                                var deleteObject = unitOfWork
                                    .session
                                    .Query<PreTask>()
                                    .Where(x =>
                                        x.Id == item.Id
                                        && (x.Status == TaskStatus.Cancelled || x.Status == TaskStatus.Completed)
                                    )
                                    .FirstOrDefault();

                                if (deleteObject != null)
                                    unitOfWork.session.Delete(deleteObject);

                                unitOfWork.Commit();
                            }
                            _logger.Debug1(string.Format("{0} 归档成功.", item), typeof(BusinessHelper), item);

                            PopDeletePreTaskList(item);

                            //Wcs.Framework.EventBus.EventBus.Instance.Publish(new Wcs.Framework.Events.PreTaskArchivedEvent(item.Id, item.PreTaskCode));
                        }
                        catch (Exception ex)
                        {
                            _logger.Error1(ex, typeof(BusinessHelper), item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, typeof(BusinessHelper));
                }
            }
        }
        #endregion
    }

    public class PreTaskCancelResult
    {
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public CancelResults CancleResult { get; set; }
    }

    /// <summary>
    /// 预任务取消结果
    /// </summary>
    public enum CancelResults
    {
        /// <summary>
        /// 等待取消
        /// </summary>
        WaitingCancel = 0,
        /// <summary>
        /// 已取消
        /// </summary>
        Canceled = 1,
        /// <summary>
        /// 不能取消
        /// </summary>
        Unable = 2,
        /// <summary>
        /// 无此任务
        /// </summary>
        NoTask = 3
    }

    /// <summary>
    /// 上报消息
    /// </summary>
    public class ReportInfo
    {
        public Wcs.FrameworkExtend.PreTask PreTask { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastReportedAt { get; set; }

        public MessageTypes MessageType { get; set; }

        public string ReportJson { get; set; }

        public ReplyBase ReplyBase { get; set; }
    }

    public enum MessageTypes
    {
        /// <summary>
        /// 开始消息
        /// </summary>
        StartingMessage = 1,
        /// <summary>
        /// 提前解绑
        /// </summary>
        PreUnbildMessage = 2,
        /// <summary>
        /// 完成消息
        /// </summary>
        CompeletedMessage = 3
    }
}
