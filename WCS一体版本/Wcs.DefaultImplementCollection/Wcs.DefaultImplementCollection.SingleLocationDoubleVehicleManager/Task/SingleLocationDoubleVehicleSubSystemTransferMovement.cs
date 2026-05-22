using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.SingleLocationDoubleVehicleManager
{
    /// <summary>
    /// 表示堆垛机调度的逻辑动作
    /// </summary>
    public class SingleLocationDoubleVehicleSubSystemTransferMovement : LogicMovement
    {
        protected SingleLocationDoubleVehicleSubSystemTransferMovement()
            : base()
        {
        }
        public SingleLocationDoubleVehicleSubSystemTransferMovement(TaskableDevice device, Int32 routeId, SingleLocationDoubleVehicleSubSystemLocation startLocation, SingleLocationDoubleVehicleSubSystemLocation endLocation, Int16 containerCode)
            : base(device,routeId, startLocation,endLocation, containerCode)
        {
        }

        protected override void CreateEquipmentActions()
        {
            SingleLocationDoubleVehicleSubSystemLocation startLocation = (SingleLocationDoubleVehicleSubSystemLocation)LocationConverter.UserCodeToLcation(this.StartLocation.UserCode);
            SingleLocationDoubleVehicleSubSystemLocation endLocation =(SingleLocationDoubleVehicleSubSystemLocation)LocationConverter.UserCodeToLcation(this.EndLocation.UserCode);
            SingleLocationDoubleVehicleSubSystem craneSubSystem = DeviceConverter.ToDevice<SingleLocationDoubleVehicleSubSystem>(this.DeviceName);
            EquipmentActionGroup group = new EquipmentActionGroup();
            int equipmentTaskId = SerialNumberFactory.GenerateEquipmentTaskId();
            SingleLocationDoubleVehicleSubSystemTransferAction action = new SingleLocationDoubleVehicleSubSystemTransferAction(
                craneSubSystem, 
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
