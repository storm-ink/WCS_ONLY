using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework.Cfg;
namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 输送线设备集合配置节点的默认实现。
    /// </summary>
    /// <remarks>
    /// 该节点包含一个 selectionType 属性，必填。指示其使用哪个对象进行解析。<br />
    /// </remarks>
    /// <example>
    /// <code lang="xml">
    /// <configuration><devices><conveyors type="Wcs.Framework.Cfg.ConveyorsSelection, Wcs.Framework"></conveyors></devices></configuration>
    /// </code>
    /// <seealso cref="T:Wcs.Framework.Cfg.ConveyorElement"/>
    /// </example>
    public class ConveyorCollection :Wcs.Framework.Cfg.ParticularDeviceCollection
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public ConveyorCollection(XmlNode node, WcsConfiguration configuration)
            : base(node,configuration)
        {
        }

        public override DeviceElement CreateDeviceElement(XmlNode deviceNode)
        {
            return new ConveyorElement(deviceNode,this.WcsConfiguration);
        }
    }
}
