using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.Framework
{
    public class BasicEquipmentActionSchedulerFilter : EquipmentActionSchedulerFilter
    {
        public override ActionSchedulerFilterResult Filter(EquipmentActionScheduler scheduler, EquipmentAction action)
        {
#warning 此处应该考虑去掉，完全是为了后期纠错。
            if (scheduler.CurrentAction != null
                && (scheduler.CurrentAction.Status == EquipmentActionStatus.Completed
                    || scheduler.CurrentAction.Status == EquipmentActionStatus.Cancelled
                ))
            {
                string msg=string.Format("{0} 已处于 {1} 状态，但仍然存在于 {2}。将自动移除。",
                    scheduler.CurrentAction,
                    scheduler.CurrentAction.Status.GetDescription(),
                    scheduler);
                this.Logger.Warn1(msg, this, action);

                scheduler.Remove(action);

                return new ActionSchedulerFilterResult(true,msg);
            }

            if (!scheduler.Device.IsConnected)
            {
                string msg=string.Format("{0} 处于未联机状态。",scheduler.Device);
                return new ActionSchedulerFilterResult(true,msg);
            }

            var isidle = scheduler.Device.IsIdle;
            if (!isidle.Result)
            {
                string msg = string.Format("{0} 处于繁忙状态（tips:{1}）", scheduler.Device, isidle.Information);
                return new ActionSchedulerFilterResult(true, msg);
            }

            if (!scheduler.Device.AllowConcurrency)
            {
                if (scheduler.CurrentAction != null
                && scheduler.CurrentAction.EquipmentTaskId != action.EquipmentTaskId
                    )
                {
                    string msg = string.Format("{0} 不允许并行任务，并且已经存在一个正在执行的任务 {1}，本次动作为 {2}。", scheduler.Device, scheduler.CurrentAction,action);
                    return new ActionSchedulerFilterResult(true, msg);
                }

                var errorAction = scheduler.Actions.FirstOrDefault(x => x.Status == EquipmentActionStatus.Error);
                if (errorAction != null)
                {
                    string msg = string.Format("{0} 不允许并行任务，并且已经存在一个发生错误的任务 {1}。", scheduler.Device, errorAction);
                    return new ActionSchedulerFilterResult(true, msg);
                }

                if (scheduler.CurrentAction != null)
                {
                    var spspendAction = scheduler.Actions.FirstOrDefault(x => x.Status == EquipmentActionStatus.Suspend);
                    if (spspendAction!=null && spspendAction.Id == scheduler.CurrentAction.Id)
                    {
                        string msg = string.Format("{0} 不允许并行任务，并且当前动作 {1} 处于暂停状态。", scheduler.Device, spspendAction);
                        return new ActionSchedulerFilterResult(true, msg);
                    }
                }

                var executingAction = scheduler.Actions.FirstOrDefault(x => x.Status == EquipmentActionStatus.Executing);
                if (executingAction != null)
                {
                    if (scheduler.CurrentAction != null 
                        && scheduler.CurrentAction.EquipmentTaskId == action.EquipmentTaskId 
                        && scheduler.CurrentAction.EquipmentTaskId == executingAction.EquipmentTaskId)
                    {
                        Logger.Warn1(string.Format("单任务设备 {0} 的当前动作为 {1},本次需要发送的动作为 {2}，正在运行的动作为 {3}。正常的处理应该直接否决本次动作发送，但为兼容双货叉设备跳过了该检查代码。", scheduler.Device, scheduler.CurrentAction, action, executingAction), this);
                    }
                    else
                    {
                        string msg = string.Format("{0} 不允许并行任务，并且已经存在一个正在运行的任务 {1}。", scheduler.Device, executingAction);
                        return new ActionSchedulerFilterResult(true, msg);
                    }
                }
            }

            String reason;
            if (!action.CanPerform(out reason))
            {
                string msg = string.Format("{0} 被 CanPerform 函数否决。原因是 {1}", action, reason);
                return new ActionSchedulerFilterResult(true, msg);
            }

            return new ActionSchedulerFilterResult(false, null);
        }
    }
}
