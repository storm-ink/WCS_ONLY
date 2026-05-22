using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 表示输送线从 A 将货物带到 B 点的逻辑动作
    /// </summary>
    public class ConveyorTransferMovement:LogicMovement
    {
        protected ConveyorTransferMovement()
            : base()
        {
        }
        public ConveyorTransferMovement(TaskableDevice device, Int32 routeId, ConveyorLocation startLocation, ConveyorLocation endLocation, Int16 containerCode)
            : base(device,routeId, startLocation,endLocation, containerCode)
        {
        }

        protected override void CreateEquipmentActions()
        {
            ConveyorLocation startLocation = ConveyorHelper.GetConveyorLocation(this.StartLocation);
            ConveyorLocation endLocation = ConveyorHelper.GetConveyorLocation(this.EndLocation);
            ConveyorDevice conveyor = ConveyorHelper.GetConveyorDevice(this.DeviceName);
            EquipmentActionGroup group = new EquipmentActionGroup();
            int equipmentTaskId = SerialNumberFactory.GenerateEquipmentTaskId();
            ConveyorTransferAction action = new ConveyorTransferAction(
                conveyor, 
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
