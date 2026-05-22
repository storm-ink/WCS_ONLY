using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    public sealed class LogicMovementElement:ConfigurationElement
    {
        public Type Type { get; private set; }
        public LogicMovementElement(XmlNode node, WcsConfiguration configuration)
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
                throw new System.Configuration.ConfigurationErrorsException(string.Format("未找到 {0} 节点 {1} 属性值 “{2}” 标识的类型", path, "logicMovement", typeName));
            }

            if (!type.IsSubclassOf(typeof(LogicMovement)))
            {
                var path = this.Node.GetXPath();
                throw new System.Configuration.ConfigurationErrorsException(string.Format("{0} 节点 {1} 属性值 “{2}” 标识的类型未实现 {3} 接口", path, "logicMovement", typeName, typeof(LogicMovement)));
            }

            this.Type = type;
        }
    }
}
