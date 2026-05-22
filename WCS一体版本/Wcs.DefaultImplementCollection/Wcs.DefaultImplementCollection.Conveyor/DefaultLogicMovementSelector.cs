using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    public class DefaultLogicMovementSelector : LogicMovementSelector
    {
        public override LogicMovement ToLogicMovement(Route route, Task task)
        {
            if (!(route.Device is ConveyorDevice))
            {
                throw new InvalidOperationException(string.Format("{0} 所属设备不是 {1} 类型", route, typeof(ConveyorDevice)));
            }

            ConveyorLocation startLocation, endLocation;
            if (route.StartLocation is ConveyorLocation)
            {
                startLocation = (ConveyorLocation)route.StartLocation;
            }
            else
            {
                var synonymousRackLocations = ((ConveyorDevice)route.Device)
                    .Locations
                    .Where(x => x.Equals(route.StartLocation) && !(x is ILocationWildcard))
                    .ToArray();
                if (synonymousRackLocations.Length == 0)
                {
                    throw new InvalidOperationException(string.Format("{0} 的起始点 {1} 在 {2} 中找未到了同义位置", route, route.StartLocation, route.Device));
                }

                if (synonymousRackLocations.Length > 1)
                {
                    var synonymousRackLocationsMsg = string.Join(",", synonymousRackLocations.Select(x => x.ToString()).ToArray());
                    throw new InvalidOperationException(string.Format("{0} 的起始点 {1} 在 {2} 中找到了多个同义位置 {3}", route, route.StartLocation, route.Device, synonymousRackLocationsMsg));
                }

                startLocation = (ConveyorLocation)synonymousRackLocations.Single();
            }

            if (route.EndLocation is ConveyorLocation)
            {
                endLocation = (ConveyorLocation)route.EndLocation;
            }
            else
            {
                var synonymousRackLocations = ((ConveyorDevice)route.Device)
                    .Locations
                    .Where(x => x.Equals(route.EndLocation) && !(x is ILocationWildcard))
                    .ToArray();
                if (synonymousRackLocations.Length == 0)
                {
                    throw new InvalidOperationException(string.Format("{0} 的结束点 {1} 在 {2} 中未找到了同义位置", route, route.EndLocation, route.Device));
                }

                if (synonymousRackLocations.Length > 1)
                {
                    var synonymousRackLocationsMsg = string.Join(",", synonymousRackLocations.Select(x => x.ToString()).ToArray());
                    throw new InvalidOperationException(string.Format("{0} 的结束点 {1} 在 {2} 中找到了多个同义位置 {3}", route, route.EndLocation, route.Device, synonymousRackLocationsMsg));
                }

                endLocation = (ConveyorLocation)synonymousRackLocations.Single();
            }

            object result;
            result = route.LogicMovementTypes[0].Assembly.CreateInstance(
                    route.LogicMovementTypes[0].FullName
                    , false
                    , System.Reflection.BindingFlags.CreateInstance
                    , null
                    , new object[] { route.Device, route.Id, startLocation, endLocation, Convert.ToInt16(0) }
                    , null
                    , null);

            return (LogicMovement)result;
        }

        public override LogicMovement ToLogicMovement(RouteHead route, Task task, Location start, Location end)
        {
            var device = DeviceConverter.ToDevice<TaskableDevice>(route.Device);
            if (!(device is ConveyorDevice))
            {
                throw new InvalidOperationException(string.Format("{0} 所属设备不是 {1} 类型", route, typeof(ConveyorDevice)));
            }

            ConveyorLocation startLocation, endLocation;
            if (start is ConveyorLocation)
            {
                startLocation = (ConveyorLocation)start;
            }
            else
            {
                var synonymousRackLocations = ((ConveyorDevice)device)
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

                startLocation = (ConveyorLocation)synonymousRackLocations.Single();
            }

            if (end is ConveyorLocation)
            {
                endLocation = (ConveyorLocation)end;
            }
            else
            {
                var synonymousRackLocations = ((ConveyorDevice)device)
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

                endLocation = (ConveyorLocation)synonymousRackLocations.Single();
            }

            object result = device.LogicMovementTypes[0].Assembly.CreateInstance(
                        device.LogicMovementTypes[0].FullName
                        , false
                        , System.Reflection.BindingFlags.CreateInstance
                        , null
                        , new object[] { DeviceConverter.ToDevice<ConveyorDevice>(route.Device), route.Id, startLocation, endLocation, Convert.ToInt16(0) }
                        , null
                        , null);

            return (LogicMovement)result;
        }
    }
}
