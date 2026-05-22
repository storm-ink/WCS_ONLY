using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 序列动作过滤器集合配置节点
    /// </summary>
    /// <remarks>
    /// 子节点：<see cref="T:Wcs.Framework.Cfg.SequenceActionFilterElement"/>
    /// </remarks>
    /// <example>
    /// <code lang="xml">
    /// <sequence><actionFilters></actionFilters></sequence>
    /// </code>
    /// </example>
    public class SequenceActionFilterSelection:ConfigurationElement
    {
        List<SequenceActionFilterElement> sequenceActionFilterElements = new List<SequenceActionFilterElement>();
        /// <summary>
        /// 获取序列动作过滤器配置节点集合
        /// </summary>
        public SequenceActionFilterElement[] SequenceActionFilterElements
        {
            get
            {
                return sequenceActionFilterElements.ToArray();
            }
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public SequenceActionFilterSelection(XmlNode node)
            : base(node)
        {

        }
        /// <summary>
        /// 执行解析
        /// </summary>
        /// <param name="node">配置节点</param>
        protected override void Deserialize(System.Xml.XmlNode node)
        {
            if (node == null)
            {
                return;
            }
            foreach (XmlNode item in node.SelectNodes("filter"))
            {
                SequenceActionFilterElement element = new SequenceActionFilterElement(item);
                sequenceActionFilterElements.Add(element);
            }
        }
    }
}
