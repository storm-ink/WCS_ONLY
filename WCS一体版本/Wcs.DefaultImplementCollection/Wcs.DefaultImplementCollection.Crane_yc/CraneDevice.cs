using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Wcs.Framework;
using Wcs;
using System.Runtime.Remoting.Messaging;
using NHibernate.Linq;
using System.Windows.Forms;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    [System.ComponentModel.Description("堆垛机")]
    public class CraneDevice : TcpProtocolTaskableDevice, IEditableTaskOwner
    {
        protected System.Threading.Thread _requestStateCommandThreadProcThread;

        public String ConveyorDeviceName;
        /// <summary>
        /// 获取当前堆垛机的工作模式
        /// </summary>
        public virtual CraneDeviceWorkMode WorkMode
        {
            get
            {
                if (this.LastStatus == null)
                {
                    return CraneDeviceWorkMode.Manual;
                }

                if (this.LastStatus.State == CraneStatus.ManualMode)
                {
                    return CraneDeviceWorkMode.Manual;
                }

                return CraneDeviceWorkMode.Auto;
            }
        }
        /// <summary>
        /// 指示当堆垛机请求状态回报线程是否处于工作状态
        /// </summary>
        Boolean _requestStateCommandThreadIsRunning;
        Dictionary<String, RackLocation> locationsDictionary = new Dictionary<string, RackLocation>();
        RequestStateCommandReplyTelexTransferObject lastRequestStateCommandReplyTelexTransferObject = null;

        #region Properities
        /// <summary>
        /// 获取堆垛机最后回报的状态数据
        /// </summary>
        public RequestStateCommandReplyTelexTransferObject LastStatus
        {
            get
            {
                return lastRequestStateCommandReplyTelexTransferObject;
            }
            private set
            {
                lastRequestStateCommandReplyTelexTransferObject = value;
            }
        }

        #endregion

        #region Events
        /// <summary>
        /// 在发送的新任务命令被回复后发生
        /// </summary>
        public event DeviceEventHandler<CraneDevice, AddTaskCommandRepliedEventArgs> AddTaskCommandReplied;
        #endregion
        public CraneDevice(string name, Int32 no, Int32 receiveTimeout, Int32 connectTimeout, Int32 sendTimeout, IPEndPoint ipEndPoint, IPEndPoint bindEndPoint, IDataReceiver dataReceiver)
            : base(name, no, receiveTimeout, connectTimeout, sendTimeout, false, ipEndPoint, bindEndPoint, dataReceiver)
        {
            this.Connected += CraneDevice_Connected;
            this.Disconnected += CraneDevice_Disconnected;
        }

        #region Implement TcpProtocolTaskableDevice
        protected override void OnDataReceived(NetPacket netPacket, NetTransferObject netTransferObject)
        {
            if (netTransferObject is RequestStateCommandReplyTelexTransferObject)
            {
                if (lastRequestStateCommandReplyTelexTransferObject != null)
                {
                    var compareResult = lastRequestStateCommandReplyTelexTransferObject.Compare((RequestStateCommandReplyTelexTransferObject)netTransferObject);

                    FireEvents(compareResult);
                }
                else
                    FireEvents((RequestStateCommandReplyTelexTransferObject)netTransferObject);

                lastRequestStateCommandReplyTelexTransferObject = (RequestStateCommandReplyTelexTransferObject)netTransferObject;
            }
            else if (netTransferObject is AddTaskCommandReplyTelexTransferObject)
            {
                AddTaskCommandReplyTelexTransferObject cmdResult = (AddTaskCommandReplyTelexTransferObject)netTransferObject;
                this._logger.Debug1(String.Format("{0} 收到了任务 {1} 的确认信息 {2}({3})", this, cmdResult.TaskId, cmdResult.ToTelex(), cmdResult.Result.GetDescription()), this, cmdResult);
                int equipmentTaskId;
                if (int.TryParse(cmdResult.TaskId, out equipmentTaskId))
                {
                    FireAddTaskCommandRepliedEvent(new AddTaskCommandRepliedEventArgs(equipmentTaskId, cmdResult.Result));
                }
                else
                {
                    this._logger.Warn1(string.Format("收到了 {0} 命令，但未处理", cmdResult), this, cmdResult);
                }
            }
        }

        public override int[] OccupiedEquipmentTasks
        {
            get
            {
                if (this.LastStatus == null)
                {
                    return new int[0];
                }

                Int32 equipmentTaskId = 0;
                if (int.TryParse(this.LastStatus.TaskId, out equipmentTaskId))
                {
                    return new int[] { equipmentTaskId };
                }
                else
                {
                    return new int[0];
                }
            }
        }

        /// <summary>
        /// 取消堆垛机当前任务
        /// </summary>
        public virtual void CancleTask()
        {
            CancleCommand cmd = new CancleCommand();
            Write(cmd, cmd.SendSuccess);
        }
        public override void CancelTask(EquipmentAction action)
        {
            if (this.LastStatus == null || ParseEquipmentTaskId(this.LastStatus) != action.EquipmentTaskId)
            {
                return;
            }

            if (this.LastStatus.State != CraneStatus.AlarmAndShutdown && this.LastStatus.State != CraneStatus.无货待命 && this.LastStatus.State != CraneStatus.有货待命)
            {
                throw new NotImplementedException("非报警停机/无货待命/有货待命 状态的堆垛机不支持取消任务操作.");
            }
            //if (MessageBox.Show("是否需要发送堆垛机任务取消命令"))
            //{

            //}
            //CancleTask();
        }

        public override IsIdleResult IsIdle
        {
            get
            {
                if (this.Locker != null && !this.Locker.IsEmpty)
                    return new IsIdleResult(false, "WCS锁不为空");

                if (!this.IsConnected)
                    return new IsIdleResult(false, "设备未连接");

                var newObject = this.LastStatus;
                if (newObject == null)
                    return new IsIdleResult(false, "设备最新数据（LastStatus）为空");

                //设备处于手动模式
                if (newObject.State == CraneStatus.ManualMode)
                    return new IsIdleResult(false, "设备最新数据显示 State == CraneStatus.ManualMode");

                if (newObject.Event != CraneEvent.Initialized
                    && newObject.Event != CraneEvent.Finished
                    && newObject.Event != CraneEvent.CompletedWithError)
                    return new IsIdleResult(false, "设备最新数据显示 newObject.Event != CraneEvent.Initialized  && newObject.Event != CraneEvent.Finished && newObject.Event != CraneEvent.CompletedWithError");

                if (newObject.State != CraneStatus.Initialized
                    && newObject.State != CraneStatus.无货待命
                    && newObject.State != CraneStatus.有货待命
                    && newObject.State != CraneStatus.右探无货
                    && newObject.State != CraneStatus.右探有货
                    && newObject.State != CraneStatus.左探无货
                    && newObject.State != CraneStatus.左探有货
                    && newObject.State != CraneStatus.左无右无
                    && newObject.State != CraneStatus.左无右有
                    && newObject.State != CraneStatus.左有右无
                    && newObject.State != CraneStatus.左有右有)
                    return new IsIdleResult(false, "设备最新数据显示 newObject.State != CraneStatus.Initialized  && newObject.State != CraneStatus.无货待命 && newObject.State != CraneStatus.有货待命 && newObject.State != CraneStatus.右探无货 && newObject.State != CraneStatus.右探有货 && newObject.State != CraneStatus.左探无货 && newObject.State != CraneStatus.左探有货 && newObject.State != CraneStatus.左无右无 && newObject.State != CraneStatus.左无右有 && newObject.State != CraneStatus.左有右无 && newObject.State != CraneStatus.左有右有");

                if (!string.IsNullOrWhiteSpace(newObject.ErrorCode))
                    return new IsIdleResult(false, "设备最新数据显示 !string.IsNullOrWhiteSpace(newObject.ErrorCode)");

                //状态和事件都为初始化时说明堆垛机才上电未回原点
                if (newObject.Event == CraneEvent.Initialized && newObject.State == CraneStatus.Initialized)
                    return new IsIdleResult(false, "设备最新数据显示 newObject.Event == CraneEvent.Initialized && newObject.State == CraneStatus.Initialized");


                if (this.WorkMode == CraneDeviceWorkMode.Manual)
                    return new IsIdleResult(false, "设备最新数据显示 this.WorkMode == CraneDeviceWorkMode.Manual");

                if (this.EquipmentActionScheduler.CurrentAction == null && newObject.State == CraneStatus.有货待命)
                    return new IsIdleResult(false, "设备最新数据显示 this.EquipmentActionScheduler.CurrentAction == null && newObject.State == CraneStatus.有货待命");

                return new IsIdleResult(true, "");
            }
        }

        public override string[] Warnings
        {
            get
            {
                List<string> result = new List<string>();
                var newObject = this.LastStatus;
                if (newObject == null && this.IsConnected)
                {
                    result.Add("已连接但设备状态未获取");
                }
                if (newObject != null)
                {
                    if (newObject.ErrorCodeList != null && newObject.ErrorCodeList.Count(x => !string.IsNullOrWhiteSpace(x)) != 0)
                    {
                        var errorList = newObject.ErrorCodeList.Where(x => !string.IsNullOrWhiteSpace(x));
                        foreach (var item in errorList)
                        {
                            var alarm = DeviceErrorHelper.GetDeviceErrorFromErrorCode("堆垛机", item);
                            if (alarm == null)
                                result.Add(string.Format("处于报警状态 {0}_{1}", item, "未登记故障"));
                            else
                                result.Add(string.Format("处于报警状态 {0}_{1}", item, alarm.ErrorName));
                        }
                    }

                    if (newObject.State == CraneStatus.ManualMode)
                        result.Add(string.Format("设备被切换为手动模式"));

                    if (newObject.State == CraneStatus.Disconnected)
                        result.Add(string.Format("设备被切换为脱机状态"));

                    if (this.Locker != null && !this.Locker.IsEmpty)
                        result.Add(string.Format("设备被 {0} 远程锁定", this.Locker));


                    if (newObject.Event == CraneEvent.EmergencyStop)
                        result.Add("急停事件被触发");

                    if (newObject.State == CraneStatus.AlarmAndShutdown || newObject.State == CraneStatus.ResetAlarm)
                        result.Add(string.Format("处于 {0} 状态", newObject.State.GetDescription()));

                    if (newObject.Event == CraneEvent.Initialized && newObject.State == CraneStatus.Initialized)
                        result.Add("处于初始化状态，需要回原点后才能执行任务");

                    if (this.WorkMode == CraneDeviceWorkMode.Manual)
                        result.Add("设备处于手动模式");

                    if (this.EquipmentActionScheduler.CurrentAction == null && newObject.State == CraneStatus.有货待命)
                        result.Add("设备未执行任务，但载货台有货");

                    //if (WithRemoteController)
                    //{
                    //    ConveyorDevice conveyorDevice;
                    //    try
                    //    {
                    //        conveyorDevice = DeviceConverter.ToDevice<ConveyorDevice>(ConveyorDeviceName);
                    //        var status = conveyorDevice.ReadStatus<WithRemoteControllerCraneDeviceNetTransferObject>();
                    //        if (status == null || status.Length == 0)
                    //            result.Add("手操盒所在输送线手操盒状态获取失败");

                    //        var state = status.SingleOrDefault(x => x.DeviceNo == this.No);
                    //        if (state == null)
                    //            result.Add("手操盒状态获取失败");
                    //        else
                    //        {
                    //            if (state.IsEmergency || state.IsManual)
                    //                result.Add("手操盒状态处于 急停/手动状态");
                    //        }
                    //    }
                    //    catch
                    //    {
                    //        result.Add("手操盒所在PLC配置异常");
                    //    }
                    //}
                }

                return result.ToArray();
            }
        }

        public override IDeviceUserInterface CreateUserInterface()
        {
            return new CraneDeviceUserInterface();
        }

        public override void Write<TData>(TData data, Func<TaskableDevice, TData, bool> isSuccess)
        {
            if (this.WorkMode == CraneDeviceWorkMode.Manual
                && !data.GetType().Name.Contains("EmergencyStopCommand")
                && !data.GetType().Name.Contains("CancleCommand")
                )
            //if (this.WorkMode == CraneDeviceWorkMode.Manual && !(data is EmergencyStopCommand) && !(data is CancelEmergencyStopCommand))
            {
                throw new InvalidOperationException("手动模式下无法进行除急停、取消急停、取消任务外的一切操作。");
            }

            base.Write<TData>(data, isSuccess);
        }
        #endregion

        #region Public Methods
        public virtual void AcceptLocation(params RackLocation[] locations)
        {
            if (locations == null)
            {
                throw new ArgumentNullException("locations");
            }

            if (locations.Any(x => x.Device != null && x.Device != this))
            {
                throw new InvalidOperationException("有已被分配给其它设备的位置对象，而这些对象并不允许修改所属设备。");
            }

            foreach (var loc in locations)
            {
                if (LocationIsExists(loc))
                {
                    throw new InvalidOperationException(String.Format("位置 {0} 在 {1} 中已存在", loc, this));
                }
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
            _locations.AddRange(locations);

            foreach (var item in locations)
            {
                locationsDictionary.Add(item.ToConvertibleCode(), item);
            }
        }

        /// <summary>
        /// 向堆垛机发送急停命令
        /// </summary>
        public virtual void EmergencyStop()
        {
            EmergencyStopCommand emergencyStopCommand = new EmergencyStopCommand();
            Write(emergencyStopCommand, emergencyStopCommand.SendSuccess);
        }

        /// <summary>
        /// 向堆垛机发送取消急停命令
        /// </summary>
        public virtual void CancelEmergencyStop(LockerInfo locker)
        {
            if (this.Locker != null && !this.Locker.IsEmpty && !this.Locker.Equals(locker))
            {
                throw new InvalidOperationException(string.Format("{0} 正处于锁定状态", this));
            }

            var newObject = this.LastStatus;
            if (newObject == null)
            {
                throw new InvalidOperationException(string.Format("{0} 状态未同步", this));
            }

            //设备处于手动模式
            if (newObject.State == CraneStatus.ManualMode)
            {
                throw new InvalidOperationException(string.Format("{0} 设备处于手动模式", this));
            }

            //if (newObject.Event != CraneEvent.Initialized
            //    && newObject.Event != CraneEvent.Finished
            //    && newObject.Event != CraneEvent.CompletedWithError
            //    && newObject.Event != CraneEvent.EmergencyStop
            //    && newObject.Event != CraneEvent.Finished
            //    )
            //{
            //    throw new InvalidOperationException(string.Format("{0} 设备的事件正处于 {1} 状态", this, newObject.Event.GetDescription()));
            //}

            //if (newObject.State != CraneStatus.Initialized
            //    && newObject.State != CraneStatus.无货待命
            //    && newObject.State != CraneStatus.有货待命
            //    && newObject.State != CraneStatus.AlarmAndShutdown
            //    && newObject.State != CraneStatus.ResetAlarm
            //    )
            //{
            //    throw new InvalidOperationException(string.Format("{0} 设备正处于 {1} 状态", this, newObject.State.GetDescription()));
            //}

            if (!this.IsConnected)
            {
                throw new InvalidOperationException(string.Format("未连接到 {0} 设备", this));
            }

            CancelEmergencyStopCommand cancelEmergencyStopCommand = new CancelEmergencyStopCommand();
            this.Write(cancelEmergencyStopCommand.GetBytes());
            //Write(cancelEmergencyStopCommand, cancelEmergencyStopCommand.SendSuccess);
        }

        /// <summary>
        /// 向堆垛机发送回原点命令
        /// </summary>
        /// <param name="locker">锁</param>
        public virtual void BackToTheOrigin(LockerInfo locker)
        {
            if (this.Locker != null && !this.Locker.IsEmpty && !this.Locker.Equals(locker))
            {
                throw new InvalidOperationException(string.Format("{0} 正处于锁定状态", this));
            }

            var newObject = this.LastStatus;
            if (newObject == null)
            {
                throw new InvalidOperationException(string.Format("{0} 状态未同步", this));
            }

            //设备处于手动模式
            if (newObject.State == CraneStatus.ManualMode)
            {
                throw new InvalidOperationException(string.Format("{0} 设备处于手动模式", this));
            }

            if (newObject.Event != CraneEvent.Initialized
                && newObject.Event != CraneEvent.Finished
                && newObject.Event != CraneEvent.CompletedWithError
                && newObject.Event != CraneEvent.EmergencyStop
                && newObject.Event != CraneEvent.Finished
                )
            {
                throw new InvalidOperationException(string.Format("{0} 设备的事件正处于 {1} 状态", this, newObject.Event.GetDescription()));
            }

            if (newObject.State != CraneStatus.Initialized
                && newObject.State != CraneStatus.无货待命
                && newObject.State != CraneStatus.右探无货
                && newObject.State != CraneStatus.右探有货
                && newObject.State != CraneStatus.左探无货
                && newObject.State != CraneStatus.左探有货
                && newObject.State != CraneStatus.左无右无
                && newObject.State != CraneStatus.左无右有
                && newObject.State != CraneStatus.左有右无
                && newObject.State != CraneStatus.左有右有
                && newObject.State != CraneStatus.有货待命
                && newObject.State != CraneStatus.AlarmAndShutdown
                && newObject.State != CraneStatus.ResetAlarm
                )
            {
                throw new InvalidOperationException(string.Format("{0} 设备正处于 {1} 状态", this, newObject.State.GetDescription()));
            }

            if (!this.IsConnected)
            {
                throw new InvalidOperationException(string.Format("未连接到 {0} 设备", this));
            }

            if (this.WorkMode == CraneDeviceWorkMode.Manual)
            {
                throw new InvalidOperationException(string.Format("{0} 设备处于手动模式", this));
            }

            BackToTheOriginCommand backToTheOriginCommand = new BackToTheOriginCommand();
            Write(backToTheOriginCommand, backToTheOriginCommand.SendSuccess);
            //Write(backToTheOriginCommand.GetBytes());
            ////Write(backToTheOriginCommand, (device, cmd) =>
            ////{
            ////    CraneDevice craneDevice = (CraneDevice)device;
            ////    return craneDevice.LastStatus != null
            ////        && (craneDevice.LastStatus.Event == CraneEvent.BackToTheOrigin || craneDevice.LastStatus.Event == CraneEvent.BeginRunning)
            ////        && craneDevice.LastStatus.State == CraneStatus.BackToTheOrigin;
            ////});
        }

        public virtual void Move(LockerInfo locker, RackLocation fromLocation, RackLocation toLocation)
        {
            if (this.Locker != null && !this.Locker.IsEmpty && !this.Locker.Equals(locker))
            {
                throw new InvalidOperationException(string.Format("{0} 正处于锁定状态", this));
            }

            if (!this.IsConnected)
            {
                throw new InvalidOperationException(string.Format("未连接到 {0} 设备", this));
            }

            if (this.WorkMode == CraneDeviceWorkMode.Manual)
            {
                throw new InvalidOperationException(string.Format("{0} 设备处于手动模式", this));
            }

            CheckStatus();

            string taskId = GetGotTaskId();
            AddTaskCommand addTaskCommand = new AddTaskCommand(taskId, AddTaskCommandType.半自动行走, fromLocation, toLocation, "", false);

            Write(addTaskCommand, addTaskCommand.SendSuccess);
        }

        public virtual void Pick(LockerInfo locker, RackLocation fromLocation)
        {
            if (this.Locker != null && !this.Locker.IsEmpty && !this.Locker.Equals(locker))
            {
                throw new InvalidOperationException(string.Format("{0} 正处于锁定状态", this));
            }

            if (!this.IsConnected)
            {
                throw new InvalidOperationException(string.Format("未连接到 {0} 设备", this));
            }


            if (this.WorkMode == CraneDeviceWorkMode.Manual)
            {
                throw new InvalidOperationException(string.Format("{0} 设备处于手动模式", this));
            }

            CheckStatus();

            if (fromLocation is CraneDeviceOriginPointLocation)
            {
                throw new InvalidOperationException(string.Format("原点位置 {0} 无法取放货", fromLocation));
            }

            string taskId = GetGotTaskId();
            // string taskId = "12345678";
            AddTaskCommand addTaskCommand = new AddTaskCommand(taskId, AddTaskCommandType.半自动取货, fromLocation, fromLocation, "", false);

            Write(addTaskCommand, addTaskCommand.SendSuccess);
        }

        public virtual void Putdown(LockerInfo locker, RackLocation toLocation)
        {
            if (this.Locker != null && !this.Locker.IsEmpty && !this.Locker.Equals(locker))
            {
                throw new InvalidOperationException(string.Format("{0} 正处于锁定状态", this));
            }

            if (!this.IsConnected)
            {
                throw new InvalidOperationException(string.Format("未连接到 {0} 设备", this));
            }


            if (this.WorkMode == CraneDeviceWorkMode.Manual)
            {
                throw new InvalidOperationException(string.Format("{0} 设备处于手动模式", this));
            }

            CheckStatus();

            if (toLocation is CraneDeviceOriginPointLocation)
            {
                throw new InvalidOperationException(string.Format("原点位置 {0} 无法取放货", toLocation));
            }

            string taskId = GetGotTaskId();
            // string taskId = "12345678";
            AddTaskCommand addTaskCommand = new AddTaskCommand(taskId, AddTaskCommandType.半自动放货, toLocation, toLocation, "", false);

            Write(addTaskCommand, addTaskCommand.SendSuccess);
        }

        /// <summary>
        /// 获取此堆垛机当前所在位置
        /// </summary>
        /// <returns>在设备未连接或取到的状态位置无法转换时将返回null</returns>
        public virtual RackLocation GetCurrentLocation()
        {
            if (this.LastStatus == null)
            {
                return null;
            }

            int currentColumn = this.LastStatus.Column;
            int currentLevel = this.LastStatus.Level;

            RackLocation currentLocation = this.Locations.Select(x => (RackLocation)x).FirstOrDefault(x => x.Column == currentColumn && x.Level == currentLevel);
            return currentLocation;
        }

        /// <summary>
        /// 重置（复位）报警
        /// </summary>
        public virtual void ResetWarn(LockerInfo locker)
        {
            if (this.Locker != null && !this.Locker.IsEmpty && !this.Locker.Equals(locker))
            {
                throw new InvalidOperationException(string.Format("{0} 正处于锁定状态", this));
            }

            if (!this.IsConnected)
            {
                throw new InvalidOperationException(string.Format("未连接到 {0} 设备", this));
            }

            if (this.LastStatus == null)
            {
                throw new InvalidOperationException(string.Format("{0} 状态未同步，当前无法执行此操作。", this));
            }

            if (this.LastStatus.State != CraneStatus.AlarmAndShutdown)
            {
                throw new InvalidOperationException(string.Format("{0} 未处于报警停机状态，当前无法执行此操作。", this));
            }

            var cmd = new ResetWarnCommand();
            Write(cmd, cmd.SendSuccess);
        }
        #endregion

        #region Private Methods
        void FireEvents(CompareResult<NetTransferObject> compareResult)
        {
            //if (compareResult == null || compareResult.differences == null || compareResult.differences.Length == 0)
            //{
            //    return;
            //}

            //if (!compareResult.IsTypeOf(typeof(RequestStateCommandReplyTelexTransferObject)))
            //{
            //    return;
            //}

            //RequestStateCommandReplyTelexTransferObject newObject = (RequestStateCommandReplyTelexTransferObject)compareResult.newObject;

            //Int32 equipmentTaskId = ParseEquipmentTaskId(newObject);

            ////报警信号
            //if (compareResult.differences
            //    .Any(x =>
            //        (x.propertyName == "ErrorCode" && x.newValue != 0)
            //        ||
            //        (x.propertyName == "State" && x.newValue == CraneStatus.AlarmAndShutdown)
            //        )
            //    )
            //{
            //    var alarm = this.DeviceWarningFactory.Create(this, newObject.ErrorCode.ToString(), null);
            //    Int32 _equipmentTaskId = ParseEquipmentTaskId2(newObject);

            //    this._logger.Debug1(string.Format("收到了任务 {0} 的报警信号，报警码 {1}。开始处理...", _equipmentTaskId, newObject.ErrorCode), this, newObject, null, _equipmentTaskId);

            //    //报警停机并且有任务
            //    if (newObject.State == CraneStatus.AlarmAndShutdown && _equipmentTaskId != 0 && Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<bool>("craneAlarmAutoSuspendTask", false))
            //    {
            //        this._logger.Debug1(string.Format("收到了任务 {0} 的报警信号，准备引发...", _equipmentTaskId), this, newObject, null, _equipmentTaskId);
            //        FireTaskErrorEvent(new TaskErrorEventArgs(_equipmentTaskId, alarm.Code, alarm.Description));
            //    }
            //}

            //运行信号
            //if (compareResult.differences.Any(x =>
            //    x.propertyName == "State"
            //    && (x.newValue == CraneStatus.Pickup || x.newValue == CraneStatus.Putin || x.newValue == CraneStatus.无货运行 || x.newValue == CraneStatus.有货运行)
            //    && (x.oldValue != CraneStatus.Pickup && x.oldValue != CraneStatus.Putin && x.oldValue != CraneStatus.无货运行 && x.oldValue != CraneStatus.有货运行)
            //    && newObject.Event != CraneEvent.Finished
            //    && newObject.Event != CraneEvent.CompletedWithError
            //    ))
            //{
            //    if (equipmentTaskId != 0)
            //    {
            //        this._logger.Debug1(string.Format("收到了任务 {0} 的运行信号，准备引发...", equipmentTaskId), this, newObject, null, equipmentTaskId);
            //        FireTaskRunningEvent(new TaskRunningEventArgs(equipmentTaskId));
            //    }
            //    else
            //    {
            //        this._logger.Debug1(string.Format("收到了任务 {0} 的运行信号，但未处理", newObject.TaskId), this, newObject, null, equipmentTaskId);
            //    }
            //}

            //完成信号
            //if (
            //    //(compareResult.differences.Any(x => x.propertyName == "Event" && (x.newValue == CraneEvent.Finished || x.newValue == CraneEvent.CompletedWithError)))
            //    //||
            //    //(
            //        compareResult.differences.Any(x => x.propertyName == "State" || x.propertyName=="Event") 
            //        && (newObject.Event == CraneEvent.CompletedWithError ||newObject.Event==CraneEvent.Finished)
            //        && (newObject.State==CraneStatus.无货待命
            //        || newObject.State == CraneStatus.右探无货
            //        || newObject.State == CraneStatus.右探有货
            //        || newObject.State == CraneStatus.左探无货
            //        || newObject.State == CraneStatus.左探有货
            //        || newObject.State == CraneStatus.左无右无
            //        || newObject.State == CraneStatus.左无右有
            //        || newObject.State == CraneStatus.左有右无
            //        || newObject.State == CraneStatus.左有右有
            //        )
            //    //)
            // )
            //{
            //    if (equipmentTaskId != 0)
            //    {
            //        this._logger.Debug1(string.Format("收到了任务 {0} 的完成信号，准备引发...", equipmentTaskId), this, newObject, null, equipmentTaskId);
            //        FireTaskCompletedEvent(new TaskCompletedEventArgs(equipmentTaskId));
            //    }
            //    else
            //    {
            //        this._logger.Debug1(string.Format("收到了任务 {0} 的完成信号，但未处理", newObject.TaskId), this, newObject, null, equipmentTaskId);
            //    }
            //}

            //位置改变信号
            //if (compareResult.differences.Any(x => x.propertyName == "Column" || x.propertyName == "Level"))
            //{
            //    var rackLocation = this._locations.Select(x => (RackLocation)x).FirstOrDefault(x => x.Level == newObject.Level && x.Column == newObject.Column);
            //    this._logger.Debug1(string.Format("收到了任务 {0} 的位置改变到 {1} 的信号，但未处理", newObject.TaskId, rackLocation), this, newObject, null, equipmentTaskId);
            //}
        }
        void FireEvents(RequestStateCommandReplyTelexTransferObject newObject)
        {
            //Int32 equipmentTaskId = ParseEquipmentTaskId(newObject);

            //if (newObject.ErrorCode != 0)
            //{
            //    var alarm = this.DeviceWarningFactory.Create(this, newObject.ErrorCode.ToString(), null);
            //    Int32 _equipmentTaskId = ParseEquipmentTaskId2(newObject);

            //    //报警停机并且有任务
            //    if (newObject.State == CraneStatus.AlarmAndShutdown && _equipmentTaskId != 0 && Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<bool>("craneAlarmAutoSuspendTask", false))
            //        FireTaskErrorEvent(new TaskErrorEventArgs(_equipmentTaskId, alarm.Code, alarm.Description));
            //}

            ////运行信号
            //if ((newObject.State == CraneStatus.Pickup || newObject.State == CraneStatus.Putin || newObject.State == CraneStatus.无货运行 || newObject.State == CraneStatus.有货运行) && newObject.Event != CraneEvent.Finished && newObject.Event != CraneEvent.CompletedWithError)
            //{
            //    if (equipmentTaskId != 0)
            ////    {
            //FireTaskRunningEvent(new TaskRunningEventArgs(equipmentTaskId));
            //    }
            //    else
            //    {
            //        this._logger.Debug1(string.Format("收到了任务 {0} 的运行信号，但未处理", newObject.TaskId), this, newObject, null, equipmentTaskId);
            //    }
            //}

            //完成信号
            //if ((newObject.Event == CraneEvent.Finished || newObject.Event == CraneEvent.CompletedWithError) && (newObject.State==CraneStatus.无货待命
            //                                                                                                     || newObject.State == CraneStatus.右探无货
            //                                                                                                     || newObject.State == CraneStatus.右探有货
            //                                                                                                     || newObject.State == CraneStatus.左探无货
            //                                                                                                     || newObject.State == CraneStatus.左探有货
            //                                                                                                     || newObject.State == CraneStatus.左无右无
            //                                                                                                     || newObject.State == CraneStatus.左无右有
            //                                                                                                     || newObject.State == CraneStatus.左有右无
            //                                                                                                     || newObject.State == CraneStatus.左有右有))
            //{
            //    if (equipmentTaskId != 0)
            //    {
            //        FireTaskCompletedEvent(new TaskCompletedEventArgs(equipmentTaskId));
            //    }
            //    else
            //    {
            //        this._logger.Debug1(string.Format("收到了任务 {0} 的完成信号，但未处理", newObject.TaskId), this, newObject, null, equipmentTaskId);
            //    }
            //}

            //var rackLocation = this._locations.Select(x => (RackLocation)x).FirstOrDefault(x => x.Level == newObject.Level && x.Column == newObject.Column);
            //this._logger.Debug1(string.Format("收到了任务 {0} 的位置改变到 {1} 的信号，但未处理", newObject.TaskId, rackLocation), this, newObject, null, equipmentTaskId);
        }
        public void PublicFireTaskRunningEvent(TaskRunningEventArgs args)
        {
            FireTaskRunningEvent(args);
        }

        void FireAddTaskCommandRepliedEvent(AddTaskCommandRepliedEventArgs args)
        {
            this.DeviceEventQueue.Enqueue(this.AddTaskCommandReplied, this, args);
        }

        void CraneDevice_Disconnected(Device device, DisconnectEventArgs args)
        {
            lastRequestStateCommandReplyTelexTransferObject = null;
            args.Handled = true;
            //if (_requestStateCommandThreadIsRunning)
            //{
            //    _requestStateCommandThreadIsRunning = false;
            //    this._logger.Debug1(string.Format("{0} 已断开连接，写入停止请求回报状态线程标志。", this), this);
            //}
        }

        void CraneDevice_Connected(Device device, ConnectedEventArgs args)
        {
            args.Handled = true;
            //if (_requestStateCommandThreadIsRunning)
            //{
            //    this._logger.Debug1(string.Format("{0} 请求回报状态线程已运行，不再启动。", this), this);
            //    return;
            //}

            //ThreadPool.QueueUserWorkItem(requestStateCommandThreadProc);
            //_requestStateCommandThreadProcThread = new Thread(requestStateCommandThreadProc);
            //_requestStateCommandThreadProcThread.IsBackground = true;
            //_requestStateCommandThreadProcThread.Name = String.Format("{0}状态回报请求", this);
            // _requestStateCommandThreadProcThread.StartAndManaged();请求堆垛机恢复命令注释，不需要发送 zhengge 20220320
        }
        /// <summary>
        /// 创建一个堆垛机状态查询命令实例
        /// </summary>
        /// <returns></returns>
        protected virtual TelexTransferObject NewRequestStateCommand()
        {
            return new RequestStateCommand();
        }

        void requestStateCommandThreadProc(object state)
        {
            this._requestStateCommandThreadIsRunning = true;
            this._logger.Trace1(string.Format("{0} 请求回报状态线程已启动.", this), this);
            try
            {
                while (true)
                {
                    Thread.Sleep(300);
                    if (!this.IsConnected)
                    {
                        this._logger.Trace1(string.Format("由于连接已断开，{0} 请求回报状态线程准备停止.", this), this);
                        break;
                    }
                    if (!this._requestStateCommandThreadIsRunning)
                    {
                        this._logger.Trace1(string.Format("收到退出信号，{0} 请求回报状态线程准备停止.", this), this);
                        break;
                    }
                    try
                    {
                        var requestStateCommand = NewRequestStateCommand();

                        Write(requestStateCommand.GetBytes());
                    }
                    catch (ThreadAbortException threadAbortException)
                    {
                        this._logger.Debug1(string.Format("{0} 请求回报状态线程正在停止.", this), this);
                    }
                    catch (Exception ex)
                    {
                        this._logger.Error1(ex, this);
                    }
                }
            }
            catch (Exception ex)
            {
                this._logger.Error1(ex, this);
            }
            finally
            {
                this._requestStateCommandThreadIsRunning = false;
            }

            this._logger.Trace1(string.Format("{0} 请求回报状态线程已停止.", this), this);
        }

        protected void CheckStatus()
        {
            if (this.LastStatus == null)
                throw new InvalidOperationException(string.Format("{0} 状态未同步。", this));

            if (this.LastStatus.State == CraneStatus.ManualMode)
                throw new InvalidOperationException(string.Format("{0} 堆垛机正处于手动模式, 不能下发任务。", this));

            if (this.LastStatus.AtStation == false)
                throw new InvalidOperationException(string.Format("{0} 堆垛机不在站点位置, 不能下发任务。", this));

            if (this.LastStatus.ForkHorizontalPosition != ForkHorizontalPosition.Center)
                throw new InvalidOperationException(string.Format("{0} 货叉不在中位, 不能下发任务。", this));

            if (this.LastStatus.Event != CraneEvent.Initialized && this.LastStatus.Event != CraneEvent.Finished)
                throw new InvalidOperationException(string.Format("{0} 堆垛机任务事件未完成, 不能下发任务。", this));

            if (this.LastStatus.State != CraneStatus.Initialized && this.LastStatus.State != CraneStatus.无货待命 && this.LastStatus.State != CraneStatus.有货待命 && this.LastStatus.State != CraneStatus.右探无货 && this.LastStatus.State != CraneStatus.右探有货 && this.LastStatus.State != CraneStatus.左探无货 && this.LastStatus.State != CraneStatus.左探有货 && this.LastStatus.State != CraneStatus.左无右无 && this.LastStatus.State != CraneStatus.左无右有 && this.LastStatus.State != CraneStatus.左有右无 && this.LastStatus.State != CraneStatus.左有右有)
                throw new InvalidOperationException(string.Format("{0} 堆垛机处于非待命状态, 不能下发任务。", this));
        }

        int _gotTaskTotals = 2;
        public String GetGotTaskId()
        {
            _gotTaskTotals++;
            string result = string.Format("GOT{0:00000}", _gotTaskTotals);
            while (this.LastStatus.TaskId.Equals(result, StringComparison.CurrentCultureIgnoreCase))
            {
                _gotTaskTotals++;
                result = string.Format("GOT{0:00000}", _gotTaskTotals);
            }

            return result;
        }
        /// <summary>
        /// 从指定的状态中转换出正确的任务号
        /// </summary>
        /// <param name="state">状态</param>
        /// <returns>如果是GOT任务将返回0</returns>
        public virtual Int32 ParseEquipmentTaskId(RequestStateCommandReplyTelexTransferObject state)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            int result;
            if (state.TaskId.StartsWith("GOT", StringComparison.CurrentCultureIgnoreCase))
                return 0;
            ///堆垛机移动盘点专用
            if (state.TaskId.StartsWith("PD", StringComparison.CurrentCultureIgnoreCase))
                int.TryParse(state.TaskId.Substring(2, 6), out result);
            else
                int.TryParse(state.TaskId, out result);

            return result;

        }
        /// <summary>
        /// 从指定的状态中转换出正确的任务号,针对单步任务设计获取任务号
        /// </summary>
        /// <param name="state">状态</param>
        /// <returns>如果是GOT任务将返回0</returns>
        public virtual Int32 ParseEquipmentTaskId2(RequestStateCommandReplyTelexTransferObject state)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            if (state.TaskId.StartsWith("GOT", StringComparison.CurrentCultureIgnoreCase))
            {
                return 0;
            }

            int result;
            int.TryParse(state.TaskId.Substring(2, 6), out result);

            return result;

        }
        #endregion

        public Boolean LocationIsExists(RackLocation loc)
        {
            return locationsDictionary.ContainsKey(loc.ToConvertibleCode());
        }

        public RackLocation FindLocation(String convertibleCode)
        {
            if (locationsDictionary.ContainsKey(convertibleCode))
            {
                return locationsDictionary[convertibleCode];
            }
            else
            {
                return null;
            }
        }


        #region Implement IEditableTaskOwner
        /// <summary>
        /// 如果在调用中想关闭验证框，请在调用前添加以下代码CallContext.SetData("单货叉.启用任务继续执行信息验证", false)
        /// </summary>
        /// <param name="action"></param>
        /// <param name="cancelled"></param>
        public void Resume(EquipmentAction action, out bool cancelled)
        {
            验证载货台(action, out cancelled);
        }

        void 验证载货台(EquipmentAction action, out bool cancelled)
        {
            cancelled = true;
            //if (this.EquipmentActionScheduler.CurrentAction.EquipmentTaskId != action.EquipmentTaskId)
            //    return;
            if (!this.IsConnected)
                throw new Exception("设备未连接");
            if (this.LastStatus == null)
                throw new Exception("设备状态未获取");

            if (action is CraneAutomaticTransferWithStepByStepAction)
            {
                if (this.LastStatus.State == CraneStatus.无货待命)
                {
                    StateManagerRestoreInfo _stateManagerRestoreInfo;
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                    {
                        _stateManagerRestoreInfo = unitOfWork.session.Query<StateManagerRestoreInfo>().FirstOrDefault(x => x.EquipmentActionId == action.Id);
                        if (_stateManagerRestoreInfo != null && _stateManagerRestoreInfo.CurrentStateName != "需要移动到取货点" && _stateManagerRestoreInfo.CurrentStateName != "等待取货")
                        {
                            _stateManagerRestoreInfo.CurrentStateName = "需要移动到取货点";
                            unitOfWork.session.Save(_stateManagerRestoreInfo);
                        }
                        unitOfWork.Commit();
                    }
                }
            }
            cancelled = false;
            return;
        }

        public void Suspend(EquipmentAction task, out bool cancelled)
        {
            //如果是物理设备的任务，只有执行中的任务才需要发送取消指令
            if (task.Status == EquipmentActionStatus.Executing)
            {
                this.CancelTask(task);
            }

            cancelled = false;
        }

        public void Cancel(EquipmentAction task, out bool cancelled)
        {
            this.CancelTask(task);

            cancelled = false;
        }

        public void Complete(EquipmentAction task, out bool cancelled)
        {
            var la = LastStatus;
            if (la == null)
            {
                _logger.Warn1("状态信息未同步，强制完成操作被取消。", this, task);
                cancelled = true;
                return;
            }

            if (la.State != CraneStatus.无货待命 && la.State != CraneStatus.右探无货 && la.State != CraneStatus.右探有货 && la.State != CraneStatus.左探无货 && la.State != CraneStatus.左探有货 && la.State != CraneStatus.左无右无 && la.State != CraneStatus.左无右有 && la.State != CraneStatus.左有右无 && la.State != CraneStatus.左有右有)
            {
                _logger.Warn1("堆垛机当前未处于无货待命状态，强制完成操作被取消。", this, task);
                cancelled = true;
                return;
            }

            cancelled = false;
        }
        #endregion

        public override TState Read<TState>()
        {
            throw new NotImplementedException();
        }
    }
}
