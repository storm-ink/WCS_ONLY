using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Xml;

namespace Wcs.Framework.Devices.Conveyor
{
    class _DefaultConveyorTcpProtocolDataReceiverEncoding
    {
        private Dictionary<Type, PropertyInfo[]> _typePropertys = new Dictionary<Type, PropertyInfo[]>();
        /// <summary>
        /// db 块配置集合
        /// </summary>
        public _CollectionSetting[] _collectionSettings;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="configNode">配置节点</param>
        public _DefaultConveyorTcpProtocolDataReceiverEncoding(XmlNode configNode)
        {
            List<_CollectionSetting> settings = new List<_CollectionSetting>();
            foreach (XmlNode collectionSettingNode in configNode.SelectNodes("collection"))
            {
                Type collectionType = Type.GetType(collectionSettingNode.Attributes["type"].Value);
                if (collectionType == null)
                {
                    throw new Exception(string.Format("未找到 collection 节点中 type 属性指定的类型 {0}。", collectionSettingNode.Attributes["type"].Value));
                }
                _CollectionSetting collectionSetting = new _CollectionSetting
                {
                    blockBytes = Convert.ToInt32(collectionSettingNode.Attributes["blockBytes"].Value),
                    itemCount = Convert.ToInt32(collectionSettingNode.Attributes["itemCount"].Value),
                    type = collectionType
                };

                List<_PropertySetting> propertySettings = new List<_PropertySetting>();
                foreach (XmlNode propertyNode in collectionSettingNode.SelectNodes("property"))
                {
                    Type propertyType = Type.GetType(String.Concat("System.", propertyNode.Attributes["type"].Value));
                    if (propertyType == null)
                    {
                        throw new Exception(string.Format("未找到 collection 节点 property 子节点中 type 属性指定的类型 {0}。其必须为 System 命名空间下的基元类型。", propertyNode.Attributes["type"].Value));
                    }
                    propertySettings.Add(new _PropertySetting
                    {
                        index = Convert.ToDecimal(propertyNode.Attributes["index"].Value),
                        name = propertyNode.Attributes["name"].Value,
                        size = Convert.ToInt32(propertyNode.Attributes["size"].Value),
                        type = propertyType
                    });
                }

                collectionSetting.propertys = propertySettings.ToArray();
                settings.Add(collectionSetting);
            }

            this._collectionSettings = settings.ToArray();

            this.TotalBytes = settings.Sum(x => x.blockBytes * x.itemCount);
        }
        /// <summary>
        /// 从指定的接收到的数据包中的数据部分中获取指定类型的状态数据
        /// </summary>
        /// <typeparam name="T">泛型参数，状态数据类型</typeparam>
        /// <param name="receivedBytes">接收到的数据包中的数据部分</param>
        /// <returns>
        /// 指定类型的状态数据集合 <br />
        /// 如果 T 类型不在配置集合中，将返回一个长度为 0 的 T 数组。
        /// </returns>
        /// <exception cref="T:Exception">_collectionSettings 为 null 时将引发此异常</exception>
        public T[] Get<T>(byte[] receivedBytes)
            where T : NetTransferObject, new()
        {
            try
            {
                if (_collectionSettings == null)
                {
                    throw new Exception("未初始化对象");
                }

                _CollectionSetting mapper = _collectionSettings.SingleOrDefault(x => x.type == typeof(T));
                if (mapper == null)
                {
                    //#warning 此处为兼容非标输送线设备无标准数据块时引起的异常
                    throw new Exception(String.Format("{0} 未为类型 {1} 配置结构信息", this, typeof(T)));
                    //return new T[] { };
                }

                int mapperIndex = _collectionSettings.ToList().IndexOf(mapper);
                int skipBytes = _collectionSettings
                    .Take(mapperIndex)
                    .Sum(x => x.blockBytes * x.itemCount);

                List<T> result = new List<T>();
                Type type = typeof(T);
                for (int i = skipBytes; i < skipBytes + mapper.blockBytes * mapper.itemCount; i += mapper.blockBytes)
                {
                    byte[] propertyValueBytes = new byte[mapper.blockBytes];
                    Array.Copy(receivedBytes, i, propertyValueBytes, 0, mapper.blockBytes);
                    T item = Decode<T>(propertyValueBytes);
                    result.Add(item);
                }

                return result.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
        /// <summary>
        /// 将指定的对象进行编码处理
        /// </summary>
        /// <typeparam name="T">泛型参数.要编码的数据类型</typeparam>
        /// <param name="obj">要编码的数据</param>
        /// <returns>在底层设备中该对象的二进制表现形式,一个 byte 数组.</returns>
        public byte[] Encode<T>(T obj)
        {
            try
            {
                if (_collectionSettings == null)
                {
                    throw new Exception("未初始化对象");
                }

                List<byte> result = new List<byte>();
                if (typeof(T) == typeof(Dictionary<Type, NetTransferObject[]>))
                {
                    foreach (var row in (obj as Dictionary<Type, NetTransferObject[]>))
                    {
                        foreach (var item in row.Value)
                        {
                            result.AddRange(Encode(item));
                        }
                    }
                }
                else
                {
                    Type type = typeof(T);
                    if (obj != null)
                    {
                        type = obj.GetType();
                    }
                    _CollectionSetting mapper = _collectionSettings.SingleOrDefault(x => x.type == type);
                    if (mapper == null)
                    {
                        throw new Exception(String.Format("{0} 未为类型 {1} 配置结构信息", this, type));
                    }

                    PropertyInfo[] proInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    if (proInfos == null || proInfos.Length == 0)
                    {
                        throw new Exception(String.Format("类型 {0} 不包含任何属性定义", type));
                    }
                    foreach (var property in mapper.propertys)
                    {
                        var properties = proInfos.Where(x => string.Equals(x.Name, property.name, StringComparison.CurrentCultureIgnoreCase));
                        if (properties.Count() != 1)
                        {
                            throw new Exception(String.Format("类型 {0} 属性 {1} 找到了 {2} 个映射", property.type, property.name, properties.Count()));
                        }

                        PropertyInfo propetryInfo = properties.Single();

                        object v = propetryInfo.GetValue(obj, null);

                        //特殊处理 bool 值
                        if (property.type == typeof(bool))
                        {
                            int byteIndex = (int)property.index;
                            int bitIndex = 0;
                            if (property.index.ToString().Split('.').Length > 1)
                            {
                                bitIndex = Convert.ToInt32(property.index.ToString().Split('.')[1].Substring(0, 1));
                            }

                            if (result.Count <= byteIndex)
                            {
                                for (int i = 0; i < property.size; i++)
                                {
                                    result.Add((byte)0);
                                }
                            }

                            BitArray bitArray = new BitArray(new byte[] { result[byteIndex] });
                            bitArray.Set(7 - bitIndex, Convert.ToBoolean(v));
                            result[byteIndex] = BitArray2Byte(bitArray);
                        }
                        else
                        {
                            byte[] bytes = convertNumberToBytes(property.type, v);
                            if (bytes.Length != property.size)
                            {
                                throw new Exception(String.Format("属性 {0} 值 {1} 在转换后长度为 {2}，但数据包内该属性长度为 {3}", property.name, v, bytes.Length, property.size));
                            }

                            result.AddRange(bytes.Reverse());
                        }
                    }

                    while (result.Count < mapper.blockBytes)
                    {
                        result.Add(0);
                    }
                }

                return result.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
        /// <summary>
        /// 将底层 byte 数组尝试解码为指定类型的对象
        /// </summary>
        /// <typeparam name="T">泛型参数.要转换的目标类型</typeparam>
        /// <param name="bytes">底层 byte 数组</param>
        /// <returns>
        /// 指定类型的状态数据 <br />
        /// 转换失败将引发一个 Exception 异常。
        /// </returns>
        /// <exception cref="T:Exception">目标类型未配置时，将引发此异常。</exception>
        public T Decode<T>(byte[] bytes)
            where T : NetTransferObject, new()
        {
            try
            {
                if (_collectionSettings == null)
                {
                    throw new Exception("未初始化对象");
                }

                Type type = typeof(T);
                _CollectionSetting mapper = null;
                if (_collectionSettings.Count(x => x.type == type) > 1)
                {
                    throw new Exception(String.Format("{0} 类型 {1} 存在多个配置结构信息", this, type));
                }
                mapper = _collectionSettings.SingleOrDefault(x => x.type == type);
                if (mapper == null)
                {
                    throw new Exception(String.Format("{0} 未为类型 {1} 配置结构信息", this, type));
                }

                PropertyInfo[] proInfos = null;
                if (_typePropertys.ContainsKey(type))
                {
                    proInfos = _typePropertys[type];
                }
                else
                {
                    proInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    _typePropertys.Add(type, proInfos);
                }

                if (proInfos == null || proInfos.Length == 0)
                {
                    throw new Exception(String.Format("类型 {0} 不包含任何属性定义", type));
                }

                T item = new T();

                foreach (var property in mapper.propertys)
                {
                    PropertyInfo propetryInfo = proInfos.Single(x => string.Equals(x.Name, property.name, StringComparison.CurrentCultureIgnoreCase));
                    if (propetryInfo == null)
                    {
                        continue;
                    }

                    object v;

                    if (property.index == (int)property.index && property.type != typeof(Boolean))
                    {
                        v = convertBytesToNumberValue(property.type, bytes.Skip((int)property.index).Take(property.size).ToArray());
                    }
                    else
                    {
                        int byteIndex = (int)property.index;
                        int bitIndex = Convert.ToInt32(property.index.ToString("#.0").Split('.')[1]);
                        byte b = bytes[byteIndex];
                        v = convertBytesToBooleanValue(b, bitIndex);
                    }

                    item[property.name] = v;
                }

                return item;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        private object convertBytesToNumberValue(Type valueType, byte[] bytes)
        {
            try
            {
                if (valueType == typeof(Int16))
                {
                    return BitConverter.ToInt16(bytes.Reverse().ToArray(), 0);
                }

                if (valueType == typeof(Int32))
                {
                    return BitConverter.ToInt32(bytes.Reverse().ToArray(), 0);
                }

                if (valueType == typeof(Int64))
                {
                    return BitConverter.ToInt64(bytes.Reverse().ToArray(), 0);
                }

                if (valueType == typeof(UInt16))
                {
                    return BitConverter.ToUInt16(bytes.Reverse().ToArray(), 0);
                }

                if (valueType == typeof(UInt32))
                {
                    return BitConverter.ToUInt32(bytes.Reverse().ToArray(), 0);
                }

                if (valueType == typeof(UInt64))
                {
                    return BitConverter.ToUInt64(bytes.Reverse().ToArray(), 0);
                }

                if (valueType == typeof(Byte))
                {
                    return bytes[0];
                }
            }
            catch (Exception)
            {
                throw;
            }

            throw new NotSupportedException(String.Format("不支持将 byte[] 转换为 {0}", valueType));
        }
        private bool convertBytesToBooleanValue(byte b, int bitIndex)
        {
            if (bitIndex < 0 || bitIndex > 7)
            {
                throw new ArgumentOutOfRangeException("bitIndex:大于等于 0,小于等于 7");
            }
            //var shifted = b >> (7 - bitIndex);
            var shifted = b >> bitIndex;

            return shifted % 2 != 0;
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

            if (valueType == typeof(Byte))
            {
                return new Byte[] { Convert.ToByte(value) };
            }

            throw new NotSupportedException(String.Format("不支持将 {0} 转换为 byte[]", valueType));
        }

        /// <summary>
        /// 解码整个数据包的数据块部分
        /// </summary>
        /// <param name="receivedDataPart"></param>
        /// <returns></returns>
        Dictionary<Type, MethodInfo> setMethodInfos = new Dictionary<Type, MethodInfo>();
        public Dictionary<Type, NetTransferObject[]> DecodeAll(byte[] receivedDataPart)
        {
            Dictionary<Type, NetTransferObject[]> result = new Dictionary<Type, NetTransferObject[]>();
            foreach (var _collectionSetting in _collectionSettings)
            {
                MethodInfo mi = null;
                if (setMethodInfos.ContainsKey(_collectionSetting.type))
                {
                    mi = setMethodInfos[_collectionSetting.type];
                }
                else
                {
                    mi = this.GetType().GetMethod("Get");
                    mi = mi.MakeGenericMethod(_collectionSetting.type);
                    setMethodInfos.Add(_collectionSetting.type, mi);
                }

                NetTransferObject[] items = (NetTransferObject[])mi.Invoke(this, new object[] { receivedDataPart });

                result.Add(_collectionSetting.type, items);
            }
            return result;
        }

        /// <summary>
        /// 数据块的总大小(相当于DB1和DB2的数据区字节总数)
        /// </summary>
        public Int32 TotalBytes { get; private set; }

        public class _CollectionSetting
        {
            public Type type { get; set; }
            public int blockBytes { get; set; }
            public int itemCount { get; set; }
            public _PropertySetting[] propertys { get; set; }
        }

        public class _PropertySetting
        {
            public String name { get; set; }
            public Decimal index { get; set; }
            public Int32 size { get; set; }
            public Type type { get; set; }
        }

        public Dictionary<Type, NetTransferObject[]> CreateFullMap()
        {
            Dictionary<Type, NetTransferObject[]> result = new Dictionary<Type, NetTransferObject[]>();
            foreach (var item in _collectionSettings)
            {
                result.Add(item.type, new NetTransferObject[item.itemCount]);
                for (int i = 0; i < item.itemCount; i++)
                {
                    result[item.type][i] = (NetTransferObject)item.type.Assembly.CreateInstance(item.type.FullName, false, BindingFlags.CreateInstance, null, null, null, null);
                }
            }
            return result;
        }

        /// <summary>
        /// 将BitArray转换为一个十进制整数
        /// </summary>
        /// <param name="bitArray">下表从低到高的顺序 与十进制整数的二进制形式从低到高的顺序一致</param>
        /// <returns></returns>
        byte BitArray2Byte(BitArray bitArray)
        {
            if (bitArray.Length != 8)
            {
                throw new ArgumentOutOfRangeException("bitArray 长度必须为 8");
            }
            byte ret = 0;
            for (byte i = 0; i < bitArray.Length; i++)
            {
                if (bitArray.Get(i))
                {
                    ret |= (byte)(1 << i);
                }
            }
            return ret;
        }
    }
}
