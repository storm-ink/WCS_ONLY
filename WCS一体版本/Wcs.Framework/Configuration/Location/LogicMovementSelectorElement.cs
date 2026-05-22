using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    public sealed class LogicMovementSelectorElement:ConfigurationElement
    {
        public LogicMovementSelector LogicMovementSelector { get; private set; }
        public LogicMovementElement[] LogicMovementElement { get; private set; }
        public LogicMovementSelectorElement(XmlNode node, WcsConfiguration configuration)
            : base(node,configuration, true)
        {

        }
        protected override void Deserialize()
        {
            String typeName = GetAttribute<String>("type");
            Type type = Type.GetType(typeName);

            if (type == null)
            {
                var path = this.Node.GetXPath();
                throw new System.Configuration.ConfigurationErrorsException(string.Format("未找到 {0} 节点 {1} 属性值 “{2}” 标识的类型", path, "type", typeName));
            }

            if (!type.IsSubclassOf(typeof(LogicMovementSelector)))
            {
                var path = this.Node.GetXPath();
                throw new System.Configuration.ConfigurationErrorsException(string.Format("{0} 节点 {1} 属性值 “{2}” 标识的类型不是 {3} 的子类", path, "type", typeName, typeof(LogicMovementSelector)));
            }

            this.LogicMovementSelector = ReflectionHelper.CreateInstance<LogicMovementSelector>(type);

            List<LogicMovementElement> logicMovementElements = new List<LogicMovementElement>();
            foreach (XmlNode filterNode in Node.SelectNodes("logicMovement"))
            {
                logicMovementElements.Add(new LogicMovementElement(filterNode, this.WcsConfiguration));
            }
            this.LogicMovementElement = logicMovementElements.ToArray();
        }
    }
}
