using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 表示将要处理 <see cref="T:Wcs.Framework.Devices.Device"/> 的 <see cref="E:Wcs.Framework.Devices.Device.TaskError"/> 事件的方法
    /// </summary>
    /// <param name="sender">           引发此事件的设备. </param>
    /// <param name="taskId">           任务号. </param>
    /// <param name="errorCode">        错误编码. </param>
    /// <param name="errorDescription"> 错误描述信息. </param>
    /// <param name="handled">          是否已处理.</param>
    public delegate void TaskErrorEventHandler(Device sender, UInt32 taskId, string errorCode, string errorDescription,ref Boolean handled);
}
