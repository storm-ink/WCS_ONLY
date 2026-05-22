using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 逻辑动作选择器配置节点
    /// </summary>
    /// <remarks>
    /// <b>属性</b><br />
    /// type:字符串，必填。指示该配置映射到的具体实现。必须为实现了 <see cref="T:Wcs.Framework.Devices.IDeviceRouteToLogicMovementSelector"/> 接口的类型。框架提供了一个默认实现 <see cref="T:Wcs.Framework.LogicMovements.DefaultDeviceRouteToLogicMovementSelector"/>。
    /// </remarks>
    /// <example>
    /// <code lang="xml">
    /// <route><logicMovementSelector type="Wcs.Framework.Devices.IDeviceRouteToLogicMovementSelector, Wcs.Framework" /></route>
    /// </code>
    /// </example>
    public class LogicMovementSelectorElement : ConfigurationElement
    {
        public Devices.IDeviceRouteToLogicMovementSelector LogicMovementSelector { get; private set; }
        public LogicMovementSelectorElement(XmlNode node)
            : base(node)
        {

        }

        protected override void Deserialize(System.Xml.XmlNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            if (node.Attributes["type"] == null)
            {
                throw new Exception("未指定 logicMovementSelector 节点的 type 属性");
            }

            Type logicMovementType = Type.GetType(node.Attributes["type"].Value);

            this.LogicMovementSelector = CreateInstance<Devices.IDeviceRouteToLogicMovementSelector>(node.Attributes["type"].Value);
        }
    }
}
