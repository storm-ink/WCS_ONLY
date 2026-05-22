using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Devices;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 路径配置节点
    /// </summary>
    /// <remarks>
    /// <b>属性</b>
    /// <ul>
    /// <li>id：数字，路径号。必填，系统内必须唯一。</li>
    /// <li>path：字符串，路径经过的所有货位集合。必填，引用自 <see cref="T:Wcs.Framework.Cfg.LocationsSelection"/> 中的 <see cref="T:Wcs.Framework.Cfg.LocationElement"/> 的 id 属性。使用半角逗号隔开。引用不存在时将引发异常。<br />
    /// 默认的可以直接填写 id,系统在解析时将以 device 属性中指定的设备中查找该位置。如果该属性中存在非 device 属性中指定设备中的位置，请使用系统可识别（可二次转换的）编码值。（请参见 <see cref="M:Wcs.Framework.Devices.GetConvertibleCode"/> 返回值）
    /// </li>
    /// <li>adjoins：邻接边，字符串。引用自 <see cref="T:Wcs.Framework.Cfg.RoutesSelection"/> 中的 <see cref="T:Wcs.Framework.Cfg.RouteElement"/> 的 id 属性。使用半角逗号隔开。引用不存在时将引发异常。</li>
    /// <li>device：字符串，设备名称。必填，引用自 <see cref="T:Wcs.Framework.Cfg.DevicesSelection"/> 中的 <see cref="T:Wcs.Framework.Cfg.DeviceElement"/> 的 name 属性,引用不存在时将引发异常。</li>
    /// <li>enabled:Boolean 值，为空时默认为 true。指示该路径是否可用。</li>
    /// <li>no:数字，路径号。为空时默认使用 id 属性值。此值为该路径在设备中存在的编号。</li>
    /// <li>notBlockUpWhenCross:字符串，在判断两条路径是否交叉时不检查的位置集合。引用自 <see cref="T:Wcs.Framework.Cfg.LocationsSelection"/> 中的 <see cref="T:Wcs.Framework.Cfg.LocationElement"/> 的 id 属性。使用半角逗号隔开。引用不存在时将引发异常。<br />
    /// 默认的可以直接填写 id,系统在解析时将以 device 属性中指定的设备中查找该位置。如果该属性中存在非 device 属性中指定设备中的位置，请使用系统可识别（可二次转换的）编码值。（请参见 <see cref="M:Wcs.Framework.Devices.Device.GetConvertibleCode"/> 返回值）</li>
    /// <li>type：<see cref="T:Wcs.Framework.Devices.DeviceRouteType"/>，路径类型(可填写多个，使用逗号分隔)，必填。指示路径属于哪种类型。</li>
    /// <li>direction:<see cref="T:Wcs.Framework.Devices.DeviceRouteDirection"/>，路径方向，必填。指示路径是运行方向，这属性会影响到连通图的生成。</li>
    /// <li>logicMovementType:在 <see cref="T:Wcs.Framework.Cfg.LogicMovementTypesSelection"/> 节点中未配置 <see cref="T:Wcs.Framework.Cfg.LogicMovementTypeElement"/> 时，系统将尝试解析该属性以为此路经映射一个逻辑动作 <see cref="T:Wcs.Framework.LogicMovement"/>。否则，该属性将被忽略。</li>
    /// </ul>
    /// <b>子节点</b>
    /// <ul>
    /// <li><see cref="T:Wcs.Framework.Cfg.LogicMovementTypesSelection"/> 该路径映射到的逻辑动作集合配置节点</li>
    /// <li><see cref="T:Wcs.Framework.Cfg.DeviceRouteStrategyElement"/> 该路径使用的策略配置节点</li>
    /// <li><see cref="T:Wcs.Framework.Cfg.LogicMovementSelectorElement"/> 该路径映射到的逻辑动作选择器配置节点。在一个设备路径同时映射到多个逻辑动作时，系统将使用此处配置的选择器决定选取哪个逻辑动作作为任务下发。</li>
    /// </ul>
    /// </remarks>
    /// <example>
    /// <code lang="xml">
    /// <routes>
    ///     <route id="1" path="1,2,3" direction="In" adjoins="2" type="Normal" device="输送线设备001" logicMovementType="Wcs.Framework.LogicMovements.ConveyorTransferMovement, Wcs.Framework"/>
    /// </routes>
    /// </code>
    /// <h1>以下示例演示了如何配置设备中的路径集合</h1>
    /// <para>
    /// <b>说明：</b><br />
    /// 本演示并不指定路径策略，由系统使用默认值。也不使用自定义的路径映射类型，而是直接使用 <see cref="T:Wcs.Framework.LogicMovements.ConveyorTransferMovement"/> 类型。由于我们没有定义 <see cref="T:Wcs.Framework.Cfg.LogicMovementTypesSelection"/> 节点，只指定了一个逻辑动作映射类型，所以这里也不再需要配置 <see cref="T:Wcs.Framework.Cfg.LogicMovementSelectorElement"/> 节点。而且，在大多数情况下我们也没有必要使用这些配置。
    /// </para>
    /// <ul>
    /// <li>1、假设电气提供了以下设备和路径：<br />
    /// <img src="../helpDoc/images/跨设备路径示例.jpg" />
    /// </li>
    /// <li>2、根据上图所示，我们配置后的设备如下（你可以点击 <see cref="T:Wcs.Framework.Cfg.ConveyorElement"/> 查看如何配置一个新的输送线设备）：
    /// <code lang="xml">
    /// <conveyor name="输送线1" no="1" allowConcurrency="true" ip="127.0.0.1" port="8000" senderDecoder="db1" receiverDecoder="db2" receiveTimeout="2000" connectTimeout="10000" sendTimeout="3000" logTarget="设备状态" >
    ///     <sequence type="Wcs.Framework.EquipmentActionSequence, Wcs.Framework" comparer="Wcs.Framework.Impl.DefaultEquipmentActionSortComparer, Wcs.Framework" logTarget="任务序列" />
    ///     <locations>
    ///         <location id="1" userCode="00-001-001" />
    ///         <location id="2" userCode="00-001-002" />
    ///         <location id="3" userCode="00-001-003" />
    ///         <location id="4" userCode="00-001-004" />
    ///         <location id="5" userCode="00-001-005" />
    ///         <location id="6" userCode="00-001-006" />
    ///         <location id="7" userCode="00-001-007" />
    ///         <location id="8" userCode="00-001-008" />
    ///         <location id="9" userCode="00-001-009" />
    ///         <location id="10" userCode="00-001-010" />
    ///         <location id="11" userCode="00-001-011" />
    ///         <location id="12" userCode="00-001-012" />
    ///         <location id="13" userCode="00-001-013" />
    ///         <location id="14" userCode="00-001-014" />
    ///         <location id="15" userCode="00-001-015" />
    ///     </locations>
    /// </conveyor>
    /// <conveyor name="输送线2" no="2" allowConcurrency="true" ip="127.0.0.1" port="8001" senderDecoder="db1" receiverDecoder="db2" receiveTimeout="2000" connectTimeout="10000" sendTimeout="3000" logTarget="设备状态" >
    ///     <sequence type="Wcs.Framework.EquipmentActionSequence, Wcs.Framework" comparer="Wcs.Framework.Impl.DefaultEquipmentActionSortComparer, Wcs.Framework" logTarget="任务序列" />
    ///     <locations>
    ///         <location id="21" userCode="00-002-001" />
    ///         <location id="22" userCode="00-002-002" />
    ///         <location id="23" userCode="00-002-003" />
    ///         <location id="24" userCode="00-002-004" />
    ///         <location id="25" userCode="00-002-005" />
    ///         <location id="26" userCode="00-002-006" />
    ///         <location id="27" userCode="00-002-007" />
    ///         <location id="28" userCode="00-002-008" />
    ///         <location id="29" userCode="00-002-009" />
    ///         <location id="210" userCode="00-002-010" />
    ///         <location id="211" userCode="00-002-011" />
    ///         <location id="212" userCode="00-002-012" />
    ///         <location id="213" userCode="00-002-013" />
    ///         <location id="214" userCode="00-002-014" />
    ///         <location id="215" userCode="00-002-015" />
    ///     </locations>
    /// </conveyor>
    /// </code>
    /// </li>
    /// <li>3、设备配置完成后我们便有了位置对象，这时开始配置路径。如上图所示，此处两个设备中一共有 13 条路径，其中 12 条为单设备路径，1 条为跨设备路径。我们先配置单设备路径。根据 <see cref="T:Wcs.Framework.Cfg.RouteElement"/> 的定义，最后编写的配置如下：
    /// <code lang="xml">
    /// <routes>
    ///     <route id="1" path="1,2,3" direction="In" adjoins="2" type="Normal" device="输送线1" logicMovementType="Wcs.Framework.LogicMovements.ConveyorTransferMovement, Wcs.Framework"/>
    ///     <route id="2" path="3,4" direction="In" adjoins="" type="Normal" device="输送线1" logicMovementType="Wcs.Framework.LogicMovements.ConveyorTransferMovement, Wcs.Framework"/>
    ///     <route id="3" path="5,6,7" direction="In" adjoins="4" type="Normal" device="输送线1" logicMovementType="Wcs.Framework.LogicMovements.ConveyorTransferMovement, Wcs.Framework"/>
    ///     <route id="4" path="7,8" direction="In" adjoins="" type="Normal" device="输送线1" logicMovementType="Wcs.Framework.LogicMovements.ConveyorTransferMovement, Wcs.Framework"/>
    ///     <route id="5" path="11,10" direction="Out" adjoins="6" type="Normal" device="输送线1" logicMovementType="Wcs.Framework.LogicMovements.ConveyorTransferMovement, Wcs.Framework"/>
    ///     <route id="6" path="15,14,13" direction="Out" adjoins="" type="Normal" device="输送线1" logicMovementType="Wcs.Framework.LogicMovements.ConveyorTransferMovement, Wcs.Framework"/>
    ///     <route id="21" no="1" path="21,22,23" direction="In" adjoins="22" type="Normal" device="输送线2" logicMovementType="Wcs.Framework.LogicMovements.ConveyorTransferMovement, Wcs.Framework"/>
    ///     <route id="22" no="2" path="23,24" direction="In" adjoins="" type="Normal" device="输送线2" logicMovementType="Wcs.Framework.LogicMovements.ConveyorTransferMovement, Wcs.Framework"/>
    ///     <route id="23" no="3" path="25,26,27" direction="In" adjoins="24" type="Normal" device="输送线2" logicMovementType="Wcs.Framework.LogicMovements.ConveyorTransferMovement, Wcs.Framework"/>
    ///     <route id="24" no="4" path="27,28" direction="In" adjoins="" type="Normal" device="输送线2" logicMovementType="Wcs.Framework.LogicMovements.ConveyorTransferMovement, Wcs.Framework"/>
    ///     <route id="25" no="5" path="211,210" direction="Out" adjoins="26" type="Normal" device="输送线2" logicMovementType="Wcs.Framework.LogicMovements.ConveyorTransferMovement, Wcs.Framework"/>
    ///     <route id="26" no="6" path="215,214,213" direction="Out" adjoins="" type="Normal" device="输送线2" logicMovementType="Wcs.Framework.LogicMovements.ConveyorTransferMovement, Wcs.Framework"/>
    ///     <route id="27" no="7" path="13@输送线1,215,214,213" direction="Out" adjoins="" type="Normal" device="输送线2" logicMovementType="Wcs.Framework.LogicMovements.ConveyorTransferMovement, Wcs.Framework"/>
    /// </routes>
    /// </code>
    /// <para>
    /// <b>说明：</b><br />
    /// id 从 1 到 6 的路径是输送线1的路径。<br />
    /// id 从 21 到 27 的路径为输送线2的路径。从图上看输送线二根本不存在 id 为 21 和 27 的路径，而是 1 到 7 的路径。原因是这样的：<br />
    /// 1 到 6 的路径 id 已被输送线使用过了，按原则，路径中的 id 必须全局唯一。此时我们将输送线2的路径号前面加了2前缀，同时我们还指定了这些路径的 no 属性。任务在下发给输送线时，路径号优先使用了 no 给定的值，这样设备接收到的仍然是 1 到 7。<br />
    /// id 为 27 的路径是一个跨设备路径，由于 path 中指定的 13 号位置并不存在于 device 中指定的 输送线2 中，所以我们这里给出了一个位置描述符 13@输送线1，这样系统在解析时就能识别 13 这个位置在 输送线1 中，而不是 输送线2中。<br />
    /// 这里我们将所有的路径都映射到了 <see cref="T:Wcs.Framework.LogicMovements.ConveyorTransferMovement"/> 类型，这个类型是框架提供的一个全自动的输送线任务默认实现。一般情况下我们可以直接使用。
    /// </para>
    /// </li>
    /// </ul>
    /// <b>框架提供了 <see cref="T:Wcs.Framework.LogicMovements.ConveyorTransferMovement"/>、<see cref="T:Wcs.Framework.LogicMovements.CraneAutomaticTransferMovement"/>、<see cref="T:Wcs.Framework.LogicMovements.CraneHalfAutomaticTransferMovement"/> 三个默认的逻辑动作实现，它们能满足绝大多数的应用场景。</b>
    /// </example>
    public class RouteElement:ConfigurationElement
    {
        /// <summary>
        /// 获取此节点映射到的路径实例
        /// </summary>
        public DeviceRoute Route { get; private set; }
        /// <summary>
        /// 获取路径映射到的逻辑动作集合配置节点。
        /// </summary>
        public LogicMovementTypesSelection LogicMovementTypesSelection { get; private set; }
        /// <summary>
        /// 获取路径使用的策略配置节点。
        /// </summary>
        public DeviceRouteStrategyElement DeviceRouteStrategyElement { get; private set; }
        /// <summary>
        /// 获取在路径同时映射到多个逻辑动作时使用的逻辑动作选择器配置节点。
        /// </summary>
        public LogicMovementSelectorElement LogicMovementSelectorElement { get; set; }
        /// <summary>
        /// 获取路径邻接边配置信息
        /// </summary>
        public String AdjoinsConfig { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public RouteElement(System.Xml.XmlNode node)
            : base(node)
        {

        }
        /// <summary>
        /// 执行解析
        /// </summary>
        /// <param name="node">配置节点</param>
        protected override void Deserialize(System.Xml.XmlNode node)
        {
            if (node.Attributes["id"] == null)
            {
                throw new NotFoundConfigElementAttributeException("id", node);
            }

            if (node.Attributes["path"] == null)
            {
                throw new NotFoundConfigElementAttributeException("path", node);
            }

            if (node.Attributes["adjoins"] == null)
            {
                throw new NotFoundConfigElementAttributeException("adjoins", node);
            }

            if (node.Attributes["device"] == null)
            {
                throw new NotFoundConfigElementAttributeException("device", node);
            }

            if (node.Attributes["type"] == null)
            {
                throw new NotFoundConfigElementAttributeException("type", node);
            }

            Int32 id = Convert.ToInt32(node.Attributes["id"].Value);
            string path = node.Attributes["path"].Value;
            DeviceRouteDirection direction = (DeviceRouteDirection)Enum.Parse(typeof(DeviceRouteDirection), node.Attributes["direction"].Value);
            AdjoinsConfig = node.Attributes["adjoins"].Value;
            DeviceRouteType type = (DeviceRouteType)Enum.Parse(typeof(DeviceRouteType), node.Attributes["type"].Value);
            string deviceName = node.Attributes["device"].Value.Trim();
            Boolean enabled = node.Attributes["enabled"] != null ? Convert.ToBoolean(node.Attributes["enabled"].Value) : true;
            Device defaultDevice = Configuration.GetDevice(deviceName);

            Int32 routeNo = id;
            if (node.Attributes["no"] != null)
            {
                routeNo = Convert.ToInt32(node.Attributes["no"].Value);
            }

            Location[] notBlockUpWhenCross = new Location[] { };
            if (node.Attributes["notBlockUpWhenCross"] != null)
            {
                notBlockUpWhenCross = convertPathSettingToLocations(defaultDevice, node.Attributes["notBlockUpWhenCross"].Value);
            }

            DeviceRoute route = new DeviceRoute
            {
                Locations = convertPathSettingToLocations(defaultDevice, path),
                Device = defaultDevice,
                Direction = direction,
                Enabled = enabled,
                Id = id,
                RouteType = type,
                NotBlockUpWhenCross = notBlockUpWhenCross,
                No = routeNo
            };

            DeviceRouteStrategyElement = new DeviceRouteStrategyElement(node.SelectSingleNode("startegy"), route);
            LogicMovementTypesSelection = new LogicMovementTypesSelection(node.SelectSingleNode("logicMovementTypes"),route);
            if (node.SelectSingleNode("logicMovementSelector") != null)
            {
                this.LogicMovementSelectorElement = new LogicMovementSelectorElement(node.SelectSingleNode("logicMovementSelector"));
                route.LogicMovementSelector = this.LogicMovementSelectorElement.LogicMovementSelector;
            }
            if (LogicMovementTypesSelection.LogicMovementTypeElements.Length == 0)
            {
                if (node.Attributes["logicMovementType"] != null)
                {
                    LogicMovementTypesSelection.AddLogicMovementType(node.Attributes["logicMovementType"].Value);
                }
            }

            if (route.LogicMovementTypes.Length == 0)
            {
                throw new Exception(string.Format("route {0} 未指定逻辑动作类型", route.Id));
            }

            this.Route = route;
        }
        /// <summary>
        /// 将路径串转换为位置实例集合
        /// </summary>
        /// <param name="defaultDevice">默认设备实例</param>
        /// <param name="path">路径串</param>
        /// <returns>位置实例集合</returns>
        private Location[] convertPathSettingToLocations(Device defaultDevice, string path)
        {
            List<Location> result = new List<Location>();
            foreach (string item in path.Split(','))
            {
                string locationValue = item.Trim();
                if (string.IsNullOrWhiteSpace(locationValue))
                {
                    throw new ArgumentNullException(string.Format("路径配置值 {0} 包含无效项", path));
                }
                Location location = Location.TryParse(item, null, defaultDevice);
                if (location == null)
                {
                    throw new ArgumentNullException(string.Format("{0} 无法转换为 Location 对象", item));
                }
                result.Add(location);
            }

            return result.ToArray();
        }
        /// <summary>
        /// 绑定邻接关系
        /// </summary>
        public void ResolveAdjoins()
        {
            if (!RouteSelection.Initialized)
            {
                throw new Exception("RouteSelection 未初始化，无法解析 DeviceRoute.Adjoins 属性");
            }

            if (string.IsNullOrWhiteSpace(this.AdjoinsConfig))
            {
                return;
            }

            List<DeviceRoute> adjoinRoutes = new List<DeviceRoute>();
            foreach (var item in this.AdjoinsConfig.Split(','))
            {
                Int32 routeId = Convert.ToInt32(item.Trim());
                DeviceRoute adjoionRoute =  Configuration.GetRoute(routeId);
                adjoinRoutes.Add(adjoionRoute);
            }

            this.Route.Adjoins = adjoinRoutes.ToArray();
        }
    }
}
