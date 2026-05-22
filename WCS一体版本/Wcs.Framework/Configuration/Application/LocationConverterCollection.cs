using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 位置转换器集合节点
    /// </summary>
    public class LocationConverterCollection:ConfigurationElement
    {
        /// <summary>
        /// 获取所有位置转换器节点
        /// </summary>
        public LocationConverterElement[] ConverterElements { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public LocationConverterCollection(XmlNode node, WcsConfiguration configuration)
            : base(node,configuration) { }
        /// <summary>
        /// 执行解析
        /// </summary>
        /// <param name="node">配置节点</param>
        protected override void Deserialize()
        {
            List<LocationConverterElement> converterElements = new List<LocationConverterElement>();
            foreach (XmlNode element in this.Node.SelectNodes("converter"))
            {
                converterElements.Add(new LocationConverterElement(element, this.WcsConfiguration));
            }
            this.ConverterElements = converterElements.ToArray();
        }
    }
}
