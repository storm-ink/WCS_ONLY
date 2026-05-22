using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;
using Wcs.Framework.Devices;
using System.Collections;

namespace Wcs.Framework.Impl
{
    /// <summary>
    /// 默认的输送线网络数据包解码器
    /// </summary>
    /// <example>
    /// <h1>如何配置一个新的输送线网络数据包解码器（以 DB1 为例）</h1>
    /// <ul>
    /// <li>
    /// 1、首先我们需要找到项目电气负责人，拿到 db1  的最新结构，如下图所示：<br />
    /// <img src="../helpDoc/images/db1.jpg" />
    /// </li>
    /// <li>
    /// 2、在 wcs.config.xml 中添加一个 netPackageDecoder 节点，路径是 /configuration/netPackageDecoders，如果 netPackageDecoders 不存在，添加一个即可。如下所示：
    ///<code lang="xml">
    ///	<configuration>
    ///	
    ///	    <netPackageDecoders>
    ///	    
    ///	        <netPackageDecoder />
    ///	        
    ///	    </netPackageDecoders>
    ///</configuration>
    /// </code>
    /// </li>
    /// <li>
    /// 3、定义 netPackageDecoder 节点明细，此处我们先对 netPackageDecoder 节点的配置进行说明：
    /// <para>
    /// netPackageDecoder 节点包含两个属性： name、type。<br />
    /// name 属性用于外部用引该对象的主键，在系统内必须唯一。<br />
    /// type 指定该节点最后转换为的对象类型。<br />
    /// 此处我们将 name 属性赋值为 db2，type 属性赋值为 Wcs.Framework.Impl.DefaultConveyorNetPackageDecoder, Wcs.Framework(表示位于 Wcs.Framework 程序集中的 Wcs.Framework.Impl.DefaultConveyorNetPackageDecoder 类型)<br />
    /// <see cref="T:Wcs.Framework.Impl.DefaultConveyorNetPackageDecoder"/> 是框架提供的一个默认的输送线网络数据编码、解码器，默认我们可以直接使用此对象。<br />
    /// 修改后的配置如下：<br />
    /// </para>
    /// <code lang="xml">
    ///  <configuration>
    ///	
    ///	    <netPackageDecoders>
    ///
    ///	        <netPackageDecoder name="db1" type="Wcs.Framework.Impl.DefaultConveyorNetPackageDecoder, Wcs.Framework"/>
    ///	        
    ///	    </netPackageDecoders>
    ///</configuration>
    /// </code>
    /// <para>
    /// 如果 type 属性中指定的类型不存在，将引发异常。
    /// </para>
    /// </li>
    /// <li>
    /// 4、根据上图所示， db1 由 Task,UnitOccupy,Clear_DB1002,DBSC,Scaner,Light 几个小块组成，其中：<br />
    /// Task,UnitOccupy,Clear_DB1002,Scaner 都为单组数据，DBSC 为 6 组，Light 为 16 组，我们需要根据它们在 db1 中的分布顺序配置刚才添加的名为 db1 的 netPackageDecoder 节点,<br />
    /// 在 netPackageDecoder 节点中添加 collection 子节点，collection 就是数据小块的在 Wcs 系统中存在的形式。<br />
    /// 从 db1 中每个数据小块 Task 开始，首先我们在 netPackageDecoder 中添加一个 collection 节点，从项目电气负责人提供的数据结果中找到 db1 中对应 Task 数据块的结构，配置这个 collection 节点。<br />
    /// 示例的 Task 数据块结构如下图所示<br />
    /// <img src="../helpDoc/images/写_主任务(db1000).jpg" /><br />
    /// 具体配置如下所示：
    /// <code lang="xml">
    ///  <configuration>
    ///	
    ///	    <netPackageDecoders>
    ///
    ///	        <netPackageDecoder name="db1" type="Wcs.Framework.Impl.DefaultConveyorNetPackageDecoder, Wcs.Framework">
    ///          <collection type="Wcs.Framework.Devices.SendTaskBlock, Wcs.Framework" blockBytes="24" itemCount="1">
    ///            <property name="HandShake" index="0" size="2" type="UInt16" />
    ///            <property name="AssignmentID" index="2" size="4" type="UInt32" />
    ///            <property name="TU_ID" index="6" size="2" type="UInt16" />
    ///            <property name="TU_Type" index="8" size="2" type="UInt16" />
    ///            <property name="IO_Data" index="10" size="2" type="UInt16" />
    ///            <property name="RotingNo" index="12" size="2" type="UInt16" />
    ///            <property name="StartMotorNo" index="14" size="2" type="UInt16" />
    ///            <property name="DestinationNo" index="16" size="2" type="UInt16" />
    ///            <property name="Index" index="18" size="2" type="UInt16" />
    ///            <property name="Spare" index="20" size="4" type="UInt32" />
    ///          </collection>
    ///	        </netPackageDecoder>
    ///	        
    ///	    </netPackageDecoders>
    ///</configuration>
    /// </code>
    /// </li>
    /// <li>
    /// 5、关于 collection 节点的说明：<br />
    /// type：状态数据在 Wcs 系统中的映射对象类型，使用 "完整类名, 程序集" 格式填写;构架中提供了输送线的一些基本数据类型，如：<see cref="T:Wcs.Framework.Devices.OccupiedSignal" />、<see cref="T:Wcs.Framework.Devices.OccupyStatus" />、<see cref="T:Wcs.Framework.Devices.SendClearLocationCurrentTask" />、<see cref="T:Wcs.Framework.Devices.SendOccupiedSignal" />、<see cref="T:Wcs.Framework.Devices.SendRgvTaskBlock" />、<see cref="T:Wcs.Framework.Devices.SendTaskBlock" />、<see cref="T:Wcs.Framework.Devices.TaskBlock" />、<see cref="T:Wcs.Framework.Devices.ConveyorLocationState" />。<br />
    /// 此处并不局限于构架中提供的输送线基本数据类型，可以根据项目需求，创建新的类型，然后在此处映射即可。本示例中就用到了两个新的类型：Wcs.JNMeide.tmsm、Wcs.JNMeide.DB1.Light。它们都存在于 Wcs.JNMeide 应用程序集中，为根据项目需求添加而得。<br />
    /// blockBytes:此对象在输送线设备中定义时所占用的总大小，以 byte 为单位;<br />
    /// itemCount:在 db 块中，数据小块一共包含几个此对象;<br />
    /// <para>
    /// 关于 collection 节点下 property 子节点的说明：<br />
    /// name：映射的类型对应的属性名;<br />
    /// type：属性的类型。以下是输送线设备中的类型和 Wcs 系统类型的对应关系：<br />
    /// 输送线设备       Wcs系统<br />
    /// INT             UInt16<br />
    /// DINT            UInt32<br />
    /// BOOL            Boolean<br />
    /// BYTE            Byte<br />
    /// </para>
    /// <para>根据以上介绍填写剩下的数据小块配置，最后得到的 db1 完整的配置如下：</para>
    /// <code lang="xml">
    ///  <configuration>
    ///	
    ///	    <netPackageDecoders>
    ///
    ///	        <netPackageDecoder name="db1" type="Wcs.Framework.Impl.DefaultConveyorNetPackageDecoder, Wcs.Framework">
    ///         <collection type="Wcs.Framework.Devices.SendTaskBlock, Wcs.Framework" blockBytes="24" itemCount="1">
    ///           <property name="HandShake" index="0" size="2" type="UInt16" />
    ///           <property name="AssignmentID" index="2" size="4" type="UInt32" />
    ///           <property name="TU_ID" index="6" size="2" type="UInt16" />
    ///           <property name="TU_Type" index="8" size="2" type="UInt16" />
    ///           <property name="IO_Data" index="10" size="2" type="UInt16" />
    ///           <property name="RotingNo" index="12" size="2" type="UInt16" />
    ///           <property name="StartMotorNo" index="14" size="2" type="UInt16" />
    ///           <property name="DestinationNo" index="16" size="2" type="UInt16" />
    ///           <property name="Index" index="18" size="2" type="UInt16" />
    ///           <property name="Spare" index="20" size="4" type="UInt32" />
    ///         </collection>
    ///         <collection type="Wcs.Framework.Devices.SendOccupiedSignal, Wcs.Framework" blockBytes="16" itemCount="1">
    ///           <property name="PosNo" index="0" size="2" type="UInt16" />
    ///           <property name="HandShake" index="2" size="2" type="UInt16" />
    ///           <property name="AssignmentID" index="4" size="4" type="UInt32" />
    ///           <property name="TU_ID" index="8" size="2" type="UInt16" />
    ///           <property name="TU_Type" index="10" size="2" type="UInt16" />
    ///           <property name="IO_Data" index="12" size="2" type="UInt16" />
    ///           <property name="Index" index="14" size="2" type="UInt16" />
    ///         </collection>
    ///         <collection type="Wcs.Framework.Devices.SendClearLocationCurrentTask, Wcs.Framework" blockBytes="10" itemCount="1">
    ///           <property name="PosNo" index="0" size="2" type="UInt16" />
    ///           <property name="TaskNo" index="2" size="4" type="UInt32" />
    ///           <property name="TUID" index="6" size="2" type="UInt16" />
    ///           <property name="Index" index="8" size="2" type="UInt16" />
    ///         </collection>
    ///         <collection type="Wcs.Framework.Devices.SendRgvTaskBlock, Wcs.Framework" blockBytes="34" itemCount="6">
    ///           <property name="SC_No" index="0" size="2" type="UInt16" />
    ///           <property name="Chain1_TaskNo" index="2" size="4" type="UInt32" />
    ///           <property name="FromStation1" index="6" size="2" type="UInt16" />
    ///           <property name="ToStation1" index="8" size="2" type="UInt16" />
    ///           <property name="Chain1_FirstAction" index="10" size="2" type="UInt16" />
    ///           <property name="Chain1_SecondAction" index="12" size="2" type="UInt16" />
    ///           <property name="Chain2_TaskNo" index="14" size="4" type="UInt32" />
    ///           <property name="FromStation2" index="18" size="2" type="UInt16" />
    ///           <property name="ToStation2" index="20" size="2" type="UInt16" />
    ///           <property name="Chain2_FirstAction" index="22" size="2" type="UInt16" />
    ///           <property name="Chain2_SecondAction" index="24" size="2" type="UInt16" />
    ///           <property name="Command" index="26" size="2" type="UInt16" />
    ///           <property name="ClearTaskCommand" index="28" size="2" type="UInt16" />
    ///           <property name="Received" index="30" size="4" type="UInt32" />
    ///         </collection>
    ///          <collection type="Wcs.JNMeide.tmsm, Wcs.JNMeide" blockBytes="6" itemCount="1">
    ///           <property name="HandShake" index="0" size="2" type="UInt16" /> 
    ///           <property name="ScanResult" index="2" size="2" type="UInt16" /> 
    ///           <property name="DB803_Index" index="4" size="2" type="UInt16" /> 
    ///           </collection>
    ///          <collection type="Wcs.JNMeide.DB1.Light, Wcs.JNMeide" blockBytes="1" itemCount="16">
    ///           <property name="lbl" index="0" size="1" type="Byte" /> 
    ///           </collection>
    ///	        </netPackageDecoder>
    ///	        
    ///	    </netPackageDecoders>
    ///</configuration>
    /// </code>
    /// </li>
    /// </ul>
    /// <h4 color="red">关于简单的验证配置是否正确的方法,我们可以使用公式：通讯 db 块的总 byte 数 - 20 byte = sum(blockBytes*itemCount)。</h4>
    /// </example>
    public class DefaultConveyorNetPackageDecoder : NetPackageDecoder
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
        public DefaultConveyorNetPackageDecoder(XmlNode configNode):base(configNode)
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
        public override T[] Get<T>(byte[] receivedBytes)
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
#warning 此处为兼容非标输送线设备无标准数据块时引起的异常
                    //throw new Exception(String.Format("{0} 未为类型 {1} 配置结构信息", this, typeof(T)));
                    return new T[] { };
                }

                int mapperIndex = _collectionSettings.ToList().IndexOf(mapper);
                int skipBytes = _collectionSettings
                    .Take(mapperIndex)
                    .Sum(x => x.blockBytes * x.itemCount);

                List<T> result = new List<T>();
                Type type = typeof(T);
                for (int i = skipBytes; i < skipBytes + mapper.blockBytes * mapper.itemCount; i += mapper.blockBytes)
                {
                    //byte[] propertyValueBytes = receivedBytes.Skip(i).Take(mapper.blockBytes).ToArray();
                    byte[] propertyValueBytes = new byte[mapper.blockBytes];
                    Array.Copy(receivedBytes, i, propertyValueBytes, 0, mapper.blockBytes);

                    //for (int index = 0; index < mapper.blockBytes; index++)
                    //{
                    //    propertyValueBytes[index] = receivedBytes[i + index];
                    //}
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
        public override byte[] Encode<T>(T obj)
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

                    PropertyInfo[] proInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance |BindingFlags.DeclaredOnly);
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
        public override T Decode<T>(byte[] bytes)
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
                        //byte[] valueBytes = new byte[property.size];
                        //Array.Copy(bytes, (int)property.index, valueBytes, 0, property.size);
                        //v = convertBytesToNumberValue(property.type, valueBytes);
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
                    //try
                    //{
                    //    propetryInfo.SetValue(item, v, null);
                    //}
                    //catch (Exception)
                    //{
                    //    throw;
                    //}
                }

                return item;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        private object convertBytesToNumberValue(Type valueType,byte[] bytes)
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
        Dictionary<Type,MethodInfo> setMethodInfos = new Dictionary<Type,MethodInfo>();
        public Dictionary<Type, NetTransferObject[]> DecodePackage(byte[] receivedDataPart)
        {
#if DEBUG
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif
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
#if DEBUG
            sw.Stop();
            Console.WriteLine("decodePackage {0} bytes used {1}", receivedDataPart.Length, sw.ElapsedMilliseconds);
#endif
            return result;
        }

        public class _CollectionSetting
        {
            public Type type{get;set;}
            public int blockBytes{get;set;}
            public int itemCount{get;set;}
            public _PropertySetting[] propertys { get; set; }
        }

        public class _PropertySetting
        {
            public String name { get; set; }
            public Decimal index { get; set; }
            public Int32 size { get; set; }
            public Type type { get; set; }
        }

        public override Dictionary<Type, NetTransferObject[]> CreateFullMap()
        {
            Dictionary<Type, NetTransferObject[]> result = new Dictionary<Type, NetTransferObject[]>();
            foreach (var item in _collectionSettings)
            {
                result.Add(item.type, new NetTransferObject[item.itemCount]);
                for (int i = 0; i < item.itemCount; i++)
                {
                    result[item.type][i] =(NetTransferObject)item.type.Assembly.CreateInstance(item.type.FullName, false, BindingFlags.CreateInstance, null, null, null, null);
                }
            }
            return result;
        }

        //
        // 摘要：
        //     将BitArray转换为一个十进制整数。
        // 参数：
        //     ba, 下表从低到高的顺序 与十进制整数的二进制形式从低到高的顺序一致
        static byte BitArray2Byte(BitArray bitArray)
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
