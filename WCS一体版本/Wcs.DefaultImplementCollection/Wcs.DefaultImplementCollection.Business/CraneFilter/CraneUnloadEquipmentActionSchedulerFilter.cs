using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs;
using Wcs.Framework;
using NHibernate.Linq;
using Wcs.DefaultImplementCollection.Crane;
using Wcs.DefaultImplementCollection.Conveyor;

namespace Wcs.DefaultImplementCollection.Business
{
    public class CraneUnloadEquipmentActionSchedulerFilter : EquipmentActionSchedulerFilter
    {
        List<String> _craneOutLocationIsSingle
        {
            get
            {
                var _str = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<String>("craneOutLocationIsSingle", "");
                if (!String.IsNullOrWhiteSpace(_str))
                    return _str.Split(',').ToList();
                else
                    return null;
            }
        }

        public override ActionSchedulerFilterResult Filter(EquipmentActionScheduler equipmentActionScheduler, EquipmentAction action)
        {
            if (!(equipmentActionScheduler.Device is CraneDevice))
                return new ActionSchedulerFilterResult(false, "非堆垛机任务");

            Location unloadLocation;
            if (action is CraneAutomaticTransferWithStepByStepAction)
            {
                var craneAutomaticTransferAction = (CraneAutomaticTransferWithStepByStepAction)action;
                unloadLocation = LocationConverter.ToLocation(craneAutomaticTransferAction.UnloadLocation);
                if (unloadLocation is RackLocation && unloadLocation.Synonymous.All(x => x is RackLocation))
                {
                    return new ActionSchedulerFilterResult(false, string.Format("结束位置 {0} 在货架内", unloadLocation));
                }
            }
            else if (action is CraneAutomaticTransferAction)
            {
                var craneAutomaticTransferAction = (CraneAutomaticTransferAction)action;
                unloadLocation = LocationConverter.ToLocation(craneAutomaticTransferAction.UnloadLocation);
                if (unloadLocation is RackLocation && unloadLocation.Synonymous.All(x => x is RackLocation))
                {
                    return new ActionSchedulerFilterResult(false, string.Format("结束位置 {0} 在货架内", unloadLocation));
                }
            }
            else
                return new ActionSchedulerFilterResult(false, "非需要过滤的堆垛机任务类型");

            ConveyorLocation conveyorLocation;
            if (unloadLocation is ConveyorLocation)
            {
                conveyorLocation = (ConveyorLocation)unloadLocation;
            }
            else
            {
                conveyorLocation = (ConveyorLocation)unloadLocation.Synonymous.Single(x => x is ConveyorLocation);
            }

            ConveyorDevice conveyorDevice = (ConveyorDevice)conveyorLocation.Device;
            var holdSignals = conveyorDevice.ReadStatus<HoldSingleBlock>();
            HoldSingleBlock holdSignal = null;
            if (holdSignals != null)
                holdSignal = holdSignals.FirstOrDefault(x => x.PosNo == Convert.ToInt32(conveyorLocation.DeviceCode));
            if (holdSignal == null)
                return new ActionSchedulerFilterResult(true, string.Format("{0} 未配置占位信息或设备信息未同步", conveyorLocation));

            LocationInfoBlock locationInfoBlock = null;
            var locationInfoBlocks = conveyorDevice.ReadStatus<LocationInfoBlock>();
            if (locationInfoBlocks != null)
                locationInfoBlock = locationInfoBlocks.FirstOrDefault(x => x.PosNo == Convert.ToInt32(conveyorLocation.DeviceCode));
            if (locationInfoBlock == null)
                return new ActionSchedulerFilterResult(true, string.Format("{0} 未配置 LocationInfoBlock 或设备 LocationInfoBlock 信息未同步", conveyorLocation));

            if (holdSignal.HandShake == RequestHandShakes.New)
            {
                if (_craneOutLocationIsSingle != null && _craneOutLocationIsSingle.Count() != 0 && _craneOutLocationIsSingle.Contains(conveyorLocation.DeviceCode))
                {
                    if (locationInfoBlock == null)
                        return new ActionSchedulerFilterResult(true, string.Format("{0} 未配置 LocationInfoBlock 或设备 LocationInfoBlock 信息未同步", conveyorLocation));
                    if (locationInfoBlock.TaskNo != 0)
                    {
                        var _locationTask = locationInfoBlocks.FirstOrDefault(x => x.TaskNo == locationInfoBlock.TaskNo && x.PosNo != locationInfoBlock.PosNo);
                        var __task = conveyorDevice.ReadStatus<TaskBlock>().FirstOrDefault(x => x.TaskNo == locationInfoBlock.TaskNo);
                        if (__task != null && __task.From == locationInfoBlock.PosNo && _locationTask != null)
                            return new ActionSchedulerFilterResult(false, "出库口为单节输送机，并且该节任务已经传递到下一节，允许提前放行");
                    }
                }
                return new ActionSchedulerFilterResult(true, string.Format("{0} 有占位，当前无法卸货", conveyorLocation));
            }
            if (locationInfoBlock.Sensor.Any(x => x > 0))
                return new ActionSchedulerFilterResult(true, string.Format("{0} 光电有信号，当前无法卸货", conveyorLocation));

            if (locationInfoBlock.TaskNo != 0)
                return new ActionSchedulerFilterResult(true, string.Format("{0} 任务号不为0，当前无法卸货", conveyorLocation));

            Boolean _task;
            String[] _locs = conveyorLocation.Synonymous.Select(x => x.UserCode).ToArray();
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                _task = unitOfWork.session.Query<Task>().Any(x => x.CurrentLocation.UserCode == conveyorLocation.UserCode || _locs.Contains(x.CurrentLocation.UserCode));
                unitOfWork.Commit();
            }
            if (_task)
                return new ActionSchedulerFilterResult(true, string.Format("至少存在一条当前位置在 {0}/{1} 的任务，当前无法卸货", conveyorLocation.UserCode, String.Join("/", _locs)));

            return new ActionSchedulerFilterResult(false, "出库口可以放货");
        }
    }
}
