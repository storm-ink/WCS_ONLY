using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using Wcs;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{

    /// <summary>
    ///  穿梭车状态报文 
    /// </summary>
    public class StateTelexTransferObject : RailGuidedVehicleTelexTransferObject
    {
        String _telex;

        /// <summary>
        /// 位置值
        /// </summary>
        [DataMember]
        public UInt32 Position { get; private set; }

        /// <summary>
        /// 当前站台号
        /// </summary>
        [DataMember]
        public UInt16 CurrentStation { get; private set; }

        /// <summary>
        /// 指示穿梭车当前是否在站点位置
        /// </summary>
        [DataMember]
        public Boolean AtStation { get; private set; }

        /// <summary>
        /// 错误码（默认为 0）
        /// </summary>
        [DataMember]
        public string ErrorCode { get; private set; }

        /// <summary>
        /// 穿梭车状态
        /// </summary>
        [DataMember]
        public RailGuidedVehicleStatus State { get; private set; }

        /// <summary>
        /// 穿梭车事件
        /// </summary>
        [DataMember]
        public RailGuidedVehicleEvent Event { get; private set; }


        /// <summary>
        /// 当前任务号
        /// </summary>
        [DataMember]
        public String TaskId { get; private set; }

        /// <summary>
        /// 托盘条码
        /// </summary>
        [DataMember]
        public UInt32 ContainerCode { get; private set; }

        /// <summary>
        /// 起点
        /// </summary>
        [DataMember]
        public UInt16 FromStation { get; private set; }

        /// <summary>
        /// 取货站链条动作
        /// </summary>
        [DataMember]
        public ChainAction FromChainAction { get; set; }

        /// <summary>
        /// 目的的
        /// </summary>
        [DataMember]
        public UInt16 ToStation { get; private set; }

        /// <summary>
        /// 目的站链条动作
        /// </summary>
        [DataMember]
        public ChainAction ToChainAction { get; set; }

        /// <summary>
        /// 任务模式
        /// </summary>
        [DataMember]
        public RailGuidedVehicleTaskMode TaskMode { get; private set; }

        /// <summary>
        /// 故障列表
        /// </summary>
        [DataMember]
        public List<string> ErrorList { get; private set; }

        /// <summary>
        /// 设备名
        /// </summary>
        [DataMember]
        public string DeviceName { get; set; }

        /// <summary>
        /// 增加传感器信号/对接信号上报等，WCS做日志记录、故障排查支持使用
        /// </summary>
        /// 跟余工沟通

        public StateTelexTransferObject()
            : base()
        {
        }
        public StateTelexTransferObject(string deviceName, string telex)
            : base()
        {
            telex = telex.Replace('\0', '0');
            if (!ValidateType(telex))
            {
                throw new InvalidOperationException(String.Format("收到的报文内容 {0} 无法转换为 StateTelexTransferObject 类型.", telex));
            }
            try
            {
                this.DeviceName = deviceName;
                this.Position = Convert.ToUInt32(telex.Substring(3, 7));
                this.CurrentStation = Convert.ToUInt16(telex.Substring(10, 3));
                this.AtStation = Convert.ToBoolean(Convert.ToInt32(telex.Substring(13, 1)));
                //this.ErrorCode = Convert.ToInt32(telex.Substring(14, 2));
                this.State = Utils.ConvertTo<RailGuidedVehicleStatus>(telex.Substring(14, 1).ToUpper());
                this.Event = Utils.ConvertTo<RailGuidedVehicleEvent>(telex.Substring(15, 1));
                this.TaskId = telex.Substring(16, 8);
                this.ContainerCode = Convert.ToUInt32(telex.Substring(24, 4));
                this.FromStation = Convert.ToUInt16(telex.Substring(28, 3));
                this.FromChainAction = Utils.ConvertTo<ChainAction>(telex.Substring(31, 1));
                this.ToStation = Convert.ToUInt16(telex.Substring(32, 3));
                this.ToChainAction = Utils.ConvertTo<ChainAction>(telex.Substring(35, 1));
                this.TaskMode = Utils.ConvertTo<RailGuidedVehicleTaskMode>(telex.Substring(36, 1));
                var bytes = Encoding.ASCII.GetBytes(telex.Substring(37, Length - 38)).ToList();
                //this.ErrorList = telex.Substring(37, Length - 38).Select(x => Convert.ToBoolean(Convert.ToInt32(x.ToString()))).ToList();
                this.ErrorList = GetErrorCodeList(bytes);
                _telex = telex;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(String.Format("收到的报文内容 {0} 在转换为 StateTelexTransferObject 时发生异常.\n{1}", telex, ex));
            }
        }


        string railGuidVehicleAlarmVersion;
        /// <summary>
        /// 穿梭车协议版本
        /// </summary>
        string RailGuidVehicleAlarmVersion
        {
            get
            {
                if (string.IsNullOrWhiteSpace(railGuidVehicleAlarmVersion))
                {
                    var _version = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("RailGuidVehicleAlarmVersion", "ByteVersion");
                    if (string.IsNullOrWhiteSpace(_version))
                        throw new ArgumentException("未配置堆垛机报警协议版本(配置关键字：CraneAlarmVersion，可配置值ByteVersion|ByteVersion-01|ByteVersion-value|BitVersion|UInt16Version|0X2Version)");
                    railGuidVehicleAlarmVersion = _version;
                }
                return railGuidVehicleAlarmVersion;
            }
        }

        string railGuidVehicleAlarmBitVesionPath;
        public string RailGuidVehicleAlarmBitVesionPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(railGuidVehicleAlarmBitVesionPath))
                {
                    var defaultPath = @".\系统配置\穿梭车\GeneralAlarmMappings.xml";
                    railGuidVehicleAlarmBitVesionPath = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("RailGuidVehicleAlarmBitVesionPath", defaultPath);
                }
                return railGuidVehicleAlarmBitVesionPath;
            }
        }

        GeneralMappings GeneralMappings = null;
        private List<string> GetErrorCodeList(List<byte> alarm_Info)
        {
            //System.Diagnostics.Stopwatch SW = new System.Diagnostics.Stopwatch();
            //SW.Start();
            List<string> list = new List<string>();
            if (RailGuidVehicleAlarmVersion == "BitVersion")
            {
                if (GeneralMappings == null)
                {
                    if (!String.IsNullOrWhiteSpace(RailGuidVehicleAlarmBitVesionPath))
                    {
                        XmlDocument xml = new XmlDocument();
                        xml.Load(RailGuidVehicleAlarmBitVesionPath);
                        var strxml = xml.OuterXml;
                        GeneralMappings = xml.DESerializer<GeneralMappings>(strxml);
                    }
                }

                var group = GeneralMappings.Groups.GroupList.FirstOrDefault(x => x.Content.Contains(this.DeviceName));
                if (group != null)
                {
                    var binding = GeneralMappings.Bindings.BindingList.FirstOrDefault(x => x.Group == group.GroupName);
                    if (binding != null)
                    {
                        var mapping = GeneralMappings.Mappings.MappingList.FirstOrDefault(x => x.Name == binding.Mapping);
                        mapping.SetPropertyValue(alarm_Info.ToArray());
                        foreach (var property in mapping.Properties)
                        {
                            var valueType = property.Type.GetBasicType();
                            if (valueType == typeof(Boolean) && !string.IsNullOrWhiteSpace(property.Content) && Boolean.TryParse(property.Content, out bool content) && content)
                            {
                                //if (int.TryParse(property.Name, out int code))
                                //    list.Add(code.ToString());
                                list.Add(property.Name);
                            }
                        }
                    }
                }
            }
            else if (RailGuidVehicleAlarmVersion.StartsWith("ByteVersion"))//1个字节
            {
                if (RailGuidVehicleAlarmVersion.EndsWith("01"))
                {
                    var errorList = Encoding.ASCII.GetString(alarm_Info.ToArray()).Select(x => Convert.ToBoolean(Convert.ToInt32(x.ToString()))).ToArray();
                    for (int i = 0; i < errorList.Length; i++)
                    {
                        if (errorList[i] && i < ByteVersion_01Helper.Alarms.Length)
                            list.Add(ByteVersion_01Helper.Alarms[i]);
                    }
                }
                else //if (RailGuidVehicleAlarmVersion.EndsWith("value"))
                    list = alarm_Info.Where(x => x != 0).Select(x => x.ToString()).ToList();
            }
            else if (RailGuidVehicleAlarmVersion == "UInt16Version")//2个字节
            {
                var count = alarm_Info.Count / 2;
                for (int i = 0; i < count; i++)
                {
                    var value = Convert.ToUInt16(BitConverter.ToInt16(alarm_Info.Skip(i * 2).Take(2).Reverse().ToArray(), 0));
                    if (value != 0)
                        list.Add(value.ToString());
                }
            }
            else if (RailGuidVehicleAlarmVersion == "0X2Version")//2个字节 十六进制标志
            {
                var count = alarm_Info.Count / 2;
                for (int i = 0; i < count; i++)
                {
                    var value = Convert.ToUInt16(BitConverter.ToInt16(alarm_Info.Skip(i * 2).Take(2).Reverse().ToArray(), 0));
                    if (value != 0)
                        list.Add(value.ToString("X4"));
                }
            }

            //SW.Stop();
            //Console.WriteLine($"{Device.Name}报警解析耗时{SW.ElapsedMilliseconds}ms");
            return list;
        }


        public override string ToTelex()
        {
            return _telex;
        }

        public override string TypeFlag
        {
            get { return "LA"; }
        }

        public override int Length
        {
            get { return 70; }
        }

        public override object this[string name]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }

    public static class TypeExtend
    {
        public static Type GetBasicType(this string typeStr)
        {
            return System.Type.GetType(string.Concat("System." + typeStr));
        }
    }

    public static class ByteVersion_01Helper
    {
        static List<string> alarms = null;
        public static string[] Alarms
        {
            get
            {
                if (alarms == null)
                {
                    var defaultPath = @".\系统配置\穿梭车\ByteVersion_01.txt";
                    if (System.IO.File.Exists(defaultPath))
                    {
                        alarms = new List<string>();
                        var _alarms = System.IO.File.ReadAllText(defaultPath).Split(new char[] { '\r', '\n' }).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                        foreach (var item in _alarms)
                        {
                            var _items = item.Split('|').ToArray();
                            if (_items.Length == 2)
                                alarms.Add(_items[0]);
                        }
                    }
                }
                return alarms.ToArray();
            }
        }
    }
}
