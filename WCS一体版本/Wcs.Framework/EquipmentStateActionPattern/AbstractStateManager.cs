using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Wcs;
using Wcs.Framework;

namespace Wcs.Framework
{
    /// <summary>
    /// 状态上下文
    /// <para>1、恢复：在实例被创建后，将从尝试恢复已持久货的信息。</para>
    /// <para>2、保存：在 CurrentState 被赋值时将会被持久化。由于在创建实例时总是会对 CurrentState 进行赋值，所以在创建实例时总能被保存。</para>
    /// <para>3、删除：在上下文执行完成，并引发了 TaskCompleted 事件后，对象自动调用 Remove 方法彻底删除整个对象。</para>
    /// <para>4、加载(恢复所有已持久化的恢复信息)：需要开发人员编写针对性的代码。<br />
    ///可以将加载代码放在专门的启动程序当中，在其中加载所有需要恢复的状态对象。<br />
    ///这样的启动程序应该放在 Wcs 的程序程序之前，否则可能会导致在启动后某些对象已经恢复，此恢复代码又尝试重复恢复它而引发异常。
    ///</para>
    /// </summary>
    public abstract class AbstractStateManager : IDisposable
    {
        static List<AbstractStateManager> _instances = new List<AbstractStateManager>();
        public AbstractState _currentState;
        private Object _currentStateLocker = new object();
        public Boolean _IsDisposing = false;

        /// <summary>
        /// 初始化 <seealso cref="T:AbsoluteStateManager"/> 类的新实例。
        /// </summary>
        /// <param name="equipmentAction">物理动作</param>
        /// <param name="device">关联设备</param>
        protected AbstractStateManager(EquipmentAction equipmentAction, TaskableDevice device)
        {
            if (device != DeviceConverter.ToDevice<TaskableDevice>(equipmentAction.DeviceName))
            {
                throw new InvalidOperationException(string.Format("{0} 所属设备和当前上下文设备不一致", equipmentAction));
            }

            if (equipmentAction.Id == 0)
            {
                throw new InvalidOperationException(string.Format("{0} 未保存，当前无法使用。", equipmentAction));
            }

            lock (_instances)
            {
                if (_instances.Any(ctx => ctx.EquipmentAction.Id == equipmentAction.Id))
                {
                    throw new InvalidOperationException(string.Format("{0} 已存在一个状态上下文，无法重复创建。", equipmentAction));
                }
            }

            this._equipmentAction = equipmentAction;
            this.Device = device;

            BuildStateLink();

            StateLineVaild();

            Restore();

            if (CurrentState == null && this.EquipmentAction != null)
            {
                CurrentState = StateLink[0];
            }

            System.Threading.ThreadPool.QueueUserWorkItem((stat) =>
            {
                Proc();
            }, this);

            //订阅物理动作状态变化事件
            Wcs.Framework.EventBus.EventBus.Instance.Subscribe<Wcs.Framework.Events.EquipmentActionStatusChangedEvent>(OnEquipmentActionStatusChanged);

            lock (_instances)
            {
                //添加到实例队列
                _instances.Add(this);
            }
        }

        /// <summary>
        /// 在状态结束时发生
        /// </summary>
        public event StateFinishedEventHandler StateFinished;

        /// <summary>
        /// 当前所处状态（执行进度）
        /// </summary>
        public virtual AbstractState CurrentState
        {
            set
            {
                lock (_currentStateLocker)
                {
                    var oldState = _currentState;
                    try
                    {
                        _currentState = value;
                        Save();
                    }
                    catch
                    {
                        _currentState = oldState;
                        throw;
                    }
                }
            }
            get
            {
                lock (_currentStateLocker)
                {
                    return _currentState;
                }
            }
        }

        /// <summary>
        /// 获取当前上下文所属设备
        /// </summary>
        public virtual TaskableDevice Device { get; private set; }

        public EquipmentAction _equipmentAction = null;
        /// <summary>
        /// 物理动作
        /// </summary>
        public virtual EquipmentAction EquipmentAction
        {
            get
            {
                EquipmentAction _action;
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                {
                    _action = unitOfWork.session.Get<EquipmentAction>(_equipmentAction.Id);
                    unitOfWork.Commit();
                }
                return _action;
            }
            private set { }
        }

        /// <summary>
        /// 状态链
        /// </summary>
        public virtual ReadOnlyCollection<AbstractState> StateLink { get; protected set; }

        /// <summary>
        /// 获取日志对象
        /// </summary>
        /// <returns></returns>
        protected abstract NLog.Logger _logger { get; }

        /// <summary>
        /// 获取(如果已存在)或创建一个物理动作的状态上下文
        /// </summary>
        /// <typeparam name="T">泛型参数，上下文类型</typeparam>
        /// <param name="equipmentAction">物理动作</param>
        /// <param name="device">设备</param>
        /// <returns>状态上下文</returns>
        public static T CreateOrGetContext<T>(EquipmentAction equipmentAction, TaskableDevice device)
            where T : AbstractStateManager
        {
            lock (_instances)
            {
                var context = _instances.SingleOrDefault(x => x.EquipmentAction.Id == equipmentAction.Id);
                if (context == null)
                {
                    context = ReflectionHelper.CreateInstance<T>(typeof(T), equipmentAction, device);
                }

                return (T)context;
            }
        }

        /// <summary>
        /// 获取指定物理动作的状态上下文
        /// </summary>
        /// <param name="equipmentAction">物理动作</param>
        /// <returns>如果不存在将返回 null.</returns>
        public static AbstractStateManager GetContext(EquipmentAction equipmentAction)
        {
            lock (_instances)
            {
                return _instances.SingleOrDefault(x => x.EquipmentAction.Id == equipmentAction.Id);
            }
        }

        /// <summary>
        /// 获取所有状态上下文对象
        /// </summary>
        public static AbstractStateManager[] Contexts
        {
            get
            {

                lock (_instances)
                {
                    return _instances.ToArray();
                }
            }
        }

        /// <summary>
        /// 销毁对象。该操作只会清理内存对象，不涉及已持久货的恢复信息。
        /// </summary>
        public virtual void Dispose()
        {
            //正在销毁对象
            _IsDisposing = true;

            //取消事件订阅
            Wcs.Framework.EventBus.EventBus.Instance.Unsubscribe<Wcs.Framework.Events.EquipmentActionStatusChangedEvent>(OnEquipmentActionStatusChanged);

            lock (_instances)
            {
                //从实例队列中移除
                _instances.Remove(this);
            }
        }

        public override string ToString()
        {
            return string.Format("AbsoluteStateManager#{0}", EquipmentAction);
        }

        /// <summary>
        /// 创建状态链
        /// </summary>
        protected abstract void BuildStateLink();

        /// <summary>
        /// 引发任务完成事件
        /// </summary>
        protected virtual void FireTaskCompleteEvent()
        {
            var device = Wcs.Framework.DeviceConverter.ToDevice<TaskableDevice>(this.EquipmentAction.DeviceName);
            Type type = device.GetType();
            var FireTaskCompletedEventMethod = type.GetMethod("FireTaskCompletedEvent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (FireTaskCompletedEventMethod == null)
            {
                throw new MissingMethodException(string.Format("未找到名为 FireTaskCompletedEvent 的方法"));
            }
            FireTaskCompletedEventMethod.Invoke(device, new object[] { new Wcs.Framework.TaskCompletedEventArgs(this.EquipmentAction.EquipmentTaskId) });

            var _action = this.Device.EquipmentActionScheduler.Actions.FirstOrDefault(x => x.EquipmentTaskId == this.EquipmentAction.EquipmentTaskId);
            if (_action == null || _action.Status == EquipmentActionStatus.Cancelled || _action.Status == EquipmentActionStatus.Completed || _action.Status == EquipmentActionStatus.Error)
            {
                this.Remove();
            }
        }

        /// <summary>
        /// 引发任务异常完成事件
        /// </summary>
        protected virtual void FireTaskErrorCompleteEvent()
        {
            this.Remove();
        }

        /// <summary>
        /// 获取下一个状态
        /// </summary>
        /// <returns>
        /// <para>如果当前状态未完成，将直接返回当前状态。</para>
        /// <para>如果当前状态已完成，并且是状态链的末位，将返回NULL</para>
        /// </returns>
        protected virtual AbstractState GetNextState()
        {
            if (!CurrentState.IsCompleted().Result)
            {
                return CurrentState;
            }

            var index = StateLink.ToList().IndexOf(CurrentState);
            var nextIndex = index + 1;
            if (nextIndex >= StateLink.Count)
            {
                return null;
            }
            else
            {
                return StateLink[nextIndex];
            }
        }

        /// <summary>
        /// 是否完成
        /// </summary>
        public IsCompeltedResult ContextIsCompelted = new IsCompeltedResult(false, "-");
        /// <summary>
        /// 是否可以发送任务
        /// </summary>
        public CanPerformResult ContextCanPerform = new CanPerformResult(false, "-");

        /// <summary>
        /// 状态上下文的执行器处理过程
        /// </summary>
        protected virtual void Proc()
        {
            _logger.Debug1(string.Format("{0} 的执行器开始运行...", this), this, this.EquipmentAction);
            while (!_IsDisposing)
            {
                System.Threading.Thread.Sleep(100);

                try
                {
//#warning  有问题，导致状态链被释放
                    if (CurrentState.Context.EquipmentAction == null || this.EquipmentAction.Status == EquipmentActionStatus.Cancelled || this.EquipmentAction.Status == EquipmentActionStatus.Completed)
                    {
                        Dispose();
                        break;
                    }
                    ContextIsCompelted = CurrentState.IsCompleted();
                    if (ContextIsCompelted.Result)
                    {
                        _logger.Debug1(string.Format("{0} 已结束，开始引发状态结束事件...", CurrentState), this, this.EquipmentAction);
                        FireStateFinishedEvent();
                        _logger.Debug1(string.Format("{0} 状态结束事件引发成功.", CurrentState), this, this.EquipmentAction);

                        _logger.Debug1(string.Format("开始获取下一个状态..."), this);
                        var nextState = GetNextState();
                        _logger.Debug1(string.Format("获取到的下一个状态是 {0}", nextState), this, this.EquipmentAction);
                        if (nextState == null)
                        {
                            _logger.Debug1(string.Format("开始引发 {0} 的任务完成事件...", EquipmentAction), this, this.EquipmentAction);
                            FireTaskCompleteEvent();
                            _logger.Debug1(string.Format("{0} 的任务完成事件引发成功.", EquipmentAction), this, this.EquipmentAction);

                            var _action = this.Device.EquipmentActionScheduler.Actions.FirstOrDefault(x => x.EquipmentTaskId == this.EquipmentAction.EquipmentTaskId);
                            if (_action == null || _action.Status == EquipmentActionStatus.Cancelled || _action.Status == EquipmentActionStatus.Completed || _action.Status == EquipmentActionStatus.Error)
                            {
                                Dispose();
                                //状态链执行结束，执行器停止
                                break;
                            }
                        }
                        else if (nextState != CurrentState)
                        {
                            _logger.Debug1(string.Format("当前状态 {0} 已完成，下一个状态为 {1}，准备切换当前状态...", CurrentState, nextState), this, this.EquipmentAction);

                            if (this.EquipmentAction != null)
                            {
                                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                                {
                                    _equipmentAction = unitOfWork.session.Get<EquipmentAction>(this.EquipmentAction.Id);
                                    unitOfWork.Commit();
                                }

                                if (_equipmentAction != null && _equipmentAction.Status != EquipmentActionStatus.Cancelled && _equipmentAction.Status != EquipmentActionStatus.Completed)
                                    CurrentState = nextState;
                                else
                                {
                                    _logger.Debug1(string.Format("开始引发 {0} 的任务异常完成事件...", EquipmentAction), this, this.EquipmentAction);
                                    FireTaskErrorCompleteEvent();
                                    _logger.Debug1(string.Format("{0} 的任务异常完成事件引发成功.", EquipmentAction), this, this.EquipmentAction);
                                    break;
                                }
                            }
                            else
                            {
                                _logger.Debug1(string.Format("开始引发 {0} 的任务异常完成事件...", EquipmentAction), this, this.EquipmentAction);
                                FireTaskErrorCompleteEvent();
                                _logger.Debug1(string.Format("{0} 的任务异常完成事件引发成功.", EquipmentAction), this, this.EquipmentAction);
                                break;
                            }

                            _logger.Debug1(string.Format("当前状态被成功切换为 {0}.", nextState), this, this.EquipmentAction);
                        }
                    }
                    else
                    {
                        var isidle = this.Device.IsIdle;
                        if (!isidle.Result)
                        {
                            continue;
                        }

                        if (
                            this.EquipmentAction.Status != EquipmentActionStatus.Executing
                            && this.EquipmentAction.Status != EquipmentActionStatus.New
                            )
                        {
                            continue;
                        }

                        //需要做一个状态同步，否则堆垛机在取货报警后继续执行时 单步调度线程中的状态不能同步过来
                        StateManagerRestoreInfo _state;
                        using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                        {
                            _state = unitOfWork.session.Get<StateManagerRestoreInfo>(this.EquipmentAction.Id);
                            unitOfWork.Commit();
                        }
                        if (this.CurrentState.Name != _state.CurrentStateName)
                        {
                            var _stt = StateLink.ToList().First(x => x.Name == _state.CurrentStateName);
                            this.CurrentState = _stt;
                        }

                        ContextCanPerform = CurrentState.CanPerform();
                        if (ContextCanPerform.Result)
                        {
                            _logger.Debug1(string.Format("{0} 可以执行动作，开始调用 Perform 方法...", CurrentState), this, this.EquipmentAction);
                            CurrentState.Perform();
                            _logger.Debug1(string.Format("{0} 的 Perform 方法调用成功.", CurrentState), this, this.EquipmentAction);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error1(new Exception(string.Format("{0}，当前状态是 {1}", ex.Message, CurrentState), ex), this, this.EquipmentAction);
                }
            }

            _logger.Debug1(string.Format("{0} 的执行器已退出.", this), this, this.EquipmentAction);
        }

        /// <summary>
        /// 移除当前上下文信息（包括恢复信息）。该方法的实现当中应在最后调用 Dispose 方法，已清理内存中对象引用。
        /// </summary>
        public abstract void Remove();

        /// <summary>
        /// 从恢复信息(如果存在)中恢复当前上下文
        /// </summary>
        protected abstract void Restore();

        /// <summary>
        /// 保存当前上下文的恢复信息
        /// </summary>
        protected abstract void Save();
        /// <summary>
        /// 引发当前状态结束事件
        /// </summary>
        private void FireStateFinishedEvent()
        {
            if (StateFinished != null)
            {
                StateFinished(this, CurrentState);
            }
        }
        /// <summary>
        /// 侦听物理动作状态变化事件，并将变化后的状态同步到当前上下文的动作对象中
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnEquipmentActionStatusChanged(Wcs.Framework.Events.EquipmentActionStatusChangedEvent args)
        {
            if (args.Id != this.EquipmentAction.Id)
            {
                return;
            }

            this.EquipmentAction.Status = args.Status;
        }

        /// <summary>
        /// 验证当前状态链的有效性
        /// </summary>
        protected virtual void StateLineVaild()
        {
            if (this.StateLink.Count == 0)
            {
                throw new Exception("状态链的长度必须大于 0");
            }

            var groupingQuery = this.StateLink.GroupBy(x => x.Name);
            var invalidStates = groupingQuery.Where(x => x.Count() > 1);

            if (invalidStates.Count() > 0)
            {
                var msg = string.Join("\n",
                    invalidStates.Select(x => string.Join(",", x.Key, x.Select(stat => stat.GetType().Name).ToArray()))
                    .ToArray());

                throw new Exception(string.Format("以下状态名称存在多种类型(同名)：{0}", msg));
            }
        }
    }
}