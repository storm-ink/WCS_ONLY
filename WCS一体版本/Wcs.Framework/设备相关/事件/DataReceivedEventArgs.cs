using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 包含 <see cref="E:Wcs.Framework.Devices.Device.DataReceived"/> 事件数据的类型
    /// </summary>
    public class DataReceivedEventArgs : HandleableEventArgs
    {
        /// <summary>
        /// 数据包
        /// </summary>
        public NetPacket Data { get; private set; }

        public DataReceivedEventArgs(NetPacket data)
        {
            this.Data = data;
        }
    }
}
