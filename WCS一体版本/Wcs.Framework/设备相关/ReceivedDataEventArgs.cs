using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 包含 <see cref="E:Wcs.Framework.Devices.Device.ReceiveData"/> 事件数据的类型
    /// </summary>
    public class ReceivedDataEventArgs:EventArgs
    {
        /// <summary>
        /// 数据报
        /// </summary>
        public Datagram Datagram { get; private set; }

        public ReceivedDataEventArgs(Datagram datagram)
        {
            this.Datagram = datagram;
        }
    }
}
