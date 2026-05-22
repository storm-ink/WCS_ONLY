using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Devices;

namespace Wcs.Framework.Impl
{
    /// <summary>
    /// 默认的设备路径逻辑动作选择器,返回第一个逻辑动作类型
    /// </summary>
    public class DefaultDeviceRouteToLogicMovementSelector : IDeviceRouteToLogicMovementSelector
    {
        /// <summary>
        /// 将指定的路径转换为逻辑动作
        /// </summary>
        /// <param name="route">要转换的路径</param>
        /// <returns></returns>
        public LogicMovement ToLogicMovement(DeviceRoute route)
        {
            var result = route.LogicMovementTypes[0].Assembly.CreateInstance(
                    route.LogicMovementTypes[0].FullName
                    , false
                    , System.Reflection.BindingFlags.CreateInstance
                    , null
                    , new object[] { route.Device, route.Id, route.StartLocation, route.EndLocation }
                    , null
                    , null);

            return (LogicMovement)result;
        }
    }
}
