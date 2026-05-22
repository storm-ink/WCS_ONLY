using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Robot
{
    /// <summary>
    /// 表示Robot调度的逻辑动作
    /// </summary>
    public class RobotTransferByStepMovement : LogicMovement
    {
        protected RobotTransferByStepMovement()
            : base()
        {
        }
        public RobotTransferByStepMovement(TaskableDevice device, Int32 routeId, Location startLocation, Location endLocation, Int16 containerCode)
            : base(device,routeId, startLocation,endLocation, containerCode)
        {
        }

        protected override void CreateEquipmentActions()
        {
            RobotLocation startLocation;
            var start = LocationConverter.ToLocation(this.StartLocation);
            if (start is RobotLocation _start)
                startLocation = _start;
            else
                startLocation = (RobotLocation)start.Synonymous.First(x => x is RobotLocation);

            RobotLocation endLocation;
            var end = LocationConverter.ToLocation(this.EndLocation);
            if (end is RobotLocation _end)
                endLocation = _end;
            else
                endLocation = (RobotLocation)end.Synonymous.First(x => x is RobotLocation);

            RobotDevice robotDevice = DeviceConverter.ToDevice<RobotDevice>(this.DeviceName);
            EquipmentActionGroup group = new EquipmentActionGroup();
            int equipmentTaskId = SerialNumberFactory.GenerateEquipmentTaskId();
            RobotTransferByStepAction action = new RobotTransferByStepAction(
                robotDevice, 
                group,
                equipmentTaskId,
                this.RouteId.Value,
                startLocation,
                endLocation);
            action.Ordering = 0;

            this.EquipmentActions.Add(action);
        }
    }
}
