using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    public sealed class RouteCollection : ConfigurationElement
    {
        public RouteCollection(XmlNode node, WcsConfiguration configuration)
            : base(node, configuration, true)
        {

        }

        public RouteElement[] RouteElements { get; private set; }
        public Route[] Routes
        {
            get
            {
                return this
                    .RouteElements
                    .Select(x => x.Route)
                    .ToArray();
            }
        }
        protected override void Deserialize()
        {
            List<RouteElement> routeElements = new List<RouteElement>();
            foreach (XmlNode routeNode in Node.SelectNodes("route"))
            {
                var routeElement = new RouteElement(routeNode, this.WcsConfiguration);
                if (routeElements.Any(x => x.Route.Id == routeElement.Route.Id))
                {
                    string msg = string.Format("已存在 id 为 {0} 的路径对象",routeElement.Route.Id);
                    throw new ConfigurationErrorsException(msg, routeElement.Node);
                }

                if (routeElement.GetAttributeOrDefault<Boolean>("ignoreRepetition") == false)
                {
                    //验证是否存在相同Locations配置的的Route对象
                    var sameRoutes = routeElements
                        .Where(x => 
                            string.Equals(x.GetAttribute<String>("path").Trim(' '),
                            routeElement.GetAttribute<String>("path").Trim(' '),
                            StringComparison.CurrentCultureIgnoreCase)
                        )
                        .Select(x => x.Route)
                        .ToList();
                    if (sameRoutes.Count > 0)
                    {
                        string msg = string.Format("已存在与 {0} 相同位置序列 {1} 的路径 {2}。如果需要忽略该错误，请指定该路径的属性 ignoreRepetition 值为 true。"
                            , routeElement.Route.Id
                            , routeElement.GetAttribute<String>("path")
                            , string.Join(",", sameRoutes.Select(x => x.Id.ToString()).ToArray())
                            );
                        throw new ConfigurationErrorsException(msg, routeElement.Node);
                    }
                }

                routeElements.Add(routeElement);
            }

            this.RouteElements = routeElements.ToArray();

            foreach (RouteElement routeElement in this.RouteElements)
            {
                applyAdjoins(routeElement);
            }
        }


        void applyAdjoins(RouteElement routeElement)
        {
            String adjoinsValue = routeElement.GetAttributeOrDefault<String>("adjoins");
            if (String.IsNullOrWhiteSpace(adjoinsValue))
            {
                routeElement.Route.AddAdjoins(new Route[0]);
                return;
            }

            List<Route> routes = new List<Route>();
            foreach (var routeIdValue in adjoinsValue.Split(','))
            {
                if (string.IsNullOrWhiteSpace(routeIdValue))
                {
                    var path = routeElement.Node.GetXPath();
                    WcsConfiguration._logger.Warn1(string.Format("{0} 节点 adjoins 属性值 “{1}” 中包含一个空值", path, adjoinsValue), this);
                    continue;
                }

                int routeId;
                if (!int.TryParse(routeIdValue, out routeId))
                {
                    var path = routeElement.Node.GetXPath();
                    throw new ConfigurationErrorsException(string.Format("{0} 节点 adjoins 属性值 “{1}” 中指定 “{2}“ 值无法转换为 {3} 类型", path, adjoinsValue, routeIdValue, typeof(Int32)));
                }

                if (routeId == routeElement.GetAttribute<Int32>("id"))
                {
                    var path = routeElement.Node.GetXPath();
                    throw new ConfigurationErrorsException(string.Format("{0} 节点 adjoins 属性值 “{1}” 中指定 “{2}“ 值无效，原因是其指向本身", path, adjoinsValue, routeIdValue));
                }

                var adjoinRouteElement = RouteElements
                    .SingleOrDefault(x => x.Route.Id == routeId);
                if (adjoinRouteElement == null)
                {
                    var path = routeElement.Node.GetXPath();
                    throw new ConfigurationErrorsException(string.Format("未找到 {0} 节点 adjoins 属性值 “{1}” 中指定 “{2}“ 路径对象", path, adjoinsValue, routeIdValue));
                }
                routes.Add(adjoinRouteElement.Route);
            }

            routeElement.Route.AddAdjoins(routes);
        }
    }
}
