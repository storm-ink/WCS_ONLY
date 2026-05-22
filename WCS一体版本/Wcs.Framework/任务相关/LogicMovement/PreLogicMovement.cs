using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    public class PreLogicMovement : LogicMovement
    {
        public PreLogicMovement(Int32? routeId, Location startLocation, Location endLocation)
        : base()
        {
            this.RouteId = routeId;
            this.StartLocation = LocationConverter.ToLocationInfo(startLocation);
            this.EndLocation = LocationConverter.ToLocationInfo(endLocation);
            this.CreatedAt = DateTime.Now;
        }

        protected override void CreateEquipmentActions()
        {
            throw new NotImplementedException();
        }
    }
}
