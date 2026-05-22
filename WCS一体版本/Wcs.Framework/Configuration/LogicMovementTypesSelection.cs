using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework.Devices;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 路径映射到的逻辑动作集合配置节点
    /// </summary>
    /// <example>
    /// <code lang="xml">
    /// <logicMovementTypes></logicMovementTypes>
    /// </code>
    /// </example>
    public class LogicMovementTypesSelection:ConfigurationElement
    {
        List<LogicMovementTypeElement> logicMovementTypeElements = new List<LogicMovementTypeElement>();
        /// <summary>
        /// 获取和此配置节点关联的路径实例
        /// </summary>
        DeviceRoute Route { get; set; }
        /// <summary>
        /// 获取逻辑动作配置节点集合
        /// </summary>
        public LogicMovementTypeElement[] LogicMovementTypeElements
        {
            get
            {
                return logicMovementTypeElements.ToArray();
            }
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        /// <param name="route">关联的路径实例</param>
        public LogicMovementTypesSelection(XmlNode node,DeviceRoute route)
            : base(node,false)
        {
            Route = route;
            this.Deserialize(node);
        }
        /// <summary>
        /// 向该映射集合添加一个逻辑动作类型
        /// </summary>
        /// <param name="typeName">逻辑动作类型名称</param>
        public void AddLogicMovementType(string typeName)
        {
            logicMovementTypeElements.Add(new LogicMovementTypeElement(typeName));

            Route.LogicMovementTypes = logicMovementTypeElements.Select(x => x.LogicMovementType).ToArray();
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

            foreach (XmlNode item in node.SelectNodes("logicMovement"))
            {
                var element = new LogicMovementTypeElement(item);
                logicMovementTypeElements.Add(element);
            }

            Route.LogicMovementTypes = logicMovementTypeElements.Select(x => x.LogicMovementType).ToArray();
        }
    }
}
