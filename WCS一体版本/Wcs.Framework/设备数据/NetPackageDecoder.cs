using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 网络数据包解码器
    /// </summary>
    public abstract class NetPackageDecoder
    {
        /// <summary>
        /// 解码器名称，用作引用的依据
        /// </summary>
        public String Name { get; set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="configNode">配置节点，某此时候相应的解码器可能需要附加很多复杂的配置。此时您可以将其传入对象，在内部解析</param>
        public NetPackageDecoder(XmlNode configNode)
        {
        }
        /// <summary>
        /// 从指定的数据包中获取指定类型的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="receivedBytes"></param>
        /// <returns></returns>
        public abstract T[] Get<T>(byte[] receivedBytes) where T : NetTransferObject,new();
        /// <summary>
        /// 将指定的对象转换为数据包中数据格式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public abstract byte[] Encode<T>(T obj);// where T : NetTransferObject;

        public abstract T Decode<T>(byte[] bytes) where T : NetTransferObject, new();
        /// <summary>
        /// 创建一个新的完整的网络数据结构
        /// </summary>
        /// <returns></returns>
        public abstract Dictionary<Type, NetTransferObject[]> CreateFullMap();
    }
}
