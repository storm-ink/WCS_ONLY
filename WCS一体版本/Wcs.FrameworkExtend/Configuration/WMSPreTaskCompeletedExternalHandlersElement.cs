using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework;

namespace Wcs.FrameworkExtend.Cfg
{
    public class WMSPreTaskCompeletedExternalHandlersElement : ConfigurationElement
    {
        public WMSPreTaskCompeletedExternalHandler[] WMSPreTaskCompeletedExternalHandlers;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public WMSPreTaskCompeletedExternalHandlersElement(XmlNode node, WcsConfiguration configuration)
            : base(node, configuration, false)
        {
            Deserialize();
        }

        protected override void Deserialize()
        {
            List<WMSPreTaskCompeletedExternalHandler> handlers = new List<WMSPreTaskCompeletedExternalHandler>();
            foreach (XmlNode handerNode in this.Node.SelectNodes("handler"))
            {
                String typeName = GetAttribute<String>("type", handerNode);
                if (String.IsNullOrWhiteSpace(typeName))
                {
                    throw new ConfigurationErrorsException(String.Format("{0} 节点的 {1} 属性值不能为空", handerNode.GetXPath(), "type"));
                }
                string[] taskTypes = GetAttributeOrDefault<String>("taskType", handerNode, "*").Split(',').Select(x => x.Trim()).ToArray();

                var handler = ReflectionHelper.CreateInstance<WMSPreTaskCompeletedExternalHandler>(typeName);
                handler.TaskTypes = taskTypes;
                handlers.Add(handler);
            }

            WMSPreTaskCompeletedExternalHandlers = handlers.ToArray();
        }
    }
}
