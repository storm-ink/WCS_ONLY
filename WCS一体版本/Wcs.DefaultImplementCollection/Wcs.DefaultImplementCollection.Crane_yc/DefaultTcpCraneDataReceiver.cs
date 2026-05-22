using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using Newtonsoft.Json;
using NLog;
using Wcs.Framework;
namespace Wcs.DefaultImplementCollection.Crane_yc
{
    public class DefaultTcpCraneDataReceiver : IDataReceiver
    {
        static Thread _thread;
        List<byte> _bytes = new List<byte>();
        Logger _logger { get; set; }
        public string Name { get; private set; }

        public string DeviceName { get; set; }

        string version;
        /// <summary>
        /// 堆垛机协议版本
        /// </summary>
        public string CraneCommandVesion
        {
            get
            {
                if (string.IsNullOrWhiteSpace(version))
                {
                    var _version = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("CraneCommandVersion", "");
                    if (string.IsNullOrWhiteSpace(_version))
                        throw new ArgumentException("未配置堆垛机协议版本(配置关键字：CraneCommandVersion)");
                    version = _version;
                }
                return version;
            }
        }

        private bool plcType;

        public bool PLCType
        {
            get
            {
                plcType = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<bool>("CranePLCReverse", false);

                return plcType;
            }
        }

        string craneAlarmVersion;
        /// <summary>
        /// 堆垛机协议版本
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


        List<string> showCraneForkStation;
        /// <summary>
        /// 显示货叉高低位
        /// </summary>
        public List<string> ShowCraneForkStation
        {
            get
            {
                if (showCraneForkStation == null)
                    showCraneForkStation = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("ShowCraneForkStation", "").Split(',').ToList();
                return showCraneForkStation;
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
                try
                {
                    Thread.Sleep(1000);
                    var transferBytes = Pop();
                    if (transferBytes == null)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                    FetchTelexTransferObject(transferBytes);
                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, this);
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
        const string splitCharacter_base = "ffffffffffffffff";
        const string splitCharacter_zt_v0 = "ffffffffffffffff000000315a54";
        const string splitCharacter_zt_v1 = "ffffffffffffffff0000005a5a54";
        const string splitCharacter_zt_v2 = "ffffffffffffffff000000625a54";
        const string splitCharacter = "ffffffffffffffff";
        /// <summary>
        /// 尝试从当前已接收的数据区获取一个完整的报文对象
        /// 获取最新一条报文
        /// 如果最新一条报文不是完整的则获取倒数第二条报文
        /// 如果倒数第二条报文也不是完整的则移除最后一条非完整报文之前的报文
        /// </summary>
        /// <returns></returns>
        TelexTransferObject FetchTelexTransferLastObject()
        {
            List<byte[]> list = new List<byte[]>();
            byte[] lastZT = null;
            bool ztFlag = false;
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
                if (8 + 4 > _bytes.Count())
                    break;

                int internalLength;
                if (!PLCType)
                    internalLength = BitConverter.ToInt32(_bytes.Skip(8).Take(4).ToArray(), 0);
                else
                    internalLength = BitConverter.ToInt32(_bytes.Skip(8).Take(4).Reverse().ToArray(), 0);

                if (8 + 4 + internalLength <= _bytes.Count())
                {
                    var transferBytes = _bytes.Take(8 + 4 + internalLength).ToArray();
                    byteStr = string.Join("", transferBytes.Select(x => x.ToString("x2")));
                    var flag = System.Text.Encoding.Default.GetString(_bytes.Skip(12).Take(2).ToArray());
                    if (flag.ToUpper() == "ZT")
                        lastZT = transferBytes;
                    else
                        list.Add(transferBytes);
                    _bytes = _bytes.Skip(8 + 4 + internalLength).ToList();
                }
                else
                    break;
            }

            if (list.Count() != 0)
                list.ForEach(x => Push(new TransferBytes() { Bytes = x, ZtFlay = ztFlag }));

            if (lastZT == null)
                return null;

            return ParseTelexTransferObject(lastZT);
        }

        TelexTransferObject ParseTelexTransferObject(byte[] bytes)
        {
            byte[] StartOf = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            if (!bytes.Take(8).SequenceEqual(StartOf))
            {
                throw new InvalidNetPacketException(bytes);
            }
            byte[] Data = bytes.Skip(StartOf.Length + 4).ToArray();
            byte[] Fun = Data.Skip(0).Take(2).ToArray();
            String FunCode = System.Text.Encoding.Default.GetString(Fun);
            if (FunCode != "ZT")
            {
                foreach (var type in TelexTypes)
                {
                    var instance = ReflectionHelper.CreateInstance<TelexTransferObject>(type);
                    if (instance.ValidateType(bytes))
                    {
                        instance = ReflectionHelper.CreateInstance<TelexTransferObject>(type, bytes);
                        return instance;
                    }
                }

                this._logger.Warn1(String.Format("设备 {0} 收到未能识别的报文内容 {1}", this.DeviceName, string.Join("", bytes.Select(x => x.ToString("x2")))), this, _bytes);

                return null;
            }
            else
            {
                #region 对此进行注释。堆垛机协议进行升级更新，
                //if (PLCType == "三菱PLC")
                //{
                //    RequestStateCommandReplyTelexTransferObject requestStateCommandReplyTelexTransferObject = new RequestStateCommandReplyTelexTransferObject();
                //    PLCToCraneStatusNetTransferObject pLCToCrane = new PLCToCraneStatusNetTransferObject();

                //    bool _showCraneForkStation = false;
                //    pLCToCrane.Device_No = (UInt16)convertBytesToNumberValue(pLCToCrane.Device_No.GetType().Name, Data.Skip(2).Take(2).ToArray());
                //    if (ShowCraneForkStation.Contains(pLCToCrane.Device_No.ToString()))
                //        _showCraneForkStation = true;
                //    pLCToCrane.Work_Model = (UInt16)convertBytesToNumberValue(pLCToCrane.Work_Model.GetType().Name, Data.Skip(4).Take(2).ToArray());
                //    //报警标志，
                //    pLCToCrane.IsAlarm = (UInt16)convertBytesToNumberValue(pLCToCrane.IsAlarm.GetType().Name, Data.Skip(6).Take(2).ToArray());
                //    pLCToCrane.IsForkGoods = (UInt16)convertBytesToNumberValue(pLCToCrane.IsForkGoods.GetType().Name, Data.Skip(8).Take(2).ToArray());
                //    pLCToCrane.AtStation = (UInt16)convertBytesToNumberValue(pLCToCrane.IsForkGoods.GetType().Name, Data.Skip(10).Take(2).ToArray());
                //    pLCToCrane.Current_Column = (UInt16)convertBytesToNumberValue(pLCToCrane.Current_Column.GetType().Name, Data.Skip(12).Take(2).ToArray());
                //    pLCToCrane.Current_Level = (UInt16)convertBytesToNumberValue(pLCToCrane.Current_Level.GetType().Name, Data.Skip(14).Take(2).ToArray());
                //    pLCToCrane.Current_Column_Value = (UInt32)convertBytesToNumberValue(pLCToCrane.Current_Column_Value.GetType().Name, Data.Skip(16).Take(4).ToArray());
                //    pLCToCrane.Current_Level_Value = (UInt32)convertBytesToNumberValue(pLCToCrane.Current_Level_Value.GetType().Name, Data.Skip(20).Take(4).ToArray());
                //    pLCToCrane.ForkVerticalPosition = (ForkVerticalPosition)(UInt16)convertBytesToNumberValue("UInt16", Data.Skip(24).Take(2).ToArray());
                //    pLCToCrane.ForkHorizontalPosition = (ForkHorizontalPosition)(UInt16)convertBytesToNumberValue("UInt16", Data.Skip(26).Take(2).ToArray());
                //    pLCToCrane.IsForkWork = (UInt16)convertBytesToNumberValue(pLCToCrane.IsForkWork.GetType().Name, Data.Skip(28).Take(2).ToArray());
                //    //货叉状态
                //    pLCToCrane.Fork_State = (UInt16)convertBytesToNumberValue(pLCToCrane.Fork_State.GetType().Name, Data.Skip(30).Take(2).ToArray());
                //    pLCToCrane.Task_Id = (UInt32)convertBytesToNumberValue(pLCToCrane.Task_Id.GetType().Name, Data.Skip(32).Take(4).ToArray());
                //    pLCToCrane.WcsTaskId = (UInt32)convertBytesToNumberValue(pLCToCrane.WcsTaskId.GetType().Name, Data.Skip(36).Take(4).ToArray());
                //    pLCToCrane.StepType = (UInt16)convertBytesToNumberValue(pLCToCrane.StepType.GetType().Name, Data.Skip(40).Take(2).ToArray());
                //    pLCToCrane.Barcode = System.Text.ASCIIEncoding.ASCII.GetString(Data.Skip(42).Take(20).ToArray());
                //    pLCToCrane.Check_Barcode = System.Text.ASCIIEncoding.ASCII.GetString(Data.Skip(62).Take(20).ToArray());
                //    pLCToCrane.Current_Z1_Value = (Int32)convertBytesToNumberValue(pLCToCrane.Current_Z1_Value.GetType().Name, Data.Skip(82).Take(4).ToArray());
                //    pLCToCrane.Current_Z2_Value = (Int32)convertBytesToNumberValue(pLCToCrane.Current_Z2_Value.GetType().Name, Data.Skip(86).Take(4).ToArray());
                //    pLCToCrane.WalkDirection = (UInt16)convertBytesToNumberValue(pLCToCrane.WalkDirection.GetType().Name, Data.Skip(90).Take(2).ToArray());
                //    pLCToCrane.LoadDirection = (UInt16)convertBytesToNumberValue(pLCToCrane.LoadDirection.GetType().Name, Data.Skip(92).Take(2).ToArray());
                //    pLCToCrane.ForkDirection = (UInt16)convertBytesToNumberValue(pLCToCrane.ForkDirection.GetType().Name, Data.Skip(94).Take(2).ToArray());
                //    pLCToCrane.CurrentLine = (UInt16)convertBytesToNumberValue(pLCToCrane.CurrentLine.GetType().Name, Data.Skip(96).Take(2).ToArray());
                //    pLCToCrane.Alarm_Info = Data.Skip(98).Take(20).ToList();

                //    //给堆垛机状态类赋值  ?新协议中堆垛机报警码以字节数组来表示，老协议中是一个字段显示。
                //    //暂未给报警码赋值处理。
                //    requestStateCommandReplyTelexTransferObject.State = GetCraneStatus(pLCToCrane.Work_Model);
                //    requestStateCommandReplyTelexTransferObject.Event = GetCraneEvent(pLCToCrane.Fork_State);
                //    requestStateCommandReplyTelexTransferObject.Column = pLCToCrane.Current_Column;
                //    requestStateCommandReplyTelexTransferObject.ColumnCodeValue = pLCToCrane.Current_Column_Value;
                //    requestStateCommandReplyTelexTransferObject.Level = pLCToCrane.Current_Level;
                //    requestStateCommandReplyTelexTransferObject.LevelCodeValue = pLCToCrane.Current_Level_Value;
                //    requestStateCommandReplyTelexTransferObject.TaskId = pLCToCrane.Task_Id.ToString("00000000");
                //    requestStateCommandReplyTelexTransferObject.WcsTaskId = pLCToCrane.WcsTaskId;
                //    if (_showCraneForkStation)
                //        requestStateCommandReplyTelexTransferObject.ForkHorizontalPosition = GetForkHorizontalPosition((int)pLCToCrane.Fork_Station);
                //    else
                //    {
                //        requestStateCommandReplyTelexTransferObject.ForkVerticalPosition = pLCToCrane.ForkVerticalPosition;
                //        requestStateCommandReplyTelexTransferObject.ForkHorizontalPosition = pLCToCrane.ForkHorizontalPosition;
                //    }
                //    requestStateCommandReplyTelexTransferObject.AtStation = Convert.ToBoolean(pLCToCrane.AtStation);
                //    requestStateCommandReplyTelexTransferObject.Barcode = pLCToCrane.Barcode;
                //    requestStateCommandReplyTelexTransferObject.Check_Barcode = pLCToCrane.Check_Barcode;
                //    requestStateCommandReplyTelexTransferObject.ForkCodeValue_Z1 = pLCToCrane.Current_Z1_Value;
                //    requestStateCommandReplyTelexTransferObject.ForkCodeValue_Z2 = pLCToCrane.Current_Z2_Value;
                //    requestStateCommandReplyTelexTransferObject.ErrorCodeList = GetErrorCodeList(pLCToCrane.Alarm_Info);
                //    return requestStateCommandReplyTelexTransferObject;
                //}
                //else //if(PLCType == "西门子PLC")
                //{
                //    RequestStateCommandReplyTelexTransferObject requestStateCommandReplyTelexTransferObject = new RequestStateCommandReplyTelexTransferObject();
                //    PLCToCraneStatusNetTransferObject pLCToCrane = new PLCToCraneStatusNetTransferObject();

                //    bool _showCraneForkStation = false;
                //    pLCToCrane.Device_No = (UInt16)convertBytesToNumberValue(pLCToCrane.Device_No.GetType().Name, Data.Skip(2).Take(2).ToArray());
                //    if (ShowCraneForkStation.Contains(pLCToCrane.Device_No.ToString()))
                //        _showCraneForkStation = true;
                //    pLCToCrane.Work_Model = (byte)convertBytesToNumberValue(pLCToCrane.Work_Model.GetType().Name, Data.Skip(4).Take(1).ToArray());
                //    //报警标志，
                //    pLCToCrane.IsAlarm = (byte)convertBytesToNumberValue(pLCToCrane.IsAlarm.GetType().Name, Data.Skip(5).Take(1).ToArray());
                //    pLCToCrane.IsForkGoods = (byte)convertBytesToNumberValue(pLCToCrane.IsForkGoods.GetType().Name, Data.Skip(6).Take(1).ToArray());

                //    if (CraneCommandVesion == "V0")//兼容艾芬达、虑毒罐项目的堆垛机通讯 else 里面是针对西门子PLC最小字节数是2个字节的调整，从BMDT及后续项目已做更新采用新协议
                //    {
                //        pLCToCrane.Current_Column = (UInt16)convertBytesToNumberValue(pLCToCrane.Current_Column.GetType().Name, Data.Skip(7).Take(2).ToArray());
                //        pLCToCrane.Current_Level = (UInt16)convertBytesToNumberValue(pLCToCrane.Current_Level.GetType().Name, Data.Skip(9).Take(2).ToArray());
                //        pLCToCrane.Current_Column_Value = (UInt32)convertBytesToNumberValue(pLCToCrane.Current_Column_Value.GetType().Name, Data.Skip(11).Take(4).ToArray());
                //        pLCToCrane.Current_Level_Value = (UInt32)convertBytesToNumberValue(pLCToCrane.Current_Level_Value.GetType().Name, Data.Skip(15).Take(4).ToArray());
                //        if (_showCraneForkStation)
                //            pLCToCrane.Fork_Station = (UInt32)convertBytesToNumberValue(pLCToCrane.Fork_Station.GetType().Name, Data.Skip(19).Take(4).ToArray());
                //        else
                //        {
                //            pLCToCrane.ForkVerticalPosition = (ForkVerticalPosition)(UInt16)convertBytesToNumberValue(pLCToCrane.Fork_Station.GetType().Name, Data.Skip(19).Take(2).ToArray());
                //            pLCToCrane.ForkHorizontalPosition = (ForkHorizontalPosition)(UInt16)convertBytesToNumberValue(pLCToCrane.Fork_Station.GetType().Name, Data.Skip(21).Take(2).ToArray());
                //        }
                //        pLCToCrane.IsForkWork = (byte)convertBytesToNumberValue(pLCToCrane.IsForkWork.GetType().Name, Data.Skip(23).Take(1).ToArray());
                //        //货叉状态
                //        pLCToCrane.Fork_State = (byte)convertBytesToNumberValue(pLCToCrane.Fork_State.GetType().Name, Data.Skip(24).Take(1).ToArray());
                //        pLCToCrane.Task_Id = (UInt32)convertBytesToNumberValue(pLCToCrane.Task_Id.GetType().Name, Data.Skip(25).Take(4).ToArray());
                //        //报警信息
                //        pLCToCrane.Alarm_Info = Data.Skip(29).Take(20).ToList();
                //    }
                //    else if (CraneCommandVesion == "V1")
                //    {
                //        pLCToCrane.AtStation = (byte)convertBytesToNumberValue(pLCToCrane.IsForkGoods.GetType().Name, Data.Skip(7).Take(1).ToArray());
                //        pLCToCrane.Current_Column = (UInt16)convertBytesToNumberValue(pLCToCrane.Current_Column.GetType().Name, Data.Skip(8).Take(2).ToArray());
                //        pLCToCrane.Current_Level = (UInt16)convertBytesToNumberValue(pLCToCrane.Current_Level.GetType().Name, Data.Skip(10).Take(2).ToArray());
                //        pLCToCrane.Current_Column_Value = (UInt32)convertBytesToNumberValue(pLCToCrane.Current_Column_Value.GetType().Name, Data.Skip(12).Take(4).ToArray());
                //        pLCToCrane.Current_Level_Value = (UInt32)convertBytesToNumberValue(pLCToCrane.Current_Level_Value.GetType().Name, Data.Skip(16).Take(4).ToArray());
                //        if (_showCraneForkStation)
                //            pLCToCrane.Fork_Station = (UInt32)convertBytesToNumberValue(pLCToCrane.Fork_Station.GetType().Name, Data.Skip(20).Take(4).ToArray());
                //        else
                //        {
                //            pLCToCrane.ForkVerticalPosition = (ForkVerticalPosition)(UInt16)convertBytesToNumberValue("UInt16", Data.Skip(20).Take(2).ToArray());
                //            pLCToCrane.ForkHorizontalPosition = (ForkHorizontalPosition)(UInt16)convertBytesToNumberValue("UInt16", Data.Skip(22).Take(2).ToArray());
                //        }
                //        pLCToCrane.IsForkWork = (byte)convertBytesToNumberValue(pLCToCrane.IsForkWork.GetType().Name, Data.Skip(24).Take(1).ToArray());
                //        //货叉状态
                //        pLCToCrane.Fork_State = (byte)convertBytesToNumberValue(pLCToCrane.Fork_State.GetType().Name, Data.Skip(25).Take(1).ToArray());
                //        pLCToCrane.Task_Id = (UInt32)convertBytesToNumberValue(pLCToCrane.Task_Id.GetType().Name, Data.Skip(26).Take(4).ToArray());
                //        pLCToCrane.Barcode = System.Text.ASCIIEncoding.ASCII.GetString(Data.Skip(30).Take(20).ToArray());
                //        pLCToCrane.Check_Barcode = System.Text.ASCIIEncoding.ASCII.GetString(Data.Skip(50).Take(20).ToArray());
                //        //报警信息
                //        pLCToCrane.Alarm_Info = Data.Skip(70).Take(20).ToList();
                //        if (Data.Length == 98)
                //        {
                //            pLCToCrane.Current_Z1_Value = (Int32)convertBytesToNumberValue(pLCToCrane.Current_Z1_Value.GetType().Name, Data.Skip(90).Take(4).ToArray());
                //            pLCToCrane.Current_Z2_Value = (Int32)convertBytesToNumberValue(pLCToCrane.Current_Z2_Value.GetType().Name, Data.Skip(94).Take(4).ToArray());
                //        }
                //    }
                //    else
                //    {
                //        pLCToCrane.AtStation = (byte)convertBytesToNumberValue(pLCToCrane.IsForkGoods.GetType().Name, Data.Skip(7).Take(1).ToArray());
                //        pLCToCrane.Current_Column = (UInt16)convertBytesToNumberValue(pLCToCrane.Current_Column.GetType().Name, Data.Skip(8).Take(2).ToArray());
                //        pLCToCrane.Current_Level = (UInt16)convertBytesToNumberValue(pLCToCrane.Current_Level.GetType().Name, Data.Skip(10).Take(2).ToArray());
                //        pLCToCrane.Current_Column_Value = (UInt32)convertBytesToNumberValue(pLCToCrane.Current_Column_Value.GetType().Name, Data.Skip(12).Take(4).ToArray());
                //        pLCToCrane.Current_Level_Value = (UInt32)convertBytesToNumberValue(pLCToCrane.Current_Level_Value.GetType().Name, Data.Skip(16).Take(4).ToArray());
                //        if (_showCraneForkStation)
                //            pLCToCrane.Fork_Station = (UInt32)convertBytesToNumberValue(pLCToCrane.Fork_Station.GetType().Name, Data.Skip(20).Take(4).ToArray());
                //        else
                //        {
                //            pLCToCrane.ForkVerticalPosition = (ForkVerticalPosition)(UInt16)convertBytesToNumberValue("UInt16", Data.Skip(20).Take(2).ToArray());
                //            pLCToCrane.ForkHorizontalPosition = (ForkHorizontalPosition)(UInt16)convertBytesToNumberValue("UInt16", Data.Skip(22).Take(2).ToArray());
                //        }
                //        pLCToCrane.IsForkWork = (byte)convertBytesToNumberValue(pLCToCrane.IsForkWork.GetType().Name, Data.Skip(24).Take(1).ToArray());
                //        //货叉状态
                //        pLCToCrane.Fork_State = (byte)convertBytesToNumberValue(pLCToCrane.Fork_State.GetType().Name, Data.Skip(25).Take(1).ToArray());
                //        pLCToCrane.Task_Id = (UInt32)convertBytesToNumberValue(pLCToCrane.Task_Id.GetType().Name, Data.Skip(26).Take(4).ToArray());
                //        pLCToCrane.WcsTaskId = (UInt32)convertBytesToNumberValue(pLCToCrane.Task_Id.GetType().Name, Data.Skip(30).Take(4).ToArray());
                //        pLCToCrane.StepType = (UInt16)convertBytesToNumberValue(pLCToCrane.Task_Id.GetType().Name, Data.Skip(34).Take(2).ToArray());
                //        pLCToCrane.Barcode = System.Text.ASCIIEncoding.ASCII.GetString(Data.Skip(36).Take(20).ToArray());
                //        pLCToCrane.Check_Barcode = System.Text.ASCIIEncoding.ASCII.GetString(Data.Skip(56).Take(20).ToArray());
                //        //报警信息
                //        pLCToCrane.Alarm_Info = Data.Skip(76).Take(20).ToList();
                //        if (Data.Length >= 104)
                //        {
                //            pLCToCrane.Current_Z1_Value = (Int32)convertBytesToNumberValue(pLCToCrane.Current_Z1_Value.GetType().Name, Data.Skip(96).Take(4).ToArray());
                //            pLCToCrane.Current_Z2_Value = (Int32)convertBytesToNumberValue(pLCToCrane.Current_Z2_Value.GetType().Name, Data.Skip(100).Take(4).ToArray());
                //        }
                //        if (Data.Length >= 106)
                //        {
                //            pLCToCrane.CurrentLine = (UInt16)convertBytesToNumberValue(pLCToCrane.Current_Z2_Value.GetType().Name, Data.Skip(104).Take(2).ToArray());
                //        }
                //    }
                //    //给堆垛机状态类赋值  ?新协议中堆垛机报警码以字节数组来表示，老协议中是一个字段显示。
                //    //暂未给报警码赋值处理。
                //    requestStateCommandReplyTelexTransferObject.State = GetCraneStatus(pLCToCrane.Work_Model);
                //    requestStateCommandReplyTelexTransferObject.Event = GetCraneEvent(pLCToCrane.Fork_State);
                //    requestStateCommandReplyTelexTransferObject.Column = pLCToCrane.Current_Column;
                //    requestStateCommandReplyTelexTransferObject.ColumnCodeValue = pLCToCrane.Current_Column_Value;
                //    requestStateCommandReplyTelexTransferObject.Level = pLCToCrane.Current_Level;
                //    requestStateCommandReplyTelexTransferObject.LevelCodeValue = pLCToCrane.Current_Level_Value;
                //    requestStateCommandReplyTelexTransferObject.TaskId = pLCToCrane.Task_Id.ToString("00000000");
                //    if (_showCraneForkStation)
                //        requestStateCommandReplyTelexTransferObject.ForkHorizontalPosition = GetForkHorizontalPosition((int)pLCToCrane.Fork_Station);
                //    else
                //    {
                //        requestStateCommandReplyTelexTransferObject.ForkVerticalPosition = pLCToCrane.ForkVerticalPosition;
                //        requestStateCommandReplyTelexTransferObject.ForkHorizontalPosition = pLCToCrane.ForkHorizontalPosition;
                //    }
                //    if (CraneCommandVesion == "V0")//兼容艾芬达、虑毒罐项目的堆垛机通讯 else 里面是针对西门子PLC最小字节数是2个字节的调整
                //        requestStateCommandReplyTelexTransferObject.AtStation = true;
                //    else
                //    {
                //        requestStateCommandReplyTelexTransferObject.AtStation = Convert.ToBoolean(pLCToCrane.AtStation);
                //        requestStateCommandReplyTelexTransferObject.Barcode = pLCToCrane.Barcode;
                //        requestStateCommandReplyTelexTransferObject.Check_Barcode = pLCToCrane.Check_Barcode;
                //        requestStateCommandReplyTelexTransferObject.ForkCodeValue_Z1 = pLCToCrane.Current_Z1_Value;
                //        requestStateCommandReplyTelexTransferObject.ForkCodeValue_Z2 = pLCToCrane.Current_Z2_Value;
                //    }
                //    requestStateCommandReplyTelexTransferObject.ErrorCodeList = GetErrorCodeList(pLCToCrane.Alarm_Info);
                //    return requestStateCommandReplyTelexTransferObject;
                //}
                #endregion


                RequestStateCommandReplyTelexTransferObject requestStateCommandReplyTelexTransferObject = new RequestStateCommandReplyTelexTransferObject();
                PLCToCraneStatusNetTransferObject pLCToCrane = new PLCToCraneStatusNetTransferObject();
                bool _showCraneForkStation = false;
                if (ShowCraneForkStation.Contains(pLCToCrane.Device_No.ToString()))
                    _showCraneForkStation = true;

                //设备编号
                pLCToCrane.Device_No = (UInt16)convertBytesToNumberValue(pLCToCrane.Device_No.GetType().Name, Data.Skip(2).Take(2).ToArray());
                //工作模式
                pLCToCrane.Work_Model = (UInt16)convertBytesToNumberValue(pLCToCrane.Work_Model.GetType().Name, Data.Skip(4).Take(2).ToArray());
                //报警标志，
                pLCToCrane.IsAlarm = (UInt16)convertBytesToNumberValue(pLCToCrane.IsAlarm.GetType().Name, Data.Skip(6).Take(2).ToArray());
                //货叉是否有货
                pLCToCrane.IsForkGoods = (UInt16)convertBytesToNumberValue(pLCToCrane.IsForkGoods.GetType().Name, Data.Skip(8).Take(2).ToArray());
                //是否在站点
                pLCToCrane.AtStation = (UInt16)convertBytesToNumberValue(pLCToCrane.IsForkGoods.GetType().Name, Data.Skip(10).Take(2).ToArray());
                //当前列
                pLCToCrane.Current_Column = (UInt16)convertBytesToNumberValue(pLCToCrane.Current_Column.GetType().Name, Data.Skip(12).Take(2).ToArray());
                //当前层
                pLCToCrane.Current_Level = (UInt16)convertBytesToNumberValue(pLCToCrane.Current_Level.GetType().Name, Data.Skip(14).Take(2).ToArray());
                //当前列值
                pLCToCrane.Current_Column_Value = (UInt32)convertBytesToNumberValue(pLCToCrane.Current_Column_Value.GetType().Name, Data.Skip(16).Take(4).ToArray());
                //当前层值
                pLCToCrane.Current_Level_Value = (UInt32)convertBytesToNumberValue(pLCToCrane.Current_Level_Value.GetType().Name, Data.Skip(20).Take(4).ToArray());
                //货叉高低位
                pLCToCrane.ForkVerticalPosition = (ForkVerticalPosition)(UInt16)convertBytesToNumberValue("UInt16", Data.Skip(24).Take(2).ToArray());
                //货叉水平位
                pLCToCrane.ForkHorizontalPosition = (ForkHorizontalPosition)(UInt16)convertBytesToNumberValue("UInt16", Data.Skip(26).Take(2).ToArray());
                //货叉任务是否完成
                pLCToCrane.IsForkWork = (UInt16)convertBytesToNumberValue(pLCToCrane.IsForkWork.GetType().Name, Data.Skip(28).Take(2).ToArray());
                //货叉状态
                pLCToCrane.Fork_State = (UInt16)convertBytesToNumberValue(pLCToCrane.Fork_State.GetType().Name, Data.Skip(30).Take(2).ToArray());
                //任务号
                pLCToCrane.Task_Id = (UInt32)convertBytesToNumberValue(pLCToCrane.Task_Id.GetType().Name, Data.Skip(32).Take(4).ToArray());
                //WCS任务号
                pLCToCrane.WcsTaskId = (UInt32)convertBytesToNumberValue(pLCToCrane.WcsTaskId.GetType().Name, Data.Skip(36).Take(4).ToArray());
                //任务类型
                pLCToCrane.StepType = (UInt16)convertBytesToNumberValue(pLCToCrane.StepType.GetType().Name, Data.Skip(40).Take(2).ToArray());
                //条码值（任务下发的值）
                pLCToCrane.Barcode = System.Text.ASCIIEncoding.ASCII.GetString(Data.Skip(42).Take(20).ToArray());
                //验证条码值（扫码器实际扫到的值）
                pLCToCrane.Check_Barcode = System.Text.ASCIIEncoding.ASCII.GetString(Data.Skip(62).Take(20).ToArray());
                //报警信息
                pLCToCrane.Alarm_Info.AddRange(Data.Skip(82).Take(20).ToArray());
                //货叉编码器值Z1
                pLCToCrane.Current_Z1_Value = (Int32)convertBytesToNumberValue(pLCToCrane.Current_Z1_Value.GetType().Name, Data.Skip(102).Take(4).ToArray());
                //货叉编码器值Z2
                pLCToCrane.Current_Z2_Value = (Int32)convertBytesToNumberValue(pLCToCrane.Current_Z2_Value.GetType().Name, Data.Skip(106).Take(4).ToArray());
                //堆垛机行走方向
                pLCToCrane.WalkDirection = (UInt16)convertBytesToNumberValue(pLCToCrane.WalkDirection.GetType().Name, Data.Skip(110).Take(2).ToArray());
                //堆垛机载货台运行方向
                pLCToCrane.LoadDirection = (UInt16)convertBytesToNumberValue(pLCToCrane.LoadDirection.GetType().Name, Data.Skip(112).Take(2).ToArray());
                //堆垛机货叉运行方向
                pLCToCrane.ForkDirection = (UInt16)convertBytesToNumberValue(pLCToCrane.ForkDirection.GetType().Name, Data.Skip(114).Take(2).ToArray());
                //堆垛机所在货架排
                pLCToCrane.CurrentLine = (UInt16)convertBytesToNumberValue(pLCToCrane.CurrentLine.GetType().Name, Data.Skip(116).Take(2).ToArray());


                //给堆垛机状态类赋值  ?新协议中堆垛机报警码以字节数组来表示，老协议中是一个字段显示。
                //暂未给报警码赋值处理。
                requestStateCommandReplyTelexTransferObject.State = GetCraneStatus(pLCToCrane.Work_Model);
                requestStateCommandReplyTelexTransferObject.Event = GetCraneEvent(pLCToCrane.Fork_State);
                requestStateCommandReplyTelexTransferObject.Column = pLCToCrane.Current_Column;
                requestStateCommandReplyTelexTransferObject.ColumnCodeValue = pLCToCrane.Current_Column_Value;
                requestStateCommandReplyTelexTransferObject.Level = pLCToCrane.Current_Level;
                requestStateCommandReplyTelexTransferObject.LevelCodeValue = pLCToCrane.Current_Level_Value;
                requestStateCommandReplyTelexTransferObject.TaskId = pLCToCrane.Task_Id.ToString("00000000");
                if (_showCraneForkStation)
                    requestStateCommandReplyTelexTransferObject.ForkHorizontalPosition = GetForkHorizontalPosition((int)pLCToCrane.Fork_Station);
                else
                {
                    requestStateCommandReplyTelexTransferObject.ForkVerticalPosition = pLCToCrane.ForkVerticalPosition;
                    requestStateCommandReplyTelexTransferObject.ForkHorizontalPosition = pLCToCrane.ForkHorizontalPosition;
                }
                requestStateCommandReplyTelexTransferObject.AtStation = Convert.ToBoolean(pLCToCrane.AtStation);
                requestStateCommandReplyTelexTransferObject.Barcode = pLCToCrane.Barcode;
                requestStateCommandReplyTelexTransferObject.Check_Barcode = pLCToCrane.Check_Barcode;
                requestStateCommandReplyTelexTransferObject.ForkCodeValue_Z1 = pLCToCrane.Current_Z1_Value;
                requestStateCommandReplyTelexTransferObject.ForkCodeValue_Z2 = pLCToCrane.Current_Z2_Value;
                //requestStateCommandReplyTelexTransferObject.ErrorCodeList = pLCToCrane.Alarm_Info.Where(x => x != 0).Select(x => (Int32)x).ToList();
                requestStateCommandReplyTelexTransferObject.ErrorCodeList = GeneralMappingHelper.GetErrorCodeList(pLCToCrane.Alarm_Info, this.DeviceName, CraneAlarmVersion, GeneralMappings, CraneAlarmBitVesionPath);
                return requestStateCommandReplyTelexTransferObject;

            }
        }


        GeneralMappings GeneralMappings = null;

        private object convertBytesToNumberValue(string valueType, byte[] bytes)
        {
            try
            {
                if (!PLCType)
                {
                    if (valueType == "Int16")
                    {
                        return BitConverter.ToInt16(bytes, 0);
                    }
                    if (valueType == "Int32")
                    {
                        return BitConverter.ToInt32(bytes, 0);
                    }

                    if (valueType == "Int64")
                    {
                        return BitConverter.ToInt64(bytes, 0);
                    }

                    if (valueType == "UInt16")
                    {
                        return BitConverter.ToUInt16(bytes, 0);
                    }

                    if (valueType == "UInt32")
                    {
                        return BitConverter.ToUInt32(bytes, 0);
                    }

                    if (valueType == "UInt64")
                    {
                        return BitConverter.ToUInt64(bytes, 0);
                    }

                    if (valueType == "Single")
                    {
                        return BitConverter.ToSingle(bytes, 0);
                    }

                    if (valueType == "Byte")
                    {
                        return Convert.ToByte(bytes[0]);
                        return bytes[0];
                    }
                    if (valueType == "Byte[]")
                    {
                        return bytes;
                    }
                    if (valueType == "String")
                    {
                        return System.Text.Encoding.Default.GetString((Byte[])bytes.ToArray());
                        return bytes.ToArray();
                    }
                }
                else
                {
                    if (valueType == "Int16")
                    {
                        return BitConverter.ToInt16(bytes.Reverse().ToArray(), 0);
                    }
                    if (valueType == "Int32")
                    {
                        return BitConverter.ToInt32(bytes.Reverse().ToArray(), 0);
                        return BitConverter.ToInt32(bytes.Reverse().ToArray(), 0);
                    }

                    if (valueType == "Int64")
                    {
                        return BitConverter.ToInt64(bytes.Reverse().ToArray(), 0);
                        return BitConverter.ToInt64(bytes.Reverse().ToArray(), 0);
                    }

                    if (valueType == "UInt16")
                    {
                        return Convert.ToUInt16(BitConverter.ToUInt16(bytes.Reverse().ToArray(), 0));
                        return BitConverter.ToUInt16(bytes.Reverse().ToArray(), 0);
                    }

                    if (valueType == "UInt32")
                    {
                        return Convert.ToUInt32(BitConverter.ToUInt32(bytes.Reverse().ToArray(), 0));
                        return BitConverter.ToUInt32(bytes.Reverse().ToArray(), 0);
                    }

                    if (valueType == "UInt64")
                    {
                        return Convert.ToUInt64(BitConverter.ToUInt64(bytes.Reverse().ToArray(), 0));
                        return BitConverter.ToUInt64(bytes.Reverse().ToArray(), 0);
                    }

                    if (valueType == "Single")
                    {
                        return BitConverter.ToSingle(bytes.Reverse().ToArray(), 0);
                    }

                    if (valueType == "Byte")
                    {
                        return Convert.ToByte(bytes[0]);
                        return bytes[0];
                    }
                    if (valueType == "Byte[]")
                    {
                        return bytes;
                    }
                    if (valueType == "String")
                    {
                        return System.Text.Encoding.Default.GetString((Byte[])bytes.ToArray());
                        return bytes.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

            throw new NotSupportedException(String.Format("不支持将 byte[] 转换为 {0}", valueType));
        }
        public static AddTaskCommandReplyTelexTransferObjectResult GetAddTaskCommandReplyTelexTransferObjectResult(int type)
        {
            AddTaskCommandReplyTelexTransferObjectResult result;
            try
            {
                string name = Enum.GetName(typeof(AddTaskCommandReplyTelexTransferObjectResult), type);
                result = (AddTaskCommandReplyTelexTransferObjectResult)Enum.Parse(typeof(AddTaskCommandReplyTelexTransferObjectResult), name);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
        public static CraneStatus GetCraneStatus(int type)
        {
            CraneStatus result;
            try
            {
                string name = Enum.GetName(typeof(CraneStatus), type);
                result = (CraneStatus)Enum.Parse(typeof(CraneStatus), name);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
        public static CraneEvent GetCraneEvent(int type)
        {
            CraneEvent result;
            try
            {
                string name = Enum.GetName(typeof(CraneEvent), type);
                result = (CraneEvent)Enum.Parse(typeof(CraneEvent), name);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
        public static ForkHorizontalPosition GetForkHorizontalPosition(int type)
        {
            ForkHorizontalPosition result;
            try
            {
                string name = Enum.GetName(typeof(ForkHorizontalPosition), type);
                result = (ForkHorizontalPosition)Enum.Parse(typeof(ForkHorizontalPosition), name);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
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

    public class PLCToCraneStatusNetTransferObject
    {
        /// <summary>
        /// 功能码
        /// </summary>
        public String Fun { get; set; }
        /// <summary>
        /// 设备号
        /// </summary>
        public UInt16 Device_No { set; get; }
        /// <summary>
        /// 堆垛机状态
        /// </summary>
        public UInt16 Work_Model { set; get; }
        /// <summary>
        /// 报警标志
        /// </summary>
        public UInt16 IsAlarm { set; get; }
        /// <summary>
        /// 是否有货
        /// </summary>
        public UInt16 IsForkGoods { set; get; }
        /// <summary>
        /// 是否在站点
        /// </summary>
        public UInt16 AtStation { get; set; }
        /// <summary>
        /// 当前列
        /// </summary>
        public UInt16 Current_Column { set; get; }
        /// <summary>
        /// 当前层
        /// </summary>
        public UInt16 Current_Level { set; get; }
        /// <summary>
        /// 当前列值
        /// </summary>
        public UInt32 Current_Column_Value { set; get; }
        /// <summary>
        /// 当前层值
        /// </summary>
        public UInt32 Current_Level_Value { set; get; }
        /// <summary>
        ///货叉位置
        /// </summary>
        public UInt32 Fork_Station { set; get; }
        /// <summary>
        /// 载货台高低位
        /// </summary>
        public ForkVerticalPosition ForkVerticalPosition { get; set; }
        /// <summary>
        /// 货叉水平位置
        /// </summary>
        public ForkHorizontalPosition ForkHorizontalPosition { get; set; }
        /// <summary>
        /// 货叉任务是否完成
        /// </summary>
        public UInt16 IsForkWork { set; get; }
        /// <summary>
        /// 货叉状态
        /// </summary>
        public UInt16 Fork_State { set; get; }
        /// <summary>
        /// 任务号
        /// </summary>
        public UInt32 Task_Id { set; get; }

        /// <summary>
        /// WCS任务号
        /// </summary>
        public UInt32 WcsTaskId { get; set; }
        /// <summary>
        /// 任务类型
        /// </summary>
        public UInt16 StepType { get; set; }
        /// <summary>
        /// 托盘条码
        /// </summary>
        public string Barcode { set; get; }
        /// <summary>
        /// 验证条码
        /// </summary>
        public string Check_Barcode { set; get; }
        /// <summary>
        /// 报警信息
        /// </summary>
        public List<byte> Alarm_Info { set; get; } = new List<byte>();
        /// <summary>
        /// 当前列值
        /// </summary>
        public Int32 Current_Z1_Value { set; get; }
        /// <summary>
        /// 当前层值
        /// </summary>
        public Int32 Current_Z2_Value { set; get; }


        /// <summary>
        /// 垛机行走方向
        /// </summary>
        public UInt16 WalkDirection { get; set; }


        /// <summary>
        /// 载货台运行方向
        /// </summary>
        public UInt16 LoadDirection { get; set; }


        /// <summary>
        /// 货叉运行方向
        /// </summary>
        public UInt16 ForkDirection { get; set; }
        /// <summary>
        /// 当前排
        /// </summary>
        public UInt16 CurrentLine { get; set; }
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
