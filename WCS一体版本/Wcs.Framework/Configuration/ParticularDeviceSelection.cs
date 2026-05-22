using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 表示一个具体的设备集合节点，所有的设备集合节点必须继承此类。
    /// </summary>
    /// <example>
    /// <h1>此处仅演示如何添加一个新的设备集合节点类型</h1>
    /// <ul>
    /// <li>1、添加一个新类，必继承 <see cref="T:Wcs.Framework.Cfg.ParticularDeviceSelection"/>.
    /// <code>
    ///public class SampleDeviceSelection : Wcs.Framework.Cfg.ParticularDeviceSelection
    ///{
    ///    public override Type DeviceElementType
    ///    {
    ///        get { return typeof(SampleDeviceElement); }
    ///    }
    ///}
    ///SampleDeviceElement 是设备节点，点击 <see cref="T:Wcs.Framework.Cfg.DeviceElement"/> 查看如何定义新的设备节点.
    /// </code>
    /// </li>
    /// <li>2、在 wcs.config 文件的 devices 节点中添加一个子节点，名称随意。并指定 selectionType 属性为我们新添加的类名 SampleDeviceSelection.
    /// <code lang="xml">
    /// <devices>
    /// <sampleDevices selectionType="SampleLib.SampleDeviceSelection, SampleLib">
    /// </sampleDevices>
    /// </devices>
    /// </code>
    /// </li>
    /// </ul>
    /// </example>
    public abstract class ParticularDeviceSelection:ConfigurationElement
    {
        List<DeviceElement> deviceElements = new List<DeviceElement>();
        /// <summary>
        /// 获取设备配置节点集合
        /// </summary>
        public DeviceElement[] DeviceElements
        {
            get
            {
                return deviceElements.ToArray();
            }
        }
        /// <summary>
        /// 设备配置节点类型
        /// </summary>
        public abstract Type DeviceElementType { get; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public ParticularDeviceSelection(XmlNode node)
            : base(node)
        {
        }
        /// <summary>
        /// 执行解析
        /// </summary>
        /// <param name="node">配置节点</param>
        protected override void Deserialize(System.Xml.XmlNode node)
        {
            if (node == null)
            {
                return;
            }

            foreach (XmlNode item in node.ChildNodes)
            {
                var deviceElement = CreateInstance<DeviceElement>(DeviceElementType.AssemblyQualifiedName, item);
                deviceElements.Add(deviceElement);
            }
        }
    }
}
