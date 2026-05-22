using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
{    
    /// <summary>
    /// 表示将要处理 <see cref="T:Wcs.Framework.Devices.Conveyor"/> 的 <see cref="E:Wcs.Framework.Devices.Conveyor.OccpiedSignalStatusChanged"/> 事件的方法
    /// </summary>
    /// <param name="conveyor"> 引发此事件的输送线对象. </param>
    /// <param name="location"> 发生的位置. </param>
    /// <param name="signal">   产生的占位信号. </param>
    /// <param name="handled">是否已处理.</param>
    public delegate void ConveyorOccpiedSignalStatusChangedEventHandler(NewConveyor conveyor, Location location, OccupiedSignal signal, ref Boolean handled);
}
