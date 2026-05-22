using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Cfg;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 主应用程序配置节点
    /// </summary>
    /// <remarks>
    /// <b>属性</b><br />
    /// logTarget：字符串，日志输出目标名称。引用自 <see cref="T:Wcs.Framework.Cfg.LogTargetsSelection"/> 节点内的 <see cref="T:Wcs.Framework.Cfg.LogTargetElement"/> 配置。<br />
    /// <b>子节点</b><br />
    /// <see cref="T:Wcs.Framework.Cfg.StartupSelection"/>
    /// </remarks>
    /// <example>
    /// <code lang="xml">
    /// <wcs-configuration><application></application></wcs-configuration>
    /// </code>
    /// </example>
    public class ApplicationElement :ConfigurationElement
    {
        /// <summary>
        /// 应用程序启动时加载对象节点
        /// </summary>
        public StartupCollection StartupSelection { get; private set; }
        /// <summary>
        /// 位置转换器节点
        /// </summary>
        public LocationConverterCollection LocationConverterSelection { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点对象</param>
        public ApplicationElement(XmlNode node, WcsConfiguration configuration) : base(node,configuration) { }
        /// <summary>
        /// 解析配置
        /// </summary>
        /// <param name="node">配置节点</param>
        protected override void Deserialize()
        {
            this.StartupSelection = new StartupCollection(GetOrGenerateNode(this.Node, "startups"), this.WcsConfiguration);
            this.LocationConverterSelection = new LocationConverterCollection(GetOrGenerateNode(this.Node, "locationConverters"), this.WcsConfiguration);
        }
    }
}
