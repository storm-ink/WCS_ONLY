using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wcs;

namespace Wcs.Framework
{
    public abstract class TcpProtocolDevice : Device
    {
        protected System.Threading.Thread _receiveThread;
        protected TcpClient _tcpClient;
        byte[] _lastSentBytes = new byte[0];
        #region Proprieties
        /// <summary>
        /// 表示网络端点的IP 地址和端口号
        /// </summary>
        public IPEndPoint IPEndPoint { get; protected set; }

        /// <summary>
        /// 通讯绑定的地址
        /// </summary>
        public IPEndPoint BindEndPoint { get; protected set; }

        /// <summary>
        /// TCP协议数据接收器
        /// </summary>
        public IDataReceiver DataReceiver { get; private set; }


        /// <summary>
        /// 网络数据发送超时时间
        /// </summary>
        public Int32 SendTimeout { get; private set; }
        #endregion


        /// <summary>
        /// 当接收到新的数据时发生
        /// </summary>
        /// <param name="netPacket">接收到的数据包</param>
        /// <param name="netTransferObject">接收到的数据包转换后的网络传输对象</param>
        protected abstract void OnDataReceived(NetPacket netPacket, NetTransferObject netTransferObject);

        /// <summary>
        /// 默认构造函数
        /// </summary>
        /// <param name="name">设备名称</param>
        /// <param name="ipEndPoint">表示网络端点的IP 地址和端口号</param>
        /// <param name="receiveTimeout">接收超时时间（毫秒）</param>
        /// <param name="connectTimeout">连接超时时间（毫秒）</param>
        public TcpProtocolDevice(string name, Int32 no, Int32 receiveTimeout, Int32 connectTimeout, Int32 sendTimeout, IPEndPoint ipEndPoint, IPEndPoint bindEndPoint, IDataReceiver dataReceiver)
            : base(name, no, receiveTimeout, connectTimeout)
        {
            if (ipEndPoint == null)
            {
                throw new ArgumentNullException("ipEndPoint", "必须为以Tcp协议通讯的设备指定一个网络端点");
            }

            this.BindEndPoint = bindEndPoint;

            if (dataReceiver == null)
            {
                throw new ArgumentNullException("dataReceiver", "必须为以Tcp协议通讯的设备指定一个接收器");
            }
            this.SendTimeout = sendTimeout;
            this.IPEndPoint = ipEndPoint;
            this.DataReceiver = dataReceiver;
            this.DataReceiver.DeviceName = name;
            this.DataReceiver.DataReceived += new EventHandler<DataReceiverReceivedEventArgs>((sender, args) =>
            {
                OnDataReceived(args.NetPacket, args.NetTransferObject);
            });
        }

        /// <summary>
        /// 接收数据缓冲区大小
        /// </summary>
        protected virtual Int32 ReceiveBufferSize
        {
            get
            {
                return 1024 * 1024 * 10;
            }
        }

        /// <summary>
        /// 开始接收来自网络的数据<br />
        /// 此方法一般只在连接成功后调用。
        /// </summary>
        protected virtual void Receive()
        {
            try
            {
                this.DataReceiver.Clear();

                this._logger.Info1(string.Format("{0} 数据接收线程已启动", this), this);
                byte[] readBuffer = new byte[ReceiveBufferSize];
                int numberOfBytesRead;
                NetworkStream networkStream = _tcpClient.GetStream();
                _tcpClient.ReceiveTimeout = this.ReceiveTimeout;
                while (_tcpClient != null)
                {
                    numberOfBytesRead = networkStream.Read(readBuffer, 0, readBuffer.Length);

                    if (numberOfBytesRead <= 0)
                    {
                        throw new Exception(String.Format("{0} 已断开连接", this));
                    }
                    this.DataReceiver.AddBytes(readBuffer.Take(numberOfBytesRead).ToArray());
                    //Thread.Sleep(1);
                }
                this._logger.Warn1(string.Format("{0} 数据接收线程已停止", this), this);

                this.DataReceiver.Clear();
                releaseTcpConnection();
                FireDisconnectedEvent(new DisconnectEventArgs(DisconnectReason.Error, new Exception("数据接收线程停止")));

            }
            catch (Exception ex)
            {
                this.DataReceiver.Clear();

                releaseTcpConnection();

                FireDisconnectedEvent(new DisconnectEventArgs(DisconnectReason.Error, ex));

                this._logger.Error1(new Exception(string.Format("{0} 数据接收线程已停止", this), ex), this);
            }

            this.DataReceiver.Clear();

            MessageBoard.AbstractMessageBoard.Instance.Add(new MessageBoard.Messages.DisconnectedMessage(this));

        }


        /// <summary>
        /// 释放tcp连接资源
        /// </summary>
        void releaseTcpConnection()
        {
            if (_tcpClient != null)
            {
                try
                {
                    if (_tcpClient.Client != null)
                    {
                        this._logger.Debug1("关闭 socket 连接并释放所有关联的资源", this);
                        if (_tcpClient.Connected)
                        {
                            _tcpClient.Client.Shutdown(SocketShutdown.Both);
                            _tcpClient.Client.Disconnect(false);
                        }
                    }

                    _tcpClient.Close();
                    this._logger.Debug1("释放 TcpClient 实例，并请求关闭基础 TCP 连接", this);
                }
                catch (Exception e)
                {
                    this._logger.Error1(e, this);
                }
            }

            _tcpClient = null;
        }

        protected virtual void Write(byte[] bytes)
        {
            if (!this.IsConnected)
            {
                throw new InvalidOperationException("未连接，无法写入数据。");
            }

            _tcpClient.GetStream().Write(bytes, 0, bytes.Length);

            _lastSentBytes = bytes;
        }

        /// <summary>
        /// 获取最后一次发出的数据
        /// </summary>
        /// <returns></returns>
        public virtual byte[] GetLastSentBytes()
        {
            return _lastSentBytes;
        }



        /// <summary>
        /// 向设备中同步写入数据
        /// </summary>
        /// <typeparam name="TData">泛型参数，要写入的数据类型</typeparam>
        /// <param name="data">要写入的具体数据</param>
        /// <param name="isSuccess">是否成功的判定表示式</param>
        /// <remarks>如果失败，则抛出异常。</remarks>
        public virtual void Write(byte[] bytes, Func<TcpProtocolDevice, byte[], Boolean> isSuccess)
        {
            var hexStr = string.Join(" ", bytes.Select(x => x.ToString("x2")));
            this.Log(string.Format("准备发送 {0} 指令", hexStr));
            Action<byte[]> act = (_bytes) =>
            {
                Write(_bytes);
            };

            DateTime startTime = DateTime.Now;
            IAsyncResult ar = act.BeginInvoke(bytes, null, null);
            if (!ar.AsyncWaitHandle.WaitOne(this.SendTimeout))
            {
                this.Log(String.Format("{0} 发送超时", hexStr));
                throw new TimeoutException(string.Format("{0} 发送超时", hexStr));
            }
            ar = null;


            if (isSuccess == null)
                return;
            Boolean success = isSuccess(this, bytes);
            while (!success && DateTime.Now.Subtract(startTime).TotalMilliseconds < this.SendTimeout)
            {
                System.Threading.Thread.Sleep(30);
                success = isSuccess(this, bytes);
            }

            if (!success)
            {
                this.Log(string.Format("{0} 在规定的 {1} 毫秒内，未接收到成功信号。发送失败", hexStr, this.SendTimeout));
                throw new TimeoutException(string.Format("{0} 在规定的 {1} 毫秒内，未接收到成功信号。发送失败", hexStr, this.SendTimeout));
            }
        }

        /// <summary>
        /// 向设备中同步写入数据
        /// </summary>
        /// <typeparam name="TData">泛型参数，要写入的数据类型</typeparam>
        /// <param name="data">要写入的具体数据</param>
        /// <param name="isSuccess">是否成功的判定表示式</param>
        /// <remarks>如果失败，则抛出异常。</remarks>
        public virtual void Write(byte[] bytes, Func<Boolean> isSuccess)
        {
            var hexStr = string.Join(" ", bytes.Select(x => x.ToString("x2")));
            this.Log(string.Format("准备发送 {0} 指令", hexStr));
            Action<byte[]> act = (_bytes) =>
            {
                Write(_bytes);
            };

            DateTime startTime = DateTime.Now;
            IAsyncResult ar = act.BeginInvoke(bytes, null, null);
            if (!ar.AsyncWaitHandle.WaitOne(this.SendTimeout))
            {
                this.Log(String.Format("{0} 发送超时", hexStr));
                throw new TimeoutException(string.Format("{0} 发送超时", hexStr));
            }
            ar = null;

            Boolean success = isSuccess();
            while (!success && DateTime.Now.Subtract(startTime).TotalMilliseconds < this.SendTimeout)
            {
                System.Threading.Thread.Sleep(30);
                success = isSuccess();
            }

            if (!success)
            {
                this.Log(string.Format("{0} 在规定的 {1} 毫秒内，未接收到成功信号。发送失败", hexStr, this.SendTimeout));

                throw new TimeoutException(string.Format("{0} 在规定的 {1} 毫秒内，未接收到成功信号。发送失败", hexStr, this.SendTimeout));
            }
        }
        #region Implement Device
        public override bool IsConnected
        {
            get
            {
                return _tcpClient != null
                    && _tcpClient.Client != null
                    && _tcpClient.Connected;
            }
        }

        DateTime? _lastReConnectAt = null;
        public override bool Connect()
        {
            try
            {
                lock (this)
                {
                    this._logger.Trace1(string.Format("开始连接到 {0}...", this.IPEndPoint), this);
                    if (IsConnected)
                    {
                        this._logger.Warn1(string.Format("{0} 已处于连接状态", this), this);
                        return true;
                    }

                    _tcpClient = new TcpClient();
                    if (BindEndPoint != null)
                    {
                        _tcpClient.Client.Bind(BindEndPoint);
                    }
                    _tcpClient.Connect(this.IPEndPoint, this.ConnectTimeout);
                    _tcpClient.Client.KeepAlive(10000);

                    this.DataReceiver.Clear();

                    this._logger.Info1("TCP 连接成功", this);

                    FireConnectedEvent();
                    this._logger.Trace1("准备启用数据接收线程...", this);


                    //ThreadPool.QueueUserWorkItem((state) =>
                    //{
                    //    Receive();
                    //});
                    _receiveThread = new Thread(new ThreadStart(Receive));
                    _receiveThread.Name = string.Format("{0}的数据接收线程", this);
                    _receiveThread.IsBackground = true;

                    _receiveThread.StartAndManaged();

                    this._logger.Info1(string.Format("{0} 连接成功", this), this);

                    _lastReConnectAt = null;


                    MessageBoard.AbstractMessageBoard.Instance.Add(new MessageBoard.Messages.TipMessage(MessageBoard.MessageLevel.Info, this.Name, "连接成功", null));


                    return true;
                }
            }
            catch (Exception ex)
            {
                this._logger.Warn1(string.Format("{0} 连接失败", this), this, ex);

                this.DataReceiver.Clear();
                releaseTcpConnection();

                FireDisconnectedEvent(new DisconnectEventArgs(DisconnectReason.ConnectFailed, ex));

                if (_lastReConnectAt == null)
                {
                    _lastReConnectAt = DateTime.Now;
                }

                //if (DateTime.Now.Subtract(_lastReConnectAt.Value).TotalSeconds >= UnableToConnectErrorTimeout)
                //{
                //    FireDeviceErrorEvent(new DeviceWarningEventArgs(this.DeviceWarningFactory.CreateUnableToConnectError(this)));
                //}

                return false;
            }
        }

        public override void Disconnect()
        {

            this._logger.Trace1(string.Format("准备强制断开 {0} 连接...", this), this);
            releaseTcpConnection();

            this.DataReceiver.Clear();

            FireDisconnectedEvent(new DisconnectEventArgs(DisconnectReason.User));
            this._logger.Info1(string.Format("{0} 被强制断开连接", this), this);

            MessageBoard.AbstractMessageBoard.Instance.Add(new MessageBoard.Messages.DisconnectedMessage(this));
        }
        #endregion
    }
}
