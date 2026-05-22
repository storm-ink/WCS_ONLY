using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.DefaultImplementCollection.RailGuidedVehicle;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.SingleLocationDoubleVehicleManager
{
    public class SingleLocationDoubleVehicleSubSystemLocationWildcard : SingleLocationDoubleVehicleSubSystemLocation, Wcs.Framework.ILocationWildcard
    {
        public SingleLocationDoubleVehicleSubSystemLocationWildcard(SingleLocationDoubleVehicleSubSystem device, Boolean ableAsOnlyLocation = false)
            :base(ChainAction.None,ChainAction.None,0,0,null,null,device)
        {
            _ableAsOnlyLocation = ableAsOnlyLocation;
        }

        public override string DeviceCode
        {
            get
            {
                return this.Device.Name;
            }
        }

        public override string UserCode
        {
            get
            {
                return this.Device.Name;
            }
        }

        public Wcs.Framework.Location[] GetMatchedLocations()
        {
            SingleLocationDoubleVehicleSubSystem device = (SingleLocationDoubleVehicleSubSystem)this.Device;

            return device.Locations
                .Select(x => (SingleLocationDoubleVehicleSubSystemLocation)x)
                .Where(x => x != this)
                .ToArray();
        }

        bool _ableAsOnlyLocation;
        public bool AbleAsOnlyLocation
        {
            get { return _ableAsOnlyLocation; }
        }
    }
}
