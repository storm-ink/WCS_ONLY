using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    public class RailGuidedVehicleStationWildcard : RailGuidedVehicleStation, Wcs.Framework.ILocationWildcard
    {
        public RailGuidedVehicleStationWildcard(RailGuidedVehicleDevice device, Boolean ableAsOnlyLocation = false)
            :base(ChainAction.None,ChainAction.None,0,0,null,null,null,true,device)
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

        public override string UnifiedCode
        {
            get
            {
                return this.Device.Name;
            }
        }

        public Wcs.Framework.Location[] GetMatchedLocations()
        {
            RailGuidedVehicleDevice railGuidedVehicleDevice = (RailGuidedVehicleDevice)this.Device;

            return railGuidedVehicleDevice.Locations
                .Select(x => (RailGuidedVehicleStation)x)
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
