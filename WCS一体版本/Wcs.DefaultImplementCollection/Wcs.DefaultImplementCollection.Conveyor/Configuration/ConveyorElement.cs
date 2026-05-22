using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 输送线设备配置节点。
    /// </summary>
    /// <remarks>
    /// <b>属性</b><br />
    /// senderDecoder: 字符串，接收解码器名称，必填。引用自 /configuration/netPackageDecoders 配置。<br />
    /// receiverDecoder: 字符串，发送编码器名称，必填。引用自 /configuration/netPackageDecoders 配置。<br />
    /// <b>子节点</b><br />
    /// 任务序列：<see cref="T:Wcs.Framework.Cfg.SequenceSelection"/><br />
    /// 位置集合：<see cref="T:Wcs.Framework.Cfg.LocationsSelection"/><br />
    /// 状态更新时附加的额外动作集合：<see cref="T:Wcs.Framework.Cfg.UpdateStatusActionsSelection"/><br />
    /// </remarks>
    /// <example>
    /// <code lang="xml">
    /// <configuration><devices><conveyor name="输送线设备001" no="1" allowConcurrency="true" ip="127.0.0.1" port="8000" senderDecoder="netPackageDecoder2" receiverDecoder="netPackageDecoder1" receiveTimeout="2000" connectTimeout="10000" sendTimeout="3000" logTarget="设备状态" /></devices></configuration>
    /// </code>
    /// <h1>如何配置一个新的输送线设备</h1>
    /// <ul>
    /// <li>1、从项目电气负责人那里拿到该输送线的所有货位、路径、占位点信息。假设对方提供了如下信息：<br />
    /// <img src="../helpDoc/images/路径表_点位点_货位.jpg" />
    /// </li>
    /// <li>2、添加一个 conveyor 节点，并配置相关属性，如：设备名、设备编号、ip地址、端口、接收解码器、发送编码器。关于设备具体节点说明请点击 <see cref="T:Wcs.Framework.Cfg.DeviceElement"/> 查看。
    /// <para>以下为我们添加的 conveyor 配置详情</para>
    /// <code lang="xml"><conveyor name="输送线设备001" no="1" allowConcurrency="true" ip="127.0.0.1" port="8000" senderDecoder="db1" receiverDecoder="db2" receiveTimeout="2000" connectTimeout="10000" sendTimeout="3000" logTarget="设备状态" /></code>
    /// </li>
    /// <li>3、根据电气人员提供的货位信息，配置 conveyor 节点内的 locations 子节点。点击 <see cref="T:Wcs.Framework.LocationElement"/> 查看格式。
    /// <para>以下为我们添加了的 locations 配置的 conveyor 详情</para>
    /// <code lang="xml">
    /// <conveyor name="输送线设备001" no="1" allowConcurrency="true" ip="127.0.0.1" port="8000" senderDecoder="db1" receiverDecoder="db2" receiveTimeout="2000" connectTimeout="10000" sendTimeout="3000" logTarget="设备状态" >
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
    /// </code>
    /// </li>
    /// <li>4、为了能让设备工作，我们必须为它添加一个任务序列 sequence 节点。关于 sequence 节点说明请点击 <see cref="T:Wcs.Framework.Cfg.SequenceSelection"/> 查看。
    /// <para>以下是我们使用框架提供的默认任务序列 <see cref="T:Wcs.Framework.EquipmentActionSequence"/>，添加 sequence 配置的 conveyor 详情</para>
    /// <code lang="xml">
    /// <conveyor name="输送线设备001" no="1" allowConcurrency="true" ip="127.0.0.1" port="8000" senderDecoder="db1" receiverDecoder="db2" receiveTimeout="2000" connectTimeout="10000" sendTimeout="3000" logTarget="设备状态" >
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
    /// </code>
    /// </li>
    /// <li>
    /// 5、输送线设备的货位输送动作，依靠于设备路径，所以我们为了高输送线能工作，必须配置对应的路径。关于如何配置输送线路径请点击 <see cref="T:Wcs.Framework.Cfg.RouteElement"/> 查看。
    /// <para>以下为我们根据上面图中所示的路径列表，使用框架提供的默认输送线逻辑动作映射类型 <see cref="T:Wcs.Framework.LogicMovements.ConveyorTransferMovement"/> 添加的 route 配置详情</para>
    /// <code lang="xml">
    /// <routes>
    ///     <route id="1" path="1,2,3" direction="In" adjoins="2" type="Normal" device="输送线设备001" logicMovementType="Wcs.Framework.LogicMovements.ConveyorTransferMovement, Wcs.Framework"/>
    ///     <route id="2" path="3,4" direction="In" adjoins="" type="Normal" device="输送线设备001" logicMovementType="Wcs.Framework.LogicMovements.ConveyorTransferMovement, Wcs.Framework"/>
    ///     <route id="3" path="5,6,7" direction="In" adjoins="4" type="Normal" device="输送线设备001" logicMovementType="Wcs.Framework.LogicMovements.ConveyorTransferMovement, Wcs.Framework"/>
    ///     <route id="4" path="7,8" direction="In" adjoins="" type="Normal" device="输送线设备001" logicMovementType="Wcs.Framework.LogicMovements.ConveyorTransferMovement, Wcs.Framework"/>
    ///     <route id="5" path="11,10" direction="Out" adjoins="6" type="Normal" device="输送线设备001" logicMovementType="Wcs.Framework.LogicMovements.ConveyorTransferMovement, Wcs.Framework"/>
    ///     <route id="6" path="15,14,13" direction="Out" adjoins="" type="Normal" device="输送线设备001" logicMovementType="Wcs.Framework.LogicMovements.ConveyorTransferMovement, Wcs.Framework"/>
    /// </routes>
    /// </code>
    /// </li>
    /// <li>
    /// 6、至此，一个简单的输送线对象配置完成。设备管理器、序列管理器中对应的会出现此设备。你可以在手动任务中添加一个起始点为 1@输送线设备001，结束点为 4@输送线设备001 的手动任务验证并测试配置是否正确。
    /// </li>
    /// </ul>
    /// </example>
    public sealed class ConveyorElement :Wcs.Framework.Cfg.TcpProtocolTaskableDeviceElement
    {
        public Int32 AllowTaskBlockSpace { get; set; }
        public CompareResultHandlerCollection CompareResultHandlerCollection { get; private set; }
        public UsedConveyorTaskBlockIndexGetterCollection UsedConveyorTaskBlockIndexGetterCollection { get; private set; }
        public EquipmentActionToAddCommandPluginElement EquipmentActionToAddCommandPluginElement { get; set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public ConveyorElement(XmlNode node, WcsConfiguration configuration)
            : base(node,configuration,true)
        {
        }

        protected override void Deserialize()
        {
            this.AllowTaskBlockSpace = this.GetAttributeOrDefault<Int32>("allowTaskBlockSpace", 0);
            this.CompareResultHandlerCollection = new CompareResultHandlerCollection(this.GetOrGenerateNode(this.Node, "compareResultHandlers"), this.WcsConfiguration);
            this.UsedConveyorTaskBlockIndexGetterCollection = new UsedConveyorTaskBlockIndexGetterCollection(this.GetOrGenerateNode(this.Node, "usedConveyorTaskBlockIndexGetters"), this.WcsConfiguration);

            base.Deserialize();

            var equipmentActionToAddCommandPluginNode = this.Node.SelectSingleNode("equipmentActionToAddCommandPlugin");
            if (equipmentActionToAddCommandPluginNode == null)
            {
                this.EquipmentActionToAddCommandPluginElement = null;
                ((ConveyorDevice)this.Device).ConveyorEquipmentActionToAddCommandPlugin = null;
            }
            else
            {
                this.EquipmentActionToAddCommandPluginElement = new EquipmentActionToAddCommandPluginElement(this.Device, equipmentActionToAddCommandPluginNode, this.WcsConfiguration);
                ((ConveyorDevice)this.Device).ConveyorEquipmentActionToAddCommandPlugin = this.EquipmentActionToAddCommandPluginElement.EquipmentActionToAddCommandPlugin;
            }
        }

        protected override Framework.Device CreateDevice()
        {
            ConveyorDevice conveyorDevice = new ConveyorDevice(Name, No, IPEndPoint,BindEndPoint, ReceiveTimeout, ConnectTimeout, SendTimeout, AllowConcurrency, DataReceiver,
                this.AllowTaskBlockSpace,
                this.CompareResultHandlerCollection.CompareResultHandlerElements.Select(x=>x.Handler).ToArray(),
                this.UsedConveyorTaskBlockIndexGetterCollection.UsedConveyorTaskBlockIndexGetterElements.Select(x => x.Getter).ToArray());
            return conveyorDevice;
        }
    }
}
