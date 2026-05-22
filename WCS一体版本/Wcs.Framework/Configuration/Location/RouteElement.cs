using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    public sealed class RouteElement:ConfigurationElement
    {
        public RouteElement(XmlNode node, WcsConfiguration configuration)
            : base(node, configuration, true)
        {

        }

        public TaskableDevice Device { get; private set; }

        public Location[] Locations { get; private set; }

        public LogicMovementElement[] LogicMovementElements { get; private set; }

        public LogicMovementSelectorElement LogicMovementSelectorElement { get; private set; }

        public Route Route { get; private set; }
        public RouteStrategyElement RouteStrategyElement { get; private set; }
        protected override void Deserialize()
        {
            Int32 id = GetAttribute<Int32>("id");
            Int32 no = GetAttributeOrDefault<Int32>("no",id);
            Boolean enabled = GetAttributeOrDefault<Boolean>("enabled", true);
            Boolean allowStartFromMidway = GetAttributeOrDefault<Boolean>("allowStartFromMidway", true);
            RouteDirection direction = GetAttributeOrDefault<RouteDirection>("direction", RouteDirection.In);
            RouteType routeType = GetAttributeOrDefault<RouteType>("type", RouteType.Normal | RouteType.Counting);

            applyDevice();
            applyStategy();
            applyLocations();
            applyLogicMovements();
            applyLogicMovementSelector();

            this.Route = new Route(id, no, enabled, allowStartFromMidway, direction, routeType,
                this.Locations,
                this.RouteStrategyElement.RouteStrategy,
                this.Device,
                this.LogicMovementElements.Select(x => x.Type).ToArray(),
                this.LogicMovementSelectorElement.LogicMovementSelector);
         }


        void applyDevice()
        {
            String deviceName = GetAttribute<String>("device");
            var deviceElement = this.WcsConfiguration.DeviceCollection.ParticularDeviceCollection.SelectMany(x => x.DeviceElements).SingleOrDefault(x => string.Equals(x.Device.Name, deviceName, StringComparison.CurrentCultureIgnoreCase));
            if (deviceElement == null)
            {
                var xPath=this.Node.GetXPath();
                throw new ConfigurationErrorsException(string.Format("未找到 {0} 节点属性 {1} 值 “{2}” 指定的设备对象", xPath, "device", deviceName));
            }

            if (!(deviceElement.Device is TaskableDevice))
            {
                var xPath = this.Node.GetXPath();
                throw new ConfigurationErrorsException(string.Format("未找到 {0} 节点属性 {1} 值 “{2}” 指定的设备不是 {3} 的子类", xPath, "device", deviceName,typeof(TaskableDevice)));
            }
            this.Device = (TaskableDevice)deviceElement.Device;
        }
        void applyLocations()
        {
            var _locationsDict = this.WcsConfiguration.LocationCollection.AsDictionary();
            List<Location> locations = new List<Location>();
            String locationValue = GetAttribute<String>("path");
            if (String.IsNullOrWhiteSpace(locationValue))
            {
                var path = Node.GetXPath();
                throw new ConfigurationErrorsException(string.Format("未指定 {0} 节点 path 属性值", path));
            }
            foreach (var locCode in locationValue.Split(','))
            {
                if (string.IsNullOrWhiteSpace(locCode))
                {
                    var path = Node.GetXPath();
                    WcsConfiguration._logger.Warn1(string.Format("{0} 节点 path 属性值 “{1}” 中包含一个空值", path, locationValue), this);
                    continue;
                }

                var matchLocCode = locCode;
                if (!matchLocCode.Contains("@"))
                {
                    matchLocCode = String.Concat(matchLocCode, "@", this.Device.Name);
                }

                if (!_locationsDict.ContainsKey(matchLocCode))
                {
                    var path = Node.GetXPath();
                    throw new ConfigurationErrorsException(string.Format("未找到 {0} 节点 path 属性值 “{1}” 中指定 “{2}“ 位置对象（{3}）", path, locationValue, locCode, matchLocCode));
                }

                locations.Add(_locationsDict[matchLocCode]);

                //var locElements = this.WcsConfiguration
                //    .LocationCollection.Locations
                //    .Where(x => string.Equals(x.ToConvertibleCode(), matchLocCode, StringComparison.CurrentCultureIgnoreCase))
                //    .ToList();
                //if (locElements.Count==0)
                //{
                //    var path = Node.GetXPath();
                //    throw new ConfigurationErrorsException(string.Format("未找到 {0} 节点 path 属性值 “{1}” 中指定 “{2}“ 位置对象（{3}）", path, locationValue, locCode, matchLocCode));
                //}

                //if (locElements.Count > 1)
                //{
                //    var path = Node.GetXPath();
                //    throw new ConfigurationErrorsException(string.Format("{0} 节点 path 属性值 “{1}” 中指定 “{2}“ 位置对象（{3}） 找到多个匹配 {4}", path, locationValue, locCode, matchLocCode,string.Join(",",locElements.Select(x=>x.ToString()).ToArray())));
                //}
                //locations.Add(locElements[0]);
            }
            this.Locations = locations.ToArray();

            if (this.Locations.Length == 0)
            {
                var path = Node.GetXPath();
                throw new ConfigurationErrorsException(string.Format("未找到 {0} 节点 path 属性值 “{1}” 未指定任何位置", path, locationValue));
            }
        }

        void applyLogicMovements()
        {
            if (Node.SelectNodes("logicMovement").Count == 0 && String.IsNullOrWhiteSpace(GetAttributeOrDefault<String>("logicMovement")))
            {
                var path = Node.GetXPath();
                throw new System.Configuration.ConfigurationErrorsException(string.Format("{0} 节点必须配置 logicMovement 子节点或为其添加一个 logicMovement 属性并指定值。", path));
            }

            if (Node.SelectNodes("logicMovement").Count > 0 && !String.IsNullOrWhiteSpace(GetAttributeOrDefault<String>("logicMovement")))
            {
                var path = Node.GetXPath();
                WcsConfiguration._logger.Warn1(string.Format("{0} 节点包同时配置了 logicMovement 子节点和属性，属性将被忽略", path), this);
            }

            List<LogicMovementElement> logicMovementElements = new List<LogicMovementElement>();
            foreach (XmlNode logicMovementNode in Node.SelectNodes("logicMovement"))
            {
                logicMovementElements.Add(new LogicMovementElement(logicMovementNode, this.WcsConfiguration));
            }

            if (logicMovementElements.Count == 0)
            {
                var logicMovementNode = Node.OwnerDocument.CreateNode(XmlNodeType.Element, "logicMovement", null);
                var logicMovementNodeTypeAttr = Node.OwnerDocument.CreateAttribute("type");
                logicMovementNodeTypeAttr.Value = GetAttributeOrDefault<String>("logicMovement");
                logicMovementNode.Attributes.Append(logicMovementNodeTypeAttr);
                this.Node.AppendChild(logicMovementNode);

                logicMovementElements.Add(new LogicMovementElement(logicMovementNode, this.WcsConfiguration));
            }

            this.LogicMovementElements = logicMovementElements.ToArray();
        }

        void applyLogicMovementSelector()
        {
            if (this.Node.SelectNodes("logicMovementSelector").Count == 0 && String.IsNullOrWhiteSpace(GetAttributeOrDefault<String>("logicMovementSelector")))
            {
                var path = Node.GetXPath();
                WcsConfiguration._logger.Trace1(string.Format("{0} 节点未配置 logicMovementSelector 属性或子节点，系统将为其创建一个默认的 logicMovementSelector 配置", path), this);
            }

            if (this.Node.SelectNodes("logicMovementSelector").Count > 0 && !String.IsNullOrWhiteSpace(GetAttributeOrDefault<String>("logicMovementSelector")))
            {
                var path = Node.GetXPath();
                WcsConfiguration._logger.Warn1(string.Format("{0} 节点包同时配置了 logicMovementSelector 子节点和属性，属性将被忽略", path), this);
            }

            var logicMovementSelectorNode = this.Node.SelectSingleNode("logicMovementSelector");
            if (logicMovementSelectorNode == null)
            {
                logicMovementSelectorNode = this.Node.OwnerDocument.CreateNode(XmlNodeType.Element, "logicMovementSelector", null);
                String typeName = GetAttributeOrDefault<String>("logicMovementSelector", typeof(DefaultLogicMovementSelector).FullName);
                XmlAttribute logicMovementSelectorNodeAttr = this.Node.OwnerDocument.CreateAttribute("type");
                logicMovementSelectorNodeAttr.Value = typeName;
                logicMovementSelectorNode.Attributes.Append(logicMovementSelectorNodeAttr);
                this.Node.AppendChild(logicMovementSelectorNode);
            }
            this.LogicMovementSelectorElement = new LogicMovementSelectorElement(logicMovementSelectorNode, this.WcsConfiguration);

        }

        void applyStategy()
        {
            if (this.Node.SelectNodes("strategy").Count > 0 && !String.IsNullOrWhiteSpace(GetAttributeOrDefault<String>("strategy")))
            {
                var path = Node.GetXPath();
                WcsConfiguration._logger.Warn1(string.Format("{0} 节点包同时配置了 strategy 子节点和属性，属性将被忽略", path), this);
            }

            if (this.Node.SelectNodes("strategy").Count == 0 && String.IsNullOrWhiteSpace(GetAttributeOrDefault<String>("strategy")))
            {
                var path = Node.GetXPath();
                WcsConfiguration._logger.Trace1(string.Format("{0} 节点未配置 strategy 属性或子节点，系统将为其创建一个默认的 strategy 配置", path), this);
            }

            var routeStrategyNode = this.Node.SelectSingleNode("strategy");
            if (routeStrategyNode == null)
            {
                routeStrategyNode = this.Node.OwnerDocument.CreateNode(XmlNodeType.Element, "strategy", null);
                String typeName = GetAttributeOrDefault<String>("strategy", typeof(DefaultRouteStrategy).FullName);
                XmlAttribute routeStrategyNodeAttr = this.Node.OwnerDocument.CreateAttribute("type");
                routeStrategyNodeAttr.Value = typeName;
                routeStrategyNode.Attributes.Append(routeStrategyNodeAttr);
                this.Node.AppendChild(routeStrategyNode);
            }
            this.RouteStrategyElement = new RouteStrategyElement(routeStrategyNode, this.WcsConfiguration);
        }
    }
}
