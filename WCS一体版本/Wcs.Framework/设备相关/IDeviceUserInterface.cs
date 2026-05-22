using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.Framework
{
    /// <summary>
    /// 设备用户界面接口
    /// </summary>
    public interface IDeviceUserInterface:IDisposable
    {
        /// <summary>
        /// 向用户展示这个界面
        /// </summary>
        /// <param name="device">设备</param>
        void Show(Device device);
    }
}
