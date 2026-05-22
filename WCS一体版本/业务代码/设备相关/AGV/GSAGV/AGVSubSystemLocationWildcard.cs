using System.Linq;
using Wcs.Framework;

namespace BOE.设备相关.AGV.GSAGV
{
    /// <summary>
    /// 货架位置通配符
    /// </summary>
    public class AGVSubSystemLocationWildcard : AGVSubSystemLocation, ILocationWildcard
    {
        public AGVSubSystemLocationWildcard(GSAGVDevice device, AGVSubSystemLocationElement subSystemLocationElement)
            : base(device, device.Name, "子系统位置通配符", -1, -1, -1, 1 ,1)
        {

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

            GSAGVDevice device = (GSAGVDevice)this.Device;

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
            GSAGVDevice _device = (GSAGVDevice)this.Device;

            return _device
                .Locations
                .Where(x => x != this)
                .ToArray();
        }

        public bool AbleAsOnlyLocation
        {
            get
            {
                return true;
            }
        }
    }
}
