using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs.DefaultImplementCollection.Conveyor;
using Wcs.Framework;

namespace ZHQXC.过滤器
{
    /// <summary>
    /// 区域锁判断
    /// </summary>
    public class RegionLockEquipmentActionFilter : EquipmentActionSchedulerFilter
    {
        public override ActionSchedulerFilterResult Filter(EquipmentActionScheduler equipmentActionScheduler, EquipmentAction action)
        {
            if (action.Movement.Task.CurrentLocation.UserCode != action.Movement.StartLocation.UserCode)
                return new ActionSchedulerFilterResult(false, "");

            var end = LocationConverter.ToLocation(action.Movement.EndLocation);
            ConveyorLocation conveyorLocation;
            if (end is ConveyorLocation)
                conveyorLocation = (ConveyorLocation)end;
            else
            {
                if (end.Synonymous.Length == 0 || !end.Synonymous.Any(x => x is ConveyorLocation))
                    return new ActionSchedulerFilterResult(false, "");
                conveyorLocation = (ConveyorLocation)end.Synonymous.FirstOrDefault(x => x is ConveyorLocation);
                if (conveyorLocation == null)
                    return new ActionSchedulerFilterResult(false, "");
            }

            var region = RegionRouteLockerHelper.RegionInfos.FirstOrDefault(x => x.Infos.SelectMany(y => y.Value).Contains(conveyorLocation.DeviceCode));
            if (region == null)
                return new ActionSchedulerFilterResult(false, "");

            if (RegionRouteLockerHelper.RegionLocks != null)
            {
                var regionLock = RegionRouteLockerHelper.RegionLocks.FirstOrDefault(x => x.RegionName == region.RegionName);
                if (regionLock == null || regionLock.TaskCode == action.Id.ToString())
                    return new ActionSchedulerFilterResult(false, "");
                return new ActionSchedulerFilterResult(true, $"区域 {region.RegionName} 已经被任务 {regionLock.TaskCode} 锁定，暂时无法执行任务");
            }

            return new ActionSchedulerFilterResult(false, "");
        }
    }
}
