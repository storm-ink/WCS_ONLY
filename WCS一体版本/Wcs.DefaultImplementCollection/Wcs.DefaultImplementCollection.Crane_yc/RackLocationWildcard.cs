using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 货架位置通配符
    /// </summary>
    public class RackLocationWildcard : RackLocation, ILocationWildcard
    {
        public RackLocationWildcard(CraneDevice device, LocationElement rackLocationElement, Boolean ableAsOnlyLocation = false)
            : base(device, "*", null, ForkAction.None, -1, -1, -1, -1, -1, true, 00)
        {
            _ableAsOnlyLocation = ableAsOnlyLocation;
            _userCode = device.Name;
            _deviceCode = device.Name;
        }

        public RackLocationWildcard(CraneDevice device, LocationElement rackLocationElement, String userCode, String deviceCode, Boolean ableAsOnlyLocation = false)
            : base(device, "*", null, ForkAction.None, -1, -1, -1, -1, -1, true, 0)
        {
            _ableAsOnlyLocation = ableAsOnlyLocation;
            _userCode = userCode;
            _deviceCode = deviceCode;
        }

        Boolean _ableAsOnlyLocation;
        String _userCode;
        String _deviceCode;

        public override string DeviceCode
        {
            get
            {
                return _deviceCode;
            }
        }

        public override string UserCode
        {
            get
            {
                return _userCode;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (base.Equals(obj))
            {
                return true;
            }

            CraneDevice craneDevice = (CraneDevice)this.Device;

            return craneDevice.Locations
                .Where(x => !(x is ILocationWildcard))
                .Any(x => x.Equals(obj) || obj.Equals(x));
        }

        public Location[] GetMatchedLocations()
        {
            CraneDevice craneDevice = (CraneDevice)this.Device;

            return craneDevice
                .Locations
                .Where(x => x != this)
                .ToArray();
        }


        public bool AbleAsOnlyLocation
        {
            get { return _ableAsOnlyLocation; }
        }
    }
}
