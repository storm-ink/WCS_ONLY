using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Impl;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 表示一个堆垛机
    /// </summary>
    public class Crane:Device
    {
        /// <summary>
        /// 最后一条任务的发送时间
        /// </summary>
        public DateTime? LastTaskSendAt { get; protected set; }
        /// <summary>
        /// 最后一次收到的报文内容.
        /// </summary>
        String PreviousReceivedDataCache { get; set; }
        /// <summary>
        /// 获取设备类型.
        /// </summary>
        /// <value>
        /// <see cref="T:Wcs.Framework.Devices.DeviceType"/>.<see cref="F:Wcs.Framework.Devices.DeviceType.Crane"/>
        /// </value>
        public override DeviceType DeviceType
        {
            get 
            {
                return DeviceType.Crane;
            }
        }
        /// <summary>
        /// 获取或设备此设备所有能到的货架位置集合
        /// </summary>
        public RackLocation[] Locations { get; set; }
        /// <summary>
        /// 在任务的当前位置（容器被带到的位置）发生变化时发生.
        /// </summary>
        public event CraneTaskCurrentLocationChangedEventHandler TaskCurrentLocationChanged;
        /// <summary>
        /// 堆垛机连接代理对象
        /// </summary>
        CraneControl.Crane _craneProxy;
        /// <summary>
        /// 获取该设备使用的状态数据对比器.
        /// </summary>
        public DefaultCranStatusDataComparer StatusDataComparer { get; private set; }
        /// <summary>   
        ///  构造函数.
        /// </summary>
        /// <param name="deviceName">       设备名称. </param>
        /// <param name="ip">               远程地址. </param>
        /// <param name="port">             端口. </param>
        /// <param name="receiveTimeout">   接收超时（在指定的毫秒内如果未接收到来自设备的数据，连接将断开）. </param>
        /// <param name="connectTimeout">   连接超时. </param>
        /// <param name="sendTimeout">      数据发送超时. </param>
        /// <param name="deviceNo">         设备编号. </param>
        /// <param name="receiverDecoder">  作为接收方使用的数据包解码器. </param>
        /// <param name="senderDecoder">    作为发送方使用的数据包解码器. </param>
        /// <param name="logTarget">        日志输出对象. </param>
        /// <param name="useRemoteCraneWorkModeKeyInfo">指示是否启用堆垛机远程控制盒</param>
        public Crane(string deviceName, string ip, Int32 port, Int32 receiveTimeout, Int32 connectTimeout, Int32 sendTimeout, Int32 deviceNo, LogTarget logTarget, bool useRemoteCraneWorkModeKeyInfo)
            : base(deviceName, ip, port, receiveTimeout, connectTimeout, sendTimeout, deviceNo, logTarget)
        {
            this.UseRemoteCraneWorkModeKeyInfo = useRemoteCraneWorkModeKeyInfo;
            if (!this.UseRemoteCraneWorkModeKeyInfo)
            {
                UpdateCraneWorkModeKeyInfo(new CraneWorkModeKeyInfo
                {
                    CraneNo =Convert.ToUInt16(deviceNo),
                    IsBrake=false,
                    IsManual=false
                });
            }
            this._craneProxy = Wcs.Framework.CraneControl.CraneArray.Get(deviceName);
            this._craneProxy.IPCrane = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(this.Ip), this.Port);
            this._craneProxy.CraneDevice = this;
            this.Locations = new RackLocation[] { };
            this.StatusDataComparer = new DefaultCranStatusDataComparer();
            this.PreviousReceivedDataCache = null;
            _craneProxy.EClosed += (sender, args) =>
            {
                this.Logger.Warning(String.Format("{0} 内部代理已断开", this), this);
                if (args != null)
                {
                    OnDisconnected(new DeviceDisconnectEventArgs(args.Reason));
                }
                else
                {
                    OnDisconnected(new DeviceDisconnectEventArgs(DeviceDisconnectReason.Error));
                }
            };
            _craneProxy.EConnected += (sender, args) =>
            {
                this.Logger.Info(String.Format("{0} 内部代理已连接", this), this);
                OnConnected();
            };
            _craneProxy.EResponseTelex += (sender, args) =>
            {
                if (args.Telex == null)
                {
                    this.StatusInfo = null;
                    this.UpdateStatus(null);
                    return;
                }

                if (args.Telex is CraneControl.LA)
                {
                    this.StatusInfo = new CraneStatusInfo(args.Telex as CraneControl.LA);
                    this.StatusInfo.CraneWorkModeKeyInfo = this.CraneWorkModeKeyInfo;
                    this.StatusInfo._CraneProxyStatus.CraneWorkModeKeyInfo = this.CraneWorkModeKeyInfo;
                    this.UpdateStatus(null);
                }
                else
                {
                    this.Logger.Warning(String.Format("收到了未处理的报文 {0}", args.Telex), this, args.Telex);
                }
            };

            this.StatusDataComparer.DataChanged += (compareResult) =>
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in compareResult.differences)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(", ");
                        sb.Append(item.ToReadableDescription());
                    }
                    else
                    {
                        sb.Append(item.ToReadableDescription());
                    }
                }

                this.Logger.Info(sb.ToString(), this, compareResult);
                sb = null;
            };
        }

        /// <summary>
        /// 判断指定的对象是否和本对象相等。
        /// </summary>
        /// <param name="obj">指定的要比较的对象</param>
        /// <returns>如果指定的 Object 等于当前的对象，则为 true；否则为 false。 </returns>
        public override bool Equals(object obj)
        {
            Crane crane = obj as Crane;
            if (crane == null) return false;

            return string.Equals(crane.DeviceName, this.DeviceName, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// 空闲必须满足以下所有条件<br />
        /// 1、堆垛机状态同步成功<br />
        /// 2、状态的 IsBusy 属性 为 False<br />
        /// 3、离最后一条任务的发送时间超过 300 毫秒<br />
        /// </summary>
        public override bool IsIdle
        {
            get
            {
                if (this.Locker != null && !this.Locker.IsEmpty)
                {
                    return false;
                }

                if (LastTaskSendAt != null && DateTime.Now.Subtract(LastTaskSendAt.Value).TotalMilliseconds <= 300)
                {
                    return false;
                }

                return this.StatusInfo != null && !this.StatusInfo.IsBusy;
            }
        }
        /// <summary>
        /// 指示是否启用堆垛机远程控制盒
        /// </summary>
        public Boolean UseRemoteCraneWorkModeKeyInfo { get; private set; }

        /// <summary>
        /// 指示堆垛机当前是否正处于报警状态
        /// </summary>
        public bool Warning
        {
            get
            {
                if(StatusInfo==null)return true;

                return (!string.IsNullOrWhiteSpace(StatusInfo.ErrorCode) && StatusInfo.ErrorCode!="0000") || StatusInfo.Status==CraneStatus.Error || StatusInfo.Event==CraneEvent.EmergencyStopped;
            }
        }

        /// <summary>
        /// 指示当前堆垛机是否处于停止状态
        /// </summary>
        public bool Stopped
        {
            get
            {
                if (StatusInfo == null) return false;

                return StatusInfo.Event == CraneEvent.Completed
                    || StatusInfo.Event == CraneEvent.CompletedWithError
                    || StatusInfo.Event == CraneEvent.EmergencyStopped
                    || StatusInfo.Event == CraneEvent.Initialized
                    || StatusInfo.Status == CraneStatus.Initialized
                    || StatusInfo.Status == CraneStatus.Manual
                    || StatusInfo.Status == CraneStatus.Waiting;
            }
        }


        Boolean isFirstLoad = false;
        /// <summary>
        /// 更新设备当前状态信息.
        /// </summary>
        /// <param name="lastReceivedStatus"> 最后收到的报文数据. </param>
        protected override void UpdateStatus(Devices.NetPackage lastReceivedStatus)
        {
            if (this.StatusInfo == null)
            {
                PreviousReceivedDataCache = null;
                ExecuteUpdateStatusActions(this.StatusInfo);
                return;
            }

            isFirstLoad = PreviousReceivedDataCache == null;

            UInt32 taskId;
            UInt32.TryParse(StatusInfo.CurrentTaskId, out taskId);
            #region 执行上一次未处理的数据
            foreach (var item in this.DelayedDeviceStatusChangedNotify.Get(DelayedDeviceStatusChangedNotify.TASK_ERROR))
            {
                OnTaskError(Convert.ToUInt32(item.Args[0]), Convert.ToString(item.Args[1]), Convert.ToString(item.Args[2]));
            }

            foreach (var item in this.DelayedDeviceStatusChangedNotify.Get(DelayedDeviceStatusChangedNotify.TASK_COMPLETED))
            {
                OnTaskCompleted(Convert.ToUInt32(item.Args[0]));
            }

            foreach (var item in this.DelayedDeviceStatusChangedNotify.Get(DelayedDeviceStatusChangedNotify.TASK_RUNNING))
            {
                OnTaskRunning(Convert.ToUInt32(item.Args[0]));
            }

            foreach (var item in this.DelayedDeviceStatusChangedNotify.Get("TaskCurrentLocationChanged"))
            {
                OnTaskCurrentLocationChanged(Convert.ToUInt32(item.Args[0]),(CraneCurrentLocation)item.Args[1]);
            }
            #endregion
            if (!isFirstLoad)
            {
                #region 状态变化
                var _compareResult = this.StatusDataComparer.Compare(this.StatusInfo);
                if (_compareResult != null)
                {
                    foreach (var compareResult in _compareResult)
                    {
                        if (this.Warning && taskId > 0
                            && (
                                (
                                compareResult.differences.Any(x => x.propertyName == "ErrorCode")
                                && (!string.IsNullOrWhiteSpace(this.StatusInfo.ErrorCode) && this.StatusInfo.ErrorCode != "0000")
                                )
                                ||
                                (
                                this.StatusInfo.Status==CraneStatus.Error
                                && compareResult.differences.Any(x => x.propertyName == "Status")
                                )
                            ))
                        {
                            OnTaskError(taskId, StatusInfo.ErrorCode, StatusInfo.ErrorDescription);
                        }

                        if (
                            (
                                compareResult.differences.Any(x => x.propertyName == "ErrorCode")
                                && (!string.IsNullOrWhiteSpace(this.StatusInfo.ErrorCode) && this.StatusInfo.ErrorCode != "0000")
                            )
                            ||
                            (
                                this.StatusInfo.Status == CraneStatus.Error
                                && compareResult.differences.Any(x => x.propertyName == "Status")
                            )
                            )
                        {
                            OnDeviceError(StatusInfo.ErrorCode, StatusInfo.ErrorDescription);
                        }

                        if ((StatusInfo.Event == CraneEvent.Completed || StatusInfo.Event == CraneEvent.CompletedWithError) && taskId > 0 && compareResult.differences.Any(x => x.propertyName == "Event"))
                        {
                            OnTaskCompleted(taskId);
                        }

                        if ((StatusInfo.Status == CraneStatus.Running || StatusInfo.Status == CraneStatus.Loading || StatusInfo.Status == CraneStatus.Unloading) && taskId > 0 && compareResult.differences.Any(x => x.propertyName == "CurrentTaskId"))
                        {
                            OnTaskRunning(taskId);
                        }

                        if (taskId > 0
                            && StatusInfo.CurrentPosition != null
                            && StatusInfo.CurrentPosition.ColumnDeviceCode != null
                            && StatusInfo.CurrentPosition.ColumnUserCode != null
                            && compareResult.differences.Any(x => x.propertyName == "CurrentPosition")
                            )
                        {
                            OnTaskCurrentLocationChanged(taskId, StatusInfo.CurrentPosition);
                        }
                    }
                }
                #endregion
            }
            else
            {
                #region 第一次加载
                if (this.Warning && taskId > 0)
                {
                    OnTaskError(taskId, StatusInfo.ErrorCode, StatusInfo.ErrorDescription);
                }

                if (this.Warning)
                {
                    OnDeviceError(StatusInfo.ErrorCode, StatusInfo.ErrorDescription);
                }

                if ((StatusInfo.Event == CraneEvent.Completed || StatusInfo.Event == CraneEvent.CompletedWithError) && taskId > 0)
                {
                    OnTaskCompleted(taskId);
                }

                if ((StatusInfo.Status == CraneStatus.Running || StatusInfo.Status == CraneStatus.Loading || StatusInfo.Status == CraneStatus.Unloading) && taskId > 0)
                {
                    OnTaskRunning(taskId);
                }

                if (taskId > 0
                    && StatusInfo.CurrentPosition != null
                    && StatusInfo.CurrentPosition.ColumnDeviceCode != null
                    && StatusInfo.CurrentPosition.ColumnUserCode != null
                    )
                {
                    OnTaskCurrentLocationChanged(taskId, StatusInfo.CurrentPosition);
                }
                #endregion
            }
            ExecuteUpdateStatusActions(this.StatusInfo);
            PreviousReceivedDataCache = this.StatusInfo._CraneProxyStatus.Text ;
        }
        /// <summary>
        /// 获取或设置堆垛机远程控制盒信息
        /// </summary>
        public CraneWorkModeKeyInfo CraneWorkModeKeyInfo { get; set; }
        /// <summary>
        /// 更新当前堆垛机的远程控制盒信息.
        /// </summary>
        /// <param name="craneWorkModeKeyInfo"> 新的堆垛机远程控制盒信息. </param>
        public void UpdateCraneWorkModeKeyInfo(CraneWorkModeKeyInfo craneWorkModeKeyInfo)
        {
            CraneWorkModeKeyInfo = craneWorkModeKeyInfo;
            //如果远程急停，则向设备发送急停命令
            if (craneWorkModeKeyInfo.IsBrake && this.IsConnected)
            {
                _craneProxy.SendHE();
            }
        }
        /// <summary>
        /// 获取当前堆垛机最后收到的状态信息
        /// </summary>
        public CraneStatusInfo StatusInfo { get; private set; }
        /// <summary>
        /// 向设备发送一个任务.
        /// </summary>
        /// <param name="action">   要发送给设备的物理动作. </param>
        public override void SendTask(EquipmentAction action)
        {
            if (!this.IsIdle)
            {
                throw new InvalidOperationException(String.Format("{0} 当前状态不可发送任务", this));
            }

            var telex = action.ToEquipmentTask();
            if (telex is CraneControl.HB)
            {
                CraneControl.HB hb = (CraneControl.HB)action.ToEquipmentTask();

                _craneProxy.SendHB(hb);

                LastTaskSendAt = DateTime.Now;
            }
            else
            {
               throw new Exception(string.Format("不能识别的命名类型 {0}",telex.GetType()));
            }
        }
        /// <summary>
        /// 立即连接到设备.
        /// </summary>
        public override bool Connect()
        {
            this.Logger.Info(String.Format("{0} 开始尝试连接到 {1}:{2}...", this, Ip, Port), this);
            try
            {
                _craneProxy.Connect(this.ConnectTimeout);

                OnConnected();

                this.ConnectionRetries = 0;

                this.Logger.Info(String.Format("{0} 连接成功", this), this);

                if (this.Locker != null && !this.Locker.IsEmpty)
                {
                    _craneProxy.LockGuid = this.Locker.Id;
                    _craneProxy.LockUser = this.Locker.IPAddress;
                }
                else
                {
                    _craneProxy.LockGuid = null;
                    _craneProxy.LockUser = null;
                }

                return true;
            }
            catch (Exception ex)
            {
                this.ConnectionRetries += 1;
                String msg = String.Format("{0} 连接失败，原因：{1}", this, ex.Message);
                this.Logger.Warning(msg, this);

                OnDisconnected(new DeviceDisconnectEventArgs(DeviceDisconnectReason.Error));
                return false;
            }
        }
        /// <summary>
        /// 断开与设备的连接.
        /// </summary>
        public override void Disconnect()
        {
            _craneProxy.Disconnect();

            this.Logger.Warning(String.Format("{0} 被强制断开连接", this), this);

            OnDisconnected(new DeviceDisconnectEventArgs(DeviceDisconnectReason.User));
        }
        /// <summary>
        /// 判断一个物理动作是否已成功发送给了设备.
        /// </summary>
        /// <param name="action">   被发送的物理动作. </param>
        public override bool TaskSendSuccessfully(EquipmentAction action)
        {
            if (this.StatusInfo == null) return false;

            return this.StatusInfo.CurrentTaskId == action.EquipmentTaskId.ToString("00000000");
        }
        /// <summary>
        /// 指示当前设备是否已连机.
        /// </summary>
        public override bool IsConnected
        {
            get 
            {
                return _craneProxy.Enabled && _craneProxy.Connected;
            }
        }
        /// <summary>
        /// 取消一个设备正在执行的任务.<br />
        /// <font color="red">不支持该方法</font>
        /// </summary>
        /// <param name="subtask">  要取消的物理动作. </param>
        public override void CancelTask(EquipmentAction subtask)
        {
            //throw new NotImplementedException();
        }
        /// <summary>
        /// 判断一个物理动作是否已成功从设备任务列表中取消.
        /// </summary>
        /// <param name="action">   被取消的物理动作. </param>
        public override bool TaskCancelSuccessfully(EquipmentAction action)
        {
            if (this.StatusInfo == null) return false;

            return this.StatusInfo.IsMotionless;
        }

        private void OnTaskCurrentLocationChanged(UInt32 taskid, CraneCurrentLocation currentLocation)
        {

            if (TaskCurrentLocationChanged != null)
            {
                Boolean handled = false; 
                try
                {
                    TaskCurrentLocationChanged.Invoke(this, taskid, currentLocation);
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex.Message, this, ex);
                    this.Logger.Warning(string.Format("在处理 {0} 时发生错误:\r\n{1}", currentLocation, ex.Message), this, currentLocation);
                }
                finally
                {
                    if (!handled)
                    {
                        this.DelayedDeviceStatusChangedNotify.Add("TaskCurrentLocationChanged", null, taskid, currentLocation);
                    }
                    else
                    {
                        this.DelayedDeviceStatusChangedNotify.Remove("TaskCurrentLocationChanged", null, taskid, currentLocation);
                    }
                }
            }
        }
        /// <summary>
        /// 当前设备的警告信息.
        /// </summary>
        public override string[] Warnings
        {
            get 
            {
                List<string> result = new List<string>();
                if (this.StatusInfo == null)
                {
                    result.Add("设备状态未获取");
                }
                if (this.StatusInfo != null)
                {
                    if ((!string.IsNullOrWhiteSpace(StatusInfo.ErrorCode) && StatusInfo.ErrorCode != "0000"))
                    {
                        result.Add(string.Format("处于报警状态 {0},{1}",StatusInfo.ErrorCode,StatusInfo.ErrorDescription));
                    }
                    if (StatusInfo.CraneWorkModeKeyInfo == null)
                    {
                        result.Add(string.Format("远程控制盒状态未获取"));
                    }
                    else
                    {
                        if (StatusInfo.CraneWorkModeKeyInfo.IsBrake)
                        {
                            result.Add(string.Format("远程控制盒急停按钮被按下"));
                        }
                        if (StatusInfo.CraneWorkModeKeyInfo.IsManual)
                        {
                            result.Add(string.Format("远程控制盒已切换至手动模式"));
                        }
                    }

                    if (StatusInfo.Status == CraneStatus.Manual)
                    {
                        result.Add(string.Format("设备被切换为手动模式"));
                    }

                    if (!String.IsNullOrWhiteSpace(StatusInfo.LockId))
                    {
                        result.Add(string.Format("设备被 {0} 远程锁定",StatusInfo.LockUser));
                    }

                    if (StatusInfo.Event == CraneEvent.EmergencyStopped)
                    {
                        result.Add("急停事件被触发");
                    }

                    if (StatusInfo.Status == CraneStatus.Error)
                    {
                        result.Add("处于报警状态");
                    }

                    if (StatusInfo.Event == CraneEvent.Initialized && StatusInfo.Status == CraneStatus.Initialized)
                    {
                        result.Add("处于初始化状态，需要回原点后才能执行任务");
                    }
                }

                return result.ToArray();
            }
        }
        /// <summary>
        /// 锁定此设备.
        /// </summary>
        /// <param name="newLocker">    新的目标锁信息.<br />  通常由用户创建建，该信息包含有用户信息和一个能识别锁的唯一标识.<br />  
        ///                             不能为 LockerInfo.Empty.<br /> </param>
        public override void Lock(LockerInfo newLocker)
        {
            base.Lock(newLocker);
            if (_craneProxy != null)
            {
                _craneProxy.LockGuid = newLocker.Id;
                _craneProxy.LockUser = newLocker.IPAddress;
            }
        }
        /// <summary>
        /// 解除锁定<br />设备在被用户锁定后（Locker 不为 LockerInfo.Empty）<br />如果需要再次联机执行任务，用户必须调用此方法清除设备上的锁信息.
        /// </summary>
        /// <param name="userLocker">   
        ///     要解除的锁任务<br />  
        ///     事实上这是一个包含有调用户用户数据的对对象，用于核对该用户是否有权限清除此设备上已存在的锁信息. 
        /// </param>
        public override void Unlock(LockerInfo userLocker)
        {
            base.Unlock(userLocker);
            if (_craneProxy != null)
            {
                _craneProxy.LockGuid = null;
                _craneProxy.LockUser = null;
            }
        }
    }
}
