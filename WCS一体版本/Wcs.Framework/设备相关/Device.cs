using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using NLog;
using NHibernate.Linq;
namespace Wcs.Framework
{
    /// <summary>
    /// 表示一个设备
    /// </summary>
    public abstract class Device : ThreadRunningLog, IHoldableDevice
    {
        /// <summary>
        /// 日志
        /// </summary>
        protected Logger _logger;

        static Logger _deviceClassLogger = LogManager.GetCurrentClassLogger();
        List<EquipmentFailure> _equipmentFailures;
        LockerInfo _Locker;
        #region Properties
        /// <summary>
        /// 连接设备时的超时时间
        /// </summary>
        public Int32 ConnectTimeout { get; private set; }

        /// <summary>
        /// 接收到的事件<br />
        /// 包含 HandleableEventArgs 事件数据的事件，可托管到事件队列中。
        /// 支持在处理方处理失败时重新触发。
        /// </summary>
        public DeviceEventQueue DeviceEventQueue { get; protected set; }

        /// <summary>
        /// 获取当前设备的故障集合
        /// </summary>
        public EquipmentFailure[] EquipmentFailures
        {
            get
            {
                lock (_equipmentFailures)
                {
                    return _equipmentFailures.ToArray();
                }
            }
        }

        /// <summary>
        /// 指示当前设备是否已连机
        /// </summary>
        public abstract Boolean IsConnected { get; }

        /// <summary>
        /// 指示设备当前是否处于空闲状态，即:能接受新的任务
        /// </summary>
        public abstract IsIdleResult IsIdle { get; }

        /// <summary>
        /// 锁信息，一般在设备被锁定的情况下，将不再允许发送联机任务（可根据各情况情况自行处理锁信息）
        /// </summary>
        public virtual LockerInfo Locker
        {
            get
            {
                if (_Locker == null)
                {
                    return LockerInfo.Empty;
                }

                return _Locker;
            }
            protected set
            {
                _Locker = value;
            }
        }

        /// <summary>
        /// 设备名称，即设备在系统内的标识
        /// </summary>
        public String Name { get; private set; }
        /// <summary>
        /// 设备编号
        /// </summary>
        public Int32 No { get; private set; }
        /// <summary>
        /// 网络数据接收超时时间
        /// </summary>
        public Int32 ReceiveTimeout { get; private set; }

        /// <summary>
        /// 指示无法连接到设备的异常应该在连接失败多少秒后引发
        /// <para>默认 600 秒。如果需要立即引发，可设置值为小于等于 0</para>
        /// </summary>
        public Int32 UnableToConnectErrorTimeout { get; set; }
        /// <summary>
        /// 当前设备的警告信息
        /// </summary>
        public abstract String[] Warnings { get; }
        #endregion

        #region Events

        EquipmentFailureChecker _equipmentFailureChecker;

        Object _equipmentFailureCheckerLocker = new object();

        /// <summary>
        /// 当设备被连接上时发生
        /// </summary>
        public event DeviceEventHandler<Device, ConnectedEventArgs> Connected;

        /// <summary>
        /// 当接收到数据时发生。此处的数据指的是一个完整的数据报。
        /// </summary>
        public event DeviceEventHandler<Device, DataReceivedEventArgs> DataReceived;

        /// <summary>
        /// 当设备发生报警时发生
        /// </summary>
        public event DeviceEventHandler<Device, DeviceWarningEventArgs> DeviceWarning;

        /// <summary>
        /// 当设备被断开连接时发生
        /// </summary>
        public event DeviceEventHandler<Device, DisconnectEventArgs> Disconnected;
        /// <summary>
        /// 在新的设备故障被创建后发生。
        /// </summary>
        public event DeviceEventHandler<Device, EquipmentFailureAddedEventArgs> EquipmentFailureAdded;

        /// <summary>
        /// 在一个设备故障被删除后发生。
        /// </summary>
        public event DeviceEventHandler<Device, EquipmentFailureRemovedEventArgs> EquipmentFailureRemoved;
        /// <summary>
        /// 设备故障状态检查器。如有必要，应重写该属性
        /// </summary>
        public virtual EquipmentFailureChecker EquipmentFailureChecker
        {
            get
            {
                lock (_equipmentFailureCheckerLocker)
                {
                    if (_equipmentFailureChecker == null)
                    {
                        _equipmentFailureChecker = new EquipmentFailureChecker(this);
                    }
                }

                return _equipmentFailureChecker;
            }
        }



        #endregion

        /// <summary>
        /// 默认构造函数
        /// </summary>
        /// <param name="name">设备名称</param>
        /// <param name="receiveTimeout">接收超时时间（毫秒）</param>
        /// <param name="connectTimeout">连接超时时间（毫秒）</param>
        public Device(string name, Int32 no, Int32 receiveTimeout, Int32 connectTimeout)
        {
            this.Init($"{name}日志");
            this._logger = _deviceClassLogger;

            this._logger.Trace1(string.Format("准备初始化名称为 {0} 的 {1} 对象", name, this.GetType()), this);
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentOutOfRangeException("name", "必须为设备指定一个 name 值");
            }
            if (receiveTimeout <= 0)
            {
                throw new ArgumentOutOfRangeException("receiveTimeout", "必须大于 0");
            }

            if (connectTimeout < 500)
            {
                throw new ArgumentOutOfRangeException("connectTimeout", "必须大于等于 500");
            }

            this.Name = name;
            this.No = no;
            this.ReceiveTimeout = receiveTimeout;
            this.ConnectTimeout = connectTimeout;
            this.UnableToConnectErrorTimeout = 600;
            this.DeviceEventQueue = new DeviceEventQueue(this);
            _equipmentFailures = new List<EquipmentFailure>();

            this._logger.Trace1(string.Format("开始加载锁设备信息..."), this);
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                var locker = unitOfWork.session.Get<DeviceLockerInformation>(this.Name);
                if (locker != null)
                {
                    this.Locker = locker.LockerInfo;
                    this._logger.Trace1(string.Format("找到锁 {0},保持成功", locker), this);
                }
                else
                {
                    this._logger.Trace1(string.Format("未找到锁 {0}", locker), this);
                }

                unitOfWork.Commit();
            }
            this._logger.Trace1(string.Format("设备信息加载结束"), this);
            this._logger.Info1(string.Format("初始化完成"), this);

            this.EquipmentFailureChecker.Start();
        }

        #region Public Methos
        /// <summary>
        /// 立即连接到设备
        /// </summary>
        /// <returns>成功后返回 true，失败 返回 false</returns>
        public abstract Boolean Connect();

        /// <summary>
        /// 创建设备用户界面并返回
        /// </summary>
        /// <returns></returns>
        public abstract IDeviceUserInterface CreateUserInterface();

        /// <summary>
        /// 断开与设备的连接
        /// </summary>
        public abstract void Disconnect();
        /// <summary>
        /// 手动引发无法连接的设备故障事件
        /// </summary>
        public virtual void FireUnableToConnectError()
        {
            //FireDeviceErrorEvent(new DeviceWarningEventArgs(this.DeviceWarningFactory.CreateUnableToConnectError(this)));
        }

        /// <summary>
        /// 锁定此设备
        /// </summary>
        /// <param name="newLocker">
        ///  新的目标锁信息.<br />
        ///  通常由用户创建建，该信息包含有用户信息和一个能识别锁的唯一标识.<br />
        ///  不能为 <see cref="T:Wcs.Framework.Devices.LockerInfo.Empty"/>.<br />
        ///</param>
        public virtual void Lock(LockerInfo newLocker)
        {
            if (newLocker.IsEmpty)
            {
                String msg = "锁信息不能为空";
                this._logger.Trace1(msg, this);

                throw new ArgumentOutOfRangeException(msg);
            }

            if (!Locker.IsEmpty)
            {
                String msg = string.Format("{0} 已存在 {1}", this, Locker);
                this._logger.Warn1(msg, this);

                throw new InvalidOperationException(msg);
            }

            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                var existsLocker = unitOfWork.session.Get<DeviceLockerInformation>(this.Name);
                if (existsLocker == null)
                {
                    DeviceLockerInformation deviceLockerInformation = new DeviceLockerInformation(this.Name, newLocker);
                    unitOfWork.session.Save(deviceLockerInformation);

                }
                else if (existsLocker != null && !existsLocker.LockerInfo.Equals(newLocker))
                {
                    String msg = string.Format("{0} 已存在 {1}", this, existsLocker.LockerInfo);
                    this._logger.Warn1(msg, this);

                    throw new InvalidOperationException(msg);
                }

                unitOfWork.Commit();
            }

            Locker = newLocker;

            this._logger.Info1(string.Format("{0} 被 {1} 锁定", this, newLocker), this, newLocker);

            MessageBoard.AbstractMessageBoard.Instance.Add(new MessageBoard.Messages.LockedMessage(this));
        }

        /// <summary>
        /// 解除锁定<br />
        /// 设备在被用户锁定后（Locker 不为 <see cref="T:Wcs.Framework.Devices.LockerInfo.Empty"/>）<br />
        /// 如果需要再次联机执行任务，用户必须调用此方法清除设备上的锁信息
        /// </summary>
        /// <param name="userLocker">
        ///   要解除的锁任务<br />
        ///   事实上这是一个包含有调用户用户数据的对对象，用于核对该用户是否有权限清除此设备上已存在的锁信息.
        /// </param>
        public virtual void Unlock(LockerInfo userLocker)
        {
            if (Locker.IsEmpty)
            {
                return;
            }

            if (Locker.Equals(userLocker) || userLocker.Equals(LockerInfo.Adminstrator))
            {
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    var existsLocker = unitOfWork.session.Get<DeviceLockerInformation>(this.Name);
                    unitOfWork.session.Delete(existsLocker);
                    unitOfWork.Commit();
                }

                this._logger.Info1(string.Format("{0} 锁信息 {1} 被 {2} 解除", this, Locker, userLocker), this, userLocker);

                Locker = LockerInfo.Empty;

                MessageBoard.AbstractMessageBoard.Instance.Add(new MessageBoard.Messages.TipMessage(MessageBoard.MessageLevel.Info, this.Name, "已解除锁定", null));
            }
            else
            {
                throw new InvalidOperationException(String.Format("{0}({1}) 正在尝试解除 {2}", userLocker.UserName, userLocker.IPAddress, Locker));
            }
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// 引发 <see cref="E:Wcs.Framework.Device.Connected"/> 事件
        /// </summary>
        protected void FireConnectedEvent()
        {
            if (this.Connected != null)
            {
                this.Connected(this, new ConnectedEventArgs());
            }
        }

        /// <summary>
        /// 引发 <see cref="E:Wcs.Framework.Device.ReceiveData"/> 事件
        /// </summary>
        /// <param name="args">事件数据</param>
        protected void FireDataReceivedEvent(DataReceivedEventArgs args)
        {
            if (this.DataReceived != null)
            {
                this.DataReceived(this, args);
            }
        }

        /// <summary>
        /// 引发 <see cref="E:Wcs.Framework.Device.DeviceError"/> 事件
        /// </summary>
        /// <param name="args">事件数据</param>
        protected void FireDeviceErrorEvent(DeviceWarningEventArgs args)
        {
            if (this.DeviceWarning != null)
            {
                this.DeviceWarning(this, args);
            }
        }
        /// <summary>
        /// 引发 <see cref="E:Wcs.Framework.Device.Disconnected"/> 事件
        /// </summary>
        protected void FireDisconnectedEvent(DisconnectEventArgs args)
        {
            if (this.Disconnected != null)
            {
                //堆垛机时断时连问题处理2015-12-29 罗培胤
                var invocationList = this.Disconnected.GetInvocationList();
                foreach (var dgt in invocationList)
                {
                    if (!this.DeviceEventQueue.Any(x => x.Delegate == dgt && x.Sender == this))
                    {
                        this.DeviceEventQueue.Enqueue(this.Disconnected, this, args);
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// 引发 <see cref="E:Wcs.Framework.Device.EquipmentFailureAdded"/> 事件
        /// </summary>
        protected void FireEquipmentFailureAddedEvent(EquipmentFailureAddedEventArgs args)
        {
            if (this.EquipmentFailureAdded != null)
            {
                this.DeviceEventQueue.Enqueue(this.EquipmentFailureAdded, this, args);
            }
        }
        /// <summary>
        /// 引发 <see cref="E:Wcs.Framework.Device.EquipmentFailureAdded"/> 事件
        /// </summary>
        protected void FireEquipmentFailureRemovedEvent(EquipmentFailureRemovedEventArgs args)
        {
            if (this.EquipmentFailureRemoved != null)
            {
                this.DeviceEventQueue.Enqueue(this.EquipmentFailureRemoved, this, args);
            }
        }

        /// <summary>
        /// 添加一个警告
        /// </summary>
        /// <param name="warning"></param>
        protected void AddWarning(DeviceWarning warning)
        {
            WarningPool.AddWarning(this.Name, this.GetDeviceType(), warning);
        }

        /// <summary>
        /// 结束一个警告
        /// </summary>
        /// <param name="args"></param>
        protected void EndingWarning()
        {
            WarningPool.EndingWarning(this.Name, this.GetDeviceType());
        }
        #endregion

        #region overrides
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("{0}", this.Name);
        }
        #endregion

        #region IHoldableDevice
        IDeviceHolder _holder;
        object _holdLocker = new object();

        public IDeviceHolder Holder
        {
            get { return _holder; }
        }
        public void Hold(IDeviceHolder holder)
        {
            lock (_holdLocker)
            {
                //_logger.Debug1(string.Format("{0} 尝试持有 {1}...", holder, this), this);
                if (_holder != null)
                {
                    throw new InvalidOperationException(string.Format("{0} 已被 {1} 持有，在其被释放之前其它对象无法获取控制权", this, _holder));
                }

                this._holder = holder;
                //_logger.Debug1(string.Format("持有成功", this, _holder), this);
            }
        }

        public void Unhold(IDeviceHolder holder)
        {
            lock (_holdLocker)
            {
                //_logger.Debug1(string.Format("准备释放持有对象 {0}", _holder, this), this);
                if (holder != _holder)
                {
                    throw new InvalidOperationException(string.Format("{0} 正在尝试释放 {1} 对 {2} 的持有", holder, _holder, this));
                }
                _holder = null;
                //_logger.Debug1(string.Format("释放成功", _holder, this), this);
            }
        }
        #endregion

        #region EquipmentFailure

        /// <summary>
        /// 添加一个设备故障
        /// </summary>
        /// <param name="failure">设备故障</param>
        public void AddFailure(EquipmentFailure failure)
        {
            lock (_equipmentFailures)
            {
                if (_equipmentFailures.Any(x => x.Name == failure.Name))
                {
                    _logger.Trace1(string.Format("{0} 已存在 {1} 的故障，本故障不登记.", this.Name, failure.Name), this);
                    return;
                }

                _equipmentFailures.Add(failure);

                FireEquipmentFailureAddedEvent(new EquipmentFailureAddedEventArgs(failure));

                _logger.Trace1(string.Format("{0} 新增了 {1} 故障.", this, failure.Name), this);
            }
        }

        /// <summary>
        /// 移除一个设备故障
        /// </summary>
        /// <param name="failure">设备故障</param>
        public void RemoveFailure(EquipmentFailure failure)
        {
            lock (_equipmentFailures)
            {
                _equipmentFailures.Remove(failure);

                FireEquipmentFailureRemovedEvent(new EquipmentFailureRemovedEventArgs(failure));

                _logger.Trace1(string.Format("{0} 移除了 {1} 故障（开始于 {2:yyyy-MM-dd HH:mm:ss.fff}，结束于 {3:yyyy-MM-dd HH:mm:ss.fff}).", this, failure.Name, failure.CreatedAt, failure.FinishedAt), this);
            }
        }
        #endregion

        /// <summary>
        /// 获取当前设备类型名称
        /// </summary>
        /// <returns></returns>
        public virtual String GetDeviceType()
        {
            return this.GetType().GetDisplayName();
        }
    }
}
