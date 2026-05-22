using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework.Devices;
using Wcs.Framework.Impl;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 路径策略配置节点
    /// </summary>
    /// <remarks>
    /// <b>属性</b><br />
    /// type:子符串，必填。指示该配置映射到的具体实现。必须为 <see cref="T:Wcs.Framework.Devices.RouteStrategy"/> 的子类。框架提供了一个默认实现 <see cref="T:Wcs.Framework.Impl.DefaultRouteStrategy"/>。
    /// </remarks>
    /// <example>
    /// <code lang="xml">
    /// <route><startegy type="Wcs.Framework.Impl.DefaultRouteStrategy, Wcs.Framework" /></route>
    /// </code>
    /// </example>
    public class DeviceRouteStrategyElement:ConfigurationElement
    {
        public RouteStrategy RouteStrategy { get; private set; }
        public DeviceRoute Route { get; private set; }
        public DeviceRouteStrategyElement(XmlNode node,DeviceRoute route)
            : base(node,false)
        {
            Route = route;
            Deserialize(node);
        }
        protected override void Deserialize(System.Xml.XmlNode node)
        {
            if (node == null)
            {
                RouteStrategy = new DefaultRouteStrategy();
            }
            else
            {
                RouteStrategy = CreateInstance<RouteStrategy>(node.Attributes["type"].Value);
            }

            this.Route.Strategy = RouteStrategy;
        }
    }
}
