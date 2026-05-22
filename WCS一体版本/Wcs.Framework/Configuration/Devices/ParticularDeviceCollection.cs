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
    public abstract class ParticularDeviceCollection:ConfigurationElement
    {
        /// <summary>
        /// 获取设备配置节点集合
        /// </summary>
        public DeviceElement[] DeviceElements { get; private set; }
        /// <summary>
        /// 设备配置节点类型
        /// </summary>
        public abstract DeviceElement CreateDeviceElement(XmlNode deviceNode);
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public ParticularDeviceCollection(XmlNode node, WcsConfiguration configuration)
            : base(node,configuration)
        {
        }
        /// <summary>
        /// 执行解析
        /// </summary>
        /// <param name="node">配置节点</param>
        protected override void Deserialize()
        {
            List<DeviceElement> deviceElements = new List<DeviceElement>();
            foreach (XmlNode item in this.Node.ChildNodes)
            {
                if (item.NodeType == XmlNodeType.Element)
                {
                    var deviceElement = CreateDeviceElement(item);
                    if (deviceElements.Any(x => string.Equals(x.Name, deviceElement.Name, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        throw new System.Configuration.ConfigurationErrorsException(string.Format("{0} 节点 {1} 属性值 “{2}” 重复", item.GetXPath(), "name", deviceElement.Name));
                    }

                    deviceElements.Add(deviceElement);
                }
                else
                {
                    WcsConfiguration._logger.Warn1("未处理的内容 " + item.OuterXml,this);
                }
            }

            this.DeviceElements = deviceElements.ToArray();
        }
    }
}
