using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    /// <summary>
    /// 表示穿梭车的一个单步执行的全自动的取货放货任务
    /// 到 A 点取货然后卸到 B 点
    /// </summary>
    public class RailGuidedVehicleStepByStepMovement : LogicMovement
    {
        protected RailGuidedVehicleStepByStepMovement()
            : base()
        {
        }
        public RailGuidedVehicleStepByStepMovement(TaskableDevice device, Int32 routeId, Location startLocation, Location endLocation, Int16 containerCode)
            : base(device, routeId, startLocation, endLocation, containerCode)
        {
        }

        protected override void CreateEquipmentActions()
        {
            Location startLocation = LocationConverter.ToLocation(this.StartLocation);
            Location endLocation = LocationConverter.ToLocation(this.EndLocation);
            RailGuidedVehicleDevice railGuidedVehicleDevice = DeviceConverter.ToDevice<RailGuidedVehicleDevice>(this.DeviceName);
            EquipmentActionGroup group = new EquipmentActionGroup();
            int equipmentTaskId = SerialNumberFactory.GenerateEquipmentTaskId();

            RailGuidedVehicleStation startStation, endStation;
            if (startLocation is RailGuidedVehicleStation)
            {
                startStation = (RailGuidedVehicleStation)startLocation;
            }
            else
            {
                if (startLocation.Synonymous.Any(x => x.Synonymous.Any(y => y is RailGuidedVehicleStation)))//堆垛机与穿梭车直接对接
                {
                    var s = startLocation.Synonymous.First(x => x.Synonymous.Any(y => y is RailGuidedVehicleStation));
                    startStation = (RailGuidedVehicleStation)s.Synonymous.Single(x => x is RailGuidedVehicleStation);

                }
                else
                {
                    if (startLocation.Synonymous.Where(x => x is RailGuidedVehicleStation).Count() != 1)
                    {
                        throw new ApplicationException(string.Format("{0} 位置匹配的穿梭车站点存在 {1} 个。期望值为 1({2})", startLocation, startLocation.Synonymous.Where(x => x is RailGuidedVehicleStation).Count(), this.Task));
                    }
                    startStation = (RailGuidedVehicleStation)startLocation.Synonymous.Single(x => x is RailGuidedVehicleStation);
                }
            }

            if (endLocation is RailGuidedVehicleStation)
            {
                endStation = (RailGuidedVehicleStation)endLocation;
            }
            else
            {
                if (endLocation.Synonymous.Any(x => x.Synonymous.Any(y => y is RailGuidedVehicleStation)))//堆垛机与穿梭车直接对接
                {
                    var s = endLocation.Synonymous.First(x => x.Synonymous.Any(y => y is RailGuidedVehicleStation));
                    endStation = (RailGuidedVehicleStation)s.Synonymous.Single(x => x is RailGuidedVehicleStation);

                }
                else
                {
                    if (endLocation.Synonymous.Where(x => x is RailGuidedVehicleStation).Count() != 1)
                    {
                        throw new ApplicationException(string.Format("{0} 位置匹配的穿梭车站点存在 {1} 个。期望值为 1({2})", endLocation, endLocation.Synonymous.Where(x => x is RailGuidedVehicleStation).Count(), this.Task));
                    }

                    endStation = (RailGuidedVehicleStation)endLocation.Synonymous.Single(x => x is RailGuidedVehicleStation);
                }
            }

            RailGuidedVehicleStepByStepAction action = new RailGuidedVehicleStepByStepAction(
                railGuidedVehicleDevice,
                group,
                equipmentTaskId,
                startStation,
                endStation,
                _containerCode);

            action.Ordering = 0;

            this.EquipmentActions.Add(action);
        }
    }
}
