using NHibernate.Hql.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;
using Wcs.FrameworkExtend;

namespace ZHQXC.PreTaskHand
{
    /// <summary>
    /// 起点是单任务设备的预任务处理过滤程序
    /// </summary>
    public class SingleTaskDevicePreTaskSchedulerFilter : Wcs.FrameworkExtend.PreTaskSchedulerFilter
    {
        public override ActionSchedulerFilterResult Filter(PreTask preTask)
        {
            var device = DeviceConverter.ToDevice<TaskableDevice>(preTask.StartLocation.DeviceName);
            if (device.AllowConcurrency)
                return new ActionSchedulerFilterResult(false, "");
         

            List<Task> tasks = new List<Wcs.Framework.Task>();
            lock (PreTaskHandHelper.Tasks)
            {
                tasks = PreTaskHandHelper.Tasks.Where(x => x.TaskCode != preTask.TaskCode && x.StartLocation.DeviceName == device.Name && x.Movements.Count < 2).ToList();
            }
            if (tasks.Count() > 0)
            {
                var _taks = tasks.Select(x => x.ToString());
                return new ActionSchedulerFilterResult(true, $"单任务设备 {device.Name} 已分配任务 {string.Join("|", _taks)} 在执行中，暂时无法下发当前任务");
            }
            else
                return new ActionSchedulerFilterResult(false, "");
        }
    }
}
