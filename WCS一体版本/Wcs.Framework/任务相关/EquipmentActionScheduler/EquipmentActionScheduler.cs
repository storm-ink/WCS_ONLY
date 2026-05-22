using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NHibernate.Linq;
using System.Threading;
using Wcs.Framework.Events;
using Wcs.Framework.EventBus;
using Wcs.MethodTrack;
using Wcs;
using System.Windows.Forms;

namespace Wcs.Framework
{
    public class EquipmentActionScheduler : ThreadRunningLog, IDeviceHolder
    {
        protected List<EquipmentAction> _actions = new List<EquipmentAction>();
        protected Logger _logger;

        Thread _thread;
        protected Object _currentActionLocker = new object();

        public virtual MethodDescriptorTree _methodDescriptorTree { get; protected set; }

        #region 持久化数据
        public virtual String DeviceName { get; protected set; }

        /// <summary>
        /// 记录当前正在执行的物理动作
        /// </summary>
        public virtual EquipmentAction CurrentAction { get; set; }
        protected EquipmentActionScheduler()
        {
            _FixUnknowEquipmentTaskOnStatusChanged = true;

            _methodDescriptorTree = new MethodDescriptorTree("开始");
            _methodDescriptorTree
            .AddChildren(new MethodDescriptor("是否包含动作"))
            .AddChildren(new MethodDescriptor("设备是否已连接"))
            .AddChildren(new MethodDescriptor("设备是否空闲"))
            .AddChildren(new MethodDescriptor("是否包含待可发送任务"))
            .AddChildren(new MethodDescriptor("设备是否已被其它对象持有"))
            .AddChildren(new MethodDescriptor("持有设备"))
            .AddChildren(new MethodDescriptor("是否被过滤器否决"))
            .AddChildren(new MethodDescriptor("结束"));
        }
        #endregion

        /// <summary>
        /// 指示调度线程是否处于运行状态
        /// </summary>
        public virtual Boolean IsRunning { get; protected set; }

        /// <summary>
        /// 设置轮循间隔时间,最小100ms,最大3000ms
        /// </summary>
        public virtual Int32 Interval
        {
            get
            {
                var v = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<Int32>("调度频率." + this.Device.Name, 1000);

                if (v < 1000)
                {
                    v = 1000;
                }

                if (v > 3000)
                {
                    v = 3000;
                }

                return v;
            }
            protected set
            {
            }
        }

        /// <summary>
        /// 调度程序所属的设备
        /// </summary>
        public virtual TaskableDevice Device { get; protected set; }
        /// <summary>
        /// 调度程序动作过滤器集合
        /// </summary>
        public virtual EquipmentActionSchedulerFilter[] ActionFilters { get; protected set; }

        static object objlock = new object();
        /// <summary>
        /// 当前所有被调度的物理动作
        /// </summary>
        public virtual EquipmentAction[] Actions
        {
            get
            {
                return _actions.Where(x => x.Id != 0).ToArray();
            }
            protected set { }
        }

        /// <summary>
        /// 获取或设置一个值，指示设备是否在动作状态改变时检测需要修复的动作序列当前正在执行的动作标识
        /// </summary>
        [Obsolete("请忽在编程只引用该属性，该属性只为临时解决一些目前无法解决的问题。")]
        public virtual Boolean _FixUnknowEquipmentTaskOnStatusChanged { get; set; }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        /// <param name="device">所属设备</param>
        /// <remarks>
        /// 将自动从数据库中读取状态为 <see cref="F:Wcs.Framework.EquipmentActionStatus.New"/> 的 <see cref="T:Wcs.Framework.EquipmentAction"/>
        /// </remarks>
        public EquipmentActionScheduler(TaskableDevice device, EquipmentActionSchedulerFilter[] actionFilters)
            : this()
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            this._logger = LogManager.GetCurrentClassLogger();
            this.Device = device;
            this.DeviceName = device.Name;
            this.Init("EquipmentActionScheduler_" + this.Device.Name);

            EquipmentActionSchedulerFilter[] basicActionFilters =
                new EquipmentActionSchedulerFilter[]{
                    new BasicEquipmentActionSchedulerFilter()
                };
            if (actionFilters == null || actionFilters.Length == 0)
            {
                this.ActionFilters = basicActionFilters;
            }
            else
            {
                this.ActionFilters = basicActionFilters.Concat(actionFilters).ToArray();
            }

            EquipmentActionScheduler scheduler;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                scheduler = unitOfWork.session
                    .Query<EquipmentActionScheduler>()
                    .SingleOrDefault(x => x.DeviceName == device.Name);
                if (scheduler == null)
                {
                    scheduler = new EquipmentActionScheduler();
                    scheduler.DeviceName = device.Name;
                    unitOfWork.session.Save(scheduler);
                }

                unitOfWork.Commit();
            }

            this.CurrentAction = scheduler.CurrentAction;
            this._logger.Debug1(string.Format("预操作{0}当前动作由{1}修改为{2}。", this, null, this.CurrentAction), this, this.CurrentAction);

            EventBus.EventBus.Instance.Publish(new SchedulerCurrentActionChangedEvent(this, null, this.CurrentAction));

            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                _actions = unitOfWork
                    .session
                    .Query<EquipmentAction>()
                    .Where(x => x.DeviceName == device.Name
                        && (x.Status != EquipmentActionStatus.Completed || x.Status != EquipmentActionStatus.Cancelled)
                        )
                    .ToList();

                unitOfWork.Commit();
            }

            EventBus.EventBus.Instance.Subscribe<EquipmentActionStatusChangedEvent>(onEquipmentActionStatusChanged);
            EventBus.EventBus.Instance.Subscribe<TaskArchivedEvent>(onTaskArchived);
            EventBus.EventBus.Instance.Subscribe<TaskCurrentLocationChangedEvent>(onTaskCurrentLocationChanged);
            Wcs.Framework.EventBus.EventBus.Instance.Subscribe<TaskPriorityChangedEvent>(onPriorityChange);
            Wcs.Framework.EventBus.EventBus.Instance.Subscribe<TaskUpdateEvent>(onTaskUpdate);
        }

        /// <summary>
        /// 任务更新
        /// </summary>
        /// <param name="obj"></param>
        private void onTaskUpdate(TaskUpdateEvent obj)
        {
            var ac = this._actions.Where(x => x.Movement.Task.Id == obj.newTask.Id).FirstOrDefault();
            if (ac != null)
            {
                _logger.Debug(string.Format("TaskID:{0} 处理 TaskUpdateEvent", ac.Movement.Task.Id));

                ac.Movement.Task = obj.newTask;
            }
        }

        /// <summary>
        /// 更改任务优先级
        /// </summary>
        /// <param name="obj"></param>
        private void onPriorityChange(TaskPriorityChangedEvent obj)
        {
            var ac = this._actions.Where(x => x.Movement.Task.Id == obj.Id).FirstOrDefault();
            if (ac != null)
            {
                _logger.Debug(string.Format("TaskID:{0} Priority:{1} ChangeTo  Priority:{2}", ac.Movement.Task.Id, ac.Movement.Task.Priority, obj.Priority));

                ac.Movement.Task.Priority = obj.Priority;
            }
        }

        /// <summary>
        /// 将指定的物理动作添加到任务队列中
        /// </summary>
        /// <param name="action">物理动作</param>
        public virtual void Add(EquipmentAction action)
        {
            SupportTransactionMethod<EquipmentAction>.Join(action, (state) =>
            {
                lock (_actions)
                {
                    if (_actions.Any(x => x.EquipmentTaskId == state.EquipmentTaskId))
                    {
                        this._logger.Warn1(string.Format("尝试向{0}添加{1}时发现其已存在。", this, state), this, state);
                        return;
                    }
                    state.SequenceOrdering = _actions.Max(x => x.SequenceOrdering).GetValueOrDefault(0) + 1;
                    _actions.Add(state);
                    this._logger.Debug1(string.Format("预操作{0}被加入到{1}当中。", state, this), this, state);
                }
            }, (state) =>
            {
                lock (_actions)
                {
                    EquipmentAction act = this.Actions.SingleOrDefault(x => x.EquipmentTaskId == state.EquipmentTaskId);
                    if (act == null)
                    {
                        this._logger.Warn1(string.Format("回滚{0}被加入到{1}当中的操作时，发现在{1}中未找到这个元素", state, this), this, state);
                        return;
                    }

                    _actions.Remove(act);
                    this._logger.Warn1(string.Format("回滚{0}被加入到{1}当中的操作。", state, this), this, state);
                }
            }, (state) =>
            {
                this._logger.Debug1(string.Format("提交{0}被加入到{1}当中的操作", state, this), this, state);

                this._logger.Trace1(string.Format("{0}被加入到{1}当中", state, this), this, state);

                EventBus.EventBus.Instance.Publish(new SchedulerActionAddedEvent(this, state));

                if (!this.IsRunning)
                {
                    Start();
                }
            });
        }

        /// <summary>
        /// 将指定的物理动作从任务队列中移除
        /// </summary>
        /// <param name="_action">物理动作</param>
        public virtual void Remove(EquipmentAction _action)
        {
            SupportTransactionMethod<EquipmentAction>.Join(_action, (state) =>
            {
                lock (_actions)
                {
                    EquipmentAction act = this.Actions.SingleOrDefault(x => x.EquipmentTaskId == state.EquipmentTaskId);
                    if (act == null)
                    {
                        this._logger.Warn1(string.Format("尝试移除{0},但在{1}中未找这个元素", state, this), this, state);
                        return;
                    }

                    _actions.Remove(act);
                    this._logger.Debug1(string.Format("预操作{0}被从{1}当中移除。", state, this), this, state);
                }
            }, (state) =>
            {
                if (state.SupportTransactionObjectStatus == SupportTransactionObjectStatus.Initialized)
                {
                    return;
                }
                lock (_actions)
                {
                    if (_actions.Any(x => x.EquipmentTaskId == state.EquipmentTaskId))
                    {
                        this._logger.Warn1(string.Format("尝试向{0}添加{1}时发现其已存在。", this, state), this, state);
                        return;
                    }

                    _actions.Add(state);
                    this._logger.Warn1(string.Format("回滚{0}被从{1}当中移除的操作。", state, this), this, state);
                }
            }, (state) =>
            {
                List<IEvent> events = new List<IEvent>();
                lock (_currentActionLocker)
                {
                    if (this.CurrentAction != null && this.CurrentAction.Id == state.Id)
                    {
                        using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                        {
                            var scheduler = unitOfWork.session.Get<EquipmentActionScheduler>(this.DeviceName);

                            //注释此处，是因为上面已经判断过当前的动作了。如果此处再加判断，
                            //那么如果在外部调用 EquipmentActionScheduler.Remove 前把 CurrentAction 关联的对象给删除了，此处代码执行将出现意外。
                            //以下代码使用先删除原有对象，再创建一个新对象的原因也是因为这样，因为在引用对象被删除后再查询，引用对象将返回null，
                            //再设置null将不会再往数据库发送更新语句
                            //if (scheduler.CurrentAction != null && scheduler.CurrentAction.Id == state.Id)
                            //{
                            //    scheduler.CurrentAction = null;
                            //}

                            unitOfWork.session.Delete(scheduler);

                            var newScheduler = new EquipmentActionScheduler();
                            newScheduler.DeviceName = this.Device.Name;

                            unitOfWork.session.Save(newScheduler);

                            unitOfWork.Commit();

                            this._logger.Trace1(string.Format("提交{0}当前动作由{1}修改为null的保存动作。", this, state), this, state);
                        }

                        //如果序列的当前动作和要弹出的动作一样，则将序列当前动作更新为 null
                        this._logger.Debug1(string.Format("提交{0}当前动作由{1}修改为{2}。", this, this.CurrentAction, "<null>"), this, state);

                        events.Add(new SchedulerCurrentActionChangedEvent(this, this.CurrentAction, null));

                        this.CurrentAction = null;
                    }
                }

                events.Add(new SchedulerActionRemovedEvent(this, state));

                this._logger.Debug1(string.Format("提交{0}被从{1}当中移除的操作。", state, this), this, state);

                this._logger.Trace1(string.Format("{0}被从{1}当中移除。", state, this), this, state);

                EventBus.EventBus.Instance.Publish(events.ToArray());
            });
        }

        /// <summary>
        /// 启动调试线程
        /// </summary>
        public virtual void Start()
        {
            lock (this)
            {
                if (IsRunning)
                {
                    return;
                }

                this.IsRunning = true;
                _thread = new Thread(Tick);
                _thread.IsBackground = true;
                _thread.Name = string.Format("{0}的调度线程", this.DeviceName);
                _thread.StartAndManaged();
            }
        }

        protected virtual void Tick()
        {
            this._logger.Trace1(string.Format("{0} 轮询线程已启动", this), this);
            while (true)
            {
                try
                {
                    Thread.Sleep(Interval);
                    _methodDescriptorTree.ClearAccess();

                    _methodDescriptorTree["开始"].Access(true);

                    if (this.Actions.Length == 0)
                    {
                        var actions = _actions.Where(x => x.Id == 0).Select(x => x.EquipmentTaskId);
                        if (actions.Count() > 0)
                            _methodDescriptorTree["是否包含动作"].Access(false, $"不包含任何动作(其中 {string.Join("|", actions)} 共 {actions.Count()} 条任务id=0)");
                        else
                            _methodDescriptorTree["是否包含动作"].Access(false, $"不包含任何动作");
                        break;
                    }
                    _methodDescriptorTree["是否包含动作"].Access(true);

                    if (!this.Device.IsConnected)
                    {
                        _methodDescriptorTree["设备是否已连接"].Access(false, "设备未连接");
                        continue;
                    }
                    _methodDescriptorTree["设备是否已连接"].Access(true);

                    var isidle = this.Device.IsIdle;
                    if (!isidle.Result)
                    {
                        //String msg = "";
                        //if (this.Device.Warnings != null)
                        //{
                        //    msg = string.Join("\n", this.Device.Warnings);
                        //}
                        _methodDescriptorTree["设备是否空闲"].Access(false, isidle.Information);
                        continue;
                    }
                    _methodDescriptorTree["设备是否空闲"].Access(true);

                    var availableActions = GetAvailableActions();
                    if (availableActions == null)
                    {
                        _methodDescriptorTree["是否包含待可发送任务"].Access(false, _GetAvailableActionsDescription);
                        continue;
                    }

                    if (availableActions.Count() == 0)
                    {
                        _methodDescriptorTree["是否包含待可发送任务"].Access(false, _GetAvailableActionsDescription);
                        continue;
                    }
                    _methodDescriptorTree["是否包含待可发送任务"].Access(true);

                    if (this.Device.Holder != null)
                    {
                        _methodDescriptorTree["设备是否已被其它对象持有"].Access(false, string.Format("{0} 已被 {1} 持有", this.Device, this.Device.Holder));
                        continue;
                    }
                    _methodDescriptorTree["设备是否已被其它对象持有"].Access(true);

                    this.Device.Hold(this);
                    _methodDescriptorTree["持有设备"].Access(true);
                    try
                    {
                        foreach (var action in availableActions)
                        {
                            Boolean negatory = false;
                            foreach (var filter in this.ActionFilters)
                            {
                                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                                sw.Start();
                                var filterResult = filter.Filter(this, action);
                                sw.Stop();
                                this.Log($"任务{action.Movement.Task}，物理动作{action},任务过滤器{filter.GetType()}过滤结果({filterResult.Defeated + "," + filterResult.Reason}),耗时{sw.ElapsedMilliseconds}ms");
                                if (filterResult.Defeated == true)
                                {
                                    _methodDescriptorTree["是否被过滤器否决"].Access(false, string.Format("{0} 被 {1} 否决了发送操作，原因：{2}。", action, filter, filterResult.Reason));
                                    negatory = true;
                                    this._logger.Debug1(string.Format("{0} 被 {1} 否决了发送操作，原因：{2}。", action, filter, filterResult.Reason), this, action);
                                    break;
                                }
                            }

                            if (negatory)
                            {
                                continue;
                            }
                            _methodDescriptorTree["是否被过滤器否决"].Access(true);

                            try
                            {
                                Int32 tries = 0;
                            resend:
                                try
                                {
                                    if (tries > 0)
                                    {
                                        _logger.Warn1(String.Format("由于{0}发送失败，准备第{1}次重试...", action, tries), this, action);
                                    }

                                    Device.SendTask(action);
                                }
                                catch (Exception ex)
                                {
                                    tries++;

                                    if (tries <= 3)
                                    {
                                        _logger.Warn1(ex.ToString(), this, action);

                                        goto resend;
                                    }
                                    else
                                    {
                                        _logger.Error1(ex, this, action);

                                        _logger.Warn1(String.Format("由于{0}连续发送了3次均失败，不再重试。", action), this, action);
                                    }

                                    throw;
                                }

                                //_methodDescriptorTree["任务发送"].Access(true);

                                _logger.Trace1(string.Format("{0} 发送成功", action), this, action);

                                if (!this.Device.AllowConcurrency)
                                {
                                    EquipmentAction oldCurrentAction;
                                    lock (_currentActionLocker)
                                    {
                                        oldCurrentAction = this.CurrentAction;

                                        this.CurrentAction = action;

                                        using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                                        {
                                            var scheduler = unitOfWork.session.Get<EquipmentActionScheduler>(this.DeviceName);

                                            scheduler.CurrentAction = unitOfWork.session.Get<EquipmentAction>(action.Id, NHibernate.LockMode.None);

                                            unitOfWork.session.Flush();
                                            unitOfWork.Commit();
                                        }

                                    }

                                    this._logger.Info1(string.Format("{0}当前动作由{1}修改为{2}。", this, oldCurrentAction, this.CurrentAction), this, action);

                                    EventBus.EventBus.Instance.Publish(new SchedulerCurrentActionChangedEvent(this, oldCurrentAction, this.CurrentAction));

                                }

                                action.Status = EquipmentActionStatus.Executing;
                                action.Movement.Status = LogicMovementStatus.Executing;
                                action.Movement.Task.Status = TaskStatus.Executing;

                                //此处状态更新失败后亦无影响，原因是如果任务被成功发送。设备将不断发送任务状态信号，任务状态事件的处理程序会处理其关的状态信息
                                bool locked = false;
                                try
                                {
                                    int relockNum = 0;
                                relock:
                                    if (relockNum != 0)
                                    {
                                        _logger.Debug1(string.Format("准备第{0}尝试锁定{1}...", relockNum, action), this, action);
                                    }

                                    locked = action.EnterLock();
                                    if (!locked)
                                    {
                                        if (relockNum < 3)
                                        {
                                            relockNum++;
                                            System.Threading.Thread.Sleep(200);
                                            goto relock;
                                        }
                                        else
                                        {
                                            _logger.Warn1(String.Format("{0}在发送成功后，准备更新状态为“{1}”时锁定失败，但仍将尝试直接更新该状态。", action, EquipmentActionStatus.Executing.GetDescription()), this, action);
                                        }
                                    }

                                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.Suppress))
                                    {
                                        using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                                        {
                                            var act = unitOfWork.session.Get<EquipmentAction>(action.Id);
                                            act.Status = EquipmentActionStatus.Executing;
                                            act.Movement.Status = LogicMovementStatus.Executing;
                                            act.Movement.Task.Status = TaskStatus.Executing;
                                            if (act.SentAt == null)
                                            {
                                                act.SentAt = DateTime.Now;
                                            }

                                            unitOfWork.session.Flush();
                                            unitOfWork.Commit();
                                        }

                                        ts.Complete();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    this._logger.Error1(ex, this, action);
                                    _methodDescriptorTree["结束"].Access(false, ex.ToString());
                                }
                                finally
                                {
                                    if (locked)
                                    {
                                        action.ExitLock();
                                    }
                                }


                                EventBus.EventBus.Instance.Publish(new EquipmentActionStatusChangedEvent(action.Movement.Task.Id, action.Movement.Id, action.Id, action.Status));
                                EventBus.EventBus.Instance.Publish(new LogicMovementStatusChangedEvent(action.Movement.Id, action.Movement.Task.Id, action.Movement.Status));
                                EventBus.EventBus.Instance.Publish(new TaskStatusChangedEvent(action.Movement.Task.Id, action.Movement.Task.TaskCode, action.Movement.Task.Status, action.Movement.Task.BizType, action.Movement.Task.Source, action.Movement.Task.TaskType));

                                if (!this.Device.AllowConcurrency)
                                {
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                this._logger.Error1(ex, this, action);

                                _methodDescriptorTree["结束"].Access(false, ex.ToString());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        this._logger.Error1(ex, this);

                        _methodDescriptorTree["结束"].Access(false, ex.ToString());
                    }
                    finally
                    {
                        this.Device.Unhold(this);
                    }

                }
                catch (Exception ex)
                {
                    _methodDescriptorTree["结束"].Access(false, ex.ToString());
                    this._logger.Error1(ex, this);
                }
            }

            this._logger.Trace1(string.Format("{0} 轮询线程已停止", this), this);
            this.IsRunning = false;
        }

        protected String _GetAvailableActionsDescription = null;
        /// <summary>
        /// 获取当前可以发送的动作集合（有可能返回 null）
        /// </summary>
        /// <returns></returns>
        protected virtual IOrderedEnumerable<EquipmentAction> GetAvailableActions()
        {
            _GetAvailableActionsDescription = null;

            if (ExecutiveState.ApplicationExiting)
            {
                _GetAvailableActionsDescription = "应用程序正在退出，不再下发任何物理动作。";
            }

            lock (_currentActionLocker)
            {
                if (!this.Device.AllowConcurrency && this.CurrentAction != null)
                {
                    if (this.CurrentAction.Status == EquipmentActionStatus.New || this.CurrentAction.Status == EquipmentActionStatus.Cancelled)
                    {
                        return (new EquipmentAction[] { this.CurrentAction })
                            .OrderBy(x => x.Id);
                    }
                    else
                    {
                        _GetAvailableActionsDescription = string.Format("当前动作 {0} 的状态不为 New", this.CurrentAction);
                        return null;
                    }
                }
            }
            lock (_actions)
            {
                StringBuilder sb = new StringBuilder();
                var result = this.Actions
                               .OrderByDescending(x => x.Movement.Task.Priority)
                               .Where(x =>
                                   checkActionStatus(x, sb)
                                   && ActionFilters.All((filter) =>
                                   {
                                       System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                                       sw.Start();
                                       var r = filter.Filter(this, x);
                                       sw.Stop();
                                       this.Log($"任务{x.Movement.Task}，物理动作{x},任务过滤器{filter.GetType()}过滤结果({r.Defeated + "," + r.Reason}),耗时{sw.ElapsedMilliseconds}ms");
                                       if (r.Defeated)
                                       {
                                           sb.AppendLine(string.Format("{0} 被 {1} 否决，原因是 {2}", x, filter, r.Reason));
                                       }

                                       return r.Defeated == false;
                                   })
                                   )
                               .ToList()
                               .OrderByDescending(x => x.Movement.Task.Priority)
                               .ThenBy(x => x.SequenceOrdering)
                               .ThenBy(x => x.Id);

                _GetAvailableActionsDescription = sb.ToString();

                return result;
            }
        }

        Boolean checkActionStatus(EquipmentAction act, StringBuilder sb)
        {
            if (act.Id == 0)
            {
                sb.AppendLine(string.Format("{0} id 为 0", act));
                return false;
            }
            //该代码设计是在任务创建时修复，但在完成时会被意外调用。
            //            if (act.SupportTransactionObjectStatus != SupportTransactionObjectStatus.Committed 
            //                && act.Status==EquipmentActionStatus.New
            //                && DateTime.Now.Subtract(act.CreatedAt).TotalSeconds>60)
            //            {
            //#warning 以下这段有问题
            //                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.Suppress))
            //                {
            //                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            //                    {
            //                        if (unitOfWork.session.Get<EquipmentAction>(act.Id) != null)
            //                        {
            //                            _logger.Warn1(string.Format("{0} SupportTransactionObjectStatus 为 {1}，预期值应为 {2}，但数据库中已存在该动作。准备修正状态为 {2}。", act, act.SupportTransactionObjectStatus.GetDescription(), SupportTransactionObjectStatus.Committed.GetDescription()), this, act);
            //                            act.SupportTransactionObjectStatus = SupportTransactionObjectStatus.Committed;
            //                            _logger.Warn1(string.Format("{0} SupportTransactionObjectStatus 变更为 {1}。", act, act.SupportTransactionObjectStatus.GetDescription()), this, act);

            //                            unitOfWork.Commit();
            //                            ts.Complete();
            //                        }
            //                        else
            //                        {
            //                            sb.AppendLine(string.Format("{0} SupportTransactionObjectStatus 为 {1}，预期值应为 {2}", act, act.SupportTransactionObjectStatus.GetDescription(), SupportTransactionObjectStatus.Committed.GetDescription()));

            //                            unitOfWork.Commit();
            //                            ts.Complete();

            //                            return false;
            //                        }
            //                    }
            //                }
            //            }

            String reason;
            if (!act.CanPerform(out reason))
            {
                sb.AppendLine(reason);
                return false;
            }

            return true;
        }

        void onEquipmentActionStatusChanged(Events.EquipmentActionStatusChangedEvent args)
        {
            EquipmentAction action;
            lock (_actions)
            {
                action = _actions.SingleOrDefault(x => x.Id == args.Id);
            }

            if (action != null)
            {
                if (action.Status == args.Status)
                {
                    return;
                }

                if ((action.Status == EquipmentActionStatus.Completed && args.Status != EquipmentActionStatus.Completed)
                    ||
                    (action.Status == EquipmentActionStatus.Cancelled && args.Status != EquipmentActionStatus.Cancelled)
                    )
                {
                    _logger.Warn1(String.Format("收到一个{0}状态变化为“{1}”的事件，但发现其在序列池中状态为“{2}”，不再更改池中副本状态。", action, args.Status, action.Status), this, action);
                }
                else
                {
                    _logger.Debug1(String.Format("序列池中的{0}状态由“{1}”改变为“{2}”", action, action.Status, args.Status), this, action);
                    action.Status = args.Status;
                }
            }

            if (this.Device.AllowConcurrency)
            {
                return;
            }

            lock (_currentActionLocker)
            {
                if (CurrentAction == null)
                {
                    return;
                }

                if (CurrentAction.Id != args.Id)
                {
                    return;
                }

                if ((CurrentAction.Status == EquipmentActionStatus.Completed && args.Status != EquipmentActionStatus.Completed)
                   ||
                   (CurrentAction.Status == EquipmentActionStatus.Cancelled && args.Status != EquipmentActionStatus.Cancelled)
                   )
                {
                    _logger.Warn1(String.Format("收到一个当前动作{0}状态变化为“{1}”的事件，但发现其在序列池中状态为“{2}”，不再更改池中副本状态。", CurrentAction, args.Status, action.Status), this, CurrentAction);
                }
                else
                {
                    _logger.Debug1(String.Format("序列池中的当前动作{0}状态由“{1}”改变为“{2}”", CurrentAction, CurrentAction.Status, args.Status), this, CurrentAction);
                    CurrentAction.Status = args.Status;
                }
            }


            //if (action == null)
            //{
            //    //防止出现像双货叉那种调度程序，生成一些占位任务。
            //    //如果此处不处理，将会出现这样的任务在完成后动作序列无法清空当前任务的情况。
            //    using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            //    {
            //        action = unitOfWork.session.Query<EquipmentAction>()
            //            .FirstOrDefault(x => x.Id == args.Id && x.DeviceName == this.Device.Name);
            //        unitOfWork.Commit();
            //    }
            //}

            //if (action == null || action.DeviceName != this.Device.Name)
            //{
            //    return;
            //}

            //action.Status = args.Status;

            //if (this.Device.AllowConcurrency)
            //{
            //    return;
            //}

            //lock (_currentActionLocker)
            //{
            //    //修复因为任务下发超时，然后设备又收到任务执行，导致的动作序列当前动作未被登记的BUG
            //    if (this.CurrentAction == null && action.Status != EquipmentActionStatus.Cancelled && action.Status != EquipmentActionStatus.Completed)
            //    {
            //        if (_FixUnknowEquipmentTaskOnStatusChanged)
            //        {
            //            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            //            {
            //                var scheduler = unitOfWork.session.Get<EquipmentActionScheduler>(this.DeviceName);
            //                var oldCurrentAction = scheduler.CurrentAction;
            //                scheduler.CurrentAction = unitOfWork.session.Get<EquipmentAction>(action.Id, NHibernate.LockMode.None);

            //                unitOfWork.Commit();

            //                this.CurrentAction = action;
            //                EventBus.EventBus.Instance.Publish(new SchedulerCurrentActionChangedEvent(this, oldCurrentAction, action));

            //                this._logger.Warn1(string.Format("修复{0}当前动作由{1}修改为{2}。这通常是由于任务发送超时或发送过程中Wcs被关闭导致的。", this, oldCurrentAction, this.CurrentAction), this, this.CurrentAction);

            //            }
            //        }
            //    }
            //    else
            //    {
            //        if (this.CurrentAction != null && this.CurrentAction.Id == args.Id)
            //        {
            //            this.CurrentAction.Status = args.Status;
            //        }

            //        if (this.CurrentAction != null && (this.CurrentAction.Status == EquipmentActionStatus.Cancelled || this.CurrentAction.Status == EquipmentActionStatus.Completed))
            //        {
            //            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            //            {
            //                var scheduler = unitOfWork.session.Get<EquipmentActionScheduler>(this.DeviceName);

            //                if ((scheduler.CurrentAction != null && scheduler.CurrentAction.Id == this.CurrentAction.Id) || scheduler.CurrentAction == null)
            //                {
            //                    //此处使用CurrentAction==null 判断，主要是为了修正有一些Action在该属性被重置前被删，导致 nhibernate 加载对象时为 null
            //                    //而数据库中的值（id引用）却一直存在。
            //                    this._logger.Trace1(string.Format("当前动作由{0}修改为null。", scheduler.CurrentAction), this, this.CurrentAction);

            //                    unitOfWork.session.Delete(scheduler);

            //                    var newScheduler = new EquipmentActionScheduler();
            //                    newScheduler.DeviceName = this.Device.Name;

            //                    unitOfWork.session.Save(newScheduler);

            //                    unitOfWork.session.Flush();
            //                }
            //                unitOfWork.Commit();

            //            }

            //            this._logger.Info1(string.Format("{0}当前动作由{1}修改为{2}。", this, this.CurrentAction, "<null>"), this, this.CurrentAction);

            //            this.CurrentAction = null;
            //            EventBus.EventBus.Instance.Publish(new SchedulerCurrentActionChangedEvent(this, this.CurrentAction, null));
            //        }
            //    }
            //}
        }

        void onTaskArchived(TaskArchivedEvent args)
        {
            List<EquipmentAction> actions;
            lock (_actions)
            {
                actions = _actions.Where(x => x.Movement.Task.Id == args.Id).ToList();
            }

            while (actions.Count > 0)
            {
                Remove(actions[0]);

                actions.RemoveAt(0);
            }
        }

        void onTaskCurrentLocationChanged(TaskCurrentLocationChangedEvent args)
        {
            List<EquipmentAction> actions;
            lock (_actions)
            {
                actions = _actions
                    .Where(x => x.Movement != null && x.Movement.Task != null)
                    .Where(x => x.Movement.Task.Id == args.Id && !x.Movement.Task.CurrentLocation.Equals(args.CurrentLocation)).ToList();
            }

            while (actions.Count > 0)
            {
                actions.First().Movement.Task.CurrentLocation = args.CurrentLocation;
                actions = actions.Where(x => x.Movement.Task.Id == args.Id && !x.Movement.Task.CurrentLocation.Equals(args.CurrentLocation)).ToList();
            }
            lock (_currentActionLocker)
            {
                if (this.CurrentAction != null && this.CurrentAction.Movement != null && this.CurrentAction.Movement.Task != null && this.CurrentAction.Movement.Task.Id == args.Id)
                {
                    this.CurrentAction.Movement.Task.CurrentLocation = args.CurrentLocation;
                }
            }
        }

        public override string ToString()
        {
            return string.Format("{0}的物理动作调度程序", this.DeviceName);
        }
    }
}
