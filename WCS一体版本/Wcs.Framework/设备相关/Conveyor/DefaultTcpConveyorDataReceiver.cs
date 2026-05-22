using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Collections;

namespace Wcs.Framework.Devices.Conveyor
{
    public class DefaultConveyorTcpProtocolDataReceiver : ITcpProtocolDataReceiver
    {
        _DefaultConveyorTcpProtocolDataReceiverEncoding db1Encoding;
        _DefaultConveyorTcpProtocolDataReceiverEncoding db2Encoding;
        List<byte> _bytes = new List<byte>();
        public DefaultConveyorTcpProtocolDataReceiver(XmlNode db1EncodingNode, XmlNode db2EncodingNode)
        {
            db1Encoding = new _DefaultConveyorTcpProtocolDataReceiverEncoding(db1EncodingNode);
            db2Encoding = new _DefaultConveyorTcpProtocolDataReceiverEncoding(db2EncodingNode);
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
                var fullMap = this.db2Encoding.CreateFullMap();
                fullMap[netTransferObject.GetType()][0] = netTransferObject;
                _DB1 db1 = new _DB1();
                db1.Items = fullMap;
                return ConvertToNetPacket(db1);
            }
        }

        public void AddBytes(byte[] bytes)
        {
            ConveyorNetPacket netPacket=null; 
            lock (_bytes)
            {
                _bytes.AddRange(bytes);
                if (_bytes.Count >= db1Encoding.TotalBytes)
                {
                    netPacket = FetchPackage();
                }
            }

            if (netPacket != null && DataReceived!=null)
            {
                var items = db1Encoding.DecodeAll(netPacket.Data);
                _DB1 db1 = new _DB1();
                db1.Items = items;

                DataReceived(this, new TcpProtocolDataReceiverReceivedEventArgs(netPacket, db1));
            }
        }

        public void Clear()
        {
            lock (_bytes)
            {
                _bytes.Clear();
            }
        }

        public event EventHandler<TcpProtocolDataReceiverReceivedEventArgs> DataReceived;
        #endregion

        #region Private Methos
        _DB1 ConvertToDB1(NetPacket netPacket)
        {
            var items = this.db1Encoding.DecodeAll(netPacket.Data);
            var db1 = new _DB1();
            db1.Items = items;
            return db1;
        }

        _DB2 ConvertToDB2(NetPacket netPacket)
        {
            var items = this.db2Encoding.DecodeAll(netPacket.Data);
            var db2 = new _DB2();
            db2.Items = items;
            return db2;
        }

        NetPacket ConvertToNetPacket(_DB1 db1)
        {
            var data = this.db1Encoding.Encode(db1.Items);
            ConveyorNetPacket packet = new ConveyorNetPacket();
            packet.CreateNetPackage(data);

            return packet;
        }

        NetPacket ConvertToNetPacket(_DB2 db2)
        {
            var data = this.db1Encoding.Encode(db2.Items);
            ConveyorNetPacket packet = new ConveyorNetPacket();
            packet.CreateNetPackage(data);

            return packet;
        }

        /// <summary>
        /// 尝试从当前已接收的数据区获取一个完整的数据包
        /// </summary>
        /// <returns></returns>
        ConveyorNetPacket FetchPackage()
        {
            if (_bytes.Count < 8)
            {
                return null;
            }

            int headerIndex = -1;
            int endIndex = -1;
            byte[] byteArray = _bytes.ToArray();
            for (int i = 0; i < byteArray.Length; )
            {
                if (headerIndex == -1)
                {
                    while (i < byteArray.Length - 1 && byteArray[i] != 255)
                    {
                        i++;
                    }

                    if (byteArray.Skip(i).Count() < 8)
                    {
                        return null;
                    }

                    //8个字节如果都是f，那说明是开头(对应64位uint)
                    if (BitConverter.ToUInt64(byteArray, i).ToString("x") == "".PadLeft(16, 'f'))
                    {
                        headerIndex = i;
                        i += 8;
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

                    if (byteArray.Skip(i).Count() < 8)
                    {
                        return null;
                    }

                    //8个字节如果都是e，那说明是开头(对应64位uint)
                    if (BitConverter.ToUInt64(byteArray, i).ToString("x") == "".PadLeft(16, 'e'))
                    {
                        endIndex = i;
                        i += 8;
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
                    int contentStartIndex = headerIndex + 8;
                    int contentEndIndex = endIndex;
                    byte[] result = byteArray.Skip(headerIndex)
                       .Take(endIndex + 8 - headerIndex)
                       .ToArray();

                    _bytes = byteArray.Skip(endIndex + 8).ToList();


                    return new ConveyorNetPacket(result);
                }
            }

            return null;
        }

        #endregion
    }
}
