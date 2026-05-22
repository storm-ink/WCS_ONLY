using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Wcs.Framework
{
    /// <summary>
    /// 一般映射
    /// </summary>
    [XmlRoot("generalMappings")]
    public class GeneralMappings
    {
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public GeneralMappings()
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="xmlDocument">xml文件</param>
        public GeneralMappings(XmlDocument xmlDocument)
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="xmlDocument">xml文件</param>
        public GeneralMappings(string xmlStr)
        {
            //using (StringReader sr = new StringReader(xmlStr))
            //{
            //    XmlSerializer serializer = new XmlSerializer(typeof(GeneralMappings));
            //    this = serializer.Deserialize(sr) as GeneralMappings;
            //}
        }

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        /// <summary>
        /// 分组
        /// </summary>
        [XmlElement(ElementName = "groups")]
        public Groups Groups { get; set; }
        /// <summary>
        /// 映射
        /// </summary>
        [XmlElement(ElementName = "mappings")]
        public Mappings Mappings { get; set; }
        /// <summary>
        /// 绑定
        /// </summary>
        [XmlElement(ElementName = "bindings")]
        public Bindings Bindings { get; set; }
    }

    #region group
    public class Groups
    {
        [XmlElement("group")]
        public List<Group> GroupList { get; set; }
    }

    /// <summary>
    /// 分组
    /// </summary>
    public class Group
    {
        [XmlAttribute(AttributeName = "groupName")]
        public string GroupName { get; set; }
        [XmlText]
        public string Content
        {
            get
            {
                if (ObjectList == null || ObjectList.Count() == 0)
                    return "";

                return string.Join(",", ObjectList);
            }
            set
            {
                ObjectList = value.Split(',').ToList();
            }
        }

        [XmlIgnore]
        public List<string> ObjectList { get; set; }
    }
    #endregion

    #region mapping
    public class Mappings
    {
        [XmlElement("mapping")]
        public List<Mapping> MappingList { get; set; }
    }

    /// <summary>
    /// 映射
    /// </summary>
    public class Mapping
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "showName")]
        public string ShowName { get; set; }

        [XmlAttribute(AttributeName = "blockBytes")]
        public int BlockBytes { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
        [XmlElement("property")]
        public List<Property> Properties { get; set; }

        public dynamic GetDynamic()
        {
            Dictionary<string, object> temp = new Dictionary<string, object>();
            foreach (var item in Properties)
            {
                temp.Add(item.Name, "");
            }
            dynamic obj = new System.Dynamic.ExpandoObject();
            foreach (var item in temp)
            {
                ((IDictionary<string, object>)obj).Add(item.Key, item.Value);
            }
            return obj;
        }

        public dynamic GetDynamicShowName()
        {
            Dictionary<string, object> temp = new Dictionary<string, object>();
            foreach (var item in Properties)
            {
                temp.Add(item.ShowName, "");
            }
            dynamic obj = new System.Dynamic.ExpandoObject();
            foreach (var item in temp)
            {
                ((IDictionary<string, object>)obj).Add(item.Key, item.Value);
            }
            return obj;
        }

        /// <summary>
        /// 设置Property的值
        /// </summary>
        /// <param name="bytes"></param>
        public void SetPropertyValue(byte[] bytes)
        {
            if (bytes.Length < this.BlockBytes)
                throw new Exception($"bytes {BitConverter.ToUInt64(bytes, 0).ToString("x")} 长度（）小于所需解析长度 {this.BlockBytes}");

            var properties = Properties.OrderBy(x => x.Index).ToArray();
            for (int i = 0; i < properties.Count(); i++)
            {
                var property = properties[i];

                var valueType = property.Type.GetBasicType();
                if (valueType != typeof(Boolean))
                {
                    property.Content = convertBytesToNumberValue(valueType, bytes.Skip((int)property.Index).Take(property.Size).ToArray());
                }
                else
                {
                    int byteIndex = (int)property.Index;
                    int bitIndex = Convert.ToInt32(property.Index.ToString("#.0").Split('.')[1]);
                    byte b = bytes[byteIndex];
                    property.Content = convertBytesToBooleanValue(b, bitIndex);
                }
            }
        }

        private string convertBytesToNumberValue(Type valueType, byte[] bytes)
        {
            try
            {
                if (valueType == typeof(Int16))
                {
                    return BitConverter.ToInt16(bytes.Reverse().ToArray(), 0).ToString();
                }

                if (valueType == typeof(Int32))
                {
                    return BitConverter.ToInt32(bytes.Reverse().ToArray(), 0).ToString();
                }

                if (valueType == typeof(Int64))
                {
                    return BitConverter.ToInt64(bytes.Reverse().ToArray(), 0).ToString();
                }

                if (valueType == typeof(UInt16))
                {
                    return BitConverter.ToUInt16(bytes.Reverse().ToArray(), 0).ToString();
                }

                if (valueType == typeof(UInt32))
                {
                    return BitConverter.ToUInt32(bytes.Reverse().ToArray(), 0).ToString();
                }

                if (valueType == typeof(UInt64))
                {
                    return BitConverter.ToUInt64(bytes.Reverse().ToArray(), 0).ToString();
                }

                if (valueType == typeof(Single))
                {
                    return BitConverter.ToSingle(bytes.Reverse().ToArray(), 0).ToString();
                }

                if (valueType == typeof(Byte))
                {
                    return bytes[0].ToString();
                }

                if (valueType == typeof(String))
                {
                    return ASCIIEncoding.ASCII.GetString(bytes).Trim(' ');
                }

                //if (valueType == typeof(Byte[]))
                //{
                //    return bytes.ToList();
                //}
            }
            catch (Exception ex)
            {
                //_logger.Error1(ex, this);
                throw ex;
            }

            throw new NotSupportedException(String.Format("不支持将 byte[] 转换为 {0}", valueType));
        }
        private string convertBytesToBooleanValue(byte b, int bitIndex)
        {
            if (bitIndex < 0 || bitIndex > 7)
            {
                throw new ArgumentOutOfRangeException("bitIndex:大于等于 0,小于等于 7");
            }
            //var shifted = b >> (7 - bitIndex);
            var shifted = b >> bitIndex;

            return (shifted % 2 != 0).ToString();
        }

        private byte[] convertNumberToBytes(Type valueType, object value)
        {
            if (valueType == typeof(Int16))
            {
                return BitConverter.GetBytes(Convert.ToInt16(value));
            }

            if (valueType == typeof(Int32))
            {
                return BitConverter.GetBytes(Convert.ToInt32(value));
            }

            if (valueType == typeof(Int64))
            {
                return BitConverter.GetBytes(Convert.ToInt64(value));
            }

            if (valueType == typeof(UInt16))
            {
                return BitConverter.GetBytes(Convert.ToUInt16(value));
            }

            if (valueType == typeof(UInt32))
            {
                return BitConverter.GetBytes(Convert.ToUInt32(value));
            }

            if (valueType == typeof(UInt64))
            {
                return BitConverter.GetBytes(Convert.ToUInt64(value));
            }

            if (valueType == typeof(Boolean))
            {
                return BitConverter.GetBytes(Convert.ToBoolean(value));
            }

            if (valueType == typeof(Single))
            {
                return BitConverter.GetBytes(Convert.ToSingle(value));
            }


            if (valueType == typeof(Byte))
            {
                return new Byte[] { Convert.ToByte(value) };
            }

            throw new NotSupportedException(String.Format("不支持将 {0} 转换为 byte[]", valueType));
        }

        private byte[] cnvertAsciiStringToBytes(String value)
        {
            if (value != null)
                return ASCIIEncoding.ASCII.GetBytes(value);
            return null;
        }
    }

    public class Property
    {
        /// <summary>
        /// 名称
        /// </summary>
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        /// <summary>
        /// 显示名称
        /// </summary>
        [XmlAttribute(AttributeName = "showName")]
        public string ShowName { get; set; }
        /// <summary>
        /// 索引
        /// </summary>
        [XmlAttribute(AttributeName = "index")]
        public float Index { get; set; }
        /// <summary>
        /// 长度
        /// </summary>
        [XmlAttribute(AttributeName = "size")]
        public int Size { get; set; }

        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
        [XmlText]
        public string Content { get; set; }
    }
    #endregion

    #region
    public class Bindings
    {
        [XmlElement("binding")]
        public List<Binding> BindingList { get; set; }
    }

    public class Binding
    {
        [XmlAttribute(AttributeName = "group")]
        public string Group { get; set; }

        [XmlAttribute(AttributeName = "mapping")]
        public string Mapping { get; set; }
    }
    #endregion

    #region GeneralMappingHelper
    public static class GeneralMappingHelper
    {
        public static string GetMapping<T>(T t)
        {
            string str = "";

            return str;
        }

        public static T Decode<T>(Mapping mapping, List<byte> bytes)
        {
            throw new ArgumentNullException();
        }

        /// <summary>
        /// 获取故障列表
        /// </summary>
        /// <param name="alarm_Info">基础Byte[]</param>
        /// <param name="deviceName">设备名，group中配置的名称，一般垛机或者穿梭车为设备名，输送线为货位号</param>
        /// <param name="alarmVersion">目前仅支持 BitVersion ByteVersion UInt16Version 0X2Version 四种格式，其中BitVersion 需要提供generalMappings或者generalMappingPath参数</param>
        /// <param name="generalMappings"></param>
        /// <param name="generalMappingPath"></param>
        /// <returns></returns>
        public static List<string> GetErrorCodeList(List<byte> alarm_Info, string deviceName, string alarmVersion, GeneralMappings generalMappings = null, string generalMappingPath = null)
        {
            //System.Diagnostics.Stopwatch SW = new System.Diagnostics.Stopwatch();
            //SW.Start();
            List<string> list = new List<string>();
            if (alarmVersion == "BitVersion")
            {
                if (generalMappings == null)
                {
                    if (!String.IsNullOrWhiteSpace(generalMappingPath))
                    {
                        XmlDocument xml = new XmlDocument();
                        xml.Load(generalMappingPath);
                        var strxml = xml.OuterXml;
                        generalMappings = xml.DESerializer<GeneralMappings>(strxml);
                    }
                }

                var group = generalMappings.Groups.GroupList.FirstOrDefault(x => x.Content.Contains(deviceName));
                if (group != null)
                {
                    var binding = generalMappings.Bindings.BindingList.FirstOrDefault(x => x.Group == group.GroupName);
                    if (binding != null)
                    {
                        var mapping = generalMappings.Mappings.MappingList.FirstOrDefault(x => x.Name == binding.Mapping);
                        mapping.SetPropertyValue(alarm_Info.ToArray());
                        foreach (var property in mapping.Properties)
                        {
                            var valueType = property.Type.GetBasicType();
                            if (valueType == typeof(Boolean) && !string.IsNullOrWhiteSpace(property.Content) && Boolean.TryParse(property.Content, out bool content) && content)
                            {
                                if (int.TryParse(property.Name, out int code))
                                    list.Add(code.ToString());
                            }
                        }
                    }
                }
            }
            else if (alarmVersion == "ByteVersion")//1个字节
                list = alarm_Info.Where(x => x != 0).Select(x => x.ToString()).ToList();
            else if (alarmVersion == "UInt16Version")//2个字节
            {
                var count = alarm_Info.Count / 2;
                for (int i = 0; i < count; i++)
                {
                    UInt16 value;
                    value = Convert.ToUInt16(BitConverter.ToInt16(alarm_Info.Skip(i * 2).Take(2).Reverse().ToArray(), 0));

                    if (value != 0)
                        list.Add(value.ToString());
                }
            }
            else if (alarmVersion == "0X2Version")//2个字节 十六进制标志
            {
                var count = alarm_Info.Count / 2;
                for (int i = 0; i < count; i++)
                {
                    UInt16 value;
                    value = Convert.ToUInt16(BitConverter.ToInt16(alarm_Info.Skip(i * 2).Take(2).Reverse().ToArray(), 0));

                    if (value != 0)
                        list.Add(value.ToString("X4"));
                }
            }

            //SW.Stop();
            //Console.WriteLine($"{Device.Name}报警解析耗时{SW.ElapsedMilliseconds}ms");
            return list;
        }
    }
    #endregion

    public static class TypeExtend
    {
        public static Type GetBasicType(this string typeStr)
        {
            return System.Type.GetType(string.Concat("System." + typeStr));
        }
    }
}