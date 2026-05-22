using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 表示继续此接口的对象是一个设备命令裁定者
    /// </summary>
    /// <remarks>命令可以继承此接口，在发送时通过接口方法识别诸如命令是否发送成功</remarks>
    public interface IDeviceCommandAdjudicator
    {
        /// <summary>
        /// 指示命令发送是否成功
        /// </summary>
        /// <param name="taskableDevice">设备</param>
        /// <param name="command">命令</param>
        /// <returns></returns>
        Boolean SendSuccess(TaskableDevice taskableDevice, DeviceCommand command);
    }
}
