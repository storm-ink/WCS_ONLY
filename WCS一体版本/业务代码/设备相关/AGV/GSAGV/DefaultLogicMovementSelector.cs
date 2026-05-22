using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace BOE.设备相关.AGV.GSAGV
{
    public class DefaultLogicMovementSelector : LogicMovementSelector
    {
        public override LogicMovement ToLogicMovement(Route route, Task task)
        {
            throw new NotImplementedException();
        }

        public override LogicMovement ToLogicMovement(RouteHead route, Task task, Location start, Location end)
        {
            var device = DeviceConverter.ToDevice<TaskableDevice>(route.Device);
            if (!(device is GSAGVDevice))
            {
                throw new InvalidOperationException(string.Format("{0} 所属设备不是 {1} 类型", route, typeof(GSAGVDevice)));
            }

            AGVSubSystemLocation startLocation, endLocation;
            if (start is AGVSubSystemLocation)
            {
                startLocation = (AGVSubSystemLocation)start;
            }
            else
            {
                var synonymousRackLocations = ((GSAGVDevice)device)
                    .Locations
                    .Where(x => x.Equals(start) && !(x is ILocationWildcard))
                    .ToArray();
                if (synonymousRackLocations.Length == 0)
                {
                    throw new InvalidOperationException(string.Format("{0} 的起始点 {1} 在 {2} 中找未到了同义位置", route, start, route.Device));
                }

                if (synonymousRackLocations.Length > 1)
                {
                    var synonymousRackLocationsMsg = string.Join(",", synonymousRackLocations.Select(x => x.ToString()).ToArray());
                    throw new InvalidOperationException(string.Format("{0} 的起始点 {1} 在 {2} 中找到了多个同义位置 {3}", route, start, route.Device, synonymousRackLocationsMsg));
                }

                startLocation = (AGVSubSystemLocation)synonymousRackLocations.Single();
            }

            if (end is AGVSubSystemLocation)
            {
                endLocation = (AGVSubSystemLocation)end;
            }
            else
            {
                var synonymousRackLocations = ((GSAGVDevice)device)
                    .Locations
                    .Where(x => x.Equals(end) && !(x is ILocationWildcard))
                    .ToArray();
                if (synonymousRackLocations.Length == 0)
                    throw new InvalidOperationException(string.Format("{0} 的结束点 {1} 在 {2} 中未找到了同义位置", route, end, route.Device));

                if (synonymousRackLocations.Length > 1)
                {
                    var synonymousRackLocationsMsg = string.Join(",", synonymousRackLocations.Select(x => x.ToString()).ToArray());
                    throw new InvalidOperationException(string.Format("{0} 的起始点 {1} 在 {2} 中找到了多个同义位置 {3}", route, start, route.Device, synonymousRackLocationsMsg));
                }

                endLocation = (AGVSubSystemLocation)synonymousRackLocations.Single();
            }

            object result = device.LogicMovementTypes[0].Assembly.CreateInstance(
                        device.LogicMovementTypes[0].FullName
                        , false
                        , System.Reflection.BindingFlags.CreateInstance
                        , null
                        , new object[] { DeviceConverter.ToDevice<GSAGVDevice>(route.Device), route.Id, startLocation, endLocation, Convert.ToInt16(0) }
                        , null
                        , null);

            return (LogicMovement)result;
        }
    }
}
