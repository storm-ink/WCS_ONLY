using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Scanner
{
    public class ScanerDeviceDataReceiver : IDataReceiver 
    {
        List<byte> _bytes = new List<byte>();
        public Logger _logger { get; set; }

        public String Name { get; private set; }

        public string DeviceName { get; set; }
        public ScanerDeviceDataReceiver(String name)
        {
            this.Name = name;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        public TNetTransferObject ConvertToNetTransferObject<TNetTransferObject>(NetPacket netPacket) where TNetTransferObject : NetTransferObject
        {
            throw new NotImplementedException();
        }

        public NetPacket ConvertToNetPacket<TNetTransferObject>(TNetTransferObject netTransferObject) where TNetTransferObject : NetTransferObject
        {
            if (!(netTransferObject is ScanerDeviceTelexTransferObject))
            {
                throw new InvalidOperationException(String.Format("只支持 {0} 类型的转换", typeof(ScanerDeviceTelexTransferObject)));
            }

            ScanerDeviceTelexTransferObject telexTransferObject = netTransferObject as ScanerDeviceTelexTransferObject;
            return new ScanerDeviceNetPacket(telexTransferObject.GetBytes());
        }

        public void AddBytes(byte[] bytes)
        {
            ScanerDeviceTelexTransferObject telexTransferObject = null;
            _bytes.AddRange(bytes);

            telexTransferObject = FetchTelexTranferObject();
            if (telexTransferObject != null && DataReceived != null)
            {
                DataReceived(this, new DataReceiverReceivedEventArgs(null, telexTransferObject));
            }
        }

        public event EventHandler<DataReceiverReceivedEventArgs> DataReceived;

        public void Clear()
        {
            _bytes.Clear();
        }

        /// <summary>
        /// 尝试从当前已接收的数据区获取一个完整的报文对象
        /// </summary>
        /// <returns></returns>
        ScanerDeviceTelexTransferObject FetchTelexTranferObject()
        {
            try
            {
                if (_bytes.Count < 2)
                {
                    return null;
                }

                //_bytes = Encoding.ASCII.GetBytes("\u0002" + "ABCDE345" + "\u0003").ToList();
                int endIndex = _bytes.FindLastIndex(x => x == ScanerDeviceTelexTransferObject.Suffix);
                if (endIndex < 0)
                {
                    return null;
                }
                else
                {
                    int headerIndex = _bytes.FindLastIndex(x => x == ScanerDeviceTelexTransferObject.Prefix);
                    if (headerIndex < 0)
                    {
                        _bytes = _bytes.Skip(endIndex + 1).ToList();
                        return null;
                    }
                    else
                    {
                        byte[] telexBytes = _bytes.Skip(headerIndex).Take(endIndex + 1 - headerIndex).ToArray();
                        _bytes = _bytes.Skip(endIndex + 1).ToList();
                        return ParseTelexTransferObject(telexBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;


            }
            finally { _bytes.Clear(); }


           
        }

        ScanerDeviceTelexTransferObject ParseTelexTransferObject(byte[] bytes)
        {
            return new ScanerDeviceTelexTransferObject(bytes);
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
    }
}
