using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework;

namespace Wcs.FrameworkExtend.Cfg
{
    public class WCSPreTaskRequestHandlersElement : ConfigurationElement
    {
        public WCSPreTaskRequestHandler[] WCSPreTaskRequestHandlers;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public WCSPreTaskRequestHandlersElement(XmlNode node, WcsConfiguration configuration)
            : base(node, configuration, false)
        {
            Deserialize();
        }

        protected override void Deserialize()
        {
            List<WCSPreTaskRequestHandler> handlers = new List<WCSPreTaskRequestHandler>();
            foreach (XmlNode handerNode in this.Node.SelectNodes("handler"))
            {
                String typeName = GetAttribute<String>("type", handerNode);
                if (String.IsNullOrWhiteSpace(typeName))
                {
                    throw new ConfigurationErrorsException(String.Format("{0} 节点的 {1} 属性值不能为空", handerNode.GetXPath(), "type"));
                }
                string[] taskTypes = GetAttributeOrDefault<String>("taskType", handerNode, "*").Split(',').Select(x => x.Trim()).ToArray();

                var handler = ReflectionHelper.CreateInstance<WCSPreTaskRequestHandler>(typeName);
                handler.TaskTypes = taskTypes;
                handlers.Add(handler);
            }

            WCSPreTaskRequestHandlers = handlers.ToArray();
        }
    }
}
