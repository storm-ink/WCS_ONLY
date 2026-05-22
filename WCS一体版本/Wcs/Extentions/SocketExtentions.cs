using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Wcs
{
    /// <summary>
    /// Socket 扩展
    /// </summary>
    public static class SocketExtentions
    {
        /// <summary>
        /// 开启 TCP keep-alive 选项，并将检查时间修改为指定的时长（毫秒数）<br />
        /// 通常此操作应该在 socket 建立连接后立即调用
        /// </summary>
        /// <param name="socket">           连接对象. </param>
        /// <param name="checkInterval">    keep-alive 检查间隔毫秒数. </param>
        public static void KeepAlive(this Socket socket, Int32 checkInterval)
        {
            int SIO_KEEPALIVE_VALS = -1744830460;
            byte[] inValue = BitConverter.GetBytes(1)
                             .Concat(BitConverter.GetBytes(checkInterval))
                             .Concat(BitConverter.GetBytes(2000))
                             .ToArray();
            socket.IOControl(SIO_KEEPALIVE_VALS, inValue, null);
        }
    }
}
