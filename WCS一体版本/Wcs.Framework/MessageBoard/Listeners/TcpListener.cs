using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using NLog;

namespace Wcs.Framework.MessageBoard.Listeners
{
    public sealed class TcpListener:AbstractMessageBoardListener
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();
        System.Net.Sockets.TcpListener _tcpListener;
        Boolean _stopCalled = false;
        List<TcpClient> _clients = new List<TcpClient>();
        Object _clientsLocker = new object();
        public TcpListener(String name, Boolean async, Boolean enabled, System.Xml.XmlNode cfg)
            :base(name,async,enabled,cfg)
        {
            Enabled = enabled;
            Async = async;
            Name = name;

            if (cfg.Attributes["ip"] == null || String.IsNullOrWhiteSpace(cfg.Attributes["ip"].Value))
            {
                throw new ConfigurationErrorsException("ip 属性未配置。", cfg);
            }

            if (cfg.Attributes["port"] == null || String.IsNullOrWhiteSpace(cfg.Attributes["port"].Value))
            {
                throw new ConfigurationErrorsException("port 属性未配置。", cfg);
            }

            IPAddress ipAddress = IPAddress.Parse(cfg.Attributes["ip"].Value);
            Int32 port = int.Parse(cfg.Attributes["port"].Value);

            _tcpListener = new System.Net.Sockets.TcpListener(ipAddress, port);
            _tcpListener.Start();

            System.Threading.ThreadPool.QueueUserWorkItem(Listen,this);
        }

        void Listen(Object stat)
        {
            try
            {
                // 侦听循环
                while (!_stopCalled)
                {
                    if (!_tcpListener.Pending())
                    {
                        System.Threading.Thread.Sleep(1);
                        continue;
                    }

                    TcpClient tcpClient = _tcpListener.AcceptTcpClient();
                    System.Threading.ThreadPool.QueueUserWorkItem(CheckClientConnectionStatePorcess, tcpClient);
                }
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
            }
            finally
            {
                _tcpListener.Stop();

                _logger.Debug1(string.Format("{0} 已停止。", _tcpListener.LocalEndpoint), this);
            }
        }

        public override void Write(AbstractMessageBoard messageBoard, AbstractMessage message)
        {
            lock (_clientsLocker)
            {
                var messageBytes=System.Text.Encoding.UTF8.GetBytes(message.GetData());
                if(messageBytes.Length==0)
                {
                    return;
                }

                foreach (var client in _clients)
                {
                    try
                    {
                        client.GetStream().Write(messageBytes, 0, messageBytes.Length);
                    }
                    catch (Exception ex)
                    {
                        if (client.Client != null)
                        {
                            _logger.Warn1(string.Format("向 {0} 推送消息时发生异常：{1}", client.Client.RemoteEndPoint, ex), this);
                        }
                        else
                        {
                            _logger.Warn1(string.Format("向 {0} 推送消息时发生异常：{1}", client, ex), this);
                        }
                    }
                }
            }
        }

        public override void Dispose()
        {
            _stopCalled = true;
        }

        void CheckClientConnectionStatePorcess(Object acceptTcpClient)
        {
            TcpClient _tcpClient = (TcpClient)acceptTcpClient;

            _logger.Debug1(string.Format("{0} 已连接。", _tcpClient.Client.RemoteEndPoint), this);

            lock (_clientsLocker)
            {
                _clients.Add(_tcpClient);
            }

            byte[] readBuffer = new byte[10];
            int numberOfBytesRead;
            NetworkStream networkStream = _tcpClient.GetStream();

            byte[] emptyMessage = System.Text.Encoding.UTF8.GetBytes("<message></message>");

            try
            {
                while (!_stopCalled)
                {
                    //每秒一个心跳信号
                    System.Threading.Thread.Sleep(1000);
                    networkStream.Write(emptyMessage, 0, emptyMessage.Length);

                    if (!networkStream.DataAvailable)
                    {
                        continue;
                    }

                    numberOfBytesRead = networkStream.Read(readBuffer, 0, readBuffer.Length);

                    if (numberOfBytesRead <= 0)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
            }
            finally
            {
                _logger.Debug1(string.Format("{0} 已断开。", _tcpClient.Client.RemoteEndPoint), this);
            }


            try
            {
                lock (_clientsLocker)
                {
                    _clients.Remove(_tcpClient);
                }

                _tcpClient.Client.Shutdown(SocketShutdown.Both);
                _tcpClient.Client.Disconnect(false);
            }
            catch (Exception)
            {
            }

            _tcpClient = null;
        }

    }
}
