using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    /// <summary>
    /// 穿梭车工作模式，该参数会影响穿梭车的任务执行顺序
    /// </summary>
    public enum RailGuidedVehicleWrokMode
    {
        /// <summary>
        /// 默认。按照任务的创建时间升序执行
        /// </summary>
        [Description("先到先走")]
        Normal,
        /// <summary>
        /// 就近。选择离穿梭车位置最近的任务执行
        /// </summary>
        [Description("就近接货")]
        Nearby,

    }
}
