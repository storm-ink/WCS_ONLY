using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Collections;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Robot
{
    public class DefaultTcpRobotDataReceiver : IDataReceiver
    {
        List<byte> _bytes = new List<byte>();
        public DefaultTcpRobotDataReceiver(String name)
        {
            this.Name = name;
        }

        #region Implement ITcpProtocolDataReceiver

        public String Name { get; private set; }

        public string DeviceName { get; set; }

        public Device Device => throw new NotImplementedException();

        public void AddBytes(byte[] bytes)
        {
            RobotStatusTelexTransferObject telexTransferObject = null;
            _bytes.AddRange(bytes);

            telexTransferObject = FetchTelexTranferObject();
            if (telexTransferObject != null && DataReceived != null)
            {
                DataReceived(this, new DataReceiverReceivedEventArgs(null, telexTransferObject));
            }
            Clear();
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

        /// <summary>
        /// 尝试从当前已接收的数据区获取一个完整的报文对象
        /// </summary>
        /// <returns></returns>
        RobotStatusTelexTransferObject FetchTelexTranferObject()
        {
            if (_bytes.Count < 2)
            {
                return null;
            }

            int headerIndex = -1;
            int endIndex = -1;
            byte[] byteArray = _bytes.ToArray();

            //
            if (byteArray.Length == 47)
            {
                //byte[] result = byteArray.Skip(0)
                //  .Take(byteArray.Length-1)
                //  .ToArray();
                return new RobotStatusTelexTransferObject(byteArray);
            }
           
            //for (int i = 0; i < byteArray.Length;)
            //{
            //    if (headerIndex == -1)
            //    {
            //        while (i < byteArray.Length - 1 && byteArray[i] != 255)
            //        {
            //            i++;
            //        }

            //        if (byteArray.Skip(i).Count() < 8)
            //        {
            //            return null;
            //        }

            //        //8个字节如果都是f，那说明是开头(对应64位uint)
            //        if (BitConverter.ToUInt64(byteArray, i).ToString("x") == "".PadLeft(16, 'f'))
            //        {
            //            headerIndex = i;
            //            i += 8;
            //        }
            //        else
            //        {
            //            i++;
            //        }
            //    }
            //    else
            //    {
            //        while (i < byteArray.Length - 1 && byteArray[i] != 238)
            //        {
            //            i++;
            //        }

            //        if (byteArray.Skip(i).Count() < 8)
            //        {
            //            return null;
            //        }

            //        //8个字节如果都是e，那说明是开头(对应64位uint)
            //        if (BitConverter.ToUInt64(byteArray, i).ToString("x") == "".PadLeft(16, 'e'))
            //        {
            //            endIndex = i;
            //            i += 8;
            //            while (i < byteArray.Length && byteArray[i] == 238)
            //            {
            //                endIndex++;
            //                i++;
            //            }
            //        }
            //        else
            //        {
            //            i++;
            //        }
            //    }

            //    if (headerIndex != -1 && endIndex != -1)
            //    {
            //        int contentStartIndex = headerIndex + 8;
            //        int contentEndIndex = endIndex;
            //        byte[] result = byteArray.Skip(headerIndex)
            //           .Take(endIndex + 8 - headerIndex)
            //           .ToArray();

            //        _bytes = byteArray.Skip(endIndex + 8).ToList();


            //        return new RobotStatusTelexTransferObject(result);
            //    }
            //}

            return null;
        }


        public TNetTransferObject ConvertToNetTransferObject<TNetTransferObject>(NetPacket netPacket) where TNetTransferObject : NetTransferObject
        {
            throw new NotImplementedException();
        }

        public NetPacket ConvertToNetPacket<TNetTransferObject>(TNetTransferObject netTransferObject) where TNetTransferObject : NetTransferObject
        {
            if (netTransferObject is RobotTaskCommand)
            {
                RobotTaskCommand telexTransferObject = netTransferObject as RobotTaskCommand;
                return new RobotDeviceNetPacket(telexTransferObject.ToTelex());
            }

            throw new InvalidOperationException("不支持的命令类型");
        }

        public void Log(string msg)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
