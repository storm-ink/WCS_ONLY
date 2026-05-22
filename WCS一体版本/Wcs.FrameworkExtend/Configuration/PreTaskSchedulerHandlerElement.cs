using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.FrameworkExtend.Cfg
{
    public class PreTaskSchedulerHandlerElement : ConfigurationElement
    {
        public Logger _logger = LogManager.CreateNullLogger();
        public PreTaskSchedulerFilter[] PreTaskSchedulerFilterHandlers;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public PreTaskSchedulerHandlerElement(XmlNode node, WcsConfiguration configuration)
            : base(node, configuration, false)
        {
            Deserialize();
        }

        protected override void Deserialize()
        {
            List<PreTaskSchedulerFilter> handlers = new List<PreTaskSchedulerFilter>();
            foreach (XmlNode handerNode in this.Node.SelectNodes("handler"))
            {
                String typeName = GetAttribute<String>("type", handerNode);
                if (String.IsNullOrWhiteSpace(typeName))
                    throw new ConfigurationErrorsException(String.Format("{0} 节点的 {1} 属性值不能为空", handerNode.GetXPath(), "type"));

                var handler = ReflectionHelper.CreateInstance<PreTaskSchedulerFilter>(typeName);
                handlers.Add(handler);
            }

            PreTaskSchedulerFilterHandlers = handlers.ToArray();
        }
    }
}
