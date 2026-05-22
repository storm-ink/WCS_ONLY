using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;
namespace Wcs.DefaultImplementCollection.AGV
{
    /// <summary>
    /// 货架位置通配符
    /// </summary>
    public class AGVSubSystemLocationWildcard : AGVSubSystemLocation, ILocationWildcard
    {
        public AGVSubSystemLocationWildcard(SinevaAGVDevice device, AGVSubSystemLocationElement subSystemLocationElement)
            : base(device,"*","子系统位置通配符", "-1", -1,-1,1,1)
        {

        }

        public override string DeviceCode
        {
            get
            {
                return "*";
            }
        }

        public override string UserCode
        {
            get
            {
                return string.Format("{0} 的货架位置通配符", this.Device);
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

            SinevaAGVDevice device = (SinevaAGVDevice)this.Device;

            return device.Locations
                .Where(x => !(x is ILocationWildcard))
                .Any(x => x.Equals(obj) || obj.Equals(x));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public Location[] GetMatchedLocations()
        {
            SinevaAGVDevice _device = (SinevaAGVDevice)this.Device;

            return _device
                .Locations
                .Where(x => x != this)
                .ToArray();
        }


        public bool AbleAsOnlyLocation
        {
            get { return true; }
        }
    }
}
