using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Wcs.Framework.Cfg;
using NHibernate.Linq;
using Wcs.Framework.EventBus;
using Wcs.Framework.Events;
using NLog;

namespace Wcs.Framework
{
    /// <summary>
    /// 默认的自启动程序<br />
    /// 1、保持设备和系统的联机模式。设备脱机后自动尝试连接到设备。
    /// </summary>
    public class DefaultApplicationStartup : ThreadRunningLog, IApplicationStartup
    {
        static Dictionary<Device, System.Threading.Thread> _connectThreads = new Dictionary<Device, System.Threading.Thread>();
        static System.Threading.Thread _taskDispatcherThread;
        static System.Threading.Thread _taskPreDispatcherThread;
        static Logger _logger = LogManager.GetCurrentClassLogger();
        public Int32 Interval { get; private set; }

        public DefaultApplicationStartupMode Mode { get; private set; }

        public void Run(IWcsApplication application)
        {
            if (this.Mode == DefaultApplicationStartupMode.Both || this.Mode == DefaultApplicationStartupMode.DeviceConnector)
            {
                startDevice();
            }

            if (this.Mode == DefaultApplicationStartupMode.Both || this.Mode == DefaultApplicationStartupMode.TaskDispatcher)
            {
                ThreadPool.QueueUserWorkItem(stat =>
                {
                    fireTaskStatusChangedEvents();
                });

                _taskDispatcherThread = new Thread(startTaskDispatcher);
                _taskDispatcherThread.Name = "任务拆分";
                _taskDispatcherThread.IsBackground = true;
                _taskDispatcherThread.StartAndManaged();

                _taskPreDispatcherThread = new Thread(startPreTaskDispatcher);
                _taskPreDispatcherThread.Name = "任务预拆分";
                _taskPreDispatcherThread.IsBackground = true;
                _taskPreDispatcherThread.StartAndManaged();
            }

            //ThreadPool.QueueUserWorkItem((stat) =>
            //{
            //    startTaskDispatcher();
            //});

        }

        /// <summary>
        /// 记录设备连续断开的次数，用于管理重试连接的等待时间（单位：毫秒）。<br />
        /// interval = 5000 * num（最大 60000）
        /// </summary>
        static Dictionary<Device, Int32> ConnectionRetries = new Dictionary<Device, int>();
        static Dictionary<Device, Object> _ConnectionRetriesLockers = new Dictionary<Device, object>();
        //object _ConnectionRetriesLocker = new object();
        void startDevice()
        {
            var devices = WcsConfiguration
                .Instance
                .DeviceCollection
                .ParticularDeviceCollection.SelectMany(x => x.DeviceElements)
                .Select(x => x.Device);

            foreach (var device in devices)
            {
                _ConnectionRetriesLockers.Add(device, new object());

                //在启动时，为每一个设备添加一个无法连接的故障
                if (!device.IsConnected)
                {
                    device.AddFailure(new UnableToConnectEquipmentFailure(device));
                }

                device.Disconnected += (sender, args) =>
                {
                    if (_connectThreads.ContainsKey(sender)
                        && _connectThreads[sender] != null
                        && _connectThreads[sender].ThreadState == ThreadState.Stopped)
                    {
                        _connectThreads[sender] = null;
                        _connectThreads.Remove(sender);
                    }

                    if (args.Reason == DisconnectReason.User)
                    {
                        _logger.Info1(String.Format("{0} 已被用户断开连接", sender), this, sender);
                        lock (_ConnectionRetriesLockers)
                        {
                            ConnectionRetries[sender] = 0;
                        }
                    }
                    else
                    {
                        int connectionRetires = 0;
                        lock (_ConnectionRetriesLockers)
                        {
                            if (!ConnectionRetries.ContainsKey(sender))
                            {
                                ConnectionRetries[sender] = 1;
                            }
                            else
                            {
                                ConnectionRetries[sender] = ConnectionRetries[sender] + 1;
                            }

                            connectionRetires = ConnectionRetries[sender];
                        }


                        _logger.Debug1(String.Format("{0} 由于 “{1}” 原因已断开,已尝试连接 {2} 次", sender, args.Reason.GetDescription(), connectionRetires), this, sender);

                        //最小 5 秒间隔，最大 60 秒间隔
                        int interval = connectionRetires * 5000;
                        if (interval < 0)
                        {
                            interval = 5000;
                        }

                        if (interval > 5000 * 12)
                        {
                            interval = 5000 * 12;
                        }
                        if (args.Reason == DisconnectReason.ConnectFailed)
                        {
                            _logger.Warn1(String.Format("{0} 将在 {1} 秒钟后重新尝试连接.", sender, interval / 1000), this, sender);
                        }
                        else
                        {
                            _logger.Warn1(String.Format("{0} 已断开连接，{1} 秒钟后将重新尝试连接.", sender, interval / 1000), this, sender);
                        }
                        ThreadPool.QueueUserWorkItem((state) =>
                        {
                            Thread.Sleep(interval);
                            ((Device)state).Connect();
                        }, sender);
                    }
                    args.Handled = true;
                };

                device.Connected += (sender, args) =>
                {
                    if (_connectThreads.ContainsKey(sender)
                          && _connectThreads[sender] != null
                          && _connectThreads[sender].ThreadState == ThreadState.Stopped)
                    {
                        _connectThreads[sender] = null;
                        _connectThreads.Remove(sender);
                    }


                    lock (_ConnectionRetriesLockers)
                    {
                        if (ConnectionRetries.ContainsKey(sender))
                        {
                            ConnectionRetries[sender] = 0;
                        }
                        else
                        {
                            ConnectionRetries.Add(sender, 0);
                        }
                    }
                    args.Handled = true;
                };
            }

            _logger.Trace1("准备连接到所有设备...", this);
            foreach (var device in devices)
            {
                var thread = new Thread(new ParameterizedThreadStart((state) =>
                {
                    ((Device)state).Connect();
                }));

                thread.Name = string.Format("{0}的连接管理线程", device);

                thread.StartAndManaged(device);

                _connectThreads.Add(device, thread);

                //ThreadPool.QueueUserWorkItem((state) =>
                //{
                //    ((Device)state).Connect();
                //}, device);
            }
            _logger.Debug1("已向所有设备发起连接了请求。", this);
        }
        ///// <summary>
        ///// 任务派发器主函数
        ///// </summary>
        //void startTaskDispatcher()
        //{
        //    _logger.Info1("任务拆分线程已启动.", this);

        //    while (true)
        //    {
        //        Thread.Sleep(this.Interval);
        //        lock (Lockers.TaskScan)
        //        {
        //            try
        //            {
        //                    List<Task> tasks;
        //                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
        //                {
        //                    var q = unitOfWork
        //                          .session
        //                          .Query<Task>()
        //                          .Where(x => x.Status == TaskStatus.New || x.Status == TaskStatus.Executing)
        //                          .OrderByDescending(x => x.Priority);

        //                    if (WcsConfiguration.Instance.SettingCollection.GetSetting<bool>("出库优先"))
        //                    {
        //                        q = q.ThenByDescending(x => x.Direction == TaskDirection.Out ? 0 : 1);
        //                    }

        //                    tasks = q
        //                    .Where(task =>
        //                        !task
        //                        .Movements
        //                        .Any(movement =>
        //                            movement.Status == LogicMovementStatus.Error
        //                            || movement.Status == LogicMovementStatus.Executing
        //                            || movement.Status == LogicMovementStatus.Suspend)
        //                            )
        //                           .ToList();
        //                    unitOfWork.Commit();
        //                }


        //                foreach (var task in tasks)
        //                {
        //                    try
        //                    {
        //                        List<IEvent> events = new List<IEvent>();

        //                        using (TransactionScope tsc = new TransactionScope(TransactionScopeOption.RequiresNew))
        //                        {
        //                            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
        //                            {
        //                                Boolean isNewMovement;
        //                                var movement = task.GetNextMovement(unitOfWork, out isNewMovement);
        //                                if (isNewMovement)
        //                                {
        //                                    events.Add(new Wcs.Framework.Events.LogicMovementAddedEvent(movement));
        //                                }

        //                                unitOfWork.session.Update(task);

        //                                //强制发送 sql 语句
        //                                unitOfWork.session.Flush();

        //                                unitOfWork.Commit();
        //                            }

        //                            tsc.Complete();
        //                        }

        //                        if (events != null && events.Count > 0)
        //                        {
        //                            Wcs.Framework.EventBus.EventBus.Instance.Publish(events.ToArray());
        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        _logger.Error1(ex, this);
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                _logger.Error1(ex, this);
        //            }
        //        }

        //    }

        //    _logger.Info1("任务拆分线程已停止.", this);
        //}

        KeyValuePair<String, Boolean> _checked = new KeyValuePair<string, bool>();
        /// <summary>
        /// 任务派发器主函数
        /// </summary>
        void startTaskDispatcher()
        {
            _logger.Info1("任务拆分线程已启动.", this);

            while (true)
            {
                Thread.Sleep(250);

                if (ExecutiveState.ApplicationExiting)
                {
                    this.Log("应用程序正在退出，本次不再进行拆分");
                    continue;
                }

                try
                {
                    List<IEvent> events = new List<IEvent>();
                    using (TransactionScope tsc = new TransactionScope(TransactionScopeOption.RequiresNew))
                    {
                        using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                        {
                            var tasks = unitOfWork
                                    .session
                                    .Query<Task>()
                                    .Where(x => x.Status == TaskStatus.New || x.Status == TaskStatus.Executing)
                                    .Where(task =>
                                    !task
                                    .Movements
                                    .Any(movement =>
                                        movement.Status == LogicMovementStatus.Error
                                        || movement.Status == LogicMovementStatus.New
                                        || movement.Status == LogicMovementStatus.Executing
                                        || movement.Status == LogicMovementStatus.Suspend)
                                        )
                                   .OrderByDescending(x => x.Priority)
                                   .ThenBy(x => x.Id);

                            foreach (var task in tasks)
                            {
                                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                                sw.Start();
                                Location _currentLocation = LocationConverter.ToLocation(task.CurrentLocation);
                                Location _end = LocationConverter.ToLocation(task.EndLocation);
                                if (_currentLocation.UnifiedCode != _end.UnifiedCode
                                    && !_currentLocation.Synonymous.Any(x => x.UserCode == _end.UserCode)
                                    && !_end.Synonymous.Any(x => x.UserCode == _currentLocation.UserCode))
                                {
                                    //预拆分版本
                                    if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("TaskDispatcherVersion", "") == "V1")
                                    {
                                        if (!preDic.ContainsKey(task.TaskCode))
                                        {
                                            this.Log(String.Format("☆☆☆☆☆☆由于预拆分任务未执行，本次未拆分任务{0}", task));
                                            continue;
                                        }

                                        var preTask = preDic[task.TaskCode];
                                        var movement = preTask.PreLogicMovement;
                                        //if (movement.Id != 0 || movement.EquipmentActions.First().Id != 0)
                                        //    continue;
                                        //movement.CreatedAt = DateTime.Now;
                                        //task.AddMovement(movement);
                                        //task.TaskPredictRoutes = preTask.PreTask.TaskPredictRoutes;
                                        var _movement = task.GetNextMovement(out bool isNewMovement, out string predictRoutes, movement);
                                        task.TaskPredictRoutes = preTask.PreTask.TaskPredictRoutes;
                                        if (isNewMovement)
                                        {
                                            events.Add(new Wcs.Framework.Events.LogicMovementAddedEvent(_movement));

                                            //强制发送 sql 语句
                                            unitOfWork.session.Flush();
                                        }

                                        sw.Stop();
                                        this.Log(String.Format("☆☆☆☆☆☆本次拆分任务{0}，耗时{1}", task, sw.ElapsedMilliseconds));
                                    }
                                    //原来版本
                                    else
                                    {
                                        var movement = task.GetNextMovement(out bool isNewMovement, out string predictRoutes);

                                        if (isNewMovement)
                                        {
                                            var lastRouteIds = task.TaskPredictRoutes != null ? String.Join(",", task.TaskPredictRoutes.OrderBy(x => x.Key).Select(x => x.Value).ToArray()) : "";
                                            if (!lastRouteIds.Contains(predictRoutes))
                                            {
                                                task.TaskPredictRoutes.Clear();
                                                var routeIds = predictRoutes.Split(',').Select(x => Convert.ToInt32(x)).ToArray();
                                                for (int i = 0; i < routeIds.Length; i++)
                                                {
                                                    task.TaskPredictRoutes.Add(i, routeIds[i]);
                                                }
                                            }

                                            events.Add(new Wcs.Framework.Events.LogicMovementAddedEvent(movement));

                                            //强制发送 sql 语句
                                            unitOfWork.session.Flush();

                                            sw.Stop();
                                            this.Log(String.Format("☆☆☆☆☆☆本次拆分任务{0}，耗时{1}", task, sw.ElapsedMilliseconds));
                                        }
                                        else
                                        {
                                            sw.Stop();
                                            this.Log(String.Format("☆☆☆☆☆☆本次不需要拆分任务{0}，耗时{1}", task, sw.ElapsedMilliseconds));
                                        }
                                    }
                                }
                                else
                                {
                                    sw.Stop();
                                    this.Log(String.Format("☆☆☆☆☆☆已到达任务终点，本次未拆分任务{0}，耗时{1}", task, sw.ElapsedMilliseconds));
                                }
                            }

                            unitOfWork.Commit();
                        }

                        tsc.Complete();
                    }

                    if (events != null && events.Count > 0)
                    {
                        Wcs.Framework.EventBus.EventBus.Instance.Publish(events.ToArray());
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, this);
                }

            }

            _logger.Info1("任务拆分线程已停止.", this);
        }

        public static Dictionary<string, preDispatcherTask> preDic = new Dictionary<string, preDispatcherTask>();
        /// <summary>
        /// 任务预派发主函数
        /// </summary>
        void startPreTaskDispatcher()
        {
            _logger.Info1("任务预拆分线程已启动.", this);

            while (true)
            {
                Thread.Sleep(200);

                if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("TaskDispatcherVersion", "") != "V1")
                    break;

                if (ExecutiveState.ApplicationExiting)
                {
                    this.Log("应用程序正在退出，本次不再进行预拆分");
                    continue;
                }

                try
                {
                    List<Task> tasks = new List<Task>();
                    //var keys = preDic.Keys;
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                    {
                        tasks = unitOfWork.session.Query<Task>()
                                    //.Where(x => !keys.Contains(x.TaskCode))
                                    .Where(x => x.Status == TaskStatus.New || x.Status == TaskStatus.Executing)
                                    .OrderByDescending(x => x.Priority)
                                    .ThenBy(x => x.Id)
                                    .ToList();

                        unitOfWork.Commit();
                    }

                    foreach (var task in tasks)
                    {
                        preDispatcherTask preTask = null;
                        if (preDic.ContainsKey(task.TaskCode))
                            preTask = preDic[task.TaskCode];
                        if (preTask != null)
                        {
                            if (preTask.PreTask.Movements.Count() > task.Movements.Count())
                            {
                                //if (preTask.PreLogicMovement.Id != 0 || preTask.PreLogicMovement.EquipmentActions.First().Id != 0)
                                //{
                                //    preDic.Remove(task.TaskCode);
                                //    this.Log(String.Format("☆☆☆☆☆☆已预拆分任务，但当前预拆分任务已保存，本次重置预拆分任务{0}记录", task));
                                //}
                                //else
                                //{
                                    this.Log(String.Format("☆☆☆☆☆☆已预拆分任务，本次未重复预拆分任务{0}", task));
                                    continue;
                                //}
                            }
                            else //if (preTask.PreTask.Movements.Count() <= task.Movements.Count())
                            {
                                preDic.Remove(task.TaskCode);
                                this.Log(String.Format("☆☆☆☆☆☆已预拆分任务，但当前预拆分任务已过期，本次重置预拆分任务{0}记录", task));
                            }
                        }

                        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                        sw.Start();
                        Location _start = LocationConverter.ToLocation(task.CurrentLocation);
                        if (task.Movements.Where(x => x.Status != LogicMovementStatus.Cancelled).Count() > 0)
                            _start = LocationConverter.ToLocation(task.Movements.OrderByDescending(x => x.Ordering).First().EndLocation);
                        Location _end = LocationConverter.ToLocation(task.EndLocation);
                        if (_start.UnifiedCode != _end.UnifiedCode)
                        {
                            Boolean isNewMovement;
                            String predictRoutes;

                            var movement = task.GetNextPreMovement(out isNewMovement, out predictRoutes, this);

                            if (isNewMovement)
                            {
                                //var lastRouteIds = task.TaskPredictRoutes != null ? String.Join(",", task.TaskPredictRoutes.OrderBy(x => x.Key).Select(x => x.Value).ToArray()) : "";
                                //if (!lastRouteIds.Contains(predictRoutes))
                                //{
                                //    task.TaskPredictRoutes.Clear();
                                //    var routeIds = predictRoutes.Split(',').Select(x => Convert.ToInt32(x)).ToArray();
                                //    for (int i = 0; i < routeIds.Length; i++)
                                //    {
                                //        task.TaskPredictRoutes.Add(i, routeIds[i]);
                                //    }
                                //}

                                if (!preDic.ContainsKey(task.TaskCode))
                                {
                                    preDic.Add(task.TaskCode, new preDispatcherTask() { PreTask = task, PreLogicMovement = movement, PredictRoutes = predictRoutes });

                                    sw.Stop();
                                    this.Log(String.Format("☆☆☆☆☆☆本次新增预拆分任务{0}记录（起点{1}，终点{2}），耗时{3}", task, movement.StartLocation.UserCode, movement.EndLocation.UserCode, sw.ElapsedMilliseconds));
                                }
                                else
                                {
                                    preDic[task.TaskCode] = new preDispatcherTask() { PreTask = task, PreLogicMovement = movement, PredictRoutes = predictRoutes };

                                    sw.Stop();
                                    this.Log(String.Format("☆☆☆☆☆☆本次重置预拆分任务{0}记录（起点{1}，终点{2}），耗时{3}", task, movement.StartLocation.UserCode, movement.EndLocation.UserCode, sw.ElapsedMilliseconds));
                                }
                            }
                            else
                            {
                                sw.Stop();
                                this.Log(String.Format("☆☆☆☆☆☆本次不需要预拆分任务{0}，耗时{1},增加到等待列表", task, sw.ElapsedMilliseconds));
                            }
                        }
                        else
                        {
                            sw.Stop();
                            this.Log(String.Format("☆☆☆☆☆☆已到达任务终点，本次未预拆分任务{0}，耗时{1},增加到等待列表", task, sw.ElapsedMilliseconds));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, this);
                }

            }

            _logger.Info1("任务预拆分线程已停止.", this);
        }

        void fireTaskStatusChangedEvents()
        {
            try
            {
                List<Task> tasks = new List<Task>();
                using (TransactionScope tsc = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                    {
                        tasks = unitOfWork.session.Query<Task>().ToList();
                        unitOfWork.Commit();
                    }

                    tsc.Complete();
                }

                List<IEvent> events = new List<IEvent>();
                foreach (var task in tasks)
                {
                    events.Add(new TaskStatusChangedEvent(task.Id, task.TaskCode, task.Status, task.BizType, task.Source, task.TaskType));
                    if (task.Status == TaskStatus.Cancelled || task.Status == TaskStatus.Completed)
                    {
                        events.Add(new TaskFinishedEvent(task.Id, task.TaskCode, task.Status, task.BizType, task.Source, task.TaskType, task.FinishedAt.Value));
                    }
                }

                Wcs.Framework.EventBus.EventBus.Instance.Publish(events.ToArray());
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
            }
        }

        public void Initialize(StartupElement element)
        {
            this.Init("WCS任务拆分线程");
            this.Interval = element.GetAttributeOrDefault<Int32>("interval", 100);
            this.Mode = element.GetAttributeOrDefault<DefaultApplicationStartupMode>("mode", DefaultApplicationStartupMode.Both);
        }
    }

    public class preDispatcherTask
    {
        public Task PreTask { get; set; }
        public PreLogicMovement PreLogicMovement { get; set; }

        public string PredictRoutes { get; set; }
    }

    public enum DefaultApplicationStartupMode
    {
        Both,
        TaskDispatcher,
        DeviceConnector
    }
}
