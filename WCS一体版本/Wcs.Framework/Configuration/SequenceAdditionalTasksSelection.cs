using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 序列在空闲时的额外动作集合配置节点
    /// </summary>
    /// <example>
    /// <code lang="xml">
    /// <sequence><additionalTasks></additionalTasks></sequence>
    /// </code>
    /// <seealso cref="T:Wcs.Framework.Cfg.SequenceAdditionalTaskElement"/>
    /// </example>
    public class SequenceAdditionalTasksSelection:ConfigurationElement
    {
        public List<SequenceAdditionalTaskElement> _sequenceAdditionalTasks = new List<SequenceAdditionalTaskElement>();
        /// <summary>
        /// 获取额外动作配置节点集合
        /// </summary>
        public SequenceAdditionalTaskElement[] SequenceAdditionalTasks
        {
            get
            {
                return _sequenceAdditionalTasks.ToArray();
            }
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public SequenceAdditionalTasksSelection(XmlNode node)
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
            foreach (XmlNode item in node.SelectNodes("task"))
            {
                _sequenceAdditionalTasks.Add(new SequenceAdditionalTaskElement(item));
            }
        }
    }
}
