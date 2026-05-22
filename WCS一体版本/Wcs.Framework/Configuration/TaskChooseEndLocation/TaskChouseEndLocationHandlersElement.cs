using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 任务调整终点处理程序
    /// </summary>
    public class TaskChouseEndLocationHandlersElement:ConfigurationElement
    {
        public TaskChooseEndLocationHandler TaskChooseEndLocationHandler;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public TaskChouseEndLocationHandlersElement(XmlNode node, WcsConfiguration configuration)
            : base(node,configuration, false)
        {
            Deserialize();
        }

        protected override void Deserialize()
        {
            List<TaskChooseEndLocationHandler> handlers = new List<TaskChooseEndLocationHandler>();
            foreach (XmlNode handerNode in this.Node.SelectNodes("handler"))
            {
                String typeName = GetAttribute<String>("type", handerNode);
                if (String.IsNullOrWhiteSpace(typeName))
                {
                    throw new ConfigurationErrorsException(String.Format("{0} 节点的 {1} 属性值不能为空", handerNode.GetXPath(), "type"));
                }

                var handler = ReflectionHelper.CreateInstance<TaskChooseEndLocationHandler>(typeName);

                handlers.Add(handler);
            }

            TaskChooseEndLocationHandler = handlers.FirstOrDefault();
        }
    }
}
