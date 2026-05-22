using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Linq;
using NLog;
using Wcs.Framework.Events;

namespace Wcs.Framework
{
    public class DefaultTaskErrorEventHandler : ITaskEventHandler<TaskErrorEventArgs>
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();
        public DefaultTaskErrorEventHandler()
        {
            
        }

        public void Handle(TaskableDevice device, NHibernate.ISession session, TaskErrorEventArgs args)
        {
            try
            {
                _logger.Trace1(string.Format("{0} 开始处理设备任务 {1} 的任务报警事件...", this, args.EquipmentTaskId), this, args, null, args.EquipmentTaskId);


                _logger.Trace1(string.Format("获取任务号为 {0} 的物理动作", args.EquipmentTaskId), this, args, null, args.EquipmentTaskId);

                var action = session.Query<EquipmentAction>().SingleOrDefault(x => x.EquipmentTaskId == args.EquipmentTaskId);
                if (action == null)
                {
                    String msg = string.Format("收到了 {0} 发送来的 {1} 任务报警信号,但未找到对应的物理动作对象，忽略本信号", device, args.EquipmentTaskId);
                    _logger.Warn1(msg, this, args, null, args.EquipmentTaskId);
                    args.Handled = true;
                    return;
                }
                _logger.Trace1(string.Format("找到 {0}", action), this, action);

                //session.Lock(action, NHibernate.LockMode.Upgrade);

                if (action.Status == EquipmentActionStatus.Completed)
                {
                    String msg = string.Format("收到了 {0} 发送来的 {1} 任务报警信号,但发现对应的物理动作已处于完成状态，忽略本信号", device, args.EquipmentTaskId);
                    _logger.Warn1(msg, this, action);
                    args.Handled = true;
                    return;
                }

                action.Status = EquipmentActionStatus.Error;
                _logger.Trace1(string.Format("更新 {0}，Status:{1}", action, action.Status.GetDescription()), this, action);

                if (action.Movement != null)
                {
                    action.Movement.Status = LogicMovementStatus.Suspend;
                    _logger.Trace1(string.Format("更新 {0}，Status:{1}", action.Movement, action.Movement.Status.GetDescription()), this, action.Movement);

                    action.Movement.Task.Status = TaskStatus.Suspend;
                    _logger.Trace1(string.Format("更新 {0}，Status:{1}", action.Movement.Task, action.Movement.Task.Status.GetDescription()), this, action.Movement.Task);

                    EquipmentActionWarning warning = new EquipmentActionWarning(device, args.ErrorCode, args.ErrorDescription);
                    action.AddWarning(warning);
                    _logger.Trace1(string.Format("为 {0} 添加新的 EquipmentActionWarning 对象,({1} {2})", action, args.ErrorCode, args.ErrorDescription), this, action);

                    args.Handled = true;

                    args.AddLazyEvents(new EventBus.IEvent[] {
                        new EquipmentActionWarningAddedEvent(warning),
                        new EquipmentActionStatusChangedEvent(action.Movement.Task.Id, action.Movement.Id, action.Id, action.Status),
                        new LogicMovementStatusChangedEvent(action.Movement.Id, action.Movement.Task.Id, action.Movement.Status),
                        new TaskStatusChangedEvent(action.Movement.Task.Id, action.Movement.Task.TaskCode, action.Movement.Task.Status, action.Movement.Task.BizType, action.Movement.Task.Source, action.Movement.Task.TaskType)
                    });
                }
                else
                {
                    args.Handled = true;
                }
                _logger.Trace1(string.Format("{0} 已完成对 {1} 的任务报警事件处理过程.", this, args.EquipmentTaskId), this, args, null, args.EquipmentTaskId);
            }
            catch (Exception ex)
            {
                String msg = string.Format("收到了 {0} 发送来的 {1} 任务报警信号,但 {2} 在处理时发生异常，操作已中止", device, args.EquipmentTaskId, this);
                _logger.Error1(new Exception(msg, ex), this, args, null, args.EquipmentTaskId);
                args.Handled = false;
            }
        }
    }
}
