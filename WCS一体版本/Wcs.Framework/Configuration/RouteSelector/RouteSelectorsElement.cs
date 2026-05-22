using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    public class RouteSelectorsElement:ConfigurationElement
    {
        public RouteSelector[] RouteSelectors;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public RouteSelectorsElement(XmlNode node, WcsConfiguration configuration)
            : base(node,configuration, false)
        {
            Deserialize();
        }

        protected override void Deserialize()
        {
            List<RouteSelector> handlers = new List<RouteSelector>();
            foreach (XmlNode handerNode in this.Node.SelectNodes("handler"))
            {
                String typeName = GetAttribute<String>("type", handerNode);
                if (String.IsNullOrWhiteSpace(typeName))
                {
                    throw new ConfigurationErrorsException(String.Format("{0} 节点的 {1} 属性值不能为空", handerNode.GetXPath(), "type"));
                }

                string[] taskTypes = GetAttribute<String>("taskType", handerNode).Split(',').Select(x => x.Trim()).ToArray();
                var handler = ReflectionHelper.CreateInstance<RouteSelector>(typeName);
                handler.TaskTypes = taskTypes;

                handlers.Add(handler);
            }

            RouteSelectors = handlers.ToArray();
        }
    }
}
