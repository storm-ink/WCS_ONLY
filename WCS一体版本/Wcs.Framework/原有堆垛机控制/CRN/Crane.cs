
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using ST = System.Timers;

namespace Wcs.Framework.CraneControl
{
    /// <summary>堆垛机 通讯</summary>
    [DataContract]
    public partial class Crane
    {
        public Crane(string CName)
        {
            this.CName = CName;

            ErrorList = new List<string>();
        }
        public Crane(bool Enabled, string CName, string IP, int Port)
            : this(CName)
        {
            this.Enabled = Enabled; if (!Enabled) return;

            this.IPCrane = new IPEndPoint(IPAddress.Parse(IP), Port);
            this.EConnected += new EventHandler(EventConnected);

            ST.Timer tHA = new ST.Timer(Config.Interval);
            tHA.Elapsed += (sender, e) => 
            {
                try
                {
                    if (tcpClient != null && tcpClient.Client != null && tcpClient.Connected)
                    {
                        this.SendTelex(HA.Instance);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            };
            tHA.Start();
        }

        [DataMember]
        public string CName { get; private set; }
        [DataMember]
        public bool Enabled { get; private set; }
        [DataMember]
        public bool Connected { get; set; }
        public bool TcpClientConnected
        {
            get
            {
                return Connected;
            }
        }

        [DataMember]
        public string LockGuid { get; set; }
        [DataMember]
        public string LockUser { get; set; }

        [DataMember]
        public SHB SHB { get; private set; }
        [DataMember]
        public LA LA { get; set; }

        public Wcs.Framework.Devices.Crane CraneDevice { get; set; }

        /// <summary>堆垛机 错误信息</summary>
        [DataMember]
        public List<string> ErrorList { get; set; }
        /// <summary>堆垛机相关工作模式切换信号</summary>
        public Devices.CraneWorkModeKeyInfo CraneWorkModeKeyInfo { get; set; }

        /// <summary>设置 堆垛机状态</summary>
        public LA SetLA
        {
            set
            {
                if (value == null || value.Connected == false)
                {
                    Connected = false;
                    if (EClosed != null) EClosed(this, null);
                }
                else
                {
                    LA = value;

                    Connected = true;
                    if (EConnected != null) EConnected(this, null);
                    if (EResponseTelex != null) EResponseTelex(this, new ResponseEvent(LA));
                }
            }
        }

        public void Connect(Int32 connectTimeoutMilliseconds)
        {
            if (Enabled == false || (tcpClient != null && tcpClient.Client != null && tcpClient.Connected)) return;

            try
            {
                if (tcpClient != null)
                {
                    if (tcpClient.Client != null)
                    {
                        tcpClient.Client.Close();
                    }

                    tcpClient.Close();
                }

                tcpClient = null;
            }
            catch { }

            tcpClient = new TcpClient();
            tcpClient.Connect(IPCrane.Address.ToString(), IPCrane.Port, connectTimeoutMilliseconds);

            Connected = true;
            if (EConnected != null)
            {
                EConnected(this, null);
            }
        }

        public void Disconnect()
        {
            if (tcpClient.Client != null)
            {
                tcpClient.Client.Close();
            }
            tcpClient.Close();

            tcpClient = null;

            Close(false);
        }
        public void Close(bool isError)
        {
            try
            {
                Connected = false;
                Console.WriteLine(string.Format("{0} {1} Closed", CName, IPCrane != null ? IPCrane.ToString() : ""));
                Wcs.Framework.Devices.DeviceDisconnectEventArgs arg;
                if (isError)
                {
                    arg = new Devices.DeviceDisconnectEventArgs(Devices.DeviceDisconnectReason.Error);
                }
                else
                {
                    arg = new Devices.DeviceDisconnectEventArgs(Devices.DeviceDisconnectReason.User);
                }
                if (EClosed != null) EClosed(this, arg);
            }
            catch (Exception ex)
            {
                Console.WriteLine("堆垛机 关闭通讯 异常\r\n" + ex.ToString());
            }
        }

        private void ReadListen()
        {
            try
            {
                byte[] buffer = new byte[30];
                StringBuilder strb = new StringBuilder();

                NetworkStream networkStream = tcpClient.GetStream();
                int numberOfBytesRead;
                while (tcpClient != null)
                {
                    numberOfBytesRead = networkStream.Read(buffer, 0, buffer.Length);
                    if (numberOfBytesRead <= 0)
                    {
                        throw new Exception(String.Format("{0} 已断开连接", this));
                    }

                    string str = Encoding.ASCII.GetString(buffer, 0, numberOfBytesRead);
                    foreach (char ch in str)
                    {
                        if (ch == '\x0002')
                        {
                            strb.Length = 0;
                        }
                        else if (ch != '\x0003')
                        {
                            strb.Append(ch);
                        }
                        else
                        {
                            ResponseTelex rTelex = this.Parse(strb.ToString());
                            strb.Length = 0;
                            //if (rTelex != null)
                            //{
                            //    this.EventResponseTelex(new ResponseEvent(rTelex));
                            //}
                            //没收到报文就返回null
                            this.EventResponseTelex(new ResponseEvent(rTelex));
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                trdReadListen = null;
                Console.WriteLine(exception.ToString());
                if (tcpClient != null)
                {
                    tcpClient.Close();
                }

                var socketException = exception.InnerException as System.Net.Sockets.SocketException;
                if (socketException != null && socketException.SocketErrorCode == SocketError.ConnectionAborted)
                {
                    Close(false);
                }
                else
                {
                    Close(true);
                }
            }
        }

        private ResponseTelex Parse(string sTelex)
        {
            if (!string.IsNullOrEmpty(sTelex) && (sTelex.Length >= 2))
            {
                sTelex = sTelex.Replace((char)0, '0');
                switch (sTelex.Substring(0, 2))
                {
                    case "LA":
                        LA la=ResponseTelex.ParseLA(this.CName, sTelex);
                        if (la == null)
                        {
                            Console.WriteLine("无法识别 [{0}] 接收到的报文 <{1}>", CName, sTelex);
                        }
                        else
                        {
                            if (la.ErrorCode != "0000")
                            {
                                la.ErrorInfo = Alarm.ReadErrorInfo(la.ErrorCode);
                            }

                            if (la.ErrorCode == "0000")
                            {
                                ErrorList.Clear();
                            }
                            else if (ErrorList.Count == 0 || ErrorList.Last().Substring(0, 4) != la.ErrorCode)
                            {
                                ErrorList.Add(string.Format("{0} {1}", la.ErrorCode, la.ErrorInfo));
                            }
                        }
                        return la;

                    case "LB":
                        return ResponseTelex.ParseLB(sTelex);
                }
            }
            return null;
        } 

        private void EventConnected(object sender, EventArgs e)
        {
            if (trdReadListen != null) trdReadListen.Abort();
            trdReadListen = new Thread(new ThreadStart(ReadListen));
            trdReadListen.Start();
        }
        private void EventResponseTelex(ResponseEvent e)
        {
            //如果返回的是 null，就把 la设成null
            if (e.Telex == null)
            {
                this.LA = null;
            }
            else
            {
                if (e.Telex.Head == "LA")
                {
                    this.LA = (LA)e.Telex;
                    this.LA.CraneWorkModeKeyInfo = this.CraneWorkModeKeyInfo;
                    
                    if (this.CraneDevice != null)
                    {
                        this.LA.LockGuid = this.CraneDevice.Locker.Id;
                        this.LA.LockUser = this.CraneDevice.Locker.IPAddress;
                    }
                }
            }
            if (EResponseTelex != null) EResponseTelex(this, e);
        }
    }
    public partial class Crane
    {
        /// <summary>回原点</summary>
        public void SendHP()
        {
            this.SendTelex(HP.Instance);
        }
        /// <summary>急停</summary>
        public void SendHE()
        {
            this.SendTelex(HE.Instance);
            this.SendTelex(HC.Instance);
        }
        /// <summary>取消急停</summary>
        public void SendHC()
        {
            this.SendTelex(HC.Instance);
        }        
        /// <summary>发送任务</summary>
        public void SendHB(HB hb)
        {
            this.SendTelex(hb); SHB = hb.SHB;
        }

        /// <summary>取货/放货</summary>
        public void SendGetPut(EForkLR LR, HBCommand CMD)
        {
            this.SendTelex(new HB(LR, CMD));
        }
        /// <summary>跑站点</summary>
        public void SendMove(Position P)
        {
            this.SendTelex(new HB(P));
        }

        /// <summary>发送报文</summary>
        private void SendTelex(RequestTelex telex)
        {
            try
            {
                // 发送报文，不做判断是否连接；如果是发送任务需要抛出异常
                string sTelex = string.Format("{0}{1}{2}", (char)2, telex.Text, (char)3);

                if (!(telex is HA))
                {
                    CraneDevice.Logger.Info(string.Format("发送 {0} 指令", sTelex.PadRight(30, '0')), this, sTelex);
                }
                NetworkStream ns = tcpClient.GetStream();
                byte[] aBuffer = Encoding.ASCII.GetBytes(sTelex.PadRight(30, '0'));
                ns.Write(aBuffer, 0, aBuffer.Length);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
    public partial class Crane
    {
        public event EventHandler EConnected;
        public event EventHandler<Wcs.Framework.Devices.DeviceDisconnectEventArgs> EClosed;
        public event EventHandler<ResponseEvent> EResponseTelex;

        TcpClient tcpClient; 
        public IPEndPoint IPCrane; 
        Thread trdReadListen;
    }

    /// <summary>接收报文处理事件</summary>
    public class ResponseEvent : EventArgs
    {
        /// <summary></summary>
        public ResponseEvent(ResponseTelex rTelex)
        {
            this.Telex = rTelex;
        }

        /// <summary></summary>
        public ResponseTelex Telex { get; private set; }
    }
}
