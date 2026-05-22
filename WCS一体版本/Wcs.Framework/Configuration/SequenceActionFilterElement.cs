using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 序列动作过滤器配置节点
    /// </summary>
    /// <remarks>
    /// 属性 type:指示该配置映射到的具体实现。必须为 <see cref="T:Wcs.Framework.EquipmentSequenceActionFilter"/> 的子类。
    /// </remarks>
    /// <example>
    /// <code lang="xml">
    /// <sequence><actionFilters><action type="Wcs.Framework.Impl.DefaultCraneUnloadActionFilter, Wcs.Framework" /></actionFilters></sequence>
    /// </code>
    /// </example>
    public class SequenceActionFilterElement:ConfigurationElement
    {
        /// <summary>
        /// 获取此节点配置映射到的具体过滤器实现。
        /// </summary>
        public EquipmentSequenceActionFilter Filter { get; private set; }
        public SequenceActionFilterElement(XmlNode node)
            : base(node)
        {

        }
        /// <summary>
        /// 执行解析
        /// </summary>
        /// <param name="node">配置节点</param>
        protected override void Deserialize(System.Xml.XmlNode node)
        {
            if (node.Attributes["type"] == null)
            {
                throw new Exception("未指定 SequenceActionFilter 节点的 type 属性");
            }

            string typeName = node.Attributes["type"].Value;
            EquipmentSequenceActionFilter filter = CreateInstance<EquipmentSequenceActionFilter>(typeName, node);
            this.Filter = filter;
        }
    }
}
