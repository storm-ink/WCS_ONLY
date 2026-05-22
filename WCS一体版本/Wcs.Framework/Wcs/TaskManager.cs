using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Devices;
using NHibernate.Linq;
using Wcs.Framework.EquipmentActions;
using System.Xml;
using Wcs.Framework.Cfg;
using System.Threading;
using System.Diagnostics;
using Wcs.Framework.Events;
using Wcs.Framework.Devices.Events;

namespace Wcs.Framework
{
    /// <summary>
    /// 任务管理器
    /// </summary>
    public class TaskManager
    {
        public Logger Logger { get; private set; }
        public delegate void EntityUpdatedEventHandler(Object entity);
        public delegate void EntityDeletedEventHandler(Object entity);
        static TaskManager _instance;
        public static TaskManager GetInstance()
        {
            lock (typeof(TaskManager))
            {
                if (_instance == null)
                {
                    _instance = new TaskManager();
                }
            }

            return _instance;
        }
        /// <summary>
        /// 有任务被更新时发生
        /// </summary>
        public event EntityUpdatedEventHandler EntityUpdated;
        public event EntityDeletedEventHandler EntityDeleted;

        SimpleReaderWriterLockSlimManager _readerWriterLockSlimManager=new SimpleReaderWriterLockSlimManager();
        LifeCycleManager<TaskBlock> _sencodeHandShakeLifeCycleManager = new LifeCycleManager<TaskBlock>();

        static Random equipmentTaskIdRandom = new Random();
        /// <summary>
        /// 获取下一个设备任务号
        /// </summary>
        /// <returns></returns>
        public Int32 GetNextEquipmentTaskId(params UInt16[] excludeds)
        {
            //if (excludeds == null)
            //{
            //    excludeds = new UInt16[] { };
            //}
            //using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            //{
            //    UInt16 equipmentTaskId = Convert.ToUInt16(equipmentTaskIdRandom.Next(1, 32767));
            //    while (excludeds.Contains(equipmentTaskId) || Repository.Count<EquipmentAction>(unitOfWork, x => x.EquipmentTaskId == equipmentTaskId) > 0)
            //    {
            //        equipmentTaskId = Convert.ToUInt16(equipmentTaskIdRandom.Next(1, 32767));
            //    }

            //    unitOfWork.Commit();

            //    return equipmentTaskId;
            //}
            
            lock (this)
            {
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    DateTime today = DateTime.Now.Date;
                    EquipmentTaskIdSerialNumber snObject = unitOfWork.session.Get<EquipmentTaskIdSerialNumber>(today, NHibernate.LockMode.Upgrade);
                    
                    Int32 nextSn = 0;
                getnextsn:
                    if (snObject == null)
                    {
                        snObject = new EquipmentTaskIdSerialNumber();
                        snObject.DateValue = today;
                        snObject.NextSn = 1;
                        nextSn = snObject.NextSn;
                        snObject.NextSn++;
                        unitOfWork.session.Save(snObject);
                    }
                    else
                    {
                        //2,147,483,647
                        nextSn = snObject.NextSn;
                        snObject.NextSn++;

                        //因为堆垛机和其它设备的任务号最长为8位，所以我们此处必须将任务号限制在7位内，也就是7位的最大数值(8位是环穿的让道任务)
                        if (nextSn > 9999999)
                        {
                            snObject.NextSn = 1;
                            nextSn = snObject.NextSn;
                            snObject.NextSn++;
                        }
                        unitOfWork.session.Update(snObject);
                    }

                    if (EquipmentTaskIdIsInUse(nextSn))
                    {
                        goto getnextsn;
                    }

                    unitOfWork.Commit();

                    return nextSn;
                }
            }
        }

        /// <summary>
        /// 验证设备任务号是否已被占用
        /// </summary>
        /// <param name="equipmentTaskId">设备任务号</param>
        /// <returns>被占用返回 true,否则返回 false</returns>
        Boolean EquipmentTaskIdIsInUse(Int32 equipmentTaskId)
        {
            //扫描输送线内的任务号
            if (Cfg.Configuration
                .Devices.Where(x => x.DeviceType == DeviceType.Conveyor)
                .Select(x => x as NewConveyor)
                .Where(x => x.Tasks != null)
                .SelectMany(x => x.Tasks)
                .Any(x => x.AssignmentID == equipmentTaskId))
            {
                return true;
            }

            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                return unitOfWork
                    .session
                    .Query<EquipmentAction>()
                    .Count(x => x.EquipmentTaskId == equipmentTaskId) > 0;
            }
        }
        

        private TaskManager()
        {
            XmlNode taskManagerNode = null;
            if (Configuration.Initialized)
            {
                taskManagerNode = Configuration.GetSelection("taskManager");
            }

            LogTarget logTarget = null;
            if (taskManagerNode != null)
            {
                string logTargetName = taskManagerNode.Attributes["logTarget"] == null ? "" : taskManagerNode.Attributes["logTarget"].Value;
                if (!string.IsNullOrWhiteSpace(logTargetName))
                {
                    logTarget = Configuration.LoggerTargets.SingleOrDefault(x => string.Equals(x.Name, logTargetName, StringComparison.CurrentCultureIgnoreCase));
                }
            }

            this.Logger = new Logger(this, logTarget);
            foreach (var device in DeviceManager.GetInstance().Devices)
            {
                device.DeviceError += OnDeviceError;
                device.TaskCompleted += OnTaskCompleted;
                device.TaskRunning += OnTaskRunning;
                device.TaskError += OnTaskError;

                if (device.DeviceType == DeviceType.Conveyor)
                {
                    NewConveyor conveyor = device as NewConveyor;
                    conveyor.TaskCurrentLocationChanged += OnConveyorTaskCurrentLocationChanged;
                    conveyor.NeedSecondHandShake += onConveyorTaskNeedSecondHandShake;
                    conveyor.OccpiedSignalStatusChanged += OnOccpiedSignalStatusChanged;
                }

                if (device.DeviceType == DeviceType.Crane)
                {
                    Crane crane = device as Crane;
                    crane.TaskCurrentLocationChanged += OnCraneTaskCurrentLocationChanged;
                }
            }
            
        }
             
        /// <summary>
        /// 取消占位申请
        /// </summary>
        /// <param name="request"></param>
        public void CancelRequest(Request request)
        {
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                request = Repository.Get<Request>(unitOfWork, request.Id);

                if (request.Status != RequestStatus.New)
                {
                    Exception exception=new Exception(string.Format("{0} 当前处于 {1} 状态无法被取消",request,request.Status));
                    this.Logger.Error(exception.Message, this, exception);
                    throw exception;
                }

                RequestSignalHandleResult processResult = new RequestSignalHandleResult();
                string outputMsg = null;
                if (Configuration.RequestSignalHandlersSelection != null)
                {
                    foreach (var handler in Configuration.RequestSignalHandlersSelection.RequestSignalHandlerElements)
                    {
                        handler.RequestSignalHandler.BeforeCancel(request, unitOfWork, ref processResult);
                        if (processResult.Cancelled)
                        {
                            outputMsg = string.Format("{0} 在取消时被 {1} 阻止了,原因 {2}", request, handler.RequestSignalHandler, processResult.Reason);
                            this.Logger.Warning(outputMsg, this, request);
                            break;
                        }
                    }
                }

                if (!processResult.Cancelled)
                {
                    OperationAccountHelper.Add(unitOfWork, request, "{0} 被取消", request);
                    this.Logger.Info(string.Format("{0} 状态从 {1} 更改为 {2}", request, request.Status.GetDescription(), RequestStatus.Cancelled.GetDescription()), this, request);
                    request.Status = RequestStatus.Cancelled;
                    Repository.Update(unitOfWork, request);
                    OperationAccountHelper.Add(unitOfWork, request, "{0} 被删除", request);
                    Repository.Delete(unitOfWork, request);

                    OperationAccountHelper.Archive(unitOfWork, request);
                    unitOfWork.Commit();
                }
                else
                {
                    this.Logger.Warning(outputMsg, this, request);

                    throw new Exception(outputMsg);
                }
            }

            //将取消的请求从延迟列表中移除
            if (WmsNotifyProvider.Notifys != null)
            {
                foreach (var notify in WmsNotifyProvider.Notifys)
                {
                    DelayedWmsNotifyProcessor.GetInstance().RemoveDelayedWmsNotify(notify, "OnRequest", request);
                }
            }
        }

        public KeyValuePair<LogicMovement,DeviceRoute> GetTaskCurrentLogicMovementAtRoute(Task task)
        {
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                Task _task = Repository.Get<Task>(unitOfWork, task.Id);
                var stopAtMovement = _task.Movements.OrderBy(x => x.Ordering).FirstOrDefault(x => x.Status == LogicMovementStatus.Error || x.Status ==LogicMovementStatus.Suspend);
                if (stopAtMovement == null)
                {
                    return default(KeyValuePair<LogicMovement,DeviceRoute>);
                }

                if (stopAtMovement.RouteId != null)
                {
                    var q = Configuration.Routes.Where(x => x.Id == stopAtMovement.RouteId.Value);
                    return new KeyValuePair<LogicMovement,DeviceRoute>(stopAtMovement,q.Single());
                }

                unitOfWork.Commit();

                return default(KeyValuePair<LogicMovement, DeviceRoute>);
            }
        }


        object runningAndCompletedEventLocker = new object();
        private void OnTaskCompleted(Device sender, UInt32 equipmentTaskId, ref bool handled)
        {
            if (equipmentTaskId == 0)
            {
                return;
            }

            string lockSlimKey = string.Concat("OnTaskCompleted", equipmentTaskId);
            try
            {
                handled = true;
                if (!_readerWriterLockSlimManager[lockSlimKey].TryEnterWriteLock(0))
                {
                    this.Logger.Warning(String.Format("{0} 完成信号被忽略", equipmentTaskId), null);
                    return;
                }
                //为防止Running 和 Completed 事件同时执行导致状态更新错位，加同步锁防止其同时执行
                lock (runningAndCompletedEventLocker)
                {
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                    {
                        var task = Repository.Query<Task>(unitOfWork)
                            .Where(x => x.Movements.Any(
                                        movement => movement.EquipmentActions.Any(
                                                action => action.EquipmentTaskId == equipmentTaskId
                                                    && action.DeviceInfo.DeviceName == sender.DeviceName
                                                    && action.DeviceInfo.DeviceType == sender.DeviceType)
                                                    )
                                   )
                                   .FirstOrDefault();
                        if (task == null)
                        {
                            String msg = string.Format("收到了 {0} 任务完成信号,但未找到设备任务号为 {0} 的任务对象", equipmentTaskId);
                            this.Logger.Warning(msg, this, equipmentTaskId);
                            unitOfWork.Commit();
                            return;
                        }



                        unitOfWork.Commit();

                        if (task.Status == TaskStatus.Completed)
                        {
                            WmsNotifyProvider.OnCompleted(task);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                handled = false;
                this.Logger.Error(ex.Message, this, ex);
                this.Logger.Warning(String.Format("收到 {0} 任务完成信号，但在处理时发生了错误", equipmentTaskId), this);
            }
            finally
            {
                if (_readerWriterLockSlimManager[lockSlimKey].IsWriteLockHeld)
                {
                    _readerWriterLockSlimManager[lockSlimKey].ExitWriteLock();
                    _readerWriterLockSlimManager.Remove(lockSlimKey);
                }
            }
        }

        private void OnTaskRunning(Device sender, UInt32 equipmentTaskId, ref Boolean handled)
        {
            if (equipmentTaskId == 0)
            {
                return;
            }

            string lockSlimKey = string.Concat("OnTaskRunning", equipmentTaskId);
            try
            {
                handled = true;
                if (!_readerWriterLockSlimManager[lockSlimKey].TryEnterWriteLock(0))
                {
                    this.Logger.Warning(String.Format("{0} 运行信号被忽略", equipmentTaskId), this, equipmentTaskId);
                    return;
                }
                lock (runningAndCompletedEventLocker)
                {
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                    {
                        var task = Repository.Query<Task>(unitOfWork)
                            .Where(x => x.Movements.Any(
                                        movement => movement.EquipmentActions.Any(
                                                action => action.EquipmentTaskId == equipmentTaskId
                                                    && action.DeviceInfo.DeviceName == sender.DeviceName
                                                    && action.DeviceInfo.DeviceType == sender.DeviceType)
                                                    )
                                   )
                                   .FirstOrDefault();
                        if (task == null)
                        {
                            String msg = string.Format("收到了 {0} 任务运行信号,但未找到设备任务号为 {0} 的任务对象", equipmentTaskId);
                            this.Logger.Warning(msg, this, null);
                            Debug.WriteLine(msg);
                            unitOfWork.Commit();
                            return;
                        }

                        task.ChangeStatusEquipmentActionToRunning(equipmentTaskId);
                        unitOfWork.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                handled = false;
                this.Logger.Error(ex.Message, this, ex);
                this.Logger.Warning(String.Format("收到 {0} 运行信号，但在处理时发生了错误", equipmentTaskId), this);
            }
            finally
            {
                if (_readerWriterLockSlimManager[lockSlimKey].IsWriteLockHeld)
                {
                    _readerWriterLockSlimManager[lockSlimKey].ExitWriteLock();
                    _readerWriterLockSlimManager.Remove(lockSlimKey);
                }
            }
        }

        private void OnTaskError(Device sender, UInt32 equipmentTaskId, string errorCode, string errorDescription, ref Boolean handled)
        {
            if (equipmentTaskId == 0)
            {
                return;
            }
            string lockSlimKey = string.Concat("OnTaskError", equipmentTaskId, errorCode);
            try
            {
                handled = true;
                if (!_readerWriterLockSlimManager[lockSlimKey].TryEnterWriteLock(0))
                {
                    this.Logger.Warning(String.Format("{0} 任务报警信号被忽略", equipmentTaskId), null);
                    return;
                }

                if (sender.DeviceType == DeviceType.Crane && (sender.Locker == null || sender.Locker.IsEmpty) && Wcs.Framework.Cfg.Configuration.GetSetting<Boolean>("堆垛机报警后自动锁定", true))
                {
                    sender.Lock(new LockerInfo(System.Net.Dns.GetHostName(), LockerInfo.GetIpAddress(), Guid.NewGuid().ToString()));
                }

                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    var task = Repository.Query<Task>(unitOfWork)
                            .Where(x => x.Movements.Any(
                                        movement => movement.EquipmentActions.Any(
                                                action => action.EquipmentTaskId == equipmentTaskId
                                                    && action.DeviceInfo.DeviceName == sender.DeviceName
                                                    && action.DeviceInfo.DeviceType == sender.DeviceType)
                                                    )
                                   )
                                   .FirstOrDefault();
                    if (task == null)
                    {
                        String msg = string.Format("收到了 {0} 任务报警信号,但未找到设备任务号为 {0} 的任务对象", equipmentTaskId);
                        this.Logger.Warning(msg, this, equipmentTaskId);
                        unitOfWork.Commit();
                        return;
                    }

                    task.ChangeStatusEquipmentActionToError(equipmentTaskId, errorCode, errorDescription);
                }
            }
            catch (Exception ex)
            {
                handled = false;
                this.Logger.Error(ex.Message, this, ex);
                this.Logger.Warning(String.Format("收到 {0} 任务报警信号，但在处理时发生了错误", equipmentTaskId), this, equipmentTaskId);
            }
            finally
            {
                if (_readerWriterLockSlimManager[lockSlimKey].IsWriteLockHeld)
                {
                    _readerWriterLockSlimManager[lockSlimKey].ExitWriteLock();
                    _readerWriterLockSlimManager.Remove(lockSlimKey);
                }
            }
        }

        private void OnDeviceError(Device sender, String errorCode, String errorDescription, ref Boolean handled)
        {
            handled = true;
        }

        private void OnConveyorTaskCurrentLocationChanged(NewConveyor conveyor, LocationCurrentTask locationCurrentTask, ConveyorLocation currentLocation, ref Boolean handled)
        {
            string lockSlimKey = string.Concat("OnConveyorTaskCurrentLocationChanged", locationCurrentTask.TaskNo, "_", locationCurrentTask.PosNo);
            try
            {
                handled = true;
                if (!_readerWriterLockSlimManager[lockSlimKey].TryEnterWriteLock(0))
                {
                    this.Logger.Warning(String.Format("{0} 位置改变信号被忽略", locationCurrentTask), this, locationCurrentTask);
                    return;
                }

                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    var task = Repository.Query<Task>(unitOfWork)
                            .Where(x => x.Movements.Any(
                                        movement => movement.EquipmentActions.Any(
                                                action => action.EquipmentTaskId == locationCurrentTask.TaskNo
                                                    && action.DeviceInfo.DeviceName == conveyor.DeviceName
                                                    && action.DeviceInfo.DeviceType == conveyor.DeviceType)
                                                    )
                                   )
                                   .FirstOrDefault();
                    if (task == null)
                    {
                        String msg = string.Format("未找到设备任务号为 {0} 的任务对象", locationCurrentTask.TaskNo);
                        this.Logger.Warning(msg, this, locationCurrentTask);
                        Debug.WriteLine(msg);
                        return;
                    }

                    task.ChangeTaskCurrentLocation(locationCurrentTask.TaskNo, currentLocation);
                    unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                handled = false;
                this.Logger.Error(ex.Message, this, ex);
                this.Logger.Warning(String.Format("{0} 位置改变信号，但在处理时发生了错误", locationCurrentTask), this);
            }
            finally
            {
                if (_readerWriterLockSlimManager[lockSlimKey].IsWriteLockHeld)
                {
                    _readerWriterLockSlimManager[lockSlimKey].ExitWriteLock();
                    _readerWriterLockSlimManager.Remove(lockSlimKey);
                }
            }
        }
        private void onConveyorTaskNeedSecondHandShake(NewConveyor conveyor, TaskBlock taskBlock, ref Boolean handled)
        {
            try
            {
                handled = true;
                var item = _sencodeHandShakeLifeCycleManager[taskBlock.AssignmentID];
                if (item != null && !item.IsOverdue)
                {
                    return;
                }

                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    var task = Repository.Query<Task>(unitOfWork)
                            .Where(x => x.Movements.Any(
                                        movement => movement.EquipmentActions.Any(
                                                act => act.EquipmentTaskId == taskBlock.AssignmentID
                                                    && act.DeviceInfo.DeviceName == conveyor.DeviceName
                                                    && act.DeviceInfo.DeviceType == conveyor.DeviceType)
                                                    )
                                   )
                                   .FirstOrDefault();
                    if (task == null)
                    {
                        String msg = string.Format("未找到设备任务号为 {0} 的任务对象", taskBlock.AssignmentID);
                        this.Logger.Warning(msg, this, taskBlock.AssignmentID);
                        Debug.WriteLine(msg);
                        return;
                    }
                    var action = task.Movements.SelectMany(x => x.EquipmentActions).Single(x => x.EquipmentTaskId == taskBlock.AssignmentID);
                    conveyor.SencodConfirm(action);
                    _sencodeHandShakeLifeCycleManager.Add(taskBlock.AssignmentID, taskBlock, conveyor.SendTimeout);
                    unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                handled = false;
                this.Logger.Error(ex.Message, this, ex);
                this.Logger.Warning(String.Format("收到任务号为 {0} 的二次握手信号，但在处理时发生错误", taskBlock.AssignmentID), this);
            }
        }
        private void OnCraneTaskCurrentLocationChanged(Crane crane, UInt32 equipmentTaskId, CraneCurrentLocation currentLocation)
        {
             
        }
        /// <summary>
        /// 当占位信号产生时执行此方法.<br />
        /// 需要注意的是，不管请求对象是否被持久化到数据库没有，设备请求信号都会被删除.
        /// </summary>
        /// <param name="conveyor"> 输送线设备. </param>
        /// <param name="location"> 信号位置. </param>
        /// <param name="signal">   信号原始结构. </param>
        /// <param name="handled">  是否处理成功. </param>
        private void OnOccpiedSignalStatusChanged(NewConveyor conveyor,Location location, OccupiedSignal signal,ref Boolean handled)
        {
            string lockSlimKey = string.Concat("OnOccpiedSignalStatusChanged", conveyor, signal.PosNo);
            try
            {
                handled = true;
                if (!_readerWriterLockSlimManager[lockSlimKey].TryEnterWriteLock(0))
                {
                    this.Logger.Warning(String.Format("{0} 状态改变信号被忽略", signal), this, signal);
                    return;
                }

                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    var oldSingal = Repository.Query<Request>(unitOfWork, x =>
                        x.Source.DeviceType == conveyor.DeviceType
                        && x.Source.DeviceName == conveyor.DeviceName
                        && x.Source.DeviceCode == signal.PosNo.ToString()
                        && x.Status == RequestStatus.New
                        ).SingleOrDefault();

                    if (oldSingal != null)
                    {
                        String msg = string.Format("{0} 位置 {1} 已存在一个未处理的请求 #{2}", conveyor, location.UserCode, oldSingal.Id);
                        this.Logger.Warning(msg, this, oldSingal);
                        unitOfWork.Commit();
                    }
                    else
                    {
                        var newRequest = new Request();
                        newRequest.Source = location.CreateLocationInfo();
                        newRequest.OccupiedSignalInfo.AssignmentID = Convert.ToInt32(signal.AssignmentID);
                        newRequest.OccupiedSignalInfo.IO_Data = signal.IO_Data;
                        newRequest.OccupiedSignalInfo.TU_ID = signal.TU_ID;
                        newRequest.OccupiedSignalInfo.TU_Type = signal.TU_Type;

                        RequestSignalHandleResult processResult = new RequestSignalHandleResult();
                        if (Configuration.RequestSignalHandlersSelection != null)
                        {
                            foreach (var handler in Configuration.RequestSignalHandlersSelection.RequestSignalHandlerElements)
                            {
                                handler.RequestSignalHandler.BeforeSave(newRequest, signal, unitOfWork, ref processResult);

                                //如果被任意处理程序申请中止，不再执行其它处理程序
                                if (processResult.Cancelled)
                                {
                                    String msg = string.Format("{0} 位置 {1} 新的请求在创建时被 {2} 中止了,原因 {3}", conveyor, location.UserCode, handler.RequestSignalHandler, processResult.Reason);
                                    this.Logger.Warning(msg, this, newRequest);
                                    break;
                                }
                            }
                        }

                        //如果未被中止，保存该申请到数据库
                        if (!processResult.Cancelled)
                        {
                            Repository.Save(unitOfWork, newRequest);

                            OperationAccountHelper.Add(unitOfWork, newRequest, "{0} 位置 {1} 新的请求被创建", conveyor, location);
                        }

                        unitOfWork.Commit();

                        if (!processResult.Cancelled)
                        {
                            WmsNotifyProvider.OnRequest(newRequest);
                            String msg = string.Format("{0} 位置 {1} 新的请求 #{2} 被创建", conveyor, location.UserCode, newRequest.Id);
                            this.Logger.Info(msg, this, newRequest);
                        }
                    }

                    conveyor.SetOccupiedSignalHandshake(location, OccupiedSignalHandShake.ApplyToDelete);
                }
            }
            catch (Exception ex)
            {
                handled = false;
                this.Logger.Error(ex.Message, this, ex);
                this.Logger.Warning(String.Format("{0} 状态改变信号,但在处理时发生错误", signal), this, signal);
            }
            finally
            {
                if (_readerWriterLockSlimManager[lockSlimKey].IsWriteLockHeld)
                {
                    _readerWriterLockSlimManager[lockSlimKey].ExitWriteLock();
                    _readerWriterLockSlimManager.Remove(lockSlimKey);
                }
            }   
        }
  
        private void OnEntityUpdated<TEntity>(TEntity entity)
        {
            if (EntityUpdated == null)
            {
                return;
            }

            //Action<object> act=(obj)=>
            //{
            //    EntityUpdated.Invoke(obj);
            //};

            //if (entity is Task)
            //{
            //    act.BeginInvoke(entity, null, null);
            //    //EntityUpdated.Invoke(entity);
            //    foreach (var movement in (entity as Task).Movements)
            //    {
            //        //EntityUpdated.Invoke(movement);
            //        act.BeginInvoke(movement, null, null);
            //        foreach (var action in movement.EquipmentActions)
            //        {
            //            act.BeginInvoke(action, null, null);
            //            //EntityUpdated.Invoke(action);
            //        }
            //    }
            //}
            //else if (entity is LogicMovement)
            //{
            //    var movement = entity as LogicMovement;
            //    //EntityUpdated.Invoke(movement.Task);
            //    //EntityUpdated.Invoke(movement);
            //    act.BeginInvoke(movement.Task, null, null);
            //    act.BeginInvoke(movement, null, null);
            //    foreach (var action in movement.EquipmentActions)
            //    {
            //        act.BeginInvoke(action, null, null);
            //        //EntityUpdated.Invoke(action);
            //    }
            //}
            //else if (entity is EquipmentAction)
            //{
            //    var action = entity as EquipmentAction;
            //    //EntityUpdated.Invoke(action.Movement.Task);
            //    //EntityUpdated.Invoke(action.Movement);
            //    //EntityUpdated.Invoke(action);
            //    act.BeginInvoke(action.Movement.Task, null, null);
            //    act.BeginInvoke(action.Movement, null, null);
            //    act.BeginInvoke(action, null, null);
            //}
            //else
            //{
            //    act.BeginInvoke(entity, null, null);
            //}

            System.Threading.ThreadPool.QueueUserWorkItem((state) =>
            {
                EntityUpdated.Invoke(state);
            }, entity);
            //EntityUpdated.Invoke(entity);
            //EntityUpdated.BeginInvoke(entity,null,null);
        }

        private void OnEntityDeleted<TEntity>(TEntity entity)
        {
            if (EntityDeleted != null)
            {
                EntityDeleted.Invoke(entity);
            }
        }
    }
}
