using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 位置类型<br />
    /// 所有扩展的设备位置必须要有一个特定的类型，如果其值不在此枚举内，您就可能需要在此枚举中添加新的成员了
    /// </summary>
    public enum LocationType
    {
        /// <summary>
        /// 货架位置
        /// </summary>
        RackLocation,
        /// <summary>
        /// 输送线位置
        /// </summary>
        ConveyorLocation,
        /// <summary>
        /// 穿梭车位置.
        /// </summary>
        RailGuidedVehicleLocation,
        /// <summary>
        /// 位置通配符.<br />
        /// 这是一种特殊的位置类型，正常的位置只能表示特定某个物理位置。而此类型的位置可能表示一个或多个物理位置
        /// </summary>
        LocationWildcard,
    }
}
