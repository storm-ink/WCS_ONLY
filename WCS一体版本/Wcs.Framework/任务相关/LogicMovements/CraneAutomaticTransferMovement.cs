using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.EquipmentActions;
using Wcs.Framework.Devices;

namespace Wcs.Framework.LogicMovements
{
    /// <summary>
    /// 表示堆垛机的一个全自动的取货放货任务
    /// 到 A 点取货然后卸到 B 点
    /// </summary>
    public class CraneAutomaticTransferMovement:LogicMovement
    {
        protected CraneAutomaticTransferMovement()
            : base()
        {
        }
        public CraneAutomaticTransferMovement(Devices.Device device, Int32? routeId, Location startLocation, Location endLocation)
            : base(device,routeId, startLocation,endLocation)
        {
        }

        protected override void InitializeEquipmentActions()
        {
            RackLocation actionStartLocation = null;
            if (this.StartLocation.GetLocation() is RackLocation)
            {
                actionStartLocation = (RackLocation)this.StartLocation.GetLocation();
            }
            else
            {
                actionStartLocation = (RackLocation)this.StartLocation.GetLocation().SameAs.Single(x => x.Device == this.DeviceInfo.GetDevice());
            }

            RackLocation actionEndLocation = null;
            if (this.EndLocation.GetLocation() is RackLocation)
            {
                actionEndLocation = (RackLocation)this.EndLocation.GetLocation();
            }
            else
            {
                actionEndLocation = (RackLocation)this.EndLocation.GetLocation().SameAs.Single(x => x.Device == this.DeviceInfo.GetDevice());
            }
            Int32 equipmentTaskId = TaskManager.GetInstance().GetNextEquipmentTaskId();
            EquipmentActionGroup group = new EquipmentActionGroup();
            CraneAutomaticTransferAction action = new CraneAutomaticTransferAction(this.DeviceInfo.GetDevice(), equipmentTaskId, 0, actionStartLocation, actionEndLocation);
            action.Ordering = 0;
            group.AcceptAction(action);

            this.EquipmentActions = new Iesi.Collections.Generic.HashedSet<EquipmentAction> 
            { 
                action 
            };
        }
    }
}
