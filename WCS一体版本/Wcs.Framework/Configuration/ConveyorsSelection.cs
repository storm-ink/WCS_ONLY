using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework.Devices;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 输送线设备集合配置节点的默认实现。
    /// </summary>
    /// <remarks>
    /// 该节点包含一个 selectionType 属性，必填。指示其使用哪个对象进行解析。<br />
    /// </remarks>
    /// <example>
    /// <code lang="xml">
    /// <configuration><devices><conveyors selectionType="Wcs.Framework.Cfg.ConveyorsSelection, Wcs.Framework"></conveyors></devices></configuration>
    /// </code>
    /// <seealso cref="T:Wcs.Framework.Cfg.ConveyorElement"/>
    /// </example>
    public class ConveyorsSelection :ParticularDeviceSelection
    {
        /// <summary>
        /// 获取输送线设备配置节点集合
        /// </summary>
        public ConveyorElement[] ConveyorElements
        {
            get
            {
                return this.DeviceElements.Cast<ConveyorElement>().ToArray();
            }
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public ConveyorsSelection(XmlNode node)
            : base(node)
        {
        }
        /// <summary>
        /// 获取设备节点类型
        /// </summary>
        public override Type DeviceElementType
        {
            get { return typeof(ConveyorElement); }
        }
    }
}
