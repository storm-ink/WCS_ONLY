using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.AGV
{
    public class DefaultLogicMovementSelector : LogicMovementSelector
    {
        /// <summary>
        /// 老框架支持的创建逻辑动作方法
        /// </summary>
        /// <param name="route"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        public override LogicMovement ToLogicMovement(Route route, Task task)
        {
            LogicMovement result;
            if (task != null)
            {
                String containerCode = task.ContainerCodes.FirstOrDefault();
                Int16 containerCodeIntValue = 0;
                if (!string.IsNullOrWhiteSpace(containerCode) && containerCode.Length >= 3)
                {
                    Int16.TryParse(containerCode.Substring(containerCode.Length - 3), out containerCodeIntValue);
                }

                result = (LogicMovement)route.LogicMovementTypes[0].Assembly.CreateInstance(
                        route.LogicMovementTypes[0].FullName
                        , false
                        , System.Reflection.BindingFlags.CreateInstance
                        , null
                        , new object[] { route.Device, route.Id, route.StartLocation, route.EndLocation, containerCodeIntValue }
                        , null
                        , null);
            }
            else
            {
                result = (LogicMovement)route.LogicMovementTypes[0].Assembly.CreateInstance(
                        route.LogicMovementTypes[0].FullName
                        , false
                        , System.Reflection.BindingFlags.CreateInstance
                        , null
                        , new object[] { route.Device, route.Id, route.StartLocation, route.EndLocation, Convert.ToInt16(0) }
                        , null
                        , null);
            }

            return result;
        }

        /// <summary>
        /// 新框架根据RouteHead创建逻辑动作
        /// </summary>
        /// <param name="route"></param>
        /// <param name="task"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public override LogicMovement ToLogicMovement(RouteHead route, Task task, Location start, Location end)
        {
            LogicMovement result;

            SinevaAGVDevice device = DeviceConverter.ToDevice<SinevaAGVDevice>(route.Device);

            if (task != null)
            {
                String containerCode = task.ContainerCodes.FirstOrDefault();
                Int16 containerCodeIntValue = 0;
                if (!string.IsNullOrWhiteSpace(containerCode) && containerCode.Length >= 3)
                {
                    Int16.TryParse(containerCode.Substring(containerCode.Length - 3), out containerCodeIntValue);
                }

                result = (LogicMovement)device.LogicMovementTypes[0].Assembly.CreateInstance(
                        device.LogicMovementTypes[0].FullName
                        , false
                        , System.Reflection.BindingFlags.CreateInstance
                        , null
                        , new object[] { device, route.Id, start, end, containerCodeIntValue }
                        , null
                        , null);
            }
            else
            {
                result = (LogicMovement)device.LogicMovementTypes[0].Assembly.CreateInstance(
                        device.LogicMovementTypes[0].FullName
                        , false
                        , System.Reflection.BindingFlags.CreateInstance
                        , null
                        , new object[] { device, route.Id, start, end, Convert.ToInt16(0) }
                        , null
                        , null);
            }

            return result;
        }
    }
}
