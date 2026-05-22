using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework.Devices;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 路径配置集合节点
    /// </summary>
    /// <example>
    /// <code lang="xml">
    /// <configuration><routes></routes></configuration>
    /// </code>
    /// </example>
    /// <seealso cref="T:Wcs.Framework.Cfg.RouteElement"/>
    public class RouteSelection:ConfigurationElement
    {
        /// <summary>
        /// 指示该配置是否已初始化
        /// </summary>
        public static Boolean Initialized { get; private set; }
        List<RouteElement> routeElements = new List<RouteElement>();
        DeviceRoute[] routes = null;
        /// <summary>
        /// 获取所有设备路径
        /// </summary>
        public DeviceRoute[] Routes
        {
            get
            {
                lock (this)
                {
                    if (routes == null)
                    {
                        routes = routeElements.Select(x => x.Route).ToArray();
                    }
                }

                return routes;
            }
        }
        /// <summary>
        /// 获取路径配置节点集合
        /// </summary>
        public RouteElement[] RouteElements
        {
            get
            {
                return routeElements.ToArray();
            }
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public RouteSelection(XmlNode node)
            : base(node)
        {
        }
        /// <summary>
        /// 执行解析
        /// </summary>
        /// <param name="node">配置节点</param>
        protected override void Deserialize(System.Xml.XmlNode node)
        {
            foreach (XmlNode item in node.SelectNodes("route"))
            {
                var element = new RouteElement(item);
                routeElements.Add(element);
            }
            Initialized = true;
        }
        /// <summary>
        /// 绑定各路径关联
        /// </summary>
        public void ResolveAdjoins()
        {
            foreach (var item in routeElements)
            {
                item.ResolveAdjoins();
            }
        }
    }
}
