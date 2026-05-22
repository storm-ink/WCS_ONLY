using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    public sealed class RouteStrategyElement:ConfigurationElement
    {
        public RouteStrategy RouteStrategy { get; private set; }
        public RouteStrategyElement(XmlNode node, WcsConfiguration configuration)
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

            if (!type.IsSubclassOf(typeof(RouteStrategy)))
            {
                var path = this.Node.GetXPath();
                throw new System.Configuration.ConfigurationErrorsException(string.Format("{0} 节点 {1} 属性值 “{2}” 标识的类型未实现 {3} 接口", path, "type", typeName, typeof(RouteStrategy)));
            }

            this.RouteStrategy = ReflectionHelper.CreateInstance<RouteStrategy>(type);
        }
    }
}
