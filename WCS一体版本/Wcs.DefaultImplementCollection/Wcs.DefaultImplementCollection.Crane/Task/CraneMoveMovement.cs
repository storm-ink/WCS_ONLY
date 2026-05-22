using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Crane
{
    /// <summary>
    /// 表示堆垛机的一个半自动行走任务
    /// 到 A 点行走到 B 点
    /// </summary>
    public class CraneMoveMovement:LogicMovement
    {
        protected CraneMoveMovement()
            : base()
        {
        }
        public CraneMoveMovement(TaskableDevice device, Int32 routeId, Location startLocation, Location endLocation, Int16 containerCode)
            : base(device, routeId, startLocation, endLocation, containerCode)
        {
        }

        protected override void CreateEquipmentActions()
        {
            Location startLocation = Wcs.Framework.LocationConverter.ToLocation(this.StartLocation);
            Location endLocation = Wcs.Framework.LocationConverter.ToLocation(this.EndLocation);
            CraneDevice crane = Wcs.Framework.DeviceConverter.ToDevice<CraneDevice>(this.DeviceName);
            EquipmentActionGroup group = new EquipmentActionGroup();
            int equipmentTaskId = SerialNumberFactory.GenerateEquipmentTaskId();

            RackLocation startRackLocation, endRackLocation;
            if (startLocation is RackLocation)
            {
                startRackLocation = (RackLocation)startLocation;
            }
            else
            {
                startRackLocation = (RackLocation)startLocation.Synonymous.Single(x => x is RackLocation) ;
            }

            if (endLocation is RackLocation)
            {
                endRackLocation = (RackLocation)endLocation;
            }
            else
            {
                endRackLocation = (RackLocation)endLocation.Synonymous.Single(x => x is RackLocation);
            }

            CraneMoveAction action = new CraneMoveAction(
                crane,
                group,
                equipmentTaskId,
                endRackLocation,
                _containerCode);

            action.Ordering = 0;

            this.EquipmentActions.Add(action);
        }
    }
}
