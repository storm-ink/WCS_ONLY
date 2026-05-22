using NHibernate.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Wcs;
using Wcs.Framework.Events;

namespace Wcs.Framework
{
    /// <summary>
    /// 任务管理
    /// 分区域管理
    /// </summary>
    public class TaskManager
    {
        /// <summary>
        /// 区域名称
        /// </summary>
        public string TaskManagerName { get; set; }
        /// <summary>
        /// 区域位置
        /// </summary>
        public List<Location> Locations { get; set; }

        /// <summary>
        /// 调度任务池
        /// </summary>
        public List<Task> TaskPool { get; set; }

        /// <summary>
        /// 不需要调度的任务池
        /// </summary>
        public List<string> ExceptionTaskPool { get; set; }
        /// <summary>
        /// 允许执行的任务池
        /// </summary>
        public List<string> EnableTaskPool { get; set; }
        /// <summary>
        /// 执行中的任务池
        /// </summary>
        public List<string> RunningTaskPool { get; set; }
        /// <summary>
        /// 暂时不允许执行的任务池
        /// </summary>
        public Dictionary<string, string> UnableTaskPool { get; set; }
        /// <summary>
        /// 自动提升优先级任务池
        /// </summary>
        static Thread _atuoUpTaskPriorityThread;
        /// <summary>
        /// 日志
        /// </summary>
        public Logger _logger { get; set; }
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public TaskManager()
        {
            TaskManagerName = "wcs";
            Locations = new List<Location>();
            _logger = LogManager.GetCurrentClassLogger();

            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                TaskPool = unitOfWork.session.Query<Task>().ToList();
                unitOfWork.Commit();
            }
            _logger.Info1($"任务管理器已加载{TaskPool.Count}条任务", this);



            ExceptionTaskPool = new List<string>();
            EnableTaskPool = new List<string>();
            UnableTaskPool = new Dictionary<string, string>();


            Wcs.Framework.EventBus.EventBus.Instance.Subscribe<Wcs.Framework.Events.TaskAddedEvent>(TaskAddedEventHand);
            Wcs.Framework.EventBus.EventBus.Instance.Subscribe<Wcs.Framework.Events.TaskCurrentLocationChangedEvent>(TaskCurrentLocationChangedEventHand);
            Wcs.Framework.EventBus.EventBus.Instance.Subscribe<Wcs.Framework.Events.TaskFinishedEvent>(TaskFinishedEventHand);
            //Wcs.Framework.EventBus.EventBus.Instance.Subscribe<Wcs.Framework.Events.TaskPriorityChangedEvent>(TaskPriorityChangedEventHand);
            //Wcs.Framework.EventBus.EventBus.Instance.Subscribe<Wcs.Framework.Events.TaskStatusChangedEvent>(TaskStatusChangedEventHand);
            //Wcs.Framework.EventBus.EventBus.Instance.Subscribe<Wcs.Framework.Events.TaskUpdateEvent>(TaskUpdateEventHand);
            //Wcs.Framework.EventBus.EventBus.Instance.Subscribe<Wcs.Framework.Events.TaskFinishedEvent>(TaskFinishedEventHand);
            //Wcs.Framework.EventBus.EventBus.Instance.Subscribe<Wcs.Framework.Events.TaskArchivedEvent>(TaskArchivedEventHand);
            //Wcs.Framework.EventBus.EventBus.Instance.Subscribe<LogicMovementAddedEvent>(LogicMovementAddedEventHand);

            ParameterizedThreadStart Start = new ParameterizedThreadStart(AutoUpTaskProirity);
            _atuoUpTaskPriorityThread = new Thread(Start);
            _atuoUpTaskPriorityThread.IsBackground = true;
            _atuoUpTaskPriorityThread.Start();
            this._logger.Debug1(String.Format("{0} 线程已经启动！", this), this);
        }

        private void TaskFinishedEventHand(TaskFinishedEvent obj)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="taskManagerName"></param>
        /// <param name="locations"></param>
        public TaskManager(string taskManagerName, List<Location> locations)
            : this()
        {
            TaskManagerName = taskManagerName;
            Locations = locations;
        }


        private void TaskAddedEventHand(TaskAddedEvent obj)
        {
            lock (TaskPool)
            {
                if (!TaskPool.Any(x => x.TaskCode == obj.Task.TaskCode))
                    TaskPool.Add(obj.Task);
            }
        }

        private void TaskCurrentLocationChangedEventHand(TaskCurrentLocationChangedEvent obj)
        {
            lock (TaskPool)
            {
                var task = TaskPool.FirstOrDefault(x => x.TaskCode == obj.TaskCode);
                if (task != null)
                    task.CurrentLocation = obj.CurrentLocation;
            }
        }

        /// <summary>
        /// 是否自动提升任务优先级
        /// </summary>
        public bool AutoUpTaskPriority
        {
            get
            {
                return Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<bool>("autoUpTaskPriority", false);
            }
        }
        /// <summary>
        /// 自动提升优先级时间设置
        /// 默认10分钟
        /// </summary>
        public int AutoUpTaskPriorityInterval
        {
            get
            {
                return Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<int>("autoUpTaskPriorityInterval", 10);
            }
        }
        /// <summary>
        /// 自动提升优先级检测时间（即任务超时时间，超时多久开始进行自动提升优先级检测）
        /// </summary>
        public int AutoUpTaskPriorityStartTime
        {
            get
            {
                return Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<int>("autoUpTaskPriorityStartTime", 15);
            }
        }

        /// <summary>
        /// 设置优先级自动提升是否启用
        /// </summary>
        /// <param name="setAuto"></param>
        public void SetAutoUpTaskPriority(bool setAuto)
        {
            Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.SetSetting<bool>("autoUpTaskPriority", setAuto);
        }
        /// <summary>
        /// 设置优先级自动提升检测时间
        /// 单位 分钟
        /// </summary>
        /// <param name="interval"></param>
        public void SetAutoUpTaskPriorityInterval(int interval)
        {
            Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.SetSetting<int>("autoUpTaskPriorityInterval", interval);
        }
        /// <summary>
        /// 自动提升优先级检测时间（即任务超时时间，超时多久开始进行自动提升优先级检测）
        /// </summary>
        public void SetAutoUpTaskPriorityStartTime(int checkTime)
        {
            Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.SetSetting<int>("autoUpTaskPriorityStartTime", checkTime);
        }

        /// <summary>
        /// 自动提升优先级进程
        /// 检测频率 1分钟 所有任务检测一次
        /// </summary>
        /// <param name="obj"></param>
        private void AutoUpTaskProirity(object obj)
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(1000 * 60);
                    if (!AutoUpTaskPriority)
                        continue;

                    foreach (var item in TaskPool)
                    {
                        var ts = DateTime.Now.Subtract(item.CreatedAt).TotalMinutes;
                        if (ts < AutoUpTaskPriorityStartTime)
                            continue;
                        var calc = Math.Ceiling((ts - AutoUpTaskPriorityStartTime) / AutoUpTaskPriorityStartTime);
                        var _priority = (int)Math.Pow(2, calc);
                        if (_priority == item.WcsPriority)
                            continue;

                        using (NHUnitOfWork uniOfWork = new NHUnitOfWork())
                        {
                            var task = uniOfWork.session.Get<Task>(item.Id);
                            if (task.WcsPriority != _priority)
                            {
                                task.WcsPriority = _priority;
                                uniOfWork.session.SaveOrUpdate(task);
                            }
                            uniOfWork.Commit();
                        }
                        Wcs.Framework.EventBus.EventBus.Instance.Publish<TaskPriorityChangedEvent>(new TaskPriorityChangedEvent(item.Id, item.TaskCode, item.WcsPriority));
                        item.WcsPriority = _priority;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, this);
                }
            }
        }
    }
}
