using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Wcs;
using NHibernate.Linq;
using Wcs.Framework.Events;
namespace Wcs.Framework
{
    public static class TaskHelper
    {
        static Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        /// <summary>
        /// 暂停指定的任务
        /// </summary>
        /// <param name="taskId">任务 Id</param>
        /// <remarks>
        /// 
        /// </remarks>
        public static void Suspend(String taskCode)
        {
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                _logger.Trace1(String.Format("准备暂停 任务号 为 {0} 的 Task 对象", taskCode), null);
                Task tsk = unitOfWork.session.Query<Task>().FirstOrDefault(x => x.TaskCode == taskCode);

                if (tsk == null)
                {
                    Exception ex = new ApplicationException(String.Format("未找到 任务号 为 {0} 的 Task 对象", taskCode));
                    _logger.Error1(ex, null);
                    throw ex;
                }

                var tasks = unitOfWork.session.Query<Task>().Where(x => x.Group == tsk.Group && x.Group != null).ToList();
                _logger.Trace1(String.Format("{0} 属于组 {1}，该组有 {2} 个任务，准备暂停本组所有任务.", tsk, tsk.Group, tasks.Count), null, tsk);

                List<Wcs.Framework.EventBus.IEvent> events = new List<Wcs.Framework.EventBus.IEvent>();

                foreach (var task in tasks)
                {
                    _logger.Trace1(String.Format("开始处理组 {0} 中的任务 {1}...", task.Group, task), null, task);

                    if (task.Status == TaskStatus.Suspend || task.Status == TaskStatus.Cancelled || task.Status == TaskStatus.Completed)
                    {
                        _logger.Trace1(String.Format("{0} 已处于 {1} 状态，忽略本次操作。", task, task.Status), null, task);
                        continue;
                    }

                    if (task.Status != TaskStatus.Executing && task.Status != TaskStatus.New)
                    {
                        Exception ex = new InvalidOperationException(String.Format("无法暂停 {0},因为它当前正处于 {1} 状态", task, task.Status.GetDescription()));
                        _logger.Error1(ex, null, task);
                        throw ex;
                    }

                    foreach (var movement in task.Movements)
                    {
                        if (movement.Status != LogicMovementStatus.Executing && movement.Status != LogicMovementStatus.New)
                        {
                            continue;
                        }

                        foreach (var action in movement.EquipmentActions)
                        {
                            if (action.Status != EquipmentActionStatus.Executing && action.Status != EquipmentActionStatus.New)
                            {
                                continue;
                            }

                            if (action.Status == EquipmentActionStatus.Executing)
                            {
                                _logger.Trace1(String.Format("{0} 处理 {1} 状态，准备取消设备任务", action, action.Status.GetDescription()), null, action);

                                var device = Wcs.Framework.DeviceConverter.ToDevice<Wcs.Framework.TaskableDevice>(action.DeviceName);

                                if (device is IEditableTaskOwner)
                                {
                                    var editableTaskOwner = (IEditableTaskOwner)device;
                                    Boolean cancelled;
                                    editableTaskOwner.Suspend(action, out cancelled);

                                    if (cancelled)
                                    {
                                        throw new Exception(string.Format("已取消暂停 {0} 的操作.", task));
                                    }
                                }
                                else
                                {
                                    //如果是物理设备的任务，只有执行中的任务才需要发送取消指令
                                    if (action.Status == EquipmentActionStatus.Executing)
                                    {
                                        device.CancelTask(action);
                                    }
                                }

                                _logger.Trace1(String.Format("{0} 设备任务取消成功", action), null, action);
                            }

                            action.Status = EquipmentActionStatus.Suspend;

                            events.Add(new Events.EquipmentActionStatusChangedEvent(action.Movement.Task.Id, action.Movement.Id, action.Id, action.Status));

                            _logger.Trace1(String.Format("{0} 状态更改为 {1}", action, action.Status.GetDescription()), null, action);
                        }

                        movement.Status = LogicMovementStatus.Suspend;

                        events.Add(new Events.LogicMovementStatusChangedEvent(movement.Id, movement.Task.Id, movement.Status));

                        _logger.Trace1(String.Format("{0} 状态更改为 {1}", movement, movement.Status.GetDescription()), null, movement);
                    }

                    task.Status = TaskStatus.Suspend;

                    events.Add(new Events.TaskStatusChangedEvent(task.Id, task.TaskCode, task.Status, task.BizType, task.Source, task.TaskType));

                    _logger.Trace1(String.Format("{0} 状态更改为 {1}", task, task.Status.GetDescription()), null, task);

                    _logger.Info1(String.Format("{0} 暂停成功", task), null, task);
                }

                unitOfWork.Commit();

                Wcs.Framework.EventBus.EventBus.Instance.Publish(events.ToArray());

                MessageBoard.AbstractMessageBoard.Instance.Add(
                    new MessageBoard.Messages.TipMessage(MessageBoard.MessageLevel.Warning,
                        "任务处理",
                        String.Format("任务 {0} 被 {1} 暂停。", tsk.TaskCode, Wcs.Security.WcsPrincipal.CurrentPrincipal.Identity.Name)
                        , null));

            }
        }
        /// <summary>
        /// 暂停指定的任务
        /// </summary>
        /// <param name="taskId">任务 Id</param>
        /// <remarks>
        /// 
        /// </remarks>
        public static void Suspend(Int32 taskId)
        {
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                _logger.Trace1(String.Format("准备暂停 id 为 {0} 的 Task 对象", taskId), null);
                Task tsk = unitOfWork.session.Get<Task>(taskId);

                if (tsk == null)
                {
                    Exception ex = new ApplicationException(String.Format("未找到 id 为 {0} 的 Task 对象", taskId));
                    _logger.Error1(ex, null);
                    throw ex;
                }

                var tasks = unitOfWork.session.Query<Task>().Where(x => x.Group == tsk.Group && x.Group != null).ToList();
                _logger.Trace1(String.Format("{0} 属于组 {1}，该组有 {2} 个任务，准备暂停本组所有任务.", tsk, tsk.Group, tasks.Count), null, tsk);

                List<Wcs.Framework.EventBus.IEvent> events = new List<Wcs.Framework.EventBus.IEvent>();

                foreach (var task in tasks)
                {
                    _logger.Trace1(String.Format("开始处理组 {0} 中的任务 {1}...", task.Group, task), null, task);

                    if (task.Status == TaskStatus.Suspend || task.Status == TaskStatus.Cancelled || task.Status == TaskStatus.Completed)
                    {
                        _logger.Trace1(String.Format("{0} 已处于 {1} 状态，忽略本次操作。", task, task.Status), null, task);
                        continue;
                    }

                    if (task.Status != TaskStatus.Executing && task.Status != TaskStatus.New)
                    {
                        Exception ex = new InvalidOperationException(String.Format("无法暂停 {0},因为它当前正处于 {1} 状态", task, task.Status.GetDescription()));
                        _logger.Error1(ex, null, task);
                        throw ex;
                    }

                    foreach (var movement in task.Movements)
                    {
                        if (movement.Status != LogicMovementStatus.Executing && movement.Status != LogicMovementStatus.New)
                        {
                            continue;
                        }

                        foreach (var action in movement.EquipmentActions)
                        {
                            if (action.Status != EquipmentActionStatus.Executing && action.Status != EquipmentActionStatus.New)
                            {
                                continue;
                            }

                            if (action.Status == EquipmentActionStatus.Executing)
                            {
                                _logger.Trace1(String.Format("{0} 处理 {1} 状态，准备取消设备任务", action, action.Status.GetDescription()), null, action);

                                var device = Wcs.Framework.DeviceConverter.ToDevice<Wcs.Framework.TaskableDevice>(action.DeviceName);

                                if (device is IEditableTaskOwner)
                                {
                                    var editableTaskOwner = (IEditableTaskOwner)device;
                                    Boolean cancelled;
                                    editableTaskOwner.Suspend(action, out cancelled);

                                    if (cancelled)
                                    {
                                        throw new Exception(string.Format("已取消暂停 {0} 的操作.", task));
                                    }
                                }
                                else
                                {
                                    //如果是物理设备的任务，只有执行中的任务才需要发送取消指令
                                    if (action.Status == EquipmentActionStatus.Executing)
                                    {
                                        device.CancelTask(action);
                                    }
                                }

                                _logger.Trace1(String.Format("{0} 设备任务取消成功", action), null, action);
                            }

                            action.Status = EquipmentActionStatus.Suspend;

                            events.Add(new Events.EquipmentActionStatusChangedEvent(action.Movement.Task.Id, action.Movement.Id, action.Id, action.Status));

                            _logger.Trace1(String.Format("{0} 状态更改为 {1}", action, action.Status.GetDescription()), null, action);
                        }

                        movement.Status = LogicMovementStatus.Suspend;

                        events.Add(new Events.LogicMovementStatusChangedEvent(movement.Id, movement.Task.Id, movement.Status));

                        _logger.Trace1(String.Format("{0} 状态更改为 {1}", movement, movement.Status.GetDescription()), null, movement);
                    }

                    task.Status = TaskStatus.Suspend;

                    events.Add(new Events.TaskStatusChangedEvent(task.Id, task.TaskCode, task.Status, task.BizType, task.Source, task.TaskType));

                    _logger.Trace1(String.Format("{0} 状态更改为 {1}", task, task.Status.GetDescription()), null, task);

                    _logger.Info1(String.Format("{0} 暂停成功", task), null, task);
                }

                unitOfWork.Commit();

                Wcs.Framework.EventBus.EventBus.Instance.Publish(events.ToArray());

                MessageBoard.AbstractMessageBoard.Instance.Add(
                    new MessageBoard.Messages.TipMessage(MessageBoard.MessageLevel.Warning,
                        "任务处理",
                        String.Format("任务 {0} 被 {1} 暂停。", tsk.TaskCode, Wcs.Security.WcsPrincipal.CurrentPrincipal.Identity.Name)
                        , null));

            }
        }

        public static void Cancle(dynamic obj, Int32 Id)
        {
            if (obj is Task)
                CancelTask(Id);
            else if (obj is LogicMovement)
                CancleLogicMovement(Id);
            else if (obj is EquipmentAction)
                CancleEquipmentAction(Id);
            else
                throw new NotImplementedException(string.Format("未实现对 {0} 类型的任务的强制完成处理。", obj.GetType()));
        }
        /// <summary>
        /// 取消指定的Task
        /// </summary>
        /// <param name="taskId">任务 Id</param>
        public static void CancelTask(String taskCode)
        {
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                _logger.Trace1(String.Format("准备取消 任务号 为 {0} 的 Task 对象", taskCode), null);
                Task tsk = unitOfWork.session.Query<Task>().FirstOrDefault(x => x.TaskCode == taskCode);
                if (tsk == null)
                {
                    Exception ex = new ApplicationException(String.Format("未找到 任务号 为 {0} 的 Task 对象", taskCode));
                    _logger.Error1(ex, null);
                    throw ex;
                }

                var tasks = unitOfWork.session.Query<Task>().Where(x => x.Group == tsk.Group && x.Group != null).ToList();
                _logger.Trace1(String.Format("{0} 属于组 {1}，该组有 {2} 个任务，准备取消本组所有任务.", tsk, tsk.Group, tasks.Count), null, tsk);

                List<Wcs.Framework.EventBus.IEvent> events = new List<Wcs.Framework.EventBus.IEvent>();

                foreach (var task in tasks)
                {
                    _logger.Trace1(String.Format("开始处理组 {0} 中的任务 {1}...", task.Group, task), null, task);
                    if (task.Status == TaskStatus.Completed || task.Status == TaskStatus.Cancelled)
                    {
                        _logger.Trace1(String.Format("{0} 已处于 {1} 状态，忽略本次操作。", task, task.Status), null, task);
                        continue;
                    }

                    if (task.Status != TaskStatus.Suspend && task.Status == TaskStatus.Error)
                    {
                        Exception ex = new InvalidOperationException(String.Format("无法取消 {0},因为它当前正处于 {1} 状态。只有处理“暂停”或“错误”的任务可以被取消", task, task.Status.GetDescription()));
                        _logger.Error1(ex, null, task);
                        throw ex;
                    }


                    foreach (var movement in task.Movements)
                    {
                        if (movement.Status == LogicMovementStatus.Cancelled || movement.Status == LogicMovementStatus.Completed)
                        {
                            continue;
                        }

                        if (movement.Status != LogicMovementStatus.Error && movement.Status != LogicMovementStatus.Suspend)
                        {
                            Exception ex = new InvalidOperationException(String.Format("无法取消 {0},因为它当前正处于 {1} 状态。只有处理“暂停”或“错误”的任务可以被取消", movement, movement.Status.GetDescription()));
                            _logger.Error1(ex, null, task);
                            throw ex;
                        }

                        foreach (var action in movement.EquipmentActions)
                        {
                            if (action.Status == EquipmentActionStatus.Cancelled || action.Status == EquipmentActionStatus.Completed)
                            {
                                continue;
                            }

                            if (action.Status != EquipmentActionStatus.Error && action.Status != EquipmentActionStatus.Suspend)
                            {
                                Exception ex = new InvalidOperationException(String.Format("无法取消 {0},因为它当前正处于 {1} 状态。只有处理“暂停”或“错误”的任务可以被继续执行", action, action.Status.GetDescription()));
                                _logger.Error1(ex, null, task);
                                throw ex;
                            }

                            var device = DeviceConverter.ToDevice<TaskableDevice>(action.DeviceName);
                            if (device is IEditableTaskOwner)
                            {
                                var editableTaskOwner = (IEditableTaskOwner)device;
                                Boolean cancelled;
                                editableTaskOwner.Cancel(action, out cancelled);

                                if (cancelled)
                                {
                                    throw new Exception(string.Format("已取消取消 {0} 的操作.", task));
                                }
                            }
                            //如果是物理设备的任务，已暂停或发生错误的任务不再需要向设备发送取消指令
                            //else
                            //{
                            //    device.CancelTask(action);
                            //}

                            action.Status = EquipmentActionStatus.Cancelled;
                            action.FinishedAt = DateTime.Now;
                            if (action.SentAt == null)
                                action.SentAt = action.FinishedAt;

                            DeviceConverter.ToDevice<TaskableDevice>(action.DeviceName).EquipmentActionScheduler.Remove(action);

                            events.Add(new Events.EquipmentActionStatusChangedEvent(action.Movement.Task.Id, action.Movement.Id, action.Id, action.Status));

                            _logger.Trace1(String.Format("{0} 状态更改为 {1}", action, action.Status.GetDescription()), null, action);
                        }

                        movement.Status = LogicMovementStatus.Cancelled;
                        movement.FinishedAt = DateTime.Now;

                        events.Add(new Events.LogicMovementStatusChangedEvent(movement.Id, movement.Task.Id, movement.Status));

                        _logger.Trace1(String.Format("{0} 状态更改为 {1}", movement, movement.Status.GetDescription()), null, movement);
                    }

                    task.Status = TaskStatus.Cancelled;
                    task.FinishedAt = DateTime.Now;

                    var total = task.FinishedAt.Value.Subtract(task.CreatedAt);
                    if (task.AdditionalInfo.ContainsKey("TTS")) 
                        task.AdditionalInfo["TTS"] = total.ToString();
                    else
                        task.AdditionalInfo.Add("TTS", total.ToString());

                    var rts = CalTaskRunningTimeSpan(task);
                    if (rts == null)
                        rts = new TimeSpan();
                    if (task.AdditionalInfo.ContainsKey("RTS"))
                        task.AdditionalInfo["RTS"] = rts.ToString();
                    else
                        task.AdditionalInfo.Add("RTS", rts.ToString());

                    var wts = total - rts;
                    if (task.AdditionalInfo.ContainsKey("WTS"))
                        task.AdditionalInfo["WTS"] = wts.ToString();
                    else
                        task.AdditionalInfo.Add("WTS", wts.ToString());

                    events.Add(new Events.TaskStatusChangedEvent(task.Id, task.TaskCode, task.Status, task.BizType, task.Source, task.TaskType));
                    events.Add(new Events.TaskFinishedEvent(task.Id, task.TaskCode, task.Status, task.BizType, task.Source, task.TaskType, task.FinishedAt.Value));


                    _logger.Trace1(String.Format("{0} 状态更改为 {1}", task, task.Status.GetDescription()), null, task);

                    _logger.Info1(String.Format("{0} 取消成功", task), null, task);
                }

                unitOfWork.Commit();
                Wcs.Framework.EventBus.EventBus.Instance.Publish(events.ToArray());


                MessageBoard.AbstractMessageBoard.Instance.Add(
                    new MessageBoard.Messages.TipMessage(MessageBoard.MessageLevel.Warning,
                        "任务处理",
                        String.Format("任务 {0} 被 {1} 取消。", tsk.TaskCode, Wcs.Security.WcsPrincipal.CurrentPrincipal.Identity.Name)
                        , null));

            }
        }

        public static TimeSpan? CalTaskRunningTimeSpan(Task task)
        {
            try
            {
                if (task.Movements == null)
                    return null;
                TimeSpan runningTimeSpan = new TimeSpan();
                foreach (var logicMovement in task.Movements)
                {
                    if (logicMovement.EquipmentActions == null)
                        continue;
                    foreach (var action in logicMovement.EquipmentActions)
                    {
                        if (action.FinishedAt == null)
                            continue;
                        if (action.SentAt == null)
                            continue;

                        runningTimeSpan += action.FinishedAt.Value - action.SentAt.Value;
                    }
                }
                return runningTimeSpan;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 取消指定的Task
        /// </summary>
        /// <param name="taskId">任务 Id</param>
        public static void CancelTask(Int32 taskId)
        {
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                _logger.Trace1(String.Format("准备取消 id 为 {0} 的 Task 对象", taskId), null);
                Task tsk = unitOfWork.session.Get<Task>(taskId);
                if (tsk == null)
                {
                    Exception ex = new ApplicationException(String.Format("未找到 id 为 {0} 的 Task 对象", taskId));
                    _logger.Error1(ex, null);
                    throw ex;
                }

                var tasks = unitOfWork.session.Query<Task>().Where(x => x.Group == tsk.Group && x.Group != null).ToList();
                _logger.Trace1(String.Format("{0} 属于组 {1}，该组有 {2} 个任务，准备取消本组所有任务.", tsk, tsk.Group, tasks.Count), null, tsk);

                List<Wcs.Framework.EventBus.IEvent> events = new List<Wcs.Framework.EventBus.IEvent>();

                foreach (var task in tasks)
                {
                    _logger.Trace1(String.Format("开始处理组 {0} 中的任务 {1}...", task.Group, task), null, task);
                    if (task.Status == TaskStatus.Completed || task.Status == TaskStatus.Cancelled)
                    {
                        _logger.Trace1(String.Format("{0} 已处于 {1} 状态，忽略本次操作。", task, task.Status), null, task);
                        continue;
                    }

                    if (task.Status != TaskStatus.Suspend && task.Status == TaskStatus.Error)
                    {
                        Exception ex = new InvalidOperationException(String.Format("无法取消 {0},因为它当前正处于 {1} 状态。只有处理“暂停”或“错误”的任务可以被取消", task, task.Status.GetDescription()));
                        _logger.Error1(ex, null, task);
                        throw ex;
                    }


                    foreach (var movement in task.Movements)
                    {
                        if (movement.Status == LogicMovementStatus.Cancelled || movement.Status == LogicMovementStatus.Completed)
                        {
                            continue;
                        }

                        if (movement.Status != LogicMovementStatus.Error && movement.Status != LogicMovementStatus.Suspend)
                        {
                            Exception ex = new InvalidOperationException(String.Format("无法取消 {0},因为它当前正处于 {1} 状态。只有处理“暂停”或“错误”的任务可以被取消", movement, movement.Status.GetDescription()));
                            _logger.Error1(ex, null, task);
                            throw ex;
                        }

                        foreach (var action in movement.EquipmentActions)
                        {
                            if (action.Status == EquipmentActionStatus.Cancelled || action.Status == EquipmentActionStatus.Completed)
                            {
                                continue;
                            }

                            if (action.Status != EquipmentActionStatus.Error && action.Status != EquipmentActionStatus.Suspend)
                            {
                                Exception ex = new InvalidOperationException(String.Format("无法取消 {0},因为它当前正处于 {1} 状态。只有处理“暂停”或“错误”的任务可以被继续执行", action, action.Status.GetDescription()));
                                _logger.Error1(ex, null, task);
                                throw ex;
                            }

                            var device = DeviceConverter.ToDevice<TaskableDevice>(action.DeviceName);
                            if (device is IEditableTaskOwner)
                            {
                                var editableTaskOwner = (IEditableTaskOwner)device;
                                Boolean cancelled;
                                editableTaskOwner.Cancel(action, out cancelled);

                                if (cancelled)
                                {
                                    throw new Exception(string.Format("已取消取消 {0} 的操作.", task));
                                }
                            }
                            //如果是物理设备的任务，已暂停或发生错误的任务不再需要向设备发送取消指令
                            //else
                            //{
                            //    device.CancelTask(action);
                            //}

                            action.Status = EquipmentActionStatus.Cancelled;
                            action.FinishedAt = DateTime.Now;
                            if (action.SentAt == null)
                                action.SentAt = action.FinishedAt;

                            DeviceConverter.ToDevice<TaskableDevice>(action.DeviceName).EquipmentActionScheduler.Remove(action);

                            events.Add(new Events.EquipmentActionStatusChangedEvent(action.Movement.Task.Id, action.Movement.Id, action.Id, action.Status));

                            _logger.Trace1(String.Format("{0} 状态更改为 {1}", action, action.Status.GetDescription()), null, action);
                        }

                        movement.Status = LogicMovementStatus.Cancelled;
                        movement.FinishedAt = DateTime.Now;

                        events.Add(new Events.LogicMovementStatusChangedEvent(movement.Id, movement.Task.Id, movement.Status));

                        _logger.Trace1(String.Format("{0} 状态更改为 {1}", movement, movement.Status.GetDescription()), null, movement);
                    }

                    task.Status = TaskStatus.Cancelled;
                    task.FinishedAt = DateTime.Now;

                    var total = task.FinishedAt.Value.Subtract(task.CreatedAt);
                    if (task.AdditionalInfo.ContainsKey("TTS"))
                        task.AdditionalInfo["TTS"] = total.ToString();
                    else
                        task.AdditionalInfo.Add("TTS", total.ToString());

                    var rts = CalTaskRunningTimeSpan(task);
                    if (rts == null)
                        rts = new TimeSpan();
                    if (task.AdditionalInfo.ContainsKey("RTS"))
                        task.AdditionalInfo["RTS"] = rts.ToString();
                    else
                        task.AdditionalInfo.Add("RTS", rts.ToString());

                    var wts = total - rts;
                    if (task.AdditionalInfo.ContainsKey("WTS"))
                        task.AdditionalInfo["WTS"] = wts.ToString();
                    else
                        task.AdditionalInfo.Add("WTS", wts.ToString());

                    events.Add(new Events.TaskStatusChangedEvent(task.Id, task.TaskCode, task.Status, task.BizType, task.Source, task.TaskType));
                    events.Add(new Events.TaskFinishedEvent(task.Id, task.TaskCode, task.Status, task.BizType, task.Source, task.TaskType, task.FinishedAt.Value));


                    _logger.Trace1(String.Format("{0} 状态更改为 {1}", task, task.Status.GetDescription()), null, task);

                    _logger.Info1(String.Format("{0} 取消成功", task), null, task);
                }

                unitOfWork.Commit();
                Wcs.Framework.EventBus.EventBus.Instance.Publish(events.ToArray());


                MessageBoard.AbstractMessageBoard.Instance.Add(
                    new MessageBoard.Messages.TipMessage(MessageBoard.MessageLevel.Warning,
                        "任务处理",
                        String.Format("任务 {0} 被 {1} 取消。", tsk.TaskCode, Wcs.Security.WcsPrincipal.CurrentPrincipal.Identity.Name)
                        , null));

            }
        }
        /// <summary>
        /// 取消指定的LogicMovement
        /// </summary>
        /// <param name="logicMovementId"></param>
        public static void CancleLogicMovement(Int32 logicMovementId)
        {
            List<Wcs.Framework.EventBus.IEvent> events = new List<Wcs.Framework.EventBus.IEvent>();

            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                _logger.Trace1(String.Format("准备取消 id 为 {0} 的 LogicMovement 对象", logicMovementId), null);
                LogicMovement movement = unitOfWork.session.Get<LogicMovement>(logicMovementId);
                if (movement.Status == LogicMovementStatus.Cancelled || movement.Status == LogicMovementStatus.Completed)
                {
                    _logger.Info(String.Join("逻辑动作 {0} 已经处于 取消/完成状态，不需要重复执行", movement), typeof(TaskHelper));
                    return;
                }

                if (movement.Status != LogicMovementStatus.Error && movement.Status != LogicMovementStatus.Suspend)
                {
                    Exception ex = new InvalidOperationException(String.Format("无法取消 {0},因为它当前正处于 {1} 状态。只有处理“暂停”或“错误”的任务可以被取消", movement, movement.Status.GetDescription()));
                    _logger.Error1(ex, null, movement);
                    throw ex;
                }

                foreach (var action in movement.EquipmentActions)
                {
                    if (action.Status == EquipmentActionStatus.Cancelled || action.Status == EquipmentActionStatus.Completed)
                    {
                        continue;
                    }

                    if (action.Status != EquipmentActionStatus.Error && action.Status != EquipmentActionStatus.Suspend)
                    {
                        Exception ex = new InvalidOperationException(String.Format("无法取消 {0},因为它当前正处于 {1} 状态。只有处理“暂停”或“错误”的任务可以被继续执行", action, action.Status.GetDescription()));
                        _logger.Error1(ex, null, movement);
                        throw ex;
                    }

                    var device = DeviceConverter.ToDevice<TaskableDevice>(action.DeviceName);
                    if (device is IEditableTaskOwner)
                    {
                        var editableTaskOwner = (IEditableTaskOwner)device;
                        Boolean cancelled;
                        editableTaskOwner.Cancel(action, out cancelled);

                        if (cancelled)
                        {
                            throw new Exception(string.Format("已取消取消 {0} 的操作.", movement));
                        }
                    }
                    //如果是物理设备的任务，已暂停或发生错误的任务不再需要向设备发送取消指令
                    //else
                    //{
                    //    device.CancelTask(action);
                    //}

                    action.Status = EquipmentActionStatus.Cancelled;
                    action.FinishedAt = DateTime.Now;
                    if (action.SentAt == null)
                        action.SentAt = action.FinishedAt;

                    DeviceConverter.ToDevice<TaskableDevice>(action.DeviceName).EquipmentActionScheduler.Remove(action);

                    events.Add(new Events.EquipmentActionStatusChangedEvent(action.Movement.Task.Id, action.Movement.Id, action.Id, action.Status));

                    _logger.Trace1(String.Format("{0} 状态更改为 {1}", action, action.Status.GetDescription()), null, action);
                }

                movement.Status = LogicMovementStatus.Cancelled;
                movement.FinishedAt = DateTime.Now;

                events.Add(new Events.LogicMovementStatusChangedEvent(movement.Id, movement.Task.Id, movement.Status));

                _logger.Trace1(String.Format("{0} 状态更改为 {1}", movement, movement.Status.GetDescription()), null, movement);

                unitOfWork.Commit();
                Wcs.Framework.EventBus.EventBus.Instance.Publish(events.ToArray());

                MessageBoard.AbstractMessageBoard.Instance.Add(
                    new MessageBoard.Messages.TipMessage(MessageBoard.MessageLevel.Warning,
                        "任务处理",
                        String.Format("逻辑任务 {0} 被 {1} 取消。", movement, Wcs.Security.WcsPrincipal.CurrentPrincipal.Identity.Name)
                        , null));
            }
        }
        /// <summary>
        /// 取消指定的EquipmentAction
        /// </summary>
        /// <param name="equipmentTaskId"></param>
        public static void CancleEquipmentAction(Int32 equipmentTaskId)
        {
            List<Wcs.Framework.EventBus.IEvent> events = new List<Wcs.Framework.EventBus.IEvent>();

            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                _logger.Trace1(String.Format("准备取消 id 为 {0} 的 EquipmentAction 对象", equipmentTaskId), null);
                EquipmentAction action = unitOfWork.session.Get<EquipmentAction>(equipmentTaskId);
                if (action.Status == EquipmentActionStatus.Cancelled || action.Status == EquipmentActionStatus.Completed)
                {
                    _logger.Info(String.Join("物理动作 {0} 已经处于 取消/完成状态，不需要重复执行", action), action);
                    return;
                }

                if (action.Status != EquipmentActionStatus.Suspend && action.Status != EquipmentActionStatus.Error)
                {
                    Exception ex = new InvalidOperationException(String.Format("无法取消 {0},因为它当前正处于 {1} 状态。只有处理“暂停”或“错误”的物理动作可以被取消", action, action.Status.GetDescription()));
                    _logger.Error1(ex, null, action);
                    throw ex;
                }

                action.Status = EquipmentActionStatus.Cancelled;
                action.FinishedAt = DateTime.Now;
                if (action.SentAt == null)
                    action.SentAt = action.FinishedAt;

                events.Add(new Events.EquipmentActionStatusChangedEvent(action.Movement.Task.Id,action.Movement.Id,action.Id, EquipmentActionStatus.Cancelled));

                _logger.Trace1(String.Format("{0} 状态更改为 {1}", action, action.Status.GetDescription()), null, action);

                unitOfWork.Commit();
                Wcs.Framework.EventBus.EventBus.Instance.Publish(events.ToArray());

                MessageBoard.AbstractMessageBoard.Instance.Add(
                    new MessageBoard.Messages.TipMessage(MessageBoard.MessageLevel.Warning,
                        "任务处理",
                        String.Format("物理任务 {0} 被 {1} 取消。", action, Wcs.Security.WcsPrincipal.CurrentPrincipal.Identity.Name)
                        , null));
            }
        }

        /// <summary>
        /// 强制完成指定的任务
        /// </summary>
        /// <param name="taskId">任务 Id</param>
        public static void Complete(Int32 taskId)
        {
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                _logger.Trace1(String.Format("准备强制完成 id 为 {0} 的 Task 对象", taskId), null);
                Task tsk = unitOfWork.session.Get<Task>(taskId);
                if (tsk == null)
                {
                    Exception ex = new ApplicationException(String.Format("未找到 id 为 {0} 的 Task 对象", taskId));
                    _logger.Error1(ex, null);
                    throw ex;
                }

                var tasks = unitOfWork.session.Query<Task>().Where(x => x.Group == tsk.Group && x.Group != null).ToList();
                _logger.Trace1(String.Format("{0} 属于组 {1}，该组有 {2} 个任务，准备取消本组所有任务.", tsk, tsk.Group, tasks.Count), null, tsk);

                List<Wcs.Framework.EventBus.IEvent> events = new List<Wcs.Framework.EventBus.IEvent>();

                foreach (var task in tasks)
                {
                    _logger.Trace1(String.Format("开始处理组 {0} 中的任务 {1}...", task.Group, task), null, task);
                    if (task.Status == TaskStatus.Completed || task.Status == TaskStatus.Cancelled)
                    {
                        _logger.Trace1(String.Format("{0} 已处于 {1} 状态，忽略本次操作。", task, task.Status), null, task);
                        continue;
                    }

                    if (task.Status != TaskStatus.Suspend && task.Status != TaskStatus.Error)
                    {
                        Exception ex = new InvalidOperationException(String.Format("无法强制完成 {0},因为它当前正处于 {1} 状态。只有处理“暂停”或“错误”的任务可以被强制完成", task, task.Status.GetDescription()));
                        _logger.Error1(ex, null, task);
                        throw ex;
                    }

                    foreach (var movement in task.Movements)
                    {
                        if (movement.Status == LogicMovementStatus.Cancelled || movement.Status == LogicMovementStatus.Completed)
                        {
                            continue;
                        }

                        foreach (var action in movement.EquipmentActions)
                        {
                            if (action.Status == EquipmentActionStatus.Cancelled || action.Status == EquipmentActionStatus.Completed)
                            {
                                continue;
                            }

                            var device = DeviceConverter.ToDevice<TaskableDevice>(action.DeviceName);
                            if (device is IEditableTaskOwner)
                            {
                                var editableTaskOwner = (IEditableTaskOwner)device;
                                Boolean cancelled;
                                editableTaskOwner.Cancel(action, out cancelled);

                                if (cancelled)
                                {
                                    throw new Exception(string.Format("已取消完成 {0} 的操作.", task));
                                }

                                editableTaskOwner.Complete(action, out cancelled);

                                if (cancelled)
                                {
                                    throw new Exception(string.Format("已取消完成 {0} 的操作.", action));
                                }
                            }
                            else
                            {
                                //如果是物理设备的任务，只有执行中的任务才需要发送取消指令
                                if (action.Status == EquipmentActionStatus.Executing)
                                {
                                    device.CancelTask(action);
                                }
                            }

                            action.Status = EquipmentActionStatus.Completed;
                            action.FinishedAt = DateTime.Now;
                            if (action.SentAt == null)
                                action.SentAt = action.FinishedAt;

                            device.EquipmentActionScheduler.Remove(action);

                            events.Add(new Events.EquipmentActionStatusChangedEvent(action.Movement.Task.Id, action.Movement.Id, action.Id, action.Status));

                            _logger.Trace1(String.Format("{0} 状态更改为 {1}", action, action.Status.GetDescription()), null, action);
                        }

                        movement.Status = LogicMovementStatus.Completed;
                        movement.FinishedAt = DateTime.Now;

                        events.Add(new Events.LogicMovementStatusChangedEvent(movement.Id, movement.Task.Id, movement.Status));

                        _logger.Trace1(String.Format("{0} 状态更改为 {1}", movement, movement.Status.GetDescription()), null, movement);
                    }

                    var lastMovement = task.Movements.OrderByDescending(x => x.Id).FirstOrDefault();
                    if (lastMovement != null)
                    {
                        if (!task.CurrentLocation.Equals(lastMovement.EndLocation))
                        {
                            _logger.Trace1(string.Format("{0} 的当前位置和 {1} 的结束位置不一致，更新 {1} 当前位置为 {2}", task, lastMovement, lastMovement.EndLocation), null, task);

                            task.CurrentLocation = lastMovement.EndLocation;
                            events.Add(new TaskCurrentLocationChangedEvent(task.Id, task.TaskCode, task.CurrentLocation));
                        }

                        if (task is UndefinedEndLocationTask)
                        {
                            UndefinedEndLocationTask uet = (UndefinedEndLocationTask)task;
                            //如果当前位置当前完成的物理动作（当前位置）已是可选结束位置的其中的一个，则任务报完成，并将当前位置设为结束点。
                            if (uet.AvailableEndLocations.Any(loc => loc.Equals(lastMovement.EndLocation)))
                            {
                                var oldEndLocation = task.EndLocation;
                                uet.EndLocation = lastMovement.EndLocation;

                                _logger.Info1(string.Format("{0} 是终点未明的任务，并且 {1} 的结束点（{2}）是其可选位置中的一个，修正任务的结束点为 {2}（原为{3}）",
                                    task,
                                    lastMovement,
                                    lastMovement.EndLocation,
                                    oldEndLocation
                                    ), null, lastMovement);
                            }
                        }
                    }

                    task.FinishedAt = DateTime.Now;
                    task.Status = TaskStatus.Completed;

                    var total = task.FinishedAt.Value.Subtract(task.CreatedAt);
                    if (task.AdditionalInfo.ContainsKey("TTS"))
                        task.AdditionalInfo["TTS"] = total.ToString();
                    else
                        task.AdditionalInfo.Add("TTS", total.ToString());

                    var rts = CalTaskRunningTimeSpan(task);
                    if (rts == null)
                        rts = new TimeSpan();
                    if (task.AdditionalInfo.ContainsKey("RTS"))
                        task.AdditionalInfo["RTS"] = rts.ToString();
                    else
                        task.AdditionalInfo.Add("RTS", rts.ToString());

                    var wts = total - rts;
                    if (task.AdditionalInfo.ContainsKey("WTS"))
                        task.AdditionalInfo["WTS"] = wts.ToString();
                    else
                        task.AdditionalInfo.Add("WTS", wts.ToString());

                    events.Add(new Events.TaskStatusChangedEvent(task.Id, task.TaskCode, task.Status, task.BizType, task.Source, task.TaskType));
                    events.Add(new Events.TaskFinishedEvent(task.Id, task.TaskCode, task.Status, task.BizType, task.Source, task.TaskType, task.FinishedAt.Value));

                    _logger.Trace1(String.Format("{0} 状态更改为 {1}", task, task.Status.GetDescription()), null, task);

                    task.SetManuallyCompleteInfo();

                    _logger.Info1(String.Format("{0} 强制完成成功", task), null, task);
                }

                unitOfWork.Commit();

                Wcs.Framework.EventBus.EventBus.Instance.Publish(events.ToArray());


                MessageBoard.AbstractMessageBoard.Instance.Add(
                    new MessageBoard.Messages.TipMessage(MessageBoard.MessageLevel.Warning,
                        "任务处理",
                        String.Format("任务 {0} 被 {1} 强制完成。", tsk.TaskCode, Wcs.Security.WcsPrincipal.CurrentPrincipal.Identity.Name)
                        , null));

            }
        }

        /// <summary>
        /// 强制完成指定的逻辑动作
        /// </summary>
        /// <param name="movementId">逻辑动作 Id</param>
        public static void CompleteMovement(Int32 movementId)
        {
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                _logger.Trace1(String.Format("准备强制完成 id 为 {0} 的 LogicMovement 对象", movementId), null);
                LogicMovement movement = unitOfWork.session.Get<LogicMovement>(movementId);
                if (movement == null)
                {
                    throw new ApplicationException(String.Format("未找到 id 为 {0} 的 LogicMovement 对象", movementId));
                }

                var groupTaskTotals = unitOfWork.session.Query<Task>().Where(x => x.Group == movement.Task.Group && x.Group != null).Count();

                if (groupTaskTotals > 1)
                {
                    throw new InvalidCastException(String.Format("{0} 属于组 {1}，该组有 {2} 个任务，所以无法强制完成逻辑动作.", movement.Task, movement.Task.Group, groupTaskTotals));
                }

                if (movement.Status != LogicMovementStatus.Suspend && movement.Status != LogicMovementStatus.Error)
                {
                    throw new InvalidOperationException(String.Format("无法强制完成 {0},因为它当前正处于 {1} 状态。只有处理“暂停”或“错误”的任务可以被强制完成", movement, movement.Status.GetDescription()));
                }
                List<Wcs.Framework.EventBus.IEvent> events = new List<Wcs.Framework.EventBus.IEvent>();

                movement.FinishedAt = DateTime.Now;
                movement.Status = LogicMovementStatus.Completed;
                events.Add(new Events.LogicMovementStatusChangedEvent(movement.Id, movement.Task.Id, movement.Status));

                _logger.Trace1(String.Format("{0} 状态更改为 {1}", movement, movement.Status.GetDescription()), null, movement);

                //if (!movement.Task.CurrentLocation.Equals(movement.EndLocation))
                if (!(movement.Task.CurrentLocation.UserCode == movement.EndLocation.UserCode || LocationConverter.ToLocation(movement.Task.CurrentLocation).Synonymous.Any(x => x.UserCode == LocationConverter.ToLocation(movement.EndLocation).UserCode))) ;
                {
                    movement.Task.CurrentLocation = movement.EndLocation;
                    events.Add(new TaskCurrentLocationChangedEvent(movement.Task.Id, movement.Task.TaskCode, movement.Task.CurrentLocation));
                }

                foreach (var action in movement.EquipmentActions)
                {
                    if (action.Status == EquipmentActionStatus.Cancelled || action.Status == EquipmentActionStatus.Completed)
                    {
                        continue;
                    }

                    var device = DeviceConverter.ToDevice<TaskableDevice>(action.DeviceName);
                    if (device is IEditableTaskOwner)
                    {
                        var editableTaskOwner = (IEditableTaskOwner)device;
                        Boolean cancelled;
                        editableTaskOwner.Cancel(action, out cancelled);

                        if (cancelled)
                        {
                            throw new Exception(string.Format("已取消完成 {0} 的操作.", action));
                        }

                        editableTaskOwner.Complete(action, out cancelled);

                        if (cancelled)
                        {
                            throw new Exception(string.Format("已取消完成 {0} 的操作.", action));
                        }
                    }
                    else
                    {
                        //如果是物理设备的任务，只有执行中的任务才需要发送取消指令
                        if (action.Status == EquipmentActionStatus.Executing)
                        {
                            device.CancelTask(action);
                        }
                    }

                    if (action.Movement != null && action.Movement.Task != null && action.Movement.Task is UndefinedEndLocationTask)
                    {
                        UndefinedEndLocationTask uet = (UndefinedEndLocationTask)action.Movement.Task;
                        //如果当前位置当前完成的物理动作（当前位置）已是可选结束位置的其中的一个，则任务报完成，并将当前位置设为结束点。
                        if (uet.AvailableEndLocations.Any(loc => loc.Equals(action.Movement.EndLocation)))
                        {
                            var oldEndLocation = action.Movement.Task.EndLocation;
                            uet.EndLocation = action.Movement.EndLocation;

                            _logger.Info1(string.Format("{0} 是终点未明的任务，并且 {1} 的结束点（{2}）是其可选位置中的一个，修正任务的结束点为 {2}（原为{3}）",
                                action.Movement.Task,
                                action.Movement,
                                action.Movement.EndLocation,
                                oldEndLocation
                                ), null, action);
                        }
                        else
                        {

                            _logger.Trace1(string.Format("{0} 是终点未明的任务，当前结束点是 {1}，准备尝试重新分配结束位置...", action.Movement.Task, action.Movement.Task.EndLocation), null, action);

                            var oldEndLocation = action.Movement.Task.EndLocation;

                            ((UndefinedEndLocationTask)action.Movement.Task).AssignNewEndLocation();

                            _logger.Info1(string.Format("{0} 原结束位置为 {1}，重新分配的结束位置为 {2}", action.Movement.Task, oldEndLocation, action.Movement.Task.EndLocation), null, action);
                        }


                        //_logger.Trace1(string.Format("{0} 是终点未明的任务，当前结束点是 {1}，准备尝试重新分配结束位置...", action.Movement.Task, action.Movement.Task.EndLocation), null, action);

                        //var oldEndLocation = action.Movement.Task.EndLocation;

                        //((UndefinedEndLocationTask)action.Movement.Task).AssignNewEndLocation();

                        //_logger.Info1(string.Format("为 {0} 原结束位置为 {1}，重新分配的结束位置为 {2}", action.Movement.Task, oldEndLocation, action.Movement.Task.EndLocation), null, action);

                    }

                    action.Status = EquipmentActionStatus.Completed;
                    action.FinishedAt = DateTime.Now;
                    if (action.SentAt == null)
                        action.SentAt = action.FinishedAt;

                    device.EquipmentActionScheduler.Remove(action);

                    events.Add(new Events.EquipmentActionStatusChangedEvent(action.Movement.Task.Id, action.Movement.Id, action.Id, action.Status));

                    _logger.Trace1(String.Format("{0} 状态更改为 {1}", action, action.Status.GetDescription()), null, action);
                }


                if (
                    LocationConverter
                    .ToLocation(movement.Task.CurrentLocation)
                    .Equals(
                        LocationConverter.ToLocation(movement.Task.EndLocation)
                        )
                    )
                //if (movement.Task.CurrentLocation.Equals(movement.Task.EndLocation))
                {
                    movement.Task.Status = TaskStatus.Completed;
                    movement.Task.FinishedAt = movement.FinishedAt;
                    _logger.Trace1(String.Format("{0} 状态更改为 {1}", movement.Task, movement.Task.Status.GetDescription()), null, movement.Task);

                    var total = movement.Task.FinishedAt.Value.Subtract(movement.Task.CreatedAt);
                    if (movement.Task.AdditionalInfo.ContainsKey("TTS"))
                        movement.Task.AdditionalInfo["TTS"] = total.ToString();
                    else
                        movement.Task.AdditionalInfo.Add("TTS", total.ToString());

                    var rts = CalTaskRunningTimeSpan(movement.Task);
                    if (rts == null)
                        rts = new TimeSpan();
                    if (movement.Task.AdditionalInfo.ContainsKey("RTS"))
                        movement.Task.AdditionalInfo["RTS"] = rts.ToString();
                    else
                        movement.Task.AdditionalInfo.Add("RTS", rts.ToString());

                    var wts = total - rts;
                    if (movement.Task.AdditionalInfo.ContainsKey("WTS"))
                        movement.Task.AdditionalInfo["WTS"] = wts.ToString();
                    else
                        movement.Task.AdditionalInfo.Add("WTS", wts.ToString());

                    movement.Task.SetManuallyCompleteInfo();

                    events.Add(new TaskStatusChangedEvent(movement.Task.Id, movement.Task.TaskCode, movement.Task.Status, movement.Task.BizType, movement.Task.Source, movement.Task.TaskType));
                    events.Add(new TaskFinishedEvent(movement.Task.Id, movement.Task.TaskCode, movement.Task.Status, movement.Task.BizType, movement.Task.Source, movement.Task.TaskType, movement.Task.FinishedAt.Value));
                }


                unitOfWork.Commit();

                Wcs.Framework.EventBus.EventBus.Instance.Publish(events.ToArray());

                _logger.Info1(String.Format("{0} 强制完成成功", movement), null, movement);


                MessageBoard.AbstractMessageBoard.Instance.Add(
                    new MessageBoard.Messages.TipMessage(MessageBoard.MessageLevel.Warning,
                        "任务处理",
                        String.Format("任务 {0} 的逻辑动作 {1} 被 {2} 强制完成。", movement.Task.TaskCode, movement.Id, Wcs.Security.WcsPrincipal.CurrentPrincipal.Identity.Name)
                        , null));
            }
        }

        /// <summary>
        /// 强制完成指定的物理动作
        /// </summary>
        /// <param name="actionId">物理动作 Id</param>
        public static void CompleteAction(Int32 actionId)
        {
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                _logger.Trace1(String.Format("准备强制完成 id 为 {0} 的 EquipmentAction 对象", actionId), null);
                EquipmentAction currentAction = unitOfWork.session.Get<EquipmentAction>(actionId);
                if (currentAction == null)
                {
                    throw new ApplicationException(String.Format("未找到 id 为 {0} 的 EquipmentAction 对象", actionId));
                }

                var allActionsInGroup = unitOfWork.session.Query<Task>()
                    .Where(x => x.Group == currentAction.Movement.Task.Group && x.Group != null)
                    .Select(x => x.Movements.SelectMany(m => m.EquipmentActions).OrderByDescending(e => e.Id).FirstOrDefault())
                    .Where(x => x != null)
                    .ToList();

                if (allActionsInGroup.Count > 1)
                {
                    if (allActionsInGroup.Any(x => x.Status != EquipmentActionStatus.Completed
                        && x.Status != EquipmentActionStatus.Cancelled
                        && x.Status != EquipmentActionStatus.Error
                        && x.Status != EquipmentActionStatus.Suspend))
                    {
                        throw new InvalidCastException(String.Format("{0} 属于组 {1}，该组有 {2} 个任务，且至少有一个当前状态不允许该操作。", currentAction.Movement.Task, currentAction.Movement.Task.Group, allActionsInGroup.Count));
                    }
                }

                //var groupTaskTotals = unitOfWork.session.Query<Task>().Where(x => x.Group == action.Movement.Task.Group && x.Group != null).Count();

                //if (groupTaskTotals > 1)
                //{
                //    throw new InvalidCastException(String.Format("{0} 属于组 {1}，该组有 {2} 个任务，所以无法强制完成物理动作.", action.Movement.Task, action.Movement.Task.Group, groupTaskTotals));
                //}
                List<Wcs.Framework.EventBus.IEvent> events = new List<Wcs.Framework.EventBus.IEvent>();

                foreach (var action in allActionsInGroup)
                {
                    if (action.Status != EquipmentActionStatus.Suspend && action.Status != EquipmentActionStatus.Error)
                    {
                        throw new InvalidOperationException(String.Format("无法强制完成 {0},因为它当前正处于 {1} 状态。只有处理“暂停”或“错误”的任务可以被强制完成", action, action.Status.GetDescription()));
                    }

                    var device = DeviceConverter.ToDevice<TaskableDevice>(action.DeviceName);
                    if (device is IEditableTaskOwner)
                    {
                        var editableTaskOwner = (IEditableTaskOwner)device;
                        Boolean cancelled;
                        editableTaskOwner.Cancel(action, out cancelled);

                        if (cancelled)
                        {
                            throw new Exception(string.Format("已取消完成 {0} 的操作.", action));
                        }

                        editableTaskOwner.Complete(action, out cancelled);

                        if (cancelled)
                        {
                            throw new Exception(string.Format("已取消完成 {0} 的操作.", action));
                        }
                    }
                    else
                    {
                        //如果是物理设备的任务，只有执行中的任务才需要发送取消指令
                        if (action.Status == EquipmentActionStatus.Executing)
                        {
                            device.CancelTask(action);
                        }
                    }


                    action.Status = EquipmentActionStatus.Completed;
                    action.FinishedAt = DateTime.Now;
                    if (action.SentAt == null)
                        action.SentAt = action.FinishedAt;

                    device.EquipmentActionScheduler.Remove(action);

                    _logger.Trace1(String.Format("{0} 状态更改为 {1}", action, action.Status.GetDescription()), null, action);

                    events.Add(new Events.EquipmentActionStatusChangedEvent(action.Movement.Task.Id, action.Movement.Id, action.Id, action.Status));

                    if (action.Movement.EquipmentActions.Last().Id == action.Id)
                    {
                        var movement = action.Movement;

                        movement.FinishedAt = DateTime.Now;
                        movement.Status = LogicMovementStatus.Completed;

                        _logger.Trace1(String.Format("{0} 状态更改为 {1}", movement, movement.Status.GetDescription()), null, movement);

                        events.Add(new Events.LogicMovementStatusChangedEvent(movement.Id, movement.Task.Id, movement.Status));

                        //if (!movement.Task.CurrentLocation.Equals(movement.EndLocation))
                        if (!(movement.Task.CurrentLocation.UserCode == movement.EndLocation.UserCode || LocationConverter.ToLocation(movement.Task.CurrentLocation).Synonymous.Any(x => x.UserCode == LocationConverter.ToLocation(movement.EndLocation).UserCode))) ;
                        {
                            movement.Task.CurrentLocation = movement.EndLocation;
                            events.Add(new TaskCurrentLocationChangedEvent(movement.Task.Id, movement.Task.TaskCode, movement.Task.CurrentLocation));
                        }

                        if (action.Movement != null && action.Movement.Task != null && action.Movement.Task is UndefinedEndLocationTask)
                        {
                            UndefinedEndLocationTask uet = (UndefinedEndLocationTask)action.Movement.Task;
                            //如果当前位置当前完成的物理动作（当前位置）已是可选结束位置的其中的一个，则任务报完成，并将当前位置设为结束点。
                            if (uet.AvailableEndLocations.Any(loc => loc.Equals(action.Movement.EndLocation)))
                            {
                                var oldEndLocation = action.Movement.Task.EndLocation;
                                uet.EndLocation = action.Movement.EndLocation;

                                _logger.Info1(string.Format("{0} 是终点未明的任务，并且 {1} 的结束点（{2}）是其可选位置中的一个，修正任务的结束点为 {2}（原为{3}）",
                                    action.Movement.Task,
                                    action.Movement,
                                    action.Movement.EndLocation,
                                    oldEndLocation
                                    ), null, action);
                            }
                            else
                            {

                                _logger.Trace1(string.Format("{0} 是终点未明的任务，当前结束点是 {1}，准备尝试重新分配结束位置...", action.Movement.Task, action.Movement.Task.EndLocation), null, action);

                                var oldEndLocation = action.Movement.Task.EndLocation;

                                ((UndefinedEndLocationTask)action.Movement.Task).AssignNewEndLocation();

                                _logger.Info1(string.Format("{0} 原结束位置为 {1}，重新分配的结束位置为 {2}", action.Movement.Task, oldEndLocation, action.Movement.Task.EndLocation), null, action);
                            }

                            //_logger.Trace1(string.Format("{0} 是终点未明的任务，当前结束点是 {1}，准备尝试重新分配结束位置...", action.Movement.Task, action.Movement.Task.EndLocation), null, action);

                            //var oldEndLocation = action.Movement.Task.EndLocation;

                            //((UndefinedEndLocationTask)action.Movement.Task).AssignNewEndLocation();

                            //_logger.Info1(string.Format("为 {0} 原结束位置为 {1}，重新分配的结束位置为 {2}", action.Movement.Task, oldEndLocation, action.Movement.Task.EndLocation), null, action);
                        }


                        if (LocationConverter.ToLocation(movement.Task.CurrentLocation).Equals(LocationConverter.ToLocation(movement.Task.EndLocation)))
                        //if (movement.Task.CurrentLocation.Equals(movement.Task.EndLocation))
                        {
                            movement.Task.Status = TaskStatus.Completed;
                            movement.Task.FinishedAt = movement.FinishedAt;

                            var total = movement.Task.FinishedAt.Value.Subtract(movement.Task.CreatedAt);
                            if (movement.Task.AdditionalInfo.ContainsKey("TTS"))
                                movement.Task.AdditionalInfo["TTS"] = total.ToString();
                            else
                                movement.Task.AdditionalInfo.Add("TTS", total.ToString());

                            var rts = CalTaskRunningTimeSpan(movement.Task);
                            if (rts == null)
                                rts = new TimeSpan();
                            if (movement.Task.AdditionalInfo.ContainsKey("RTS"))
                                movement.Task.AdditionalInfo["RTS"] = rts.ToString();
                            else
                                movement.Task.AdditionalInfo.Add("RTS", rts.ToString());

                            var wts = total - rts;
                            if (movement.Task.AdditionalInfo.ContainsKey("WTS"))
                                movement.Task.AdditionalInfo["WTS"] = wts.ToString();
                            else
                                movement.Task.AdditionalInfo.Add("WTS", wts.ToString());

                            _logger.Trace1(String.Format("{0} 状态更改为 {1}", movement.Task, movement.Task.Status.GetDescription()), null, movement.Task);

                            movement.Task.SetManuallyCompleteInfo();

                            events.Add(new TaskStatusChangedEvent(movement.Task.Id, movement.Task.TaskCode, movement.Task.Status, movement.Task.BizType, movement.Task.Source, movement.Task.TaskType));
                            events.Add(new TaskFinishedEvent(movement.Task.Id, movement.Task.TaskCode, movement.Task.Status, movement.Task.BizType, movement.Task.Source, movement.Task.TaskType, movement.Task.FinishedAt.Value));
                        }
                    }

                    _logger.Info1(String.Format("{0} 强制完成成功", action), null, action);

                    MessageBoard.AbstractMessageBoard.Instance.Add(
                        new MessageBoard.Messages.TipMessage(MessageBoard.MessageLevel.Warning,
                            "任务处理",
                            String.Format("任务 {0} 的物理动作 {1} 被 {2} 强制完成。", action.Movement.Task.TaskCode, action.Id, Wcs.Security.WcsPrincipal.CurrentPrincipal.Identity.Name)
                            , null));
                }

                unitOfWork.Commit();

                Wcs.Framework.EventBus.EventBus.Instance.Publish(events.ToArray());

            }
        }

        /// <summary>
        /// 继续执行指定的任务
        /// </summary>
        /// <param name="taskId">任务 Id</param>
        /// <param name="currentLocation">当前位置。为 null 时将直接使用 Task.CurrentLocation 的值</param>
        public static void Resume(Int32 taskId, Location currentLocation)
        {
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                _logger.Trace1(String.Format("准备继续执行 id 为 {0} 的 Task 对象", taskId), null);
                Task tsk = unitOfWork.session.Get<Task>(taskId);
                if (tsk == null)
                {
                    Exception ex = new ApplicationException(String.Format("未找到 id 为 {0} 的 Task 对象", taskId));
                    _logger.Error1(ex, null);
                    throw ex;
                }

                var tasks = unitOfWork.session.Query<Task>().Where(x => x.Group == tsk.Group && x.Group != null).ToList();
                _logger.Trace1(String.Format("{0} 属于组 {1}，该组有 {2} 个任务，准备继续执行本组所有任务.", tsk, tsk.Group, tasks.Count), null, tsk);

                List<Wcs.Framework.EventBus.IEvent> events = new List<Wcs.Framework.EventBus.IEvent>();

                foreach (var task in tasks)
                {
                    _logger.Trace1(String.Format("开始处理组 {0} 中的任务 {1}...", task.Group, task), null, task);

                    if (task.Status != TaskStatus.Suspend && task.Status == TaskStatus.Error)
                    {
                        Exception ex = new InvalidOperationException(String.Format("无法继续执行 {0},因为它当前正处于 {1} 状态。只有处理“暂停”或“错误”的任务可以被继续执行", task, task.Status.GetDescription()));
                        _logger.Error1(ex, null, task);
                        throw ex;
                    }

                    if (currentLocation == null)
                    {
                        currentLocation = LocationConverter.ToLocation(task.CurrentLocation);
                    }


                    var movement = task.Movements.FirstOrDefault(x => x.Status == LogicMovementStatus.Suspend || x.Status == LogicMovementStatus.Error);
                    //var lastMovement = task.Movements.OrderByDescending(x => x.Ordering).FirstOrDefault();
                    //var paths = Task.FindNextPath(Wcs.Framework.Cfg.WcsConfiguration.ParseLocation(task.StartLocation.DeviceName, task.StartLocation.DeviceCode), Wcs.Framework.Cfg.WcsConfiguration.ParseLocation(task.EndLocation.DeviceName, task.EndLocation.DeviceCode), currentLocation, lastMovement == null ? null : lastMovement.RouteId, task.BizType == TaskBizType.Counting ? RouteType.Counting : RouteType.Normal);
                    //if (paths == null || paths.Count == 0)
                    //{
                    //    Exception ex = new ApplicationException(String.Format("为 {0} 指定的当前位置 {1} 与结束位置 {2} 无法连通", task, currentLocation, task.EndLocation));
                    //    _logger.Error1(ex, null);
                    //    throw ex;
                    //}

                    if (task.Id == taskId)
                    {
                        task.CurrentLocation = LocationConverter.ToLocationInfo(currentLocation);

                        _logger.Trace1(String.Format("{0} 当前位置更改为 {1}", task, task.CurrentLocation), null, task);

                        events.Add(new Events.TaskCurrentLocationChangedEvent(task.Id, task.TaskCode, task.CurrentLocation));
                    }
                    else
                    {
                        currentLocation = LocationConverter.ToLocation(task.CurrentLocation);
                    }

                    if (movement != null)
                    {
                        foreach (var action in movement.EquipmentActions)
                        {
                            if (action.Status == EquipmentActionStatus.Cancelled || action.Status == EquipmentActionStatus.Completed)
                            {
                                continue;
                            }

                            if (action.Status != EquipmentActionStatus.Suspend && action.Status != EquipmentActionStatus.Error)
                            {
                                Exception ex = new InvalidOperationException(String.Format("无法继续执行 {0},因为它当前正处于 {1} 状态。只有处理“暂停”或“错误”的任务可以被继续执行", action, action.Status.GetDescription()));
                                _logger.Error1(ex, null, action);
                                throw ex;
                            }

                            var device = DeviceConverter.ToDevice<TaskableDevice>(action.DeviceName);
                            if (device is IEditableTaskOwner)
                            {
                                var editableTaskOwner = (IEditableTaskOwner)device;
                                Boolean cancelled;
                                editableTaskOwner.Resume(action, out cancelled);

                                if (cancelled)
                                {
                                    throw new Exception(string.Format("已取消继续执行任务 {0} 的操作.", task));
                                }
                            }

                            action.Status = EquipmentActionStatus.New;

                            events.Add(new Events.EquipmentActionStatusChangedEvent(action.Movement.Task.Id, action.Movement.Id, action.Id, action.Status));

                            _logger.Trace1(String.Format("{0} 状态更改为 {1}", action, action.Status.GetDescription()), null, action);
                        }

                        movement.Status = LogicMovementStatus.Executing;

                        events.Add(new Events.LogicMovementStatusChangedEvent(movement.Id, movement.Task.Id, movement.Status));

                        _logger.Trace1(String.Format("{0} 状态更改为 {1}", movement, movement.Status.GetDescription()), null, movement);
                    }
                    else
                    {
                        _logger.Trace1(String.Format("{0} 不存在任何 Movement 对象", task), null, movement);
                    }

                    task.Status = TaskStatus.Executing;

                    events.Add(new Events.TaskStatusChangedEvent(task.Id, task.TaskCode, task.Status, task.BizType, task.Source, task.TaskType));

                    _logger.Trace1(String.Format("{0} 状态更改为 {1}", task, task.Status.GetDescription()), null, task);

                    _logger.Info1(String.Format("{0} 继续执行成功", task), null, task);
                }

                unitOfWork.Commit();

                EventBus.EventBus.Instance.Publish(events.ToArray());

                MessageBoard.AbstractMessageBoard.Instance.Add(
                    new MessageBoard.Messages.TipMessage(MessageBoard.MessageLevel.Warning,
                        "任务处理",
                        String.Format("任务 {0} 被 {1} 从 {2} 继续执行。", tsk.TaskCode, Wcs.Security.WcsPrincipal.CurrentPrincipal.Identity.Name, tsk.CurrentLocation)
                        , null));
            }
        }

        /// <summary>
        /// 归档指定的任务
        /// </summary>
        /// <param name="taskId">任务 Id</param>
        public static void Archive(Int32 taskId)
        {
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                _logger.Trace1(String.Format("准备归档 id 为 {0} 的 Task 对象", taskId), null);
                Task tsk = unitOfWork.session.Get<Task>(taskId);
                if (tsk == null)
                {
                    Exception ex = new ApplicationException(String.Format("未找到 id 为 {0} 的 Task 对象", taskId));
                    _logger.Error1(ex, null);
                    throw ex;
                }

                var tasks = unitOfWork.session.Query<Task>().Where(x => x.Group == tsk.Group && x.Group != null).ToList();
                _logger.Trace1(String.Format("{0} 属于组 {1}，该组有 {2} 个任务，准备归档本组所有任务.", tsk, tsk.Group, tasks.Count), null, tsk);

                List<Wcs.Framework.EventBus.IEvent> events = new List<Wcs.Framework.EventBus.IEvent>();

                foreach (var task in tasks)
                {
                    _logger.Trace1(String.Format("开始处理组 {0} 中的任务 {1}...", task.Group, task), null, task);

                    if (task.Status != TaskStatus.Cancelled && task.Status != TaskStatus.Completed)
                    {
                        Exception ex = new InvalidOperationException(String.Format("无法归档 {0},因为它当前正处于 {1} 状态", task, task.Status.GetDescription()));
                        _logger.Error1(ex, null, task);
                        throw ex;
                    }

                    foreach (var movement in task.Movements)
                    {
                        if (movement.Status != LogicMovementStatus.Cancelled && movement.Status != LogicMovementStatus.Completed)
                        {
                            Exception ex = new InvalidOperationException(String.Format("无法归档 {0},因为它当前正处于 {1} 状态", movement, movement.Status.GetDescription()));
                            _logger.Error1(ex, null, movement);
                            throw ex;
                        }

                        foreach (var action in movement.EquipmentActions)
                        {
                            if (action.Status != EquipmentActionStatus.Completed && action.Status != EquipmentActionStatus.Cancelled)
                            {
                                Exception ex = new InvalidOperationException(String.Format("无法归档 {0},因为它当前正处于 {1} 状态", action, action.Status.GetDescription()));
                                _logger.Error1(ex, null, action);
                                throw ex;
                            }
                        }
                    }

                    unitOfWork.session.Delete(task);

                    events.Add(new Events.TaskArchivedEvent(task.Id, task.TaskCode));

                    _logger.Trace1(String.Format("{0} 已删除", task), null, task);

                    _logger.Info1(String.Format("{0} 归档成功", task), null, task);

                }

                unitOfWork.Commit();

                EventBus.EventBus.Instance.Publish(events.ToArray());

                MessageBoard.AbstractMessageBoard.Instance.Add(
                    new MessageBoard.Messages.TipMessage(MessageBoard.MessageLevel.Warning,
                        "任务处理",
                        String.Format("任务 {0} 被 {1} 归档。", tsk.TaskCode, Wcs.Security.WcsPrincipal.CurrentPrincipal.Identity.Name)
                        , null));
            }
        }
        /// <summary>
        /// 归档指定的任务
        /// </summary>
        /// <param name="taskId">任务 Id</param>
        public static void Archive(String taskCode)
        {
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                _logger.Trace1(String.Format("准备归档 任务号 为 {0} 的 Task 对象", taskCode), null);
                Task tsk = unitOfWork.session.Query<Task>().FirstOrDefault(x => x.TaskCode == taskCode);
                if (tsk == null)
                {
                    Exception ex = new ApplicationException(String.Format("未找到 任务号 为 {0} 的 Task 对象", taskCode));
                    _logger.Error1(ex, null);
                    throw ex;
                }

                var tasks = unitOfWork.session.Query<Task>().Where(x => x.Group == tsk.Group && x.Group != null).ToList();
                _logger.Trace1(String.Format("{0} 属于组 {1}，该组有 {2} 个任务，准备归档本组所有任务.", tsk, tsk.Group, tasks.Count), null, tsk);

                List<Wcs.Framework.EventBus.IEvent> events = new List<Wcs.Framework.EventBus.IEvent>();

                foreach (var task in tasks)
                {
                    _logger.Trace1(String.Format("开始处理组 {0} 中的任务 {1}...", task.Group, task), null, task);

                    if (task.Status != TaskStatus.Cancelled && task.Status != TaskStatus.Completed)
                    {
                        Exception ex = new InvalidOperationException(String.Format("无法归档 {0},因为它当前正处于 {1} 状态", task, task.Status.GetDescription()));
                        _logger.Error1(ex, null, task);
                        throw ex;
                    }

                    foreach (var movement in task.Movements)
                    {
                        if (movement.Status != LogicMovementStatus.Cancelled && movement.Status != LogicMovementStatus.Completed)
                        {
                            Exception ex = new InvalidOperationException(String.Format("无法归档 {0},因为它当前正处于 {1} 状态", movement, movement.Status.GetDescription()));
                            _logger.Error1(ex, null, movement);
                            throw ex;
                        }

                        foreach (var action in movement.EquipmentActions)
                        {
                            if (action.Status != EquipmentActionStatus.Completed && action.Status != EquipmentActionStatus.Cancelled)
                            {
                                Exception ex = new InvalidOperationException(String.Format("无法归档 {0},因为它当前正处于 {1} 状态", action, action.Status.GetDescription()));
                                _logger.Error1(ex, null, action);
                                throw ex;
                            }
                        }
                    }

                    unitOfWork.session.Delete(task);

                    events.Add(new Events.TaskArchivedEvent(task.Id, task.TaskCode));

                    _logger.Trace1(String.Format("{0} 已删除", task), null, task);

                    _logger.Info1(String.Format("{0} 归档成功", task), null, task);

                }

                unitOfWork.Commit();

                EventBus.EventBus.Instance.Publish(events.ToArray());

                MessageBoard.AbstractMessageBoard.Instance.Add(
                    new MessageBoard.Messages.TipMessage(MessageBoard.MessageLevel.Warning,
                        "任务处理",
                        String.Format("任务 {0} 被 {1} 归档。", tsk.TaskCode, Wcs.Security.WcsPrincipal.CurrentPrincipal.Identity.Name)
                        , null));
            }
        }

        /// <summary>
        /// 调整指定任务集合的优先级
        /// </summary>
        /// <param name="newPriority">新的优先级</param>
        /// <param name="taskIds">任务 id 列表</param>
        public static void ChangePriority(int newPriority, params Int32[] taskIds)
        {
            if (taskIds == null || taskIds.Length == 0)
            {
                Exception ex = new ArgumentNullException("taskIds");
                _logger.Error1(ex, null);
                throw ex;
            }

            String idStr = string.Join(",", taskIds.Select(x => x.ToString()).ToArray());
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                _logger.Trace1(String.Format("准备调整 id 为 {0} 的 Task 对象优先级", idStr), null);
                var tasks = unitOfWork
                    .session
                    .Query<Task>()
                    .Where(x => taskIds.Contains(x.Id))
                    .ToList();

                var invalidTaskIds = taskIds
                    .Where(id => !tasks.Any(task => task.Id == id))
                    .Select(x => x.ToString())
                    .ToArray();
                if (invalidTaskIds.Length > 0)
                {
                    Exception ex = new Exception(String.Format("未找到id 为 {0} 的 Task 对象", string.Join(",", invalidTaskIds)));
                    _logger.Error1(ex, null);
                    throw ex;
                }
                foreach (var task in tasks)
                {
                    task.Priority = newPriority;

                    _logger.Trace1(String.Format("{0} 优先级更改为 {1}", task, newPriority), null, task);

                }

                unitOfWork.Commit();

                _logger.Info1(String.Format("任务 {0} 优先级调整成功", idStr), null, idStr);

                foreach (var task in tasks)
                {
                    Wcs.Framework.EventBus.EventBus.Instance.Publish(new Events.TaskPriorityChangedEvent(task.Id, task.TaskCode, task.Priority));

                    MessageBoard.AbstractMessageBoard.Instance.Add(
                        new MessageBoard.Messages.TipMessage(MessageBoard.MessageLevel.Warning,
                            "任务处理",
                            String.Format("任务 {0} 被 {1} 将优先级调整为 {2}。", task.TaskCode, Wcs.Security.WcsPrincipal.CurrentPrincipal.Identity.Name, task.Priority)
                            , null));
                }
            }
        }

        /// <summary>
        /// 获取指定任务继续执行时需要使用的 Route
        /// </summary>
        /// <param name="taskId">任务 Id</param>
        /// <returns></returns>
        public static RouteHead GetTaskResumeAtRoute(Int32 taskId)
        {
            EquipmentAction act;

            return GetTaskResumeAtRoute(taskId, out act);
        }


        /// <summary>
        /// 获取指定任务继续执行时需要使用的 Route
        /// </summary>
        /// <param name="taskId">任务 Id</param>
        /// <returns></returns>
        public static RouteHead GetTaskResumeAtRoute(Int32 taskId, out EquipmentAction act)
        {
            Task task;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                task = unitOfWork.session.Get<Task>(taskId);
                unitOfWork.Commit();
            }

            if (task == null)
            {
                Exception ex = new ApplicationException(String.Format("未找到 id 为 {0} 的 Task 对象", taskId));
                _logger.Error1(ex, null);
                throw ex;
            }

            var stopAtMovement = task.Movements.OrderBy(x => x.Ordering).FirstOrDefault(x => x.Status == LogicMovementStatus.Error || x.Status == LogicMovementStatus.Suspend);
            if (stopAtMovement == null)
            {
                act = null;
                return null;
            }

            if (stopAtMovement.RouteId != null)
            {
                var route = RouteHelper.RouteHeads.SingleOrDefault(x => x.Id == stopAtMovement.RouteId);

                act = stopAtMovement.EquipmentActions.First();

                return route;
            }

            act = null;

            return null;
        }
    }
}
