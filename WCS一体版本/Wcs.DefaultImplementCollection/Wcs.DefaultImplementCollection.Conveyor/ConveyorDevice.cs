using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Wcs.Framework;
using System.Threading;
using NHibernate.Linq;
using System.Linq.Expressions;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    [System.ComponentModel.Description("输送线")]
    public sealed class ConveyorDevice : TcpProtocolTaskableDevice
    {
        NetPacket lastReceivedNetPacket = null;
        _DB2 lastReceivedDB2 = null;
        static Random _rnd = new Random();

        List<IUsedConveyorTaskBlockIndexGetter> _usedConveyorTaskBlockIndexGetters = new List<IUsedConveyorTaskBlockIndexGetter>();

        #region Properties
        /// <summary>
        /// 获取从设备状态数据缓存中读取到的货位当前任务列表
        /// </summary>
        public LocationInfoBlock[] LocationInfoBlocks
        {
            get
            {
                return ReadStatus<LocationInfoBlock>();
            }
        }
        /// <summary>
        /// 获取从设备状态数据缓存中读取到的任务列表
        /// </summary>
        public TaskBlock[] TaskBlocks
        {
            get
            {
                return ReadStatus<TaskBlock>();
            }
        }
        /// <summary>   
        /// 获取从设备状态数据缓存中读取到的占位信号. 
        /// </summary>
        public RequestBlock[] RequestBlocks
        {
            get
            {
                return ReadStatus<RequestBlock>();
            }
        }
        /// <summary>
        /// 数据对比器<br />
        /// 该对象用于对接收到的数据进行对比。在外部可用处理该对象的关事件已达到跟踪数据变化的目的。
        /// </summary>
        public IDataComparer<NetTransferObject, _DB2> DataComparer { get; private set; }
        /// <summary>
        /// 允许被WCS系统使用的任务块大小（从1到后xx的索引地址）
        /// </summary>
        public Int32 AllowTaskBlockSpace { get; set; }
        /// <summary>
        /// 缓冲区数据对比结果处理程序集合
        /// </summary>
        public ICompareResultHandler[] CompareResultHandlers { get; private set; }
        /// <summary>
        /// 输送线命令外挂补丁程序
        /// </summary>
        public ConveyorEquipmentActionToAddCommandPlugin ConveyorEquipmentActionToAddCommandPlugin { get; set; }
        #endregion

        #region Events

        /// <summary>
        /// 在任务需要二次握手时发生
        /// </summary>
        public event DeviceEventHandler<ConveyorDevice, TaskConfirmEventArgs> TaskConfirm;
        #endregion,

        public ConveyorDevice(string name, Int32 no, IPEndPoint ipEndPoint, IPEndPoint bindEndPoint, Int32 receiveTimeout, Int32 connectTimeout, Int32 sendTimeout, Boolean allowConcurrency, IDataReceiver dataReceiver, Int32 allowTaskBlockSpace, ICompareResultHandler[] compareResultHandlers, IUsedConveyorTaskBlockIndexGetter[] usedConveyorTaskBlockIndexGetters)
            : base(name, no, receiveTimeout, connectTimeout, sendTimeout, allowConcurrency, ipEndPoint, bindEndPoint, dataReceiver)
        {
            this.DataComparer = new DefaultTcpConveyorDataComparer();
            this.AllowTaskBlockSpace = allowTaskBlockSpace;
            this.CompareResultHandlers = compareResultHandlers;
            if (this.CompareResultHandlers == null)
            {
                this.CompareResultHandlers = new ICompareResultHandler[0];
            }

            //添加默认的已使用的输送线设备任务地址索引获取器
            if (!_usedConveyorTaskBlockIndexGetters.Any(x => x.GetType() == typeof(DefaultUsedConveyorTaskBlockIndexGetter)))
            {
                _usedConveyorTaskBlockIndexGetters.Add(new DefaultUsedConveyorTaskBlockIndexGetter());
            }

            if (usedConveyorTaskBlockIndexGetters != null && usedConveyorTaskBlockIndexGetters.Length > 0)
            {
                _usedConveyorTaskBlockIndexGetters.AddRange(usedConveyorTaskBlockIndexGetters);
            }

            Disconnected += ConveyorDevice_Disconnected;

            //增加一个任务完成信号检测（有时任务完成时信号采集失败，有可能导致完成事件丢失）
            var _thread = new System.Threading.Thread(AutoFireTaskCompletedEvents);
            _thread.Name = String.Format("{0}的任务完成状态检查线程", name);
            _thread.IsBackground = true;
            _thread.StartAndManaged();

        }

        void ConveyorDevice_Disconnected(Device device, DisconnectEventArgs args)
        {
            args.Handled = true;

            //断线后清理缓冲区数据
            lastReceivedNetPacket = null;
            lastReceivedDB2 = null;
        }

        #region Implement TcpProtocolTaskableDevice

        /// <summary>
        /// ConveyorDevice 不再支持此方法，请使用 ReadStatus&lt;TState&gt;() 方法代替
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <returns></returns>
        public override TState Read<TState>()
        {
            throw new NotSupportedException(string.Format("{0} 不支持本方法调用，请使用 ReadStatus<TState>() 方法代替。", this.GetType()));
        }

        protected override void OnDataReceived(NetPacket netPacket, NetTransferObject netTransferObject)
        {
            if (!(netTransferObject is _DB2))
            {
                this._logger.Warn1(string.Format("接收到未处理的数据类型 {0}", netTransferObject.GetType()), this);
                return;
            }

            _DB2 db2 = (_DB2)netTransferObject;
            if (netPacket == lastReceivedNetPacket)
            {
                lastReceivedDB2 = db2;
                return;
            }

            if (lastReceivedDB2 != null && db2 != null)
            {
                CompareResult<NetTransferObject>[] compareResult = this.DataComparer.Compare(lastReceivedDB2, db2);
                if (compareResult != null && compareResult.Length > 0)
                {
                    //引发事件前先将缓冲区数据更新掉，否则可能引起其它进程中访问到脏数据
                    lastReceivedNetPacket = netPacket;
                    lastReceivedDB2 = db2;

                    //saveReceivedDataLog(compareResult);
                    //saveReceivedDataLogToFile(compareResult);
                    FireEvents(compareResult);
                }
            }
            else if (lastReceivedDB2 == null && db2 != null)
            {
                //引发事件前先将缓冲区数据更新掉，否则可能引起其它进程中访问到脏数据
                lastReceivedNetPacket = netPacket;
                lastReceivedDB2 = db2;

                //saveReceivedDataLog(db2);
                //saveReceivedDataLogToFile(db2);
                FireEvents(db2);
            }
            lastReceivedNetPacket = netPacket;
            lastReceivedDB2 = db2;
        }

        public override IsIdleResult IsIdle
        {
            get
            {
                if (this.Locker != null && !this.Locker.IsEmpty)
                    return new IsIdleResult(false, "WCS锁不为空");

                if (!this.IsConnected)
                    return new IsIdleResult(false, "设备未连接");

                if (this.TaskBlocks == null || this.TaskBlocks.Length == 0)
                    return new IsIdleResult(false, "PLC上报任务块 TaskBlocks == null || TaskBlocks.Length == 0");

                return new IsIdleResult(true, "");
            }
        }

        public override string[] Warnings
        {
            get
            {
                List<string> result = new List<string>();
                if (this.TaskBlocks == null)
                {
                    result.Add("任务缓冲块未同步");
                }

                if (LocationInfoBlocks == null)
                {
                    result.Add("货位当前信息块未同步");
                }

                if (this.Locker != null && !this.Locker.IsEmpty)
                {
                    result.Add(string.Format("设备被 {0} 远程锁定", this.Locker));
                }

                return result.ToArray();
            }
        }

        public override int[] OccupiedEquipmentTasks
        {
            get
            {
                if (this.TaskBlocks == null)
                {
                    return null;
                }

                return this
                    .TaskBlocks
                    .Where(x => x.TaskNo > 0)
                    .Select(x => Convert.ToInt32(x.TaskNo))
                    .ToArray();
            }
        }

        public override IDeviceUserInterface CreateUserInterface()
        {
            return new ConveyorDeviceUserInterface();
        }

        #endregion

        #region Public Methods
        public _DB2 GetLastStatus()
        {
            return lastReceivedDB2;
        }

        public TState[] ReadStatus<TState>()
            where TState : NetTransferObject
        {
            if (lastReceivedDB2 == null)
            {
                return new TState[0];
            }
            else
            {
                return lastReceivedDB2.Get<TState>();
            }
        }


        public void AcceptLocation(params ConveyorLocation[] locations)
        {
            if (locations == null)
            {
                throw new ArgumentNullException("locations");
            }

            if (locations.Any(x => x.Device != null && x.Device != this))
            {
                throw new InvalidOperationException("有已被分配给其它设备的位置对象，而这些对象并不允许修改所属设备。");
            }

            //var invalidLocations = locations.Where(x => !(x is ILocationWildcard)).Intersect(this.Locations.Where(x => !(x is ILocationWildcard)));
            //if (invalidLocations.Any())
            //{
            //    throw new InvalidOperationException(String.Format("位置 {0} 在 {1} 中已存在", string.Join(",", invalidLocations.Select(x => x.ToString()).ToArray()), this));
            //}

            //invalidLocations = locations.Where(x => x is ILocationWildcard).Intersect(this.Locations.Where(x => x is ILocationWildcard));
            //if (invalidLocations.Any())
            //{
            //    throw new InvalidOperationException(String.Format("通配符位置 {0} 在 {1} 中已存在", string.Join(",", invalidLocations.Select(x => x.ToString()).ToArray()), this));
            //}

            foreach (var loc in locations)
            {
                if (_locations.Any(x => x.ToConvertibleCode() == loc.ToConvertibleCode()))
                {
                    throw new InvalidOperationException(String.Format("位置 {0} 在 {1} 中已存在", loc, this));
                }
            }

            _locations.AddRange(locations);
        }

        #endregion

        #region Private Methos
        Dictionary<uint, DateTime> ConfirmTaskList = new Dictionary<uint, DateTime>();

        void FireTaskConfirmEvent(TaskConfirmEventArgs args)
        {
            if (!ConfirmTaskList.ContainsKey(args.TaskBlock.TaskNo))
            {
                this.Log($"首次引发二次确认事件 {args.TaskBlock.TaskNo} {args.TaskBlock.HandShake} {args.TaskBlock.TaskState}");
                ConfirmTaskList.Add(args.TaskBlock.TaskNo, DateTime.Now);
                this.DeviceEventQueue.Enqueue(this.TaskConfirm, this, args);
            }
            else if (ConfirmTaskList.ContainsKey(args.TaskBlock.TaskNo) && DateTime.Now.Subtract(ConfirmTaskList[args.TaskBlock.TaskNo]).TotalSeconds > 5)
            {
                this.Log($"5秒后重复引发二次确认事件 {args.TaskBlock.TaskNo} {args.TaskBlock.HandShake} {args.TaskBlock.TaskState}");
                ConfirmTaskList[args.TaskBlock.TaskNo] = DateTime.Now;
                this.DeviceEventQueue.Enqueue(this.TaskConfirm, this, args);
            }
            else
                this.Log($"未能重复引发二次确认事件 {args.TaskBlock.TaskNo} {args.TaskBlock.HandShake} {args.TaskBlock.TaskState}");

            var taskIds = ReadStatus<TaskBlock>().Where(x => x.TaskNo != 0).Select(x => x.TaskNo).ToList();
            var _taskIds = ConfirmTaskList.Keys.Where(x => !taskIds.Contains(x));
            foreach (var item in _taskIds)
            {
                ConfirmTaskList.Remove(item);
            }
        }

        void FireEvents(CompareResult<NetTransferObject>[] _compareResults)
        {
            var changedTasks = _compareResults.Where(x => x.IsTypeOf(typeof(TaskBlock)));
            var statusChangedTasks = changedTasks
                .Where(x => x.differences.Any(differece => differece.propertyName == "HandShake" || differece.propertyName == "TaskState"));
            foreach (var task in statusChangedTasks.Where(x => ((TaskBlock)x.newObject).TaskState == TaskBlockTaskStatus.Error))
            {
                TaskBlock newObject = (TaskBlock)task.newObject;
                FireTaskErrorEvent(new TaskErrorEventArgs(Convert.ToInt32(newObject.TaskNo), newObject.TaskState.ToString(), "任务不能执行"));

                Wcs.Framework.MessageBoard.AbstractMessageBoard.Instance.Add(
                    new Wcs.Framework.MessageBoard.Messages.TipMessage(Framework.MessageBoard.MessageLevel.Error,
                        this.Name,
                        String.Format("任务 {0} 下发后不能执行。", newObject.TaskNo), null));
            }

            foreach (var task in statusChangedTasks.Where(x => ((TaskBlock)x.newObject).TaskState == TaskBlockTaskStatus.Finished))
            {
                TaskBlock newObject = (TaskBlock)task.newObject;
                FireTaskCompletedEvent(new TaskCompletedEventArgs(Convert.ToInt32(newObject.TaskNo)));
            }

            foreach (var task in statusChangedTasks.Where(x => ((TaskBlock)x.newObject).TaskState == TaskBlockTaskStatus.Running))
            {
                TaskBlock newObject = (TaskBlock)task.newObject;
                FireTaskRunningEvent(new TaskRunningEventArgs(Convert.ToInt32(newObject.TaskNo)));
            }

            var locationChangedTasks = _compareResults.Where(x => x.IsTypeOf(typeof(LocationInfoBlock)))
                                    .Where(x => x.differences.Any(differece => differece.propertyName == "TaskNo"));

            foreach (var task in locationChangedTasks.Where(x => ((LocationInfoBlock)x.newObject).TaskNo != 0))
            {
                LocationInfoBlock state = (LocationInfoBlock)task.newObject;
                var location = this.Locations.SingleOrDefault(x => x.DeviceCode == state.PosNo.ToString());
                if (location == null)
                {
                    _logger.Warn1(string.Format("{0} 不存在位置 {1}", this, state.PosNo), this, state);
                }
                else
                {
                    FireTaskCurrentLocationChangedEvent(new Framework.TaskCurrentLocationChangedEventArgs(Convert.ToInt32(state.TaskNo), location));
                }
            }

            foreach (var handler in this.CompareResultHandlers)
            {
                handler.Handle(this, _compareResults);
            }
        }

        void FireEvents(_DB2 db2)
        {
            foreach (var task in db2.Get<TaskBlock>().Where(x => x.TaskState == TaskBlockTaskStatus.Error))
            {
                FireTaskErrorEvent(new TaskErrorEventArgs(Convert.ToInt32(task.TaskNo), task.TaskState.ToString(), "任务不能执行"));

                Wcs.Framework.MessageBoard.AbstractMessageBoard.Instance.Add(
                    new Wcs.Framework.MessageBoard.Messages.TipMessage(Framework.MessageBoard.MessageLevel.Error,
                        this.Name,
                        String.Format("任务 {0} 下发后不能执行。", task.TaskNo), null));
            }

            foreach (var task in db2.Get<TaskBlock>().Where(x => x.TaskState == TaskBlockTaskStatus.Finished))
            {
                FireTaskCompletedEvent(new TaskCompletedEventArgs(Convert.ToInt32(task.TaskNo)));
            }

            foreach (var task in db2.Get<TaskBlock>().Where(x => x.TaskState == TaskBlockTaskStatus.Running))
            {
                FireTaskRunningEvent(new TaskRunningEventArgs(Convert.ToInt32(task.TaskNo)));
            }

            foreach (var state in db2.Get<LocationInfoBlock>().Where(x => x.PosNo != 0 && x.TaskNo != 0))
            {
                var location = this.Locations.SingleOrDefault(x => x.DeviceCode == state.PosNo.ToString());
                if (location == null)
                {
                    _logger.Warn1(string.Format("{0} 不存在位置 {1}", this, state.PosNo), this, state);
                }
                else
                {
                    FireTaskCurrentLocationChangedEvent(new Framework.TaskCurrentLocationChangedEventArgs(Convert.ToInt32(state.TaskNo), location));
                }
            }

            foreach (var handler in this.CompareResultHandlers)
            {
                handler.Handle(this, db2);
            }
        }
        #endregion

        static Object _setTaskBlockIndexLocker = new object();

        /// <summary>
        /// 为指定的动作分配一个PLC任务存储索引地址（从1开始）
        /// </summary>
        /// <typeparam name="TEquipmentAction">泛型参数。物理动作类型</typeparam>
        /// <param name="act">要分配任务存储索引地址的物理动作</param>
        /// <param name="setValuesAct">因为不同的类型在数据库中存储任务索引地址的字段也不一样，所以这里需要手动设置一下</param>
        /// <returns></returns>
        public Int32 SetTaskBlockIndex<TEquipmentAction, TField>(TEquipmentAction act, System.Linq.Expressions.Expression<Func<TEquipmentAction, TField>> property)
            where TEquipmentAction : EquipmentAction
        {
            lock (_setTaskBlockIndexLocker)
            {
                if (this.TaskBlocks == null || this.TaskBlocks.Length == 0)
                {
                    throw new InvalidOperationException(String.Format("{0} 的任务数据未同步。", this));
                }

                var body = property.Body;
                if (body.NodeType != System.Linq.Expressions.ExpressionType.MemberAccess)
                {
                    throw new InvalidOperationException("必须是从字段或属性进行读取的运算，如 obj.SampleProperty。");
                }

                System.Linq.Expressions.MemberExpression memberExpression = (System.Linq.Expressions.MemberExpression)body;
                if (memberExpression.Member.MemberType != System.Reflection.MemberTypes.Property)
                {
                    throw new InvalidOperationException("必须是从字段或属性进行读取的运算，如 obj.SampleProperty。");
                }

                System.Reflection.PropertyInfo propertyInfo = (System.Reflection.PropertyInfo)memberExpression.Member;

                if (act.Status != EquipmentActionStatus.New)
                {
                    throw new InvalidOperationException(string.Format("{0} 状态不为 New（当前为 {1}", act.Status));
                }

                var currentIndex = propertyInfo.GetValue(act, null);
                Int32 currentIndexValue = 0;
                if (!String.IsNullOrWhiteSpace(Convert.ToString(currentIndex)) && Convert.ToString(currentIndex) != "0")
                {
                    currentIndexValue = Convert.ToInt32(currentIndex);
                    _logger.Trace1(string.Format("在为 {0} 分配索引地址时发现该动作已分配过,值为 {1},准备验证地址在设备中的可用性...", act, currentIndex), this, act);
                    if (this.TaskBlocks.Length < currentIndexValue)
                    {
                        _logger.Warn1(string.Format("{0} 分配索引地址 {1} 超出了任务块大小，需要从新分配...", act, currentIndex), this, act);
                    }
                    else
                    {
                        if (this.TaskBlocks[currentIndexValue - 1].HandShake != TaskHandShakes.Empty && this.TaskBlocks[currentIndexValue - 1].TaskNo != act.EquipmentTaskId)
                        {
                            _logger.Warn1(string.Format("{0} 分配索引地址 {1} 非空（握手为 {2}），需要从新分配...", act, currentIndex, this.TaskBlocks[currentIndexValue - 1].HandShake.GetDescription()), this, act);

                            currentIndexValue = 0;
                        }
                        else
                        {
                            _logger.Debug1(string.Format("索引可用，不再重新分配.", act, currentIndex), this, act);
                        }
                    }
                }

                if (currentIndexValue == 0)
                {
                    _logger.Trace1(string.Format("准备为 {0} 分配的在设备数据块中存放的索引位置...", act), this, act);
                }
                else
                {
                    _logger.Trace1(string.Format("准备保存 {0} 的索引位置...", act), this, act);
                }

                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.Suppress))
                {
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                    {
                        Int32 index = 0;
                        if (currentIndexValue == 0)
                        {
                            index = GetFreeTaskBlockIndex();
                        }
                        else
                        {
                            index = currentIndexValue;
                        }
                        _logger.Debug1(string.Format("分配的索引位置为 {0}", index), this, act);
                        if (index <= 0)
                        {
                            throw new Exception(String.Format("未给 {0} 找到可用的标签地址", this));
                        }

                        var newStatusObject = unitOfWork.session.Get<TEquipmentAction>(act.Id, NHibernate.LockMode.Force);

                        propertyInfo.SetValue(newStatusObject, index, null);

                        unitOfWork.Commit();
                        ts.Complete();

                        //此内存对象的索引地址更新放在 Commit 之后，
                        //防止数据库操作超时在未保存到对象的情况下内存对象的 AtPlcDBIndex 被更改,而数据库对象未同步
                        //任务完成后数据库中的 AtPlcDBIndex 为空，出现无法报完成的情况

                        propertyInfo.SetValue(act, index, null);

                        _logger.Debug1(string.Format("成功更新 {0} 的索引位置 {1} 到数据库", this, index), this, act);

                        return index;
                    }
                }
            }
        }

        /// <summary>
        /// 从指定的输送线设备中获取一个空闲的可以存放任务的地址<br />
        /// 该方法返回的地址从 1 开始,与 PLC 中使用的索引地址相同。
        /// 该方法获取到的索引位置会被锁定，所以为了防止在编程时被错误的失败，将其密封在了输送线设备类中。
        /// </summary>
        protected Int32 GetFreeTaskBlockIndex()
        {
            lock (_setTaskBlockIndexLocker)
            {
                List<Int32> usedIndexs = new List<int>();
                foreach (var getter in _usedConveyorTaskBlockIndexGetters)
                {
                    var indexs = getter.GetUsedIndexs(this);
                    usedIndexs.AddRange(indexs);
                }

                //可用的任务区大小
                var taskblockSize = this.AllowTaskBlockSpace;
                if (taskblockSize > this.TaskBlocks.Length || taskblockSize <= 0)
                {
                    taskblockSize = this.TaskBlocks.Length;
                }

                //允许分配的索引列表
                int[] allIndexs = new int[taskblockSize];
                for (int i = 0; i < taskblockSize; i++)
                {
                    allIndexs[i] = i + 1;
                }

                if (allIndexs
                    .Except(usedIndexs)
                    .Count() == 0)
                {
                    throw new InvalidOperationException(String.Format("{0} 任务块已被完全分配使用，已无可用任务地址。", this));
                }

                for (UInt16 index = 1; index <= taskblockSize; index++)
                {
                    //已被分配占用
                    if (usedIndexs.Contains(index))
                    {
                        continue;
                    }

                    if (this.TaskBlocks == null || this.TaskBlocks.Length < index)
                    {
                        throw new Exception(String.Format("在分配地址时输送线任务块大小发生变化，超出索引边界。index {0},length {1}", index, this.TaskBlocks == null ? 0 : this.TaskBlocks.Length));
                    }

                    if (this.TaskBlocks[index - 1].HandShake == TaskHandShakes.Empty)
                    {
                        return index;
                    }
                }

                throw new Exception(String.Format("{0} 任务块已被写满，已无可用任务地址", this));
            }
        }

        void AutoFireTaskCompletedEvents()
        {
            Dictionary<UInt32, DateTime> _checkedValues = new Dictionary<UInt32, DateTime>();
            while (true)
            {
                System.Threading.Thread.Sleep(1000 * 5);

                try
                {
                    var items = ReadStatus<TaskBlock>();
                    if (items == null || items.Length == 0)
                    {
                        continue;
                    }

                    var completedTasks = items.Where(x =>
                            x.TaskState == TaskBlockTaskStatus.Finished
                            && x.TaskNo != 0
                        )
                        .ToArray();

                    if (completedTasks.Length == 0)
                    {
                        continue;
                    }

                    foreach (var tsk in completedTasks)
                    {
                        if (this.DeviceEventQueue.Any(x => x.Args is TaskCompletedEventArgs && ((TaskCompletedEventArgs)x.Args).EquipmentTaskId == tsk.TaskNo))
                        {
                            continue;
                        }

                        if (!_checkedValues.ContainsKey(tsk.TaskNo))
                        {
                            _checkedValues.Add(tsk.TaskNo, DateTime.Now);
                            continue;
                        }

                        if (DateTime.Now.Subtract(_checkedValues[tsk.TaskNo]).TotalSeconds > 30)
                        {
                            FireTaskCompletedEvent(new TaskCompletedEventArgs(Convert.ToInt32(tsk.TaskNo)));
                        }
                    }

                    foreach (var key in _checkedValues.Keys.ToArray())
                    {
                        if (DateTime.Now.Subtract(_checkedValues[key]).TotalMinutes > 10)
                        {
                            _checkedValues.Remove(key);
                        }
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
