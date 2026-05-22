using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 包含 <see cref="E:Wcs.Framework.Devices.Device.ReceiveData"/> 事件数据的类型
    /// </summary>
    public class ReceivedDataEventArgs : HandleableEventArgs
    {
        /// <summary>
        /// 数据包
        /// </summary>
        public NetPacket Data { get; private set; }

        public ReceivedDataEventArgs(NetPacket data)
        {
            this.Data = data;
        }
    }
}
