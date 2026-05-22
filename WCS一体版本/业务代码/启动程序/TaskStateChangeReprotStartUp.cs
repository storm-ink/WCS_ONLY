using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs;
using Wcs.Framework;
using NHibernate.Linq;
using System.Threading;
using Wcs.DefaultImplementCollection.Business;
using ZHQXC.WebAPI;
using Wcs.Framework.Events;
using Wcs.DefaultImplementCollection.Conveyor;
using Sineva.WMS.Dto.WCSDto.RequestDto;
using System.Security.Cryptography;
using ZHQXC.PreTaskHand;
using Newtonsoft.Json;
using Wcs.FrameworkExtend.Events;
using Wcs.FrameworkExtend;

namespace ZHQXC
{
    /// <summary>
    /// 任务完成处理程序
    /// 任务执行完成状态同步 task --> preTask
    /// </summary>
    public class TaskStateChangeReprotStartUp : Wcs.Framework.IApplicationStartup
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();
        static Thread _thread;

        public void Initialize(Wcs.Framework.Cfg.StartupElement element)
        {
        }

        public void Run(Wcs.IWcsApplication application)
        {
            _thread = new System.Threading.Thread(SyncCompeletedToPreTaskList);
            _thread.Name = "TaskStateReportToPreTask";
            _thread.IsBackground = true;
            _thread.StartAndManaged();
            _logger.Debug1("TaskStateReportToPreTask线程已启动", this);

            Wcs.Framework.EventBus.EventBus.Instance.Subscribe<Wcs.Framework.Events.TaskFinishedEvent>(onTaskFinished);
            Wcs.Framework.EventBus.EventBus.Instance.Subscribe<Wcs.FrameworkExtend.Events.PreTaskAddedEvent>(onPreTaskAdd);

            Task[] tasks;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                tasks = unitOfWork.session.Query<Task>().Where(x => x.Status == TaskStatus.Cancelled || x.Status == TaskStatus.Completed).ToArray();
                unitOfWork.Commit();
            }
            foreach (var task in tasks)
            {
                PushWaitSyncTaskList(task);
            }
        }

        List<string> starts = new List<string>() { "1004", "1011", "1018", "2004", "2011", "2018" };
        List<string> ends = new List<string> { "1001", "1008", "1015", "2001", "2008", "2015" };
        private void onPreTaskAdd(PreTaskAddedEvent args)
        {
            try
            {
                //if (starts.Contains(args.PreTask.StartLocation.DeviceCode))
                //{
                //    if (args.PreTask.EndLocation.DeviceName.Contains("C00"))
                //    {
                //        var device = DeviceConverter.ToDevice<ConveyorDevice>(args.PreTask.StartLocation.DeviceName);
                //        if (device != null)
                //        {
                //            //ushort port = ushort.Parse(args.PreTask.StartLocation.DeviceCode);
                //            //SendRequestResultCommand cmd = new SendRequestResultCommand();
                //            //cmd.PosNo = port;
                //            //cmd.RequestResult = 1;
                //            //cmd.BarcodeEnable = 1;
                //            //cmd.WeightEnable = 1;
                //            //cmd.Data_ID = (UInt16)new Random().Next(1, UInt16.MaxValue);
                //            //device.Write<SendRequestResultCommand>(cmd, cmd.SendSuccess);
                //        }
                //    }
                //}
                //else if (ends.Contains(args.PreTask.EndLocation.DeviceCode))
                //{
                //    var device = DeviceConverter.ToDevice<ConveyorDevice>(args.PreTask.StartLocation.DeviceName);
                //    if (device != null)
                //    {
                //        //ushort port = ushort.Parse(args.PreTask.StartLocation.DeviceCode);
                //        //var additionalinfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(args.PreTask.AdditionalInfo);
                //        //SendRequestResultCommand cmd = new SendRequestResultCommand();
                //        //cmd.PosNo = port;
                //        //cmd.RequestResult = 2;
                //        //if (additionalinfo != null && additionalinfo.ContainsKey("BARCODEENABLE") && additionalinfo["BARCODEENABLE"].ToUpper() == "FALSE")
                //        //    cmd.BarcodeEnable = 2;
                //        //else
                //        //    cmd.BarcodeEnable = 1;
                //        //if (additionalinfo != null && additionalinfo.ContainsKey("WEIGHTENABLE") && additionalinfo["WEIGHTENABLE"].ToUpper() == "FALSE")
                //        //    cmd.WeightEnable = 2;
                //        //else
                //        //    cmd.WeightEnable = 1;
                //        //cmd.Data_ID = (UInt16)new Random().Next(1, UInt16.MaxValue);
                //        //device.Write<SendRequestResultCommand>(cmd, cmd.SendSuccess);
                //    }
                //}
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
            }
        }

        void onTaskFinished(Wcs.Framework.Events.TaskFinishedEvent args)
        {
            _logger.Trace1(string.Format("收到一个 {0} 状态变为 {1} 的{2}任务的事件...", args.TaskCode, args.Status, args.TaskType), this, args.TaskCode);

            Task _task;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                _task = unitOfWork.session.Get<Task>(args.Id);
                unitOfWork.Commit();
            }
            if (_task == null)
                return;
            if (BusinessHelper.ExistArchiveProcTaskList(_task))
                return;
            _logger.Trace1("准备加入完成处理列表中", this, args.TaskCode);
            PushWaitSyncTaskList(_task);
        }

        /// <summary>
        /// 任务待上报列表
        /// </summary>
        public static List<Task> _waitSyncTasks = new List<Task>();

        /// <summary>
        /// 是否已经存在于上报列表中
        /// </summary> 
        /// <param name="task"></param>
        /// <returns></returns>
        public static Boolean ExistWaitSyncProcTaskList(Task task)
        {
            return _waitSyncTasks.Any(x => x.TaskCode == task.TaskCode);
        }

        /// <summary>
        /// 将任务添加到上报列表
        /// </summary>
        /// <param name="task"></param>
        public static void PushWaitSyncTaskList(Task task)
        {
            if (task.AdditionalInfo.ContainsKey("UnPreTask"))
                return;
            lock (_waitSyncTasks)
            {
                if (_waitSyncTasks.Any(x => x.TaskCode == task.TaskCode))
                {
                    _logger.Warn1(string.Format("{0}已存在于完成处理队列当中", task), typeof(TaskStateChangeReprotStartUp), task);
                    return;
                }

                _waitSyncTasks.Add(task);

                _logger.Debug1(string.Format("{0}被加入完成处理队列", task), typeof(TaskStateChangeReprotStartUp), task);
            }
        }

        /// <summary>
        /// 将任务从上报列表中移除
        /// </summary>
        /// <param name="task"></param>
        public static void PopWaitSyncTaskList(Task task)
        {
            if (task == null)
                return;

            lock (_waitSyncTasks)
            {
                var item = _waitSyncTasks.FirstOrDefault(x => x.TaskCode == task.TaskCode);
                if (item == null)
                {
                    _logger.Warn1(string.Format("{0}已被从上报队列移除", item), typeof(TaskStateChangeReprotStartUp), item);
                    return;
                }

                _waitSyncTasks.Remove(item);

                _logger.Debug1(string.Format("{0}被移上报队列", item), typeof(TaskStateChangeReprotStartUp), item);
            }
        }

        #region
        //Dictionary<string, string> _dic = new Dictionary<string, string>()
        //{
        //    { "1001","1004"},
        //    { "1008","1011"},
        //    { "1015","1018"},
        //    { "2001","2004"},
        //    { "2008","2011"},
        //    { "2015","2018"}
        //};
        #endregion

        private void SyncCompeletedToPreTaskList(object obj)
        {
            while (true)
            {
                System.Threading.Thread.Sleep(1000);
                try
                {
                    Task[] tasksCloned;
                    lock (_waitSyncTasks)
                    {
                        tasksCloned = _waitSyncTasks.ToArray();
                    }

                    foreach (var _tsk in tasksCloned)
                    {

                        Task _task;

                        using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                        {
                            _task = unitOfWork.session.Get<Task>(_tsk.Id);
                            unitOfWork.Commit();
                        }

                        if (_task == null)
                        {
                            _logger.Trace1(string.Format("未找到任务{0}", _tsk.TaskCode), this, _tsk.TaskCode);
                            if (_tsk != null)
                                PopWaitSyncTaskList(_tsk);
                            continue;
                        }

                        if (BusinessHelper.ExistArchiveProcTaskList(_task))
                        {
                            PopWaitSyncTaskList(_tsk);
                            continue;
                        }

                        var pretask = PreTaskHandHelper.PreTasks.FirstOrDefault(x => x.TaskCode == _task.TaskCode);
                        if (pretask == null || pretask.Status == _task.Status)
                        {
                            PopWaitSyncTaskList(_tsk);
                            BusinessHelper.PushDeleteTaskList(_tsk);
                            continue;
                        }

                        if (_task.Status != TaskStatus.Completed && _task.Status != TaskStatus.Cancelled)
                        {
                            _logger.Warn1(string.Format("任务{0}当前状态应为“{1}”或“{2}”，实际为“{3}”（这可能是因为在处理完成时上下文事务中的某一个操作失败造成的事务回滚引起的“脏事件”问题，你可以忽略该警告）。"
                                , _tsk.TaskCode
                                , TaskStatus.Completed.GetDescription()
                                , TaskStatus.Cancelled.GetDescription()
                                , _task.Status.GetDescription()
                                ), this, _tsk.TaskCode);
                            PopWaitSyncTaskList(_tsk);
                            continue;
                        }

                        try
                        {
                            foreach (var item in Wcs.Framework.Cfg.WcsConfiguration.Instance.WMSTaskCompeletedExternalHandlersElement.WMSCompeletedExternalHandlers)
                            {
                                if (item.Allowed(_task))
                                    item.Hand(ref _task);
                            }

                            _logger.Trace1("准备同步到PreTasks...", this, _task);
                            //PopWaitSyncTaskList(_task);
                            PreTaskHandHelper.PushCompeletedTaskList(_task);
                            //删除计划任务的提示
                            if (PreTaskSchedulerFilterHelper.lastPreTaskSchedulerFilterResult.ContainsKey(_task.TaskCode))
                            {
                                PreTaskSchedulerFilterHelper.lastPreTaskSchedulerFilterResult.Remove(_task.TaskCode);
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.Error1(e, typeof(TaskStateChangeReprotStartUp));
                            //Thread.Sleep(1000);
                            continue;
                        }

                        _logger.Trace1("同步到PreTasks成功", this, _task);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, typeof(TaskStateChangeReprotStartUp));
                }
            }
        }
    }
}
