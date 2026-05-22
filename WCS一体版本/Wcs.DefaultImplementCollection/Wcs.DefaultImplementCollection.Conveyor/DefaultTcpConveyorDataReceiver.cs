using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Collections;
using Wcs.Framework;
using System.Threading;
using System.Runtime.InteropServices;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    public class DefaultConveyorTcpProtocolDataReceiver : IDataReceiver
    {
        public _DefaultConveyorTcpProtocolDataReceiverEncoding SenderEncoding { get; private set; }
        public _DefaultConveyorTcpProtocolDataReceiverEncoding ReceiverEncoding { get; private set; }
        List<byte> _bytes = new List<byte>();

        public string DeviceName { get; set; }
        public DefaultConveyorTcpProtocolDataReceiver(String name, XmlNode senderEncodingNode, XmlNode receiverEncodingNode)
        {
            this.Name = name;
            SenderEncoding = new _DefaultConveyorTcpProtocolDataReceiverEncoding(senderEncodingNode);
            ReceiverEncoding = new _DefaultConveyorTcpProtocolDataReceiverEncoding(receiverEncodingNode);
        }

        bool IsRunning;
        Thread _thread;
        /// <summary>
        /// 启动解析线程
        /// </summary>
        public virtual void Start()
        {
            lock (this)
            {
                if (IsRunning)
                {
                    return;
                }

                this.IsRunning = true;
                _thread = new Thread(Tick);
                _thread.IsBackground = true;
                _thread.Name = string.Format("{0}的解析线程", this.DeviceName);
                _thread.StartAndManaged();
            }
        }

        _DB2 _lastDb2 = null;
        private void Tick()
        {
            List<byte> list = new List<byte>();
            while (true)
            {
                try
                {
                    Thread.Sleep(100);
                    var _bs = popList();
                    if (_bs != null && _bs.Length >= ReceiverEncoding.TotalBytes)
                    {
                        ConveyorNetPacket netPacket = null;
                        netPacket = FetchPackage2(_bs);

                        if (netPacket != null && DataReceived != null)
                        {
                            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                            sw.Start();
                            var items = ReceiverEncoding.DecodeAll(netPacket.Data);
                            _DB2 db2 = new _DB2();
                            db2.Items = items;

                            DataReceived(this, new DataReceiverReceivedEventArgs(netPacket, db2));
                            //Action action = () => {
                            //    DataReceived(this, new DataReceiverReceivedEventArgs(netPacket, db2));
                            //};
                            //action.BeginInvoke(null,null);

                            List<string> msgs = new List<string>();
                            foreach (var values in db2.Items.Values)
                            {
                                List<string> keys = new List<string>();
                                List<string> _msgs = new List<string>();
                                foreach (var item in values)
                                {
                                    var _item = item.GetDataGridViewShow();
                                    if (_msgs.Count() == 0)
                                    {
                                        //keys = _item.GetType().GetProperties().Select(x => x.Name).ToList();
                                        keys = ReceiverEncoding._collectionSettings.FirstOrDefault(x => x.type.Name == item.GetType().Name).propertys.Select(x => x.name).ToList();
                                        _msgs.Add($">>>>>>>>{item.GetType().Name}<<<<<<<<");
                                        _msgs.Add(string.Join("\t", keys));
                                    }
                                    List<object> _values = new List<object>();
                                    foreach (var key in keys)
                                    {
                                        if (item.GetType().GetProperty(key).PropertyType == typeof(byte[]) || item.GetType().GetProperty(key).PropertyType == typeof(Byte[]))
                                        {
                                            var byteArray = (byte[])item[key];
                                            _values.Add(string.Join(" ", byteArray.Select(x => x.ToString("X2"))));
                                        }
                                        else
                                            _values.Add(item[key]);
                                    }

                                    _msgs.Add(string.Join("\t", _values));
                                }
                                msgs.AddRange(_msgs);
                            }
                            var msgStr = string.Join("\r\n", msgs);
                            sw.Stop();
                            this.Log($"数据包解码耗时 {sw.ElapsedMilliseconds} ms(大小：{Encoding.UTF8.GetBytes(msgStr).Length} 字节)\r\n{msgStr}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Log($"数据包解码耗时出错，异常消息：\r\n{ex}");
                }
            }
        }

        #region Implement ITcpProtocolDataReceiver

        public String Name { get; private set; }

        public TNetTransferObject ConvertToNetTransferObject<TNetTransferObject>(NetPacket netPacket) where TNetTransferObject : NetTransferObject
        {
            if (typeof(TNetTransferObject) == typeof(_DB1))
            {
                return (TNetTransferObject)((NetTransferObject)ConvertToDB1(netPacket));
            }
            else if (typeof(TNetTransferObject) == typeof(_DB2))
            {
                return (TNetTransferObject)((NetTransferObject)ConvertToDB2(netPacket));
            }
            else
            {
                throw new NotSupportedException(string.Format("未处理的类型转换 {0} to {1}", netPacket.GetType(), typeof(TNetTransferObject)));
            }
        }

        public NetPacket ConvertToNetPacket<TNetTransferObject>(TNetTransferObject netTransferObject) where TNetTransferObject : NetTransferObject
        {
            if (netTransferObject is _DB1)
            {
                return ConvertToNetPacket((_DB1)((NetTransferObject)netTransferObject));
            }
            else if (netTransferObject is _DB2)
            {
                return ConvertToNetPacket((_DB2)((NetTransferObject)netTransferObject));
            }
            else
            {
                var fullMap = this.SenderEncoding.CreateFullMap();
                Type key = netTransferObject.GetType();
                while (!fullMap.ContainsKey(key) && key != typeof(Object))
                {
                    key = key.BaseType;
                }
                if (!fullMap.ContainsKey(key))
                {
                    throw new KeyNotFoundException(string.Format("给定的关键字 {0} 不在列表中", key));
                }

                fullMap[key][0] = netTransferObject;
                _DB1 db1 = new _DB1();
                db1.Items = fullMap;
                return ConvertToNetPacket(db1);
            }
        }

        object listLocker = new object();
        List<byte[]> list = new List<byte[]>();
        void pushList(byte[] _bs)
        {
            lock (listLocker)
            {
                list.Add(_bs);
            }
        }
        byte[] popList()
        {
            lock (listLocker)
            {
                //if (this.DeviceName == "成品二楼CV")
                //    Console.WriteLine($"{list.Count()} 条待解析");

                this.Log($"{list.Count()} 条待解析，即将取最后一条尝试解析");

                if (list.Count() == 0)
                    return null;

                var _bs = list.LastOrDefault();
                list.Clear();
                return _bs;
            }
        }

        public void AddBytes(byte[] bytes)
        {
            ConveyorNetPacket netPacket = null;

            //模拟器接收的是DB1，和正常应用程序处理顺序是相反的
            if (Wcs.Framework.Cfg.WcsConfiguration.IsSimulationApplication)
            {
                lock (_bytes)
                {
                    _bytes.AddRange(bytes);
                    if (_bytes.Count >= SenderEncoding.TotalBytes)
                    {
                        netPacket = FetchPackage();
                    }
                }

                if (netPacket != null && DataReceived != null)
                {
                    var items = SenderEncoding.DecodeAll(netPacket.Data);
                    _DB1 db1 = new _DB1();
                    db1.Items = items;

                    DataReceived(this, new DataReceiverReceivedEventArgs(netPacket, db1));
                }
            }
            else
            {
                lock (_bytes)
                {
                    if (!IsRunning)
                        Start();
                    _bytes.AddRange(bytes);
                    if (_bytes.Count >= ReceiverEncoding.TotalBytes)
                    {
                        int endIndex = -1;
                        byte[] byteArray = _bytes.ToArray();
                        string byteStr = string.Join("", byteArray.Select(x => x.ToString("x2")));
                        this.Log($"数据包-->十六进制报文\r\n{byteStr}");
                        int _endindex = byteStr.LastIndexOf(end);
                        if (_endindex == -1)
                        {
                            _bytes.Clear();
                            return;
                        }

                        endIndex = _endindex / 2;
                        var _bs = _bytes.Take(endIndex + 12).ToArray();
                        if (_bs.Length >= ReceiverEncoding.TotalBytes)
                            pushList(_bs);
                        _bytes = _bytes.Skip(endIndex + 12).ToList();
                        return;

                        netPacket = FetchPackage2();
                    }

                    //如果接收的无用数据过多（5倍于DB2大小），清除
                    if (_bytes.Count > ReceiverEncoding.TotalBytes * 10)
                    {
                        _bytes.Clear();
                    }
                }

                if (netPacket != null && DataReceived != null)
                {
                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    sw.Start();
                    var items = ReceiverEncoding.DecodeAll(netPacket.Data);
                    _DB2 db2 = new _DB2();
                    db2.Items = items;
                    sw.Stop();
                    this.Log($"数据包解码耗时 {sw.ElapsedMilliseconds} ms");

                    DataReceived(this, new DataReceiverReceivedEventArgs(netPacket, db2));
                }
            }
        }

        public void Clear()
        {
            lock (_bytes)
            {
                _bytes.Clear();
            }
        }

        public event EventHandler<DataReceiverReceivedEventArgs> DataReceived;
        #endregion

        #region Private Methos
        _DB1 ConvertToDB1(NetPacket netPacket)
        {
            var items = this.SenderEncoding.DecodeAll(netPacket.Data);
            var db1 = new _DB1();
            db1.Items = items;
            return db1;
        }

        _DB2 ConvertToDB2(NetPacket netPacket)
        {
            var items = this.ReceiverEncoding.DecodeAll(netPacket.Data);
            var db2 = new _DB2();
            db2.Items = items;
            return db2;
        }

        NetPacket ConvertToNetPacket(_DB1 db1)
        {
            var data = this.SenderEncoding.Encode(db1.Items);
            ConveyorNetPacket packet = new ConveyorNetPacket();
            packet = (ConveyorNetPacket)packet.CreateNetPackage(data);

            return packet;
        }

        NetPacket ConvertToNetPacket(_DB2 db2)
        {
            var data = this.ReceiverEncoding.Encode(db2.Items);
            ConveyorNetPacket packet = new ConveyorNetPacket();
            packet = (ConveyorNetPacket)packet.CreateNetPackage(data);

            return packet;
        }

        /// <summary>
        /// 尝试从当前已接收的数据区获取一个完整的数据包
        /// </summary>
        /// <returns></returns>
        ConveyorNetPacket FetchPackage()
        {
            if (_bytes.Count < 12)
            {
                return null;
            }

            int headerIndex = -1;
            int endIndex = -1;
            byte[] byteArray = _bytes.ToArray();
            for (int i = 0; i < byteArray.Length;)
            {
                if (headerIndex == -1)
                {
                    while (i < byteArray.Length - 1 && byteArray[i] != 255)
                    {
                        i++;
                    }

                    if (byteArray.Skip(i).Count() < 12)
                    {
                        return null;
                    }
                    var str = string.Join("", byteArray.Skip(i).Select(x => x.ToString("x2")));
                    //12个字节如果都是f，那说明是开头(对应64位uint)
                    if (str.StartsWith("".PadLeft(24, 'f')))
                    {
                        headerIndex = i;
                        i += 12;
                    }
                    else
                    {
                        i++;
                    }
                }
                else
                {
                    while (i < byteArray.Length - 1 && byteArray[i] != 238)
                    {
                        i++;
                    }

                    if (byteArray.Skip(i).Count() < 12)
                    {
                        return null;
                    }

                    var str = string.Join("", byteArray.Skip(i).Select(x => x.ToString("x2")));
                    //12个字节如果都是e，那说明是开头(对应64位uint)
                    if (str.StartsWith("".PadLeft(24, 'e')))
                    {
                        endIndex = i;
                        i += 12;
                        while (i < byteArray.Length && byteArray[i] == 238)
                        {
                            endIndex++;
                            i++;
                        }
                    }
                    else
                    {
                        i++;
                    }
                }

                if (headerIndex != -1 && endIndex != -1)
                {
                    int contentStartIndex = headerIndex + 12;
                    int contentEndIndex = endIndex;
                    byte[] result = byteArray.Skip(headerIndex)
                       .Take(endIndex + 12 - headerIndex)
                       .ToArray();

                    _bytes = byteArray.Skip(endIndex + 12).ToList();


                    return new ConveyorNetPacket(result);
                }
            }

            return null;
        }

        const string start = "ffffffffffffffffffffffff";
        const string end = "eeeeeeeeeeeeeeeeeeeeeeee";
        long tsMax = 0;
        /// <summary>
        /// 尝试从当前已接收的数据区获取一个完整的数据包
        /// </summary>
        /// <returns></returns>
        ConveyorNetPacket FetchPackage2()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            if (_bytes.Count < 12)
            {
                sw.Stop();
                this.Log($"当前解析报文长度{_bytes.Count}，小于12，返回本次解析结果null,本次解析总计耗时 {sw.ElapsedMilliseconds}ms");
                return null;
            }

            int headerIndex = -1;
            int endIndex = -1;
            byte[] byteArray = _bytes.ToArray();
            string byteStr = string.Join("", byteArray.Select(x => x.ToString("x2")));
            int _endindex = -1, _headerIndex = -1;
            _endindex = byteStr.LastIndexOf(end);
            if (_endindex == -1)
            {
                _headerIndex = byteStr.LastIndexOf(start);
                sw.Stop();
                if (_headerIndex == -1)
                {
                    this.Log($"当前解析报文长度{_bytes.Count}，未获取报文头{start}，未获取报文尾{end},准备清空报文接收器并返回本次解析结果null,本次解析总计耗时 {sw.ElapsedMilliseconds}ms\r\n报文:\r\n{byteStr},");
                    _bytes.Clear();
                }
                else
                    this.Log($"当前解析报文长度{_bytes.Count}，未获取报文尾{end},本次返回解析结果null，等待下次解析,本次解析总计耗时 {sw.ElapsedMilliseconds}ms\r\n报文:\r\n{byteStr},");
                return null;
            }

            endIndex = _endindex / 2;
            byteStr = byteStr.Substring(0, _endindex + 24);
            _bytes = _bytes.Skip(endIndex + 12).ToList();
            _headerIndex = byteStr.LastIndexOf(start);
            if (_headerIndex >= _endindex)
            {
                sw.Stop();
                this.Log($"当前剩余解析报文长度{_bytes.Count}，已获取报文尾索引{endIndex},但分析得出本次报文头索引为{_headerIndex},大于等于报文尾索引，本次返回解析结果null，等待下次解析,本次解析总计耗时 {sw.ElapsedMilliseconds}ms\r\n报文:\r\n{byteStr},");
                return null;
            }
            headerIndex = _headerIndex / 2;

            byte[] result = byteArray.Skip(headerIndex)
               .Take(endIndex + 12 - headerIndex)
               .ToArray();

            System.Diagnostics.Stopwatch sw0 = new System.Diagnostics.Stopwatch();
            sw0.Start();
            this.Log($"截取成功（时间戳{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")}），当前剩余报文长度{_bytes.Count}，已获取报文尾索引{endIndex},报文头索引{headerIndex}，本次截取长度为 {result.Length} 字节的报文进行解析,当前耗时 {sw.ElapsedMilliseconds}ms\r\n{string.Join(" ", result.Select(x => x.ToString("x2")))} --- {this.Name}");
            sw0.Stop();
            var ts = sw0.ElapsedMilliseconds;
            if (ts > tsMax)
                tsMax = ts;
            this.Log($"本次写文件耗时{ts}ms，本次启动最大耗时{tsMax} --- {this.Name}");


            System.Diagnostics.Stopwatch sw1 = new System.Diagnostics.Stopwatch();
            sw1.Start();
            var netPacket = new ConveyorNetPacket(result);
            sw1.Stop();

            sw.Stop();
            this.Log($"解析完成（时间戳{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")}）,报文翻译耗时 {sw1.ElapsedMilliseconds}ms,本次解析总计耗时 {sw.ElapsedMilliseconds}ms --- {this.Name}");

            return netPacket;
        }
        /// <summary>
        /// 尝试从当前已接收的数据区获取一个完整的数据包
        /// </summary>
        /// <returns></returns>
        ConveyorNetPacket FetchPackage2(byte[] byteArray)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            int headerIndex = -1;
            int endIndex = -1;
            string byteStr = string.Join("", byteArray.Select(x => x.ToString("x2")));
            int _endindex = -1, _headerIndex = -1;
            _endindex = byteStr.LastIndexOf(end);
            if (_endindex == -1)
            {
                _headerIndex = byteStr.LastIndexOf(start);
                sw.Stop();
                if (_headerIndex == -1)
                {
                    this.Log($"当前解析报文长度{byteArray.Length}，未获取报文头{start}，未获取报文尾{end},准备清空报文接收器并返回本次解析结果null,本次解析总计耗时 {sw.ElapsedMilliseconds}ms\r\n报文:\r\n{byteStr},");
                }
                else
                    this.Log($"当前解析报文长度{byteArray.Length}，未获取报文尾{end},本次返回解析结果null，等待下次解析,本次解析总计耗时 {sw.ElapsedMilliseconds}ms\r\n报文:\r\n{byteStr},");
                return null;
            }

            endIndex = _endindex / 2;
            byteStr = byteStr.Substring(0, _endindex + 24);
            _headerIndex = byteStr.LastIndexOf(start);
            if (_headerIndex >= _endindex)
            {
                sw.Stop();
                this.Log($"已获取报文尾索引{endIndex},但分析得出本次报文头索引为{_headerIndex},大于等于报文尾索引，本次返回解析结果null，等待下次解析,本次解析总计耗时 {sw.ElapsedMilliseconds}ms\r\n报文:\r\n{byteStr},");
                return null;
            }
            headerIndex = _headerIndex / 2;

            byte[] result = byteArray.Skip(headerIndex)
               .Take(endIndex + 12 - headerIndex)
               .ToArray();

            //System.Diagnostics.Stopwatch sw0 = new System.Diagnostics.Stopwatch();
            //sw0.Start();
            //this.Log($"截取成功（时间戳{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")}），已获取报文尾索引{endIndex},报文头索引{headerIndex}，本次截取长度为 {result.Length} 字节的报文进行解析,当前耗时 {sw.ElapsedMilliseconds}ms\r\n{string.Join(" ", result.Select(x => x.ToString("x2")))} --- {this.Name}");
            //sw0.Stop();
            //var ts = sw0.ElapsedMilliseconds;
            //if (ts > tsMax)
            //    tsMax = ts;
            //this.Log($"本次写文件耗时{ts}ms，本次启动最大耗时{tsMax} --- {this.Name}");

            System.Diagnostics.Stopwatch sw1 = new System.Diagnostics.Stopwatch();
            sw1.Start();
            var netPacket = new ConveyorNetPacket(result);
            sw1.Stop();

            sw.Stop();
            this.Log($"本次截取长度为 {result.Length} 字节，总计耗时 {sw.ElapsedMilliseconds}ms");

            return netPacket;
        }


        Device device = null;
        public Device Device
        {
            get
            {
                if (device == null)
                    device = DeviceConverter.ToDevice<Device>(DeviceName);
                return device;
            }
        }
        public void Log(string msg)
        {
            Device.Log(msg);
        }

        #endregion
    }
}
