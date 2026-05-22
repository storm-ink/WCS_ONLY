using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Cfg;
using System.Xml;

namespace Wcs.Framework.Cfg
{

    public class ServiceElement :ConfigurationElement
    {
        /// <summary>
        /// Wcf服务集合配置节点
        /// </summary>
        public WcfServiceCollection WcfServiceCollection { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点对象</param>
        public ServiceElement(XmlNode node, WcsConfiguration configuration) : base(node, configuration) { }
        /// <summary>
        /// 解析配置
        /// </summary>
        /// <param name="node">配置节点</param>
        protected override void Deserialize()
        {

            this.WcfServiceCollection = new WcfServiceCollection(GetOrGenerateNode(this.Node,"wcf"), this.WcsConfiguration);
        }
    }
}
