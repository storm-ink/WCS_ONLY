using Newtonsoft.Json;
using NHibernate.Linq;
using NHibernate.Mapping;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Wcs;
using Wcs.Framework;
using Wcs.Framework.Cfg;
using Wcs.Framework.Events;
using Wcs.FrameworkExtend;
using Wcs.FrameworkExtend.Events;

namespace Wcs.FrameworkExtend
{
    /// <summary>
    /// 预任务启动处理程序
    /// </summary>
    public class PreTaskHandStartup : ThreadRunningLog, IApplicationStartup
    {
        static Thread _thread;
        static Thread _taskArchiveThread;
        static Thread _reportWmsTaskStateThread;
        static Thread _requestThread;
        Logger _logger = LogManager.GetCurrentClassLogger();

        public void Initialize(StartupElement element)
        {
            var interval = element.GetAttributeOrDefault<Int32>("interval", 100);
            if (interval < 100 || interval > 1000)
                PreTaskHandHelper.interval = 100;
            else
                PreTaskHandHelper.interval = interval;
        }

        public void Run(IWcsApplication application)
        {
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                PreTaskHandHelper.PreTasks = unitOfWork.session.Query<PreTask>().ToList();
                PreTaskHandHelper.Tasks = unitOfWork.session.Query<Task>().ToList();
                unitOfWork.Commit();
            }
            foreach (var item in PreTaskHandHelper.PreTasks)
            {
                //重复上报WMS时，如果WMS找不到该条任务，则直接返回 true
                if (item.Status == TaskStatus.Completed || item.Status == TaskStatus.Cancelled)
                {
                    if (item.Source == TaskSource.Wms)
                        PreTaskHandHelper.PushWaitReportPreTaskList(item);
                    else
                        PreTaskHandHelper.PushWaitRequestPreTaskList(item);
                }
            }

            _thread = new Thread(PreTaskHandHelper.PreTaskHandProc);
            _thread.Name = "预任务派发处理程序";
            _thread.IsBackground = true;
            _thread.StartAndManaged();

            _taskArchiveThread = new Thread(PreTaskHandHelper.ArchiveProc);
            _taskArchiveThread.Name = "预任务完成通知程序任务归档队列";
            _taskArchiveThread.IsBackground = true;
            _taskArchiveThread.StartAndManaged();

            _reportWmsTaskStateThread = new Thread(PreTaskHandHelper.PreTaskReportUpHandProc);
            _reportWmsTaskStateThread.Name = "WMS任务上报程序";
            _reportWmsTaskStateThread.IsBackground = true;
            _reportWmsTaskStateThread.StartAndManaged();

            _requestThread = new Thread(PreTaskHandHelper.PreTaskRequestHandProc);
            _requestThread.Name = "WCS任务完成请求程序";
            _requestThread.IsBackground = true;
            _requestThread.StartAndManaged();

            Wcs.Framework.EventBus.EventBus.Instance.Subscribe<Wcs.FrameworkExtend.Events.PreTaskAddedEvent>(PreTaskAddedEventHand);
        }

        private void TaskArchivedEventHand(TaskArchivedEvent args)
        {
            try
            {
                lock (PreTaskHandHelper.Tasks)
                {
                    var task = PreTaskHandHelper.Tasks.FirstOrDefault(x => x.TaskCode == args.TaskCode);
                    if (task != null)
                    {
                        PreTaskHandHelper.Tasks.Remove(task);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
            }
        }

        private void PreTaskAddedEventHand(PreTaskAddedEvent args)
        {
            try
            {
                PreTaskHandHelper.PushPreTaskList(args.PreTask);
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
            }
        }
    }
}
