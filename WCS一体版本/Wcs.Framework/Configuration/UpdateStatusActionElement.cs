using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 状态更新时附加的额外动作节点
    /// </summary>
    /// <remarks>
    /// 该节点只有一个属性 type：表示该对动作对应的实现类型（继承自 <see cref="T:Wcs.Framework.UpdateStatusAction"/>）
    /// </remarks>
    /// <example>
    /// <code lang="xml">
    /// <updateStatusActions><action type="Wcs.Framework.Impl.DefaultSetCraneWorkModeKeyInfoAction, Wcs.Framework" /></updateStatusActions>
    /// </code>
    /// </example>
    /// <seealso cref="T:Wcs.Framework.Cfg.UpdateStatusActionsSelection"/>
    public class UpdateStatusActionElement:ConfigurationElement
    {
        /// <summary>
        /// 获取该配置节点映射到了具体动作
        /// </summary>
        public UpdateStatusAction UpdateStatusAction { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public UpdateStatusActionElement(XmlNode node):base(node)
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
                throw new NotFoundConfigElementAttributeException("type", node);
            }
            UpdateStatusAction = CreateInstance<UpdateStatusAction>(node.Attributes["type"].Value);
        }
    }
}
