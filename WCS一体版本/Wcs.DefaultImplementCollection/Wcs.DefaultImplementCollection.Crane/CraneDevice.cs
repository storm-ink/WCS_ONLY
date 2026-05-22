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

namespace Wcs.DefaultImplementCollection.Crane
{
    [System.ComponentModel.Description("堆垛机")]
    public class CraneDevice : TcpProtocolTaskableDevice, IEditableTaskOwner
    {
        protected System.Threading.Thread _requestStateCommandThreadProcThread;

        public String ConveyorDeviceName;
        /// <summary>
        /// 获取当前堆垛机的工作模式
        /// </summary>
        public virtual CraneWorkModels WorkMode
        {
            get
            {
                if (this.LastStatus == null)
                    return CraneWorkModels.Unknown;

                return this.LastStatus.CraneWorkModel;
            }
        }
        /// <summary>
        /// 指示当堆垛机请求状态回报线程是否处于工作状态
        /// </summary>
        Boolean _requestStateCommandThreadIsRunning;
        Dictionary<String, RackLocation> locationsDictionary = new Dictionary<string, RackLocation>();
        CraneReportInfo lastCraneReportInfo = null;

        #region Properities
        /// <summary>
        /// 获取堆垛机最后回报的状态数据
        /// </summary>
        public CraneReportInfo LastStatus
        {
            get
            {
                return lastCraneReportInfo;
            }
            private set
            {
                lastCraneReportInfo = value;
            }
        }
        public DateTime LastReceivedAt = DateTime.Now;

        #endregion

        #region Events

        #endregion
        public CraneDevice(string name, Int32 no, Int32 receiveTimeout, Int32 connectTimeout, Int32 sendTimeout, IPEndPoint ipEndPoint, IPEndPoint bindEndPoint, IDataReceiver dataReceiver)
            : base(name, no, receiveTimeout, connectTimeout, sendTimeout, false, ipEndPoint, bindEndPoint, dataReceiver)
        {
            this.Disconnected += CraneDevice_Disconnected;
        }

        #region Implement TcpProtocolTaskableDevice
        protected override void OnDataReceived(NetPacket netPacket, NetTransferObject netTransferObject)
        {
            if (netTransferObject is CraneReportInfo)
            {
                lastCraneReportInfo = (CraneReportInfo)netTransferObject;
                LastReceivedAt = DateTime.Now;
            }
        }

        public override int[] OccupiedEquipmentTasks
        {
            get
            {
                return new int[0];
            }
        }

        /// <summary>
        /// 取消堆垛机当前任务
        /// </summary>
        public virtual void CancleTask()
        {
            CraneCommand cmd = new CraneCommand(CmdTypes.ClearTask);
            Write(cmd, cmd.SendSuccess);
        }
        public override void CancelTask(EquipmentAction action)
        {
            //if (this.LastStatus == null || ParseEquipmentTaskId(this.LastStatus) != action.EquipmentTaskId)
            //{
            //    return;
            //}

            //if (this.LastStatus.State != CraneStatus.AlarmAndShutdown && this.LastStatus.State != CraneStatus.无货待命 && this.LastStatus.State != CraneStatus.有货待命)
            //{
            //    throw new NotImplementedException("非报警停机/无货待命/有货待命 状态的堆垛机不支持取消任务操作.");
            //}
            ////if (MessageBox.Show("是否需要发送堆垛机任务取消命令"))
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
                if (newObject.CraneWorkModel != CraneWorkModels.Auto)
                    return new IsIdleResult(false, $"设备最新数据（LastStatus）显示 CraneWorkModel != CraneWorkModels.Auto({(int)CraneWorkModels.Auto})");

                //任务号不为0 并且任务状态=Wait或者running时认为任务在执行中
                if (newObject.EquipmentTaskId != 0 && (newObject.TaskState == CraneTaskStatus.Wait || newObject.TaskState == CraneTaskStatus.Running))
                    return new IsIdleResult(false, "设备最新数据（LastStatus）显示 EquipmentTaskId != 0 && (newObject.TaskState == CraneTaskStatus.Wait || newObject.TaskState == CraneTaskStatus.Running)");

                if (newObject.ErrorCodeList.Count() != 0)
                    return new IsIdleResult(false, "设备最新数据（LastStatus）显示 ErrorCodeList.Count() != 0");

                if (newObject.DeviceState != CraneStatus.Watting)
                    return new IsIdleResult(false, $"设备最新数据（LastStatus）显示DeviceState != CraneStatus.Watting({(int)CraneStatus.Watting})");

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

                    if (newObject.CraneWorkModel == CraneWorkModels.Manual)
                        result.Add(string.Format("设备被切换为手动模式"));

                    if (newObject.CraneWorkModel == CraneWorkModels.Repair)
                        result.Add(string.Format("设备被切换为维修模式"));

                    if (newObject.CraneWorkModel == CraneWorkModels.SemiAuto)
                        result.Add(string.Format("设备被切换为半自动模式"));

                    if (newObject.CraneWorkModel == CraneWorkModels.Unknown)
                        result.Add(string.Format("设备被切换为未知模式"));

                    if (this.Locker != null && !this.Locker.IsEmpty)
                        result.Add(string.Format("设备被 {0} 远程锁定", this.Locker));


                    if (newObject.DeviceState == CraneStatus.RemoteEmergency)
                        result.Add("远程急停被触发");

                    if (newObject.DeviceState != CraneStatus.RemoteEmergency && newObject.DeviceState != CraneStatus.Running && newObject.DeviceState != CraneStatus.Watting)
                        result.Add(string.Format("处于 {0} 状态", newObject.DeviceState.GetDescription()));

                    if (this.EquipmentActionScheduler.CurrentAction == null && (this.LastStatus.EquipmentTaskId == 0 || (this.LastStatus.EquipmentTaskId != 0 && this.LastStatus.TaskState != CraneTaskStatus.Running && this.LastStatus.TaskState != CraneTaskStatus.Wait)) && newObject.IsLoaded == 1)
                        result.Add("设备未执行任务，但载货台有货");
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
            //if (this.WorkMode == CraneDeviceWorkMode.Manual
            //    && !data.GetType().Name.Contains("EmergencyStopCommand")
            //    && !data.GetType().Name.Contains("CancleCommand")
            //    )
            ////if (this.WorkMode == CraneDeviceWorkMode.Manual && !(data is EmergencyStopCommand) && !(data is CancelEmergencyStopCommand))
            //{
            //    throw new InvalidOperationException("手动模式下无法进行除急停、取消急停、取消任务外的一切操作。");
            //}

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
            CraneCommand cmd = new CraneCommand(CmdTypes.Emergency);
            Write(cmd, cmd.SendSuccess);
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

            if (!this.IsConnected)
            {
                throw new InvalidOperationException(string.Format("未连接到 {0} 设备", this));
            }

            //设备不是远程急停状态
            if (newObject.DeviceState != CraneStatus.RemoteEmergency)
            {
                throw new InvalidOperationException(string.Format("{0} 设备不处于远程急停状态", this));
            }

            CraneCommand cmd = new CraneCommand(CmdTypes.CancelEmergency);
            Write(cmd, cmd.SendSuccess);
        }

        public virtual void Move(LockerInfo locker, RackLocation fromLocation, RackLocation toLocation)
        {
            CheckStatus(locker);

            //string taskId = GetGotTaskId();
            //AddTaskCommand addTaskCommand = new AddTaskCommand(taskId, AddTaskCommandType.半自动行走, fromLocation, toLocation, "", false);

            //Write(addTaskCommand, addTaskCommand.SendSuccess);
        }

        public virtual void Pick(LockerInfo locker, RackLocation fromLocation)
        {
            CheckStatus(locker);

            if (fromLocation is CraneDeviceOriginPointLocation)
            {
                throw new InvalidOperationException(string.Format("原点位置 {0} 无法取放货", fromLocation));
            }

            //string taskId = GetGotTaskId();
            //// string taskId = "12345678";
            //AddTaskCommand addTaskCommand = new AddTaskCommand(taskId, AddTaskCommandType.半自动取货, fromLocation, fromLocation, "", false);

            //Write(addTaskCommand, addTaskCommand.SendSuccess);
        }

        public virtual void Putdown(LockerInfo locker, RackLocation toLocation)
        {
            CheckStatus(locker);

            if (toLocation is CraneDeviceOriginPointLocation)
            {
                throw new InvalidOperationException(string.Format("原点位置 {0} 无法取放货", toLocation));
            }

            //string taskId = GetGotTaskId();
            //// string taskId = "12345678";
            //AddTaskCommand addTaskCommand = new AddTaskCommand(taskId, AddTaskCommandType.半自动放货, toLocation, toLocation, "", false);

            //Write(addTaskCommand, addTaskCommand.SendSuccess);
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

            int currentColumn = this.LastStatus.XColumn;
            int currentLevel = this.LastStatus.YLevel;

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

            if (this.LastStatus.DeviceState != CraneStatus.AlarmDown)
            {
                throw new InvalidOperationException(string.Format("{0} 未处于远程急停，当前无法执行此操作。", this));
            }

            //var cmd = new CraneCommand( CmdTypes.);
            //Write(cmd, cmd.SendSuccess);
        }
        #endregion

        #region Private Methods
        public void PublicFireTaskRunningEvent(TaskRunningEventArgs args)
        {
            FireTaskRunningEvent(args);
        }

        void CraneDevice_Disconnected(Device device, DisconnectEventArgs args)
        {
            lastCraneReportInfo = null;
            args.Handled = true;
        }

        protected void CheckStatus(LockerInfo locker)
        {
            if (this.Locker != null && !this.Locker.IsEmpty && !this.Locker.Equals(locker))
                throw new InvalidOperationException(string.Format("{0} 正处于锁定状态", this));

            if (!this.IsConnected)
                throw new InvalidOperationException(string.Format("未连接到 {0} 设备", this));

            if (this.WorkMode != CraneWorkModels.Auto)
                throw new InvalidOperationException(string.Format("{0} 设备不是自动模式", this));

            if (this.LastStatus == null)
                throw new InvalidOperationException(string.Format("{0} 状态未同步。", this));

            if (this.LastStatus.CraneWorkModel == CraneWorkModels.Manual)
                throw new InvalidOperationException(string.Format("{0} 堆垛机正处于手动模式, 不能下发任务。", this));

            if (this.LastStatus.CraneWorkModel == CraneWorkModels.Repair)
                throw new InvalidOperationException(string.Format("{0} 堆垛机正处于维修模式, 不能下发任务。", this));

            if (this.LastStatus.CraneWorkModel == CraneWorkModels.SemiAuto)
                throw new InvalidOperationException(string.Format("{0} 堆垛机正处于半自动模式, 不能下发任务。", this));

            if (this.LastStatus.CraneWorkModel == CraneWorkModels.Unknown)
                throw new InvalidOperationException(string.Format("{0} 堆垛机正处于未知模式, 不能下发任务。", this));

            if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetAttributeOrDefault("NeedClearCraneCompeletedTaskId", false)
                && this.LastStatus.EquipmentTaskId != 0)
                throw new InvalidOperationException(string.Format("{0} 堆垛机需要清理已完成任务号，当前设备任务号不等于0, 不能下发任务。", this));

            if (this.LastStatus.EquipmentTaskId != 0
                && (this.LastStatus.TaskState == CraneTaskStatus.Wait || this.LastStatus.TaskState == CraneTaskStatus.Running))
            {
                throw new InvalidOperationException(string.Format("{0} 堆垛机已存在任务 {1}，任务状态 {2}, 不能下发任务。", this, this.LastStatus.EquipmentTaskId, this.LastStatus.TaskState));
            }
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
                if (this.LastStatus.IsLoaded == 2)
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
                if (MessageBox.Show("状态信息未同步，是否需要跳过设备检查状态直接强制完成，后续由操作员人工合理处理设备上残留的任务？", "提示：", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                {
                    _logger.Warn1("状态信息未同步，强制完成操作被取消。", this, task);
                    cancelled = true;
                    return;
                }
                else
                    _logger.Info1("状态信息未同步，用户已选择确认 \"跳过设备检查状态直接强制完成，后续由操作员人工合理处理设备上残留的任务\"", this);
            }

            if (la.TaskState != CraneTaskStatus.Compeleted && la.TaskState != CraneTaskStatus.ErrorCompeleted && la.TaskState != CraneTaskStatus.InventoryEmpry && la.TaskState != CraneTaskStatus.InventoryFull && la.TaskState != CraneTaskStatus.ManualCompeleted && la.TaskState != CraneTaskStatus.ScanOver && la.TaskState != CraneTaskStatus.Empty)
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

        static object EquipmentTaskIdObjLocker = new object();
        int basicTaskId = 10000000;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public UInt32 GetEquipmentTaskId(CraneEquipmentTaskType craneEquipmentTaskType = CraneEquipmentTaskType.Unknown)
        {
            lock (EquipmentTaskIdObjLocker)
            {
                UInt32 taskId = 0;
                if (this.LastStatus != null)
                {
                    var _basicTaskId = this.LastStatus.EquipmentTaskId / basicTaskId;
                    if (_basicTaskId == (UInt16)craneEquipmentTaskType)
                    {
                        if (this.LastStatus.EquipmentTaskId > 9999999 + (UInt16)craneEquipmentTaskType * basicTaskId)
                            taskId = (UInt32)((UInt16)craneEquipmentTaskType * basicTaskId);
                        else
                            taskId = this.LastStatus.EquipmentTaskId + 1;
                    }
                    else
                        taskId = (UInt32)((UInt16)craneEquipmentTaskType * basicTaskId);
                }
                else
                    taskId = (UInt32)((UInt32)craneEquipmentTaskType * basicTaskId + 1);

                if (taskId == 0)
                    taskId = 1;
                return taskId;
            }
        }

        public enum CraneEquipmentTaskType
        {
            /// <summary>
            /// 未知
            /// </summary>
            Unknown = 0,
            /// <summary>
            /// WCS全自动任务
            /// </summary>
            WcsTaskAuto = 1,
            /// <summary>
            /// WCS单步任务-取货
            /// </summary>
            WcsTaskStep_Pick = 2,
            /// <summary>
            /// WCS单步任务-放货
            /// </summary>
            WcsTaskStep_Put = 3,
            /// <summary>
            /// WCS单步任务-移动
            /// </summary>
            WcsTaskStep_Move = 4,
            /// <summary>
            /// WCS单步任务-探货盘存
            /// </summary>
            WcsTaskStep_SensorCheckInventory = 5,
            /// <summary>
            /// WCS单步任务-扫码盘检
            /// </summary>
            WcsTaskStep_ScanCheckInventory = 6,
            /// <summary>
            /// WCS手动任务
            /// </summary>
            Manual = 9,
        }
    }
}
