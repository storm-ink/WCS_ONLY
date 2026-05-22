using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Crane
{
    public class DefaultLogicMovementSelector : LogicMovementSelector
    {
        public override LogicMovement ToLogicMovement(Route route,Task task)
        {
            if(!(route.Device is CraneDevice))
            {
                throw new InvalidOperationException(string.Format("{0} 所属设备不是 {1} 类型",route,typeof(CraneDevice)));
            }

            RackLocation startLocation,endLocation;
            if (route.StartLocation is RackLocation)
            {
                startLocation = (RackLocation)route.StartLocation;
            }
            else
            {
                var synonymousRackLocations = ((CraneDevice)route.Device)
                    .Locations
                    .Where(x => x.Equals(route.StartLocation) && !(x is ILocationWildcard))
                    .ToArray();
                if (synonymousRackLocations.Length == 0)
                {
                    throw new InvalidOperationException(string.Format("{0} 的起始点 {1} 在 {2} 中找未到了同义位置", route, route.StartLocation,route.Device));
                }

                if (synonymousRackLocations.Length > 1)
                {
                    var synonymousRackLocationsMsg = string.Join(",", synonymousRackLocations.Select(x => x.ToString()).ToArray());
                    throw new InvalidOperationException(string.Format("{0} 的起始点 {1} 在 {2} 中找到了多个同义位置 {3}", route, route.StartLocation,route.Device,synonymousRackLocationsMsg));
                }

                startLocation = (RackLocation)synonymousRackLocations.Single();
            }

            if (route.EndLocation is RackLocation)
            {
                endLocation = (RackLocation)route.EndLocation;
            }
            else
            {
                var synonymousRackLocations = ((CraneDevice)route.Device)
                    .Locations
                    .Where(x => x.Equals(route.EndLocation) && !(x is ILocationWildcard))
                    .ToArray();
                if (synonymousRackLocations.Length == 0)
                {
                    throw new InvalidOperationException(string.Format("{0} 的结束点 {1} 在 {2} 中未找到了同义位置", route, route.EndLocation, route.Device));
                }

                if (synonymousRackLocations.Length > 1)
                {
                    var synonymousRackLocationsMsg = string.Join(",",synonymousRackLocations.Select(x=>x.ToString()).ToArray());
                    throw new InvalidOperationException(string.Format("{0} 的结束点 {1} 在 {2} 中找到了多个同义位置 {3}", route, route.EndLocation, route.Device, synonymousRackLocationsMsg));
                }

                endLocation = (RackLocation)synonymousRackLocations.Single();
            }

            object result;
            //if (task != null
            //    && task.AdditionalInfo != null
            //    && task.AdditionalInfo.ContainsKey("_禁止堆垛机取货")
            //    && task.AdditionalInfo["_禁止堆垛机取货"] == "true")
            //{
            //    var type = route.LogicMovementTypes.FirstOrDefault(x => x == typeof(CraneMoveMovement));
            //    if (type == null)
            //    {
            //        throw new InvalidOperationException(string.Format("{0} 附加属性中声明了“_禁止堆垛机取货”,必须生成一个移动动作。但在 {1} 绑定的 Movement 中并未找到移动动作类型", task, route));
            //    }

            //    String containerCode = task.ContainerCodes.FirstOrDefault();
            //    Int16 containerCodeIntValue = 0;
            //    if (!string.IsNullOrWhiteSpace(containerCode) && containerCode.Length >= 3)
            //    {
            //        Int16.TryParse(containerCode.Substring(containerCode.Length - 3), out containerCodeIntValue);
            //    }

            //    result = type.Assembly.CreateInstance(
            //            type.FullName
            //            , false
            //            , System.Reflection.BindingFlags.CreateInstance
            //            , null
            //            , new object[] { route.Device, route.Id, startLocation, endLocation, containerCodeIntValue }
            //            , null
            //            , null);
            //}
            //else
            //{
                result = route.LogicMovementTypes[0].Assembly.CreateInstance(
                        route.LogicMovementTypes[0].FullName
                        , false
                        , System.Reflection.BindingFlags.CreateInstance
                        , null
                        , new object[] { route.Device, route.Id, startLocation, endLocation, Convert.ToInt16(0) }
                        , null
                        , null);
            //}

            return (LogicMovement)result;
        }

        public override LogicMovement ToLogicMovement(RouteHead route, Task task, Location start, Location end)
        {
            var device = DeviceConverter.ToDevice<TaskableDevice>(route.Device);
            if (!(device is CraneDevice))
            {
                throw new InvalidOperationException(string.Format("{0} 所属设备不是 {1} 类型", route, typeof(CraneDevice)));
            }

            RackLocation startLocation, endLocation;
            if (start is RackLocation)
            {
                startLocation = (RackLocation)start;
            }
            else
            {
                var synonymousRackLocations = ((CraneDevice)device)
                    .Locations
                    .Where(x => x.Equals(start) && !(x is ILocationWildcard))
                    .Select(x => (RackLocation)x)
                    .Where(x => x.ForkAction == ForkAction.Pickup || x.ForkAction == ForkAction.PickAndPut)
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

                startLocation = (RackLocation)synonymousRackLocations.Single();
            }

            if (end is RackLocation)
            {
                endLocation = (RackLocation)end;
            }
            else
            {
                var synonymousRackLocations = ((CraneDevice)device)
                    .Locations
                    .Where(x => x.Equals(end) && !(x is ILocationWildcard))
                    .Select(x=>(RackLocation)x)
                    .Where(x => x.ForkAction == ForkAction.Putdown || x.ForkAction == ForkAction.PickAndPut)
                    .ToArray();
                if (synonymousRackLocations.Length == 0)
                    throw new InvalidOperationException(string.Format("{0} 的结束点 {1} 在 {2} 中未找到了同义位置", route, end, route.Device));

                if (synonymousRackLocations.Length > 1)
                {
                    var synonymousRackLocationsMsg = string.Join(",", synonymousRackLocations.Select(x => x.ToString()).ToArray());
                    throw new InvalidOperationException(string.Format("{0} 的起始点 {1} 在 {2} 中找到了多个同义位置 {3}", route, start, route.Device, synonymousRackLocationsMsg));
                }

                endLocation = (RackLocation)synonymousRackLocations.Single();
            }

            object result;
            //if (task != null
            //    && task.AdditionalInfo != null
            //    && task.AdditionalInfo.ContainsKey("_禁止堆垛机取货")
            //    && task.AdditionalInfo["_禁止堆垛机取货"] == "true"
            //    && task.TaskType != "盘点任务")
            //{
            //    var type = device.LogicMovementTypes.FirstOrDefault(x => x == typeof(CraneMoveMovement));
            //    if (type == null)
            //    {
            //        throw new InvalidOperationException(string.Format("{0} 附加属性中声明了“_禁止堆垛机取货”,必须生成一个移动动作。但在 {1} 绑定的 Movement 中并未找到移动动作类型", task, route));
            //    }

            //    String containerCode = task.ContainerCodes.FirstOrDefault();
            //    Int16 containerCodeIntValue = 0;
            //    if (!string.IsNullOrWhiteSpace(containerCode) && containerCode.Length >= 3)
            //    {
            //        Int16.TryParse(containerCode.Substring(containerCode.Length - 3), out containerCodeIntValue);
            //    }

            //    result = type.Assembly.CreateInstance(
            //            type.FullName
            //            , false
            //            , System.Reflection.BindingFlags.CreateInstance
            //            , null
            //            , new object[] { DeviceConverter.ToDevice<CraneDevice>(route.Device), route.Id, startLocation, endLocation, containerCodeIntValue }
            //            , null
            //            , null);
            //}
            //else
            //{
            result = device.LogicMovementTypes[0].Assembly.CreateInstance(
                        device.LogicMovementTypes[0].FullName
                        , false
                        , System.Reflection.BindingFlags.CreateInstance
                        , null
                        , new object[] { DeviceConverter.ToDevice<CraneDevice>(route.Device), route.Id, startLocation, endLocation, Convert.ToInt16(0) }
                        , null
                        , null);
            //}

            return (LogicMovement)result;
        }
    }
}
