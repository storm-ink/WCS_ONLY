using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    public class WcfServiceCollection:ConfigurationElement
    {
        public WcfServiceElement[] WcfServiceElements { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public WcfServiceCollection(XmlNode node, WcsConfiguration configuration)
            : base(node,configuration) { }
        /// <summary>
        /// 执行解析
        /// </summary>
        /// <param name="node">配置节点</param>
        protected override void Deserialize()
        {
            List<WcfServiceElement> services = new List<WcfServiceElement>();
            foreach (XmlNode serviceNode in this.Node.SelectNodes("service"))
            {
                services.Add(new WcfServiceElement(serviceNode, this.WcsConfiguration));
            }
            this.WcfServiceElements = services.ToArray();
        }
    }
}
