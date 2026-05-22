using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.EquipmentActions;
using Wcs.Framework.Devices;

namespace Wcs.Framework.LogicMovements
{
    /// <summary>
    /// 表示输送线从A将B带到C点的逻辑动作
    /// </summary>
    public class ConveyorTransferMovement:LogicMovement
    {
        protected ConveyorTransferMovement()
            : base()
        {
        }
        public ConveyorTransferMovement(Devices.Device device,Int32? routeId, Location startLocation, Location endLocation)
            : base(device,routeId, startLocation,endLocation)
        {
        }

        protected override void InitializeEquipmentActions()
        {
            
            EquipmentActionGroup group = new EquipmentActionGroup();
            ConveyorTransferAction action = new ConveyorTransferAction(this.DeviceInfo.GetDevice(), TaskManager.GetInstance().GetNextEquipmentTaskId(), 0,this.RouteId.Value, this.StartLocation.GetLocation<ConveyorLocation>(), this.EndLocation.GetLocation<ConveyorLocation>());
            action.Ordering = 0;
            group.AcceptAction(action);
            this.EquipmentActions = new Iesi.Collections.Generic.HashedSet<EquipmentAction>()
            {
                action
            };
        }
    }
}
