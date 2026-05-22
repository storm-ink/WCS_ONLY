using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 序列在空闲时执行的额外动作配置节点
    /// </summary>
    /// <remarks>
    /// 属性 type:指示该配置映射到的具体实现。必须为实现了接口 <see cref="T:Wcs.Framework.IEquipmentActionSequenceAdditionalTask"/> 的类型。典型的应用场景可参考堆垛机入库时的提前待命动作 <see cref="T:Wcs.Framework.Impl.DefaultCraneBeforehandToStandby"/> 实现。
    /// </remarks>
    /// <example>
    /// <code lang="xml">
    /// <sequence><additionalTasks><task type="Wcs.Framework.Impl.DefaultCraneBeforehandToStandby, Wcs.Framework" /></additionalTasks></sequence>
    /// </code>
    /// </example>
    public class SequenceAdditionalTaskElement:ConfigurationElement
    {
        /// <summary>
        /// 获取此节点配置映射到的具体额外动作实现
        /// </summary>
        public IEquipmentActionSequenceAdditionalTask SequenceAdditionalTask { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public SequenceAdditionalTaskElement(System.Xml.XmlNode node)
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
                throw new Exception("未指定 SequenceAdditionalTask 节点的 type 属性");
            }

            string typeName = node.Attributes["type"].Value;
            IEquipmentActionSequenceAdditionalTask additionalTask = CreateInstance<IEquipmentActionSequenceAdditionalTask>(typeName);
            this.SequenceAdditionalTask = additionalTask;
        }
    }
}
