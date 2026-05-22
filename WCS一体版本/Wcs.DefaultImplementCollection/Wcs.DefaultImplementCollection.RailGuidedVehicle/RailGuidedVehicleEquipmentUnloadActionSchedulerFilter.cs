using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs;
using Wcs.DefaultImpls.Conveyor;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    /// <summary>
    /// 使用占位来判断卸货点是否就绪，未就绪则过滤
    /// </summary>
    public sealed class RailGuidedVehicleEquipmentUnloadActionSchedulerFilter: EquipmentActionSchedulerFilter
    {
        public override ActionSchedulerFilterResult Filter(EquipmentActionScheduler equipmentActionScheduler, EquipmentAction action)
        {
            if (!(action is RailGuidedVehicleStepByStepAction))
            {
                throw new InvalidOperationException(String.Format("{0} 不是穿梭车动作", action));
            }

            RailGuidedVehicleStepByStepAction rgvAct = (RailGuidedVehicleStepByStepAction)action;

            var unloadStation = (RailGuidedVehicleStation)LocationConverter.ToLocation(rgvAct.UnloadLocation);

            if (unloadStation.StockingLocations != null && unloadStation.StockingLocations.Length > 0)
            {
                List<ConveyorLocation> locs = new List<ConveyorLocation>();
                foreach (var item in unloadStation.StockingLocations)
                {
                    var loc = (ConveyorLocation)LocationConverter.ConvertibleCodeToLcation(item);

                    locs.Add(loc);
                }

                if (locs.All(x => 是否有占位(x)))
                {
                    return new ActionSchedulerFilterResult(true, unloadStation.UserCode + "可放货数量为 0");
                }
            }
            else
            {
                var loc = (ConveyorLocation)unloadStation.Synonymous.FirstOrDefault(x => x is ConveyorLocation);
                //无同义位置，还能发送任务，则表明卸货点和小车属于一体，不再需要判断占位，有同义位置则需要判断占位。
                if (loc != null)
                {
                    if (是否有占位(loc))
                    {
                        return new ActionSchedulerFilterResult(true, unloadStation.UserCode + "可放货数量为 0");
                    }
                }
            }

            return new ActionSchedulerFilterResult(false, "");
        }


        Boolean 是否有占位(ConveyorLocation loc)
        {
            var conveyorDevice = (ConveyorDevice)loc.Device;
            if (conveyorDevice.OccupiedSignals == null || conveyorDevice.OccupiedSignals.Length == 0)
            {
                //设备状态未同步
                return true;
            }

            var hs = conveyorDevice.OccupiedSignals.FirstOrDefault(x => x.PosNo == Convert.ToInt32(loc.DeviceCode));
            if (hs == null)
            {
                //占位信号未配置
                return true;
            }

            if (hs.HandShake == HoldSignalNetTransferObjectHandShake.New)
            {
                return true;
            }

            return false;
        }

    }
}
