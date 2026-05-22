using NHibernate.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Events;

namespace Wcs.Framework
{
    /// <summary>
    /// 系统物理动作调度平台
    /// </summary>
    public class SystemEquipmentActionScheduler : EquipmentActionScheduler
    {
        /// <summary>
        /// 默认构造函数
        /// </summary>
        /// <param name="device">套用格式，无实际意义</param>
        /// <remarks>
        /// 将自动从数据库中读取状态为 <see cref="F:Wcs.Framework.EquipmentActionStatus.New"/> 的 <see cref="T:Wcs.Framework.EquipmentAction"/>
        /// </remarks>
        public SystemEquipmentActionScheduler(TaskableDevice device, EquipmentActionSchedulerFilter[] actionFilters)
        {
            this._logger = LogManager.GetCurrentClassLogger();
            this.DeviceName = "WCS系统";

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

            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                _actions = unitOfWork
                    .session
                    .Query<EquipmentAction>().ToList();

                unitOfWork.Commit();
            }

            EventBus.EventBus.Instance.Subscribe<EquipmentActionStatusChangedEvent>(onEquipmentActionStatusChanged);
            EventBus.EventBus.Instance.Subscribe<TaskArchivedEvent>(onTaskArchived);
            EventBus.EventBus.Instance.Subscribe<TaskCurrentLocationChangedEvent>(onTaskCurrentLocationChanged);
            Wcs.Framework.EventBus.EventBus.Instance.Subscribe<TaskPriorityChangedEvent>(onPriorityChange);
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
    }
}
