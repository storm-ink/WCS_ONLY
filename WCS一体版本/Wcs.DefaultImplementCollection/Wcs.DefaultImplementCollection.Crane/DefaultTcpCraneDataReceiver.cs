using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using Newtonsoft.Json;
using NLog;
using Wcs.Framework;
namespace Wcs.DefaultImplementCollection.Crane
{
    public class DefaultTcpCraneDataReceiver : IDataReceiver
    {
        static Thread _thread;
        List<byte> _bytes = new List<byte>();
        Logger _logger { get; set; }
        public string Name { get; private set; }

        public string DeviceName { get; set; }

        string craneAlarmVersion;
        /// <summary>
        /// 堆垛机报警版本
        /// </summary>
        public string CraneAlarmVersion
        {
            get
            {
                if (string.IsNullOrWhiteSpace(craneAlarmVersion))
                {
                    var _version = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("CraneAlarmVersion", "ByteVersion");
                    if (string.IsNullOrWhiteSpace(_version))
                        throw new ArgumentException("未配置堆垛机报警协议版本(配置关键字：CraneAlarmVersion，可配置值ByteVersion|BitVersion|ShortVersion)");
                    craneAlarmVersion = _version;
                }
                return craneAlarmVersion;
            }
        }
        string craneAlarmBitVesionPath;
        public string CraneAlarmBitVesionPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(craneAlarmBitVesionPath))
                {
                    var defaultPath = @".\系统配置\堆垛机\GeneralAlarmMappings.xml";
                    craneAlarmBitVesionPath = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("CraneAlarmBitVesionPath", defaultPath);
                }
                return craneAlarmBitVesionPath;
            }
        }

        public TNetTransferObject ConvertToNetTransferObject<TNetTransferObject>(NetPacket netPacket) where TNetTransferObject : NetTransferObject
        {
            throw new NotImplementedException();
        }

        public NetPacket ConvertToNetPacket<TNetTransferObject>(TNetTransferObject netTransferObject) where TNetTransferObject : NetTransferObject
        {
            if (!(netTransferObject is TelexTransferObject))
            {
                throw new InvalidOperationException(string.Format("只支持 {0} 类型的转换", typeof(TelexTransferObject)));
            }

            TelexTransferObject telexTransferObject = netTransferObject as TelexTransferObject;
            return new CraneNetPacket(telexTransferObject.GetBytes());
        }

        public void AddBytes(byte[] bytes)
        {
            TelexTransferObject telexTransferObject = null;

            _bytes.AddRange(bytes);
            //telexTransferObject = FetchTelexTransferObject();
            telexTransferObject = FetchTelexTransferLastObject();

            if (telexTransferObject != null && DataReceived != null)
            {
                DataReceived(this, new DataReceiverReceivedEventArgs(null, telexTransferObject));

                this.Log(JsonConvert.SerializeObject(telexTransferObject));
            }
        }

        public event EventHandler<DataReceiverReceivedEventArgs> DataReceived;

        Type[] TelexTypes { get; set; }
        public DefaultTcpCraneDataReceiver(string name, Type[] telexTypes)
        {
            this.Name = name;
            this._logger = LogManager.GetCurrentClassLogger();

            if (telexTypes == null || telexTypes.Length == 0)
                throw new ArgumentNullException("telextTypes", "至少需要指定一个报文类型");

            foreach (var type in telexTypes)
            {
                if (!type.IsSubclassOf(typeof(TelexTransferObject)))
                {
                    throw new ArgumentOutOfRangeException("telextTypes", string.Format("{0} 不是 Crane TelexTransferObject 的子类", type));
                }
            }
            this.TelexTypes = telexTypes;

            _thread = new Thread(telexHandThreadProc);
            _thread.IsBackground = true;
            _thread.Name = $"{name} 报文处理辅助线程";
            _thread.StartAndManaged();
        }

        #region telexHandThreadProc
        List<TransferBytes> TransferBytesList = new List<TransferBytes>();
        static object lockObj = new object();
        private void Push(TransferBytes transferBytes)
        {
            lock (lockObj)
                TransferBytesList.Add(transferBytes);
        }

        private TransferBytes Pop()
        {
            lock (lockObj)
            {
                if (TransferBytesList.Count() == 0)
                    return null;

                var transferBytes = TransferBytesList.First();
                TransferBytesList.RemoveAt(0);
                return transferBytes;
            }
        }
        System.Diagnostics.Stopwatch sw;
        private void telexHandThreadProc()
        {
            while (true)
            {
                TransferBytes transferBytes = null;
                try
                {
                    Thread.Sleep(1000);
                    transferBytes = Pop();
                    if (transferBytes == null)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                    FetchTelexTransferObject(transferBytes);
                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, this, transferBytes == null ? "报文=NULL" : string.Join(" ", transferBytes.Bytes.Select(x => x.ToString("x2"))), "", null);
                }
            }
        }
        void FetchTelexTransferObject(TransferBytes transferBytes)
        {
            var telexTransferObject = ParseTelexTransferObject(transferBytes.Bytes);
            if (telexTransferObject != null && DataReceived != null)
            {
                try
                {
                    DataReceived(this, new DataReceiverReceivedEventArgs(null, telexTransferObject));
                }
                catch (Exception ex)
                {
                    //_logger.Warn1($"报文 {string.Join(" ", transferBytes.Bytes.Select(x => Convert.ToString("x2")))} 处理失败", this);
                    _logger.Error1(ex, this);
                }
            }
        }
        #endregion

        /// <summary>
        /// 状态报文常用报文头
        /// </summary>
        const string splitCharacter_base = "ffffffffffffffffffffffff";
        /// <summary>
        /// 尝试从当前已接收的数据区获取一个完整的报文对象
        /// 获取最新一条报文
        /// 如果最新一条报文不是完整的则获取倒数第二条报文
        /// 如果倒数第二条报文也不是完整的则移除最后一条非完整报文之前的报文
        /// </summary>
        /// <returns></returns>
        TelexTransferObject FetchTelexTransferLastObject()
        {
            byte[] lastZT = null;
            if (_bytes.Count < 8)
                return null;

            byte[] byteArray = _bytes.ToArray();
            string byteStr = string.Join("", byteArray.Select(x => x.ToString("x2")));
            this.Log($"数据包-->十六进制报文\r\n{byteStr}");
            if (byteStr.LastIndexOf(splitCharacter_base) == -1)
            {
                _bytes.Clear();
                return null;
            }

            while (_bytes.Count() > 0)
            {
                byteArray = _bytes.ToArray();
                byteStr = string.Join("", byteArray.Select(x => x.ToString("x2")));
                var index = byteStr.IndexOf(splitCharacter_base);
                if (index == -1)
                {
                    _bytes.Clear();
                    break;
                }
                //转成16进制字符串时_bytes长度会翻一倍
                index = index / 2;
                _bytes = _bytes.Skip(index).ToList();
                if (12 + 4 > _bytes.Count())
                    break;

                int internalLength = BitConverter.ToInt32(_bytes.Skip(12).Take(4).Reverse().ToArray(), 0);
                if (_bytes.Count() >= 12 + 4 + internalLength)
                {
                    var transferBytes = _bytes.Take(12 + 4 + internalLength).ToArray();
                    lastZT = transferBytes;
                    _bytes = _bytes.Skip(12 + 4 + internalLength).ToList();
                }
                else
                    break;
            }

            if (lastZT == null)
                return null;

            return ParseTelexTransferObject(lastZT);
        }

        TelexTransferObject ParseTelexTransferObject(byte[] bytes)
        {
            try
            {
                byte[] StartOf = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
                if (!bytes.Take(12).SequenceEqual(StartOf))
                    throw new InvalidNetPacketException(bytes);

                byte[] Data = bytes.Skip(StartOf.Length + 4).ToArray();
                CraneReportInfo craneReportInfo = new CraneReportInfo();
                craneReportInfo.DeviceNo = (UInt16)convertBytesToNumberValue(craneReportInfo.DeviceNo, Data.Skip(0).Take(2).ToArray());
                craneReportInfo.CraneWorkModel = (CraneWorkModels)(Int16)convertBytesToNumberValue(craneReportInfo.CraneWorkModel, Data.Skip(2).Take(2).ToArray());
                craneReportInfo.EquipmentTaskId = (UInt32)convertBytesToNumberValue(craneReportInfo.EquipmentTaskId, Data.Skip(4).Take(4).ToArray());
                craneReportInfo.TaskState = (CraneTaskStatus)(Int16)convertBytesToNumberValue(craneReportInfo.TaskState, Data.Skip(8).Take(2).ToArray());
                craneReportInfo.DeviceState = (CraneStatus)(Int16)convertBytesToNumberValue(craneReportInfo.DeviceState, Data.Skip(10).Take(2).ToArray());
                craneReportInfo.IsLoaded = (UInt16)convertBytesToNumberValue(craneReportInfo.IsLoaded, Data.Skip(12).Take(2).ToArray());
                craneReportInfo.XRunningDirection = (UInt16)convertBytesToNumberValue(craneReportInfo.XRunningDirection, Data.Skip(14).Take(2).ToArray());
                craneReportInfo.XSpeed = (UInt32)convertBytesToNumberValue(craneReportInfo.XSpeed, Data.Skip(16).Take(4).ToArray());
                craneReportInfo.XStopped = (UInt16)convertBytesToNumberValue(craneReportInfo.XStopped, Data.Skip(20).Take(2).ToArray());
                craneReportInfo.XColumn = (UInt16)convertBytesToNumberValue(craneReportInfo.XColumn, Data.Skip(22).Take(2).ToArray());
                craneReportInfo.XTargetColumn = (UInt16)convertBytesToNumberValue(craneReportInfo.XTargetColumn, Data.Skip(24).Take(2).ToArray());
                craneReportInfo.XPosition = (UInt32)convertBytesToNumberValue(craneReportInfo.XPosition, Data.Skip(26).Take(4).ToArray());
                craneReportInfo.XTargetPosition = (UInt32)convertBytesToNumberValue(craneReportInfo.XTargetPosition, Data.Skip(30).Take(4).ToArray());
                craneReportInfo.YRunningDirection = (UInt16)convertBytesToNumberValue(craneReportInfo.YRunningDirection, Data.Skip(34).Take(2).ToArray());
                craneReportInfo.YSpeed = (UInt32)convertBytesToNumberValue(craneReportInfo.YSpeed, Data.Skip(36).Take(4).ToArray());
                craneReportInfo.YStopped = (UInt16)convertBytesToNumberValue(craneReportInfo.YStopped, Data.Skip(40).Take(2).ToArray());
                craneReportInfo.YLevel = (UInt16)convertBytesToNumberValue(craneReportInfo.YLevel, Data.Skip(42).Take(2).ToArray());
                craneReportInfo.YLevelPosition = (UInt16)convertBytesToNumberValue(craneReportInfo.YLevelPosition, Data.Skip(44).Take(2).ToArray());
                craneReportInfo.YTargetLevel = (UInt16)convertBytesToNumberValue(craneReportInfo.YTargetLevel, Data.Skip(46).Take(2).ToArray());
                craneReportInfo.YPosition = (UInt32)convertBytesToNumberValue(craneReportInfo.YPosition, Data.Skip(48).Take(4).ToArray());
                craneReportInfo.YTargetPosition = (UInt32)convertBytesToNumberValue(craneReportInfo.YTargetPosition, Data.Skip(52).Take(4).ToArray());
                craneReportInfo.ZRunningDirection = (UInt16)convertBytesToNumberValue(craneReportInfo.ZRunningDirection, Data.Skip(56).Take(2).ToArray());
                craneReportInfo.ZSpeed = (UInt32)convertBytesToNumberValue(craneReportInfo.ZSpeed, Data.Skip(58).Take(4).ToArray());
                craneReportInfo.ZRow = (UInt16)convertBytesToNumberValue(craneReportInfo.ZRow, Data.Skip(62).Take(2).ToArray());
                craneReportInfo.ZTargetRow = (UInt16)convertBytesToNumberValue(craneReportInfo.ZTargetRow, Data.Skip(64).Take(2).ToArray());
                craneReportInfo.ZDefaultPosition = (Int32)convertBytesToNumberValue(craneReportInfo.ZDefaultPosition, Data.Skip(66).Take(4).ToArray());
                craneReportInfo.ZPosition = (Int32)convertBytesToNumberValue(craneReportInfo.ZPosition, Data.Skip(70).Take(4).ToArray());
                craneReportInfo.ZTargetPosition = (Int32)convertBytesToNumberValue(craneReportInfo.ZTargetPosition, Data.Skip(74).Take(4).ToArray());
                craneReportInfo.CheckBarcode = (string)convertBytesToNumberValue(craneReportInfo.CheckBarcode, Data.Skip(78).Take(20).ToArray());
                craneReportInfo.Alarms = (Byte[])convertBytesToNumberValue(craneReportInfo.Alarms, Data.Skip(98).Take(40).ToArray());
                craneReportInfo.Cmd = (CmdTypes)(Int16)convertBytesToNumberValue(craneReportInfo.Cmd, Data.Skip(138).Take(2).ToArray());
                craneReportInfo.TaskType = (CraneTaskTypes)(Int16)convertBytesToNumberValue(craneReportInfo.TaskType, Data.Skip(140).Take(2).ToArray());
                craneReportInfo.WcsEquipmentTaskId = (UInt32)convertBytesToNumberValue(craneReportInfo.WcsEquipmentTaskId, Data.Skip(142).Take(4).ToArray());
                craneReportInfo.Pick_ConvNo = (UInt16)convertBytesToNumberValue(craneReportInfo.Pick_ConvNo, Data.Skip(146).Take(2).ToArray());
                craneReportInfo.Put_ConvNo = (UInt16)convertBytesToNumberValue(craneReportInfo.Put_ConvNo, Data.Skip(148).Take(2).ToArray());
                craneReportInfo.Fork_Pick_Row = (UInt16)convertBytesToNumberValue(craneReportInfo.Fork_Pick_Row, Data.Skip(150).Take(2).ToArray());
                craneReportInfo.Fork_Pick_Column = (UInt16)convertBytesToNumberValue(craneReportInfo.Fork_Pick_Column, Data.Skip(152).Take(2).ToArray());
                craneReportInfo.Fork_Pick_Level = (UInt16)convertBytesToNumberValue(craneReportInfo.Fork_Pick_Level, Data.Skip(154).Take(2).ToArray());
                craneReportInfo.Fork_Put_Row = (UInt16)convertBytesToNumberValue(craneReportInfo.Fork_Put_Row, Data.Skip(156).Take(2).ToArray());
                craneReportInfo.Fork_Put_Column = (UInt16)convertBytesToNumberValue(craneReportInfo.Fork_Put_Column, Data.Skip(158).Take(2).ToArray());
                craneReportInfo.Fork_Put_Level = (UInt16)convertBytesToNumberValue(craneReportInfo.Fork_Put_Level, Data.Skip(160).Take(2).ToArray());
                craneReportInfo.WcsTaskId = (UInt32)convertBytesToNumberValue(craneReportInfo.WcsTaskId, Data.Skip(162).Take(4).ToArray());
                craneReportInfo.WcsBarcode = (string)convertBytesToNumberValue(craneReportInfo.WcsBarcode, Data.Skip(166).Take(20).ToArray());
                craneReportInfo.IsNeedCheckBarcode = (UInt16)convertBytesToNumberValue(craneReportInfo.IsNeedCheckBarcode, Data.Skip(186).Take(2).ToArray());
                craneReportInfo.DataId = (UInt16)convertBytesToNumberValue(craneReportInfo.DataId, Data.Skip(188).Take(2).ToArray());

                craneReportInfo.ErrorCodeList = GeneralMappingHelper.GetErrorCodeList(craneReportInfo.Alarms.ToList(), this.DeviceName, CraneAlarmVersion, GeneralMappings, CraneAlarmBitVesionPath);
                return craneReportInfo;
            }
            catch (Exception ex)
            {
                this._logger.Error1(ex, this, $"处理报文 {string.Join(" ", bytes.Select(x => x.ToString("x2")))} 时发生异常", "", null);
                throw ex;
            }
        }

        GeneralMappings GeneralMappings = null;

        private object convertBytesToNumberValue(object obj, byte[] bytes)
        {
            var valueType = obj.GetType().Name;
            try
            {
                var basicType = obj.GetType().BaseType.Name;
                if (basicType == "Enum")
                    return BitConverter.ToInt16(bytes.Reverse().ToArray(), 0);

                if (valueType == "Int16")
                {
                    return BitConverter.ToInt16(bytes.Reverse().ToArray(), 0);
                }
                if (valueType == "Int32")
                {
                    return BitConverter.ToInt32(bytes.Reverse().ToArray(), 0);
                }

                if (valueType == "Int64")
                {
                    return BitConverter.ToInt64(bytes.Reverse().ToArray(), 0);
                }

                if (valueType == "UInt16")
                {
                    return BitConverter.ToUInt16(bytes.Reverse().ToArray(), 0);
                }

                if (valueType == "UInt32")
                {
                    return BitConverter.ToUInt32(bytes.Reverse().ToArray(), 0);
                }

                if (valueType == "UInt64")
                {
                    return BitConverter.ToUInt64(bytes.Reverse().ToArray(), 0);
                }

                if (valueType == "Single")
                {
                    return BitConverter.ToSingle(bytes.Reverse().ToArray(), 0);
                }

                if (valueType == "Byte")
                {
                    return Convert.ToByte(bytes[0]);
                }
                if (valueType == "Byte[]")
                {
                    return bytes;
                }
                if (valueType == "String")
                {
                    return System.Text.Encoding.Default.GetString(bytes);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

            throw new NotSupportedException(String.Format("不支持将 byte[] 转换为 {0}", valueType));
        }

        public void Clear()
        {
            _bytes.Clear();
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

    public class TransferBytes
    {
        /// <summary>
        /// 待处理报文
        /// </summary>
        public byte[] Bytes { get; set; }

        /// <summary>
        /// 是否需要处理状态报文
        /// </summary>
        public bool ZtFlay { get; set; }
    }
    public static class TypeExtend
    {
        public static Type GetBasicType(this string typeStr)
        {
            return System.Type.GetType(string.Concat("System." + typeStr));
        }
    }
}
