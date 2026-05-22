using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Wcs
{
    /// <summary>
    /// TclClient 扩展
    /// </summary>
    public static class TcpClientExtentions
    {
        /// <summary>
        /// 立即连接到指定的主机端口
        /// </summary>
        /// <param name="tcpClient">客户端对象</param>
        /// <param name="ip">主机地址</param>
        /// <param name="port">主机端口</param>
        /// <param name="timeoutMilliseconds">连接超时时长（毫秒）</param>
        public static void Connect(this TcpClient tcpClient, string ip, int port, int timeoutMilliseconds)
        {
            IAsyncResult ar = tcpClient.BeginConnect(ip, port, (IAsyncResult asyncResult) =>
            {
                 
            }, tcpClient);
            System.Threading.WaitHandle wh = ar.AsyncWaitHandle;
            try
            {
                if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(timeoutMilliseconds), false))
                {
                    tcpClient.Close();
                    throw new TimeoutException();
                }
                if (tcpClient != null)
                {
                    tcpClient.EndConnect(ar);
                }
            }
            finally
            {
                if (wh != null)
                {
                    wh.Close();
                }
            }
        }

        /// <summary>
        /// 立即连接到指定的主机端口
        /// </summary>
        /// <param name="tcpClient">客户端对象</param>
        /// <param name="ipEndPoint">您打算连接到的 System.Net.IPEndPoint</param>
        /// <param name="timeoutMilliseconds">连接超时时长（毫秒）</param>
        public static void Connect(this TcpClient tcpClient, IPEndPoint ipEndPoint, int timeoutMilliseconds)
        {
            Connect(tcpClient, ipEndPoint.Address.ToString(), ipEndPoint.Port, timeoutMilliseconds);
        }
    }
}
