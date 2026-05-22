using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 逻辑动作配置节点
    /// </summary>
    /// <remarks>
    /// <b>属性</b><br />
    /// type:子符串，必填。指示该配置映射到的具体实现。必须为 <see cref="T:Wcs.Framework.LogicMovements.LogicMovement"/> 的子类。框架提供了 <see cref="T:Wcs.Framework.LogicMovements.ConveyorTransferMovement"/>、<see cref="T:Wcs.Framework.LogicMovements.CraneAutomaticTransferMovement"/>、<see cref="T:Wcs.Framework.LogicMovements.CraneHalfAutomaticTransferMovement"/> 三个默认的逻辑动作实现，它们能满足绝大多数的应用场景。
    /// </remarks>
    /// <example>
    /// <code lang="xml">
    /// <logicMovementTypes><logicMovement type="Wcs.Framework.LogicMovements.CraneAutomaticTransferMovement, Wcs.Framework" /></logicMovementTypes>
    /// </code>
    /// </example>
    public class LogicMovementTypeElement:ConfigurationElement
    {
        /// <summary>
        /// 获取该配置节点映射的逻辑动作类型
        /// </summary>
        public Type LogicMovementType { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public LogicMovementTypeElement(XmlNode node)
            : base(node)
        {

        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="typeName">逻辑动作类型名称</param>
        /// <exception cref="T:System.Exception">未找到 typeName 中指定的类型</exception>
        public LogicMovementTypeElement(String typeName)
            : base(null)
        {
            Type logicMovementType = Type.GetType(typeName);
            if (logicMovementType == null)
            {
                throw new Exception(string.Format("未找到类型 {0}", typeName));
            }
            LogicMovementType = logicMovementType;
        }
        /// <summary>
        /// 执行解析
        /// </summary>
        /// <param name="node">配置节点</param>
        protected override void Deserialize(System.Xml.XmlNode node)
        {
            if (node==null)
            {
                return;
            }

            if (node.Attributes["type"] == null)
            {
                throw new Exception("未指定 logicMovement 节点的 type 属性");
            }

            Type logicMovementType = Type.GetType(node.Attributes["type"].Value);
            if (logicMovementType == null)
            {
                throw new Exception(string.Format("未找到类型 {0}", node.Attributes["type"].Value));
            }
            LogicMovementType = logicMovementType;
        }
    }
}
