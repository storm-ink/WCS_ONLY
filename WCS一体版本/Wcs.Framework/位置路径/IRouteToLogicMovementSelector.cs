using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 设备路径转换为逻辑动作时使用的逻辑动作类型选择器接口<br />
    /// 在一个设备对应多种逻辑动作时，将由该接口负责从多个逻辑动作中返回一个最合适的逻辑动作
    /// </summary>
    public interface IDeviceRouteToLogicMovementSelector
    {
        /// <summary>
        /// 将指定的路径转换为逻辑动作
        /// </summary>
        /// <param name="route">要转换的路径</param>
        /// <returns></returns>
        LogicMovement ToLogicMovement(Route route);
    }
}
