using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 表示要处理 <see cref="T:Wcs.Framework.Devices.Conveyor"/> 的 <see cref="E:Wcs.Framework.Devices.Conveyor.TaskCurrentLocationChanged"/> 事件的处理方法
    /// </summary>
    /// <param name="conveyor">             引发此事件的输送线对象. </param>
    /// <param name="locationCurrentTask">  位置当前任务信息. </param>
    /// <param name="currentLocation">      发生的位置. </param>
    /// <param name="handled">是否已处理.</param>
    public delegate void ConveyorTaskCurrentLocationChangedEventHandler(NewConveyor conveyor, LocationCurrentTask locationCurrentTask, ConveyorLocation currentLocation, ref Boolean handled);
}
