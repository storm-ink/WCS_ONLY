using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Wcs.Framework;
using Wcs;
using Newtonsoft.Json;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    public class DefaultTcpRailGuidedVehicleDataReceiver : IDataReceiver
    {
        List<byte> _bytes = new List<byte>();
        Logger _logger { get; set; }
        public string Name { get; private set; }

        public string DeviceName { get; set; }

        public TNetTransferObject ConvertToNetTransferObject<TNetTransferObject>(NetPacket netPacket) where TNetTransferObject : NetTransferObject
        {
            throw new NotImplementedException();
        }

        public NetPacket ConvertToNetPacket<TNetTransferObject>(TNetTransferObject netTransferObject) where TNetTransferObject : NetTransferObject
        {
            if (!(netTransferObject is RailGuidedVehicleTelexTransferObject))
            {
                throw new InvalidOperationException(string.Format("只支持 {0} 类型的转换", typeof(RailGuidedVehicleTelexTransferObject)));
            }

            RailGuidedVehicleTelexTransferObject telexTransferObject = netTransferObject as RailGuidedVehicleTelexTransferObject;
            return new RailGuidedVehicleNetPacket(telexTransferObject.GetBytes());
        }

        public void AddBytes(byte[] bytes)
        {
            RailGuidedVehicleTelexTransferObject telexTransferObject = null;

            _bytes.AddRange(bytes);
            telexTransferObject = FetchTelexTransferObject();

            if (telexTransferObject != null && DataReceived != null)
            {
                DataReceived(this, new DataReceiverReceivedEventArgs(null, telexTransferObject));
                this.Log(JsonConvert.SerializeObject(telexTransferObject));
            }
        }

        public void Clear()
        {
            _bytes.Clear();
        }

        public event EventHandler<DataReceiverReceivedEventArgs> DataReceived;
        Type[] TelextTypes;
        public DefaultTcpRailGuidedVehicleDataReceiver(string name, Type[] telextTypes)
        {
            this.Name = name;
            this.TelextTypes = telextTypes;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// 尝试从当前已接收的数据区获取一个完整的报文对象
        /// </summary>
        /// <returns></returns>
        RailGuidedVehicleTelexTransferObject FetchTelexTransferObject()
        {
            if (_bytes.Count < 2)
            {
                return null;
            }

            int endIndex = _bytes.FindLastIndex(x => x == RailGuidedVehicleTelexTransferObject.Suffix);

            if (endIndex < 0)
            {
                return null;
            }
            else
            {
                int headerIndex = _bytes.FindLastIndex(x => x == RailGuidedVehicleTelexTransferObject.Prefix);
                if (headerIndex < 0)
                {
                    _bytes = _bytes.Skip(endIndex + 1).ToList();
                    return null;
                }
                else
                {
                    if (headerIndex > endIndex)
                    {
                        var __bytes = _bytes.Take(endIndex + 1).ToList();
                        headerIndex = _bytes.FindLastIndex(x => x == RailGuidedVehicleTelexTransferObject.Prefix);
                        if (headerIndex < 0)
                        {
                            _bytes = _bytes.Skip(endIndex + 1).ToList();
                            return null;
                        }
                        else if (headerIndex > endIndex)
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
                    else
                    {
                        byte[] telexBytes = _bytes.Skip(headerIndex).Take(endIndex + 1 - headerIndex).ToArray();
                        _bytes = _bytes.Skip(endIndex + 1).ToList();
                        return ParseTelexTransferObject(telexBytes);
                    }
                }
            }
        }

        RailGuidedVehicleTelexTransferObject ParseTelexTransferObject(byte[] bytes)
        {
            String telex = Encoding.ASCII.GetString(bytes);
            //Type[] types = new Type[] { typeof(StateTelexTransferObject)};
            foreach (var type in TelextTypes)
            {
                var instance = ReflectionHelper.CreateInstance<RailGuidedVehicleTelexTransferObject>(type);
                if (instance.ValidateType(telex))
                {
                    instance = ReflectionHelper.CreateInstance<RailGuidedVehicleTelexTransferObject>(type, this.Device.Name, telex);
                    return instance;
                }
            }

            this._logger.Warn1(String.Format("未能识别的报文内容 {0}", telex), this, _bytes);
            return null;
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
