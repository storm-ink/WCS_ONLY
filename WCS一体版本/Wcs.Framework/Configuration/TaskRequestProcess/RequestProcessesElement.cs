using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    public class RequestProcessesElement:ConfigurationElement
    {
        public List<IRequestProcess> RequestProcesses { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public RequestProcessesElement(XmlNode node, WcsConfiguration configuration)
            : base(node,configuration, false)
        {
            RequestProcesses = new List<IRequestProcess>();
            
            Deserialize();
        }

        protected override void Deserialize()
        {
            foreach (XmlNode handerNode in this.Node.SelectNodes("process"))
            {
                String typeName = GetAttribute<String>("type", handerNode);
                if (String.IsNullOrWhiteSpace(typeName))
                {
                    throw new ConfigurationErrorsException(String.Format("{0} 节点的 {1} 属性值不能为空", handerNode.GetXPath(), "type"));
                }
                var process = ReflectionHelper.CreateInstance<IRequestProcess>(typeName);
                RequestProcesses.Add(process);
            }
        }
    }
}
