using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 位置转换器节点
    /// </summary>
    public class LocationConverterElement:ConfigurationElement
    {
        /// <summary>
        /// 获取该配置映射到的位置转换器实例
        /// </summary>
        public AbstractLocationConverter Converter { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public LocationConverterElement(System.Xml.XmlNode node, WcsConfiguration configuration)
            : base(node,configuration)
        {
        }
        /// <summary>
        /// 执行解析
        /// </summary>
        /// <param name="node">配置节点</param>
        protected override void Deserialize()
        {
            string typeName = GetAttribute<String>("type");
            string deviceName = GetAttribute<String>("deviceName");
            this.Converter = ReflectionHelper.CreateInstance<AbstractLocationConverter>(typeName, deviceName);

            LocationConverter.AddConverter(this.Converter);

            WcsConfiguration._logger.Debug1("添加了设备名称为 {0}，类型为 {1} 的位置转换器", this.Converter.DeviceName, this.Converter.GetType());
        }
    }
}
