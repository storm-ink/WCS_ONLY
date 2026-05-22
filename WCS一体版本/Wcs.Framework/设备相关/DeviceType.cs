using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 设备类型 <br />
    /// 所有扩展的设备必须要有一个特定的类型，如果其值不在此枚举内，您就可能需要在此枚举中添加新的成员了
    /// </summary>
    public enum DeviceType
    {
        /// <summary>
        /// 输送线
        /// </summary>
        [Description("输送线")]
        Conveyor,
        /// <summary>
        /// 堆垛机
        /// </summary>
        [Description("堆垛机")]
        Crane,
        /// <summary>
        /// 穿梭车
        /// </summary>
        [Description("环形穿梭车")]
        Rgv,
        /// <summary>
        /// 抛丸区及直角穿梭车.
        /// </summary>
        [Description("抛丸区及直接穿梭车")]
        zjcsc,
        /// <summary>
        /// 条码扫描设备.
        /// </summary>
        [Description("条码扫描")]
        tmsm,
        /// <summary>
        /// 称重设备.
        /// </summary>
        [Description("称重设备")]
        czsb,
        /// <summary>
        /// 空中悬挂小车.
        /// </summary>
        [Description("空中悬挂小车")]
        EMS

    }
}
