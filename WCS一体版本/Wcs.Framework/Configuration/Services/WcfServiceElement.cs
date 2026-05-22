using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Cfg
{
    public class WcfServiceElement:ConfigurationElement
    {
        public Type Type { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public WcfServiceElement(System.Xml.XmlNode node, WcsConfiguration configuration)
            : base(node,configuration)
        {
        }
        /// <summary>
        /// 执行解析
        /// </summary>
        /// <param name="node">配置节点</param>
        protected override void Deserialize()
        {
            string typeName = GetAttribute<String>("type");
            var type = Type.GetType(typeName);
            if (type == null)
            {
                var path = Node.GetXPath();
                throw new System.Configuration.ConfigurationErrorsException(string.Format("未找到 {0} 节点 {1} 属性值 “{2}” 标识的类型", path, "type", typeName));
            }

            this.Type = type;
        }
    }
}
