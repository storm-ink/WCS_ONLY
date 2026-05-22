using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// Tcp协议数据接收器的接收事件上下文数据
    /// </summary>
    public class TcpProtocolDataReceiverReceivedEventArgs:EventArgs
    {
        /// <summary>
        /// 接收到的数据包
        /// </summary>
        public NetPacket NetPacket { get; private set; }
        /// <summary>
        /// 接收到的数据包转换后的网络传输对象
        /// </summary>
        public dynamic NetTransferObject { get; private set; }
        /// <summary>
        /// 默认构造函数
        /// </summary>
        /// <param name="netPacket">接收到的数据包</param>
        /// <param name="netTransferObject">接收到的数据包转换后的网络传输对象</param>
        public TcpProtocolDataReceiverReceivedEventArgs(NetPacket netPacket, NetTransferObject netTransferObject)
        {
            this.NetPacket = netPacket;
            this.NetTransferObject = netTransferObject;
        }
    }
}
