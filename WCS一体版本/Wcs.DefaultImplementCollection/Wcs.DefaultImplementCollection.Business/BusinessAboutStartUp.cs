using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Wcs;
using Wcs.Framework;
using NHibernate.Linq;
using Wcs.Framework.Events;

namespace Wcs.DefaultImplementCollection.Business
{
    /// <summary>
    /// 业务相关启动程序
    /// </summary>
    public class BusinessAboutStartUp : IApplicationStartup
    {
        static Logger _logger;
        List<String> _taskTypes;
        List<String> _filterTaskTypes;
        bool runningAutoClearOccupySingleTaskHand = false;

        Wcs.Framework.Cfg.StartupElement _element;
        /// <summary>
        /// 任务归档线程
        /// </summary>
        static Thread _taskArchiveThread;
        /// <summary>
        /// 自复位任务完成请求线程
        /// </summary>
        static Thread _taskRequestThread;

        public void Initialize(Framework.Cfg.StartupElement element)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _element = element;
        }

        public void Run(IWcsApplication application)
        {
            _taskTypes = _element.GetAttributeOrDefault<String>("taskTypes", "").Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            _filterTaskTypes = _element.GetAttributeOrDefault<String>("filterTaskTypes", "").Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            runningAutoClearOccupySingleTaskHand = _element.GetAttributeOrDefault<bool>("runningAutoClearOccupySingleTaskHand", false);
            Wcs.Framework.EventBus.EventBus.Instance.Subscribe<Wcs.Framework.Events.TaskFinishedEvent>(OnnoWMSTaskCompletedHandler);

            Task[] tasks;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                tasks = unitOfWork.session.Query<Task>().Where(x => _taskTypes.Contains(x.TaskType) && x.Source == TaskSource.Unknow && x.Status == TaskStatus.Completed).ToArray();
                unitOfWork.Commit();
            }
            foreach (var item in tasks)
            {
                BusinessHelper.PopRequestList(item);
            }

            _taskArchiveThread = new System.Threading.Thread(BusinessHelper.ArchiveProc);
            _taskArchiveThread.Name = "任务完成通知程序任务归档队列";
            _taskArchiveThread.IsBackground = true;
            _taskArchiveThread.StartAndManaged();

            _taskRequestThread = new System.Threading.Thread(BusinessHelper.RequestProc);
            _taskRequestThread.Name = "自复位任务完成处理队列";
            _taskRequestThread.IsBackground = true;
            _taskRequestThread.StartAndManaged();
        }

        private void OnnoWMSTaskCompletedHandler(TaskFinishedEvent obj)
        {
            if (obj.Source != TaskSource.Wcs && obj.Source != TaskSource.Unknow)
                return;

            //if (!Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<bool>("自动归档Wcs任务", true))
            //    return;

            if (_filterTaskTypes.Contains(obj.TaskType))
                return;

            Task task;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                task = unitOfWork.session.Get<Task>(obj.Id);
                unitOfWork.Commit();
            }
            if (task == null)
                return;
            try
            {
                if (obj.Source == TaskSource.Unknow && _taskTypes.Contains(obj.TaskType) && task.Status == TaskStatus.Completed)
                {
                    if (BusinessHelper.ExistArchiveProcTaskList(task) || BusinessHelper.ExistRequestProcTaskList(task))
                        return;

                    BusinessHelper.PushRequestList(task);
                }
                else
                {
                    if (task.Status == TaskStatus.Completed || task.Status == TaskStatus.Cancelled)
                    {
                        if (BusinessHelper.ExistArchiveProcTaskList(task) || BusinessHelper.ExistRequestProcTaskList(task))
                            return;

                        if (runningAutoClearOccupySingleTaskHand)
                            BusinessHelper.PushDeleteTaskList(task);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("自复位占位请求任务完成处理程序在处理过程中发生异常，异常信息：" + ex.Message, this);
                throw ex;
            }
        }
    }
}
