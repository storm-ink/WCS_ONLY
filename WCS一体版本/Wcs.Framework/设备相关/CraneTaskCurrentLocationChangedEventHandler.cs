using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 表示要处理 <see cref="T:Wcs.Framework.Devices.Crane"/> 的 <see cref="E:Wcs.Framework.Devices.Crane.TaskCurrentLocationChanged"/> 事件的处理方法
    /// </summary>
    /// <param name="crane">引发此事件的堆垛机</param>
    /// <param name="taskId">任务号</param>
    /// <param name="currentLocation">当前所在位置</param>
    public delegate void CraneTaskCurrentLocationChangedEventHandler(Crane crane, UInt32 taskId, CraneCurrentLocation currentLocation);
}
