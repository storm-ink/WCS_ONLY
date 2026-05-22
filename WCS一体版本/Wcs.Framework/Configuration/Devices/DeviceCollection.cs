using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 设备集合配置节点。
    /// </summary>
    /// <example>
    /// <code lang="xml">
    /// <configuration><devices></devices></configuration>
    /// </code>
    /// </example>
    public class DeviceCollection : ConfigurationElement
    {
        /// <summary>
        /// 获取所有具体设备集合的配置节点
        /// </summary>
        public ParticularDeviceCollection[] ParticularDeviceCollection { get; private set; }
        /// <summary>
        /// 系统层面物理动作过滤
        /// </summary>
        public SystemEquipmentActionScheduler SystemEquipmentActionScheduler { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public DeviceCollection(XmlNode node, WcsConfiguration configuration)
            : base(node, configuration)
        {
        }
        protected override void Deserialize()
        {
            List<ParticularDeviceCollection> particularDeviceSelections = new List<ParticularDeviceCollection>();
            foreach (XmlNode deviceNode in this.Node.ChildNodes)
            {
                if (deviceNode.NodeType != XmlNodeType.Element)
                {
                    WcsConfiguration._logger.Warn1("未处理的内容 " + deviceNode.OuterXml, this);
                    continue;
                }

                //系统层面的物理动作调度层
                if (deviceNode.Name == "systemEquipmentActionScheduler")
                {
                    var equipmentActionSchedulerElement = new EquipmentActionSchedulerElement(deviceNode, this.WcsConfiguration);
                    SystemEquipmentActionScheduler = new SystemEquipmentActionScheduler(null, equipmentActionSchedulerElement.FilterElements.Select(x => x.EquipmentActionSchedulerFilter).ToArray());
                    continue;
                }

                var typeName = GetAttribute<String>("type", deviceNode);
                var particularDeviceCollection = ReflectionHelper.CreateInstance<ParticularDeviceCollection>(typeName, deviceNode, this.WcsConfiguration);
                particularDeviceSelections.Add(particularDeviceCollection);
                var invaldConfig = particularDeviceSelections.SelectMany(x => x.DeviceElements).GroupBy(x => x.Name)
                    .Where(x => x.Count() > 1)
                    .Select(x => new
                    {
                        Name = x.Key,
                        Nodes = string.Join("\n", x.Select(w => w.Node.GetXPath()))
                    });
                if (invaldConfig.Count() > 0)
                {
                    throw new ConfigurationErrorsException(string.Format("存在重复的 name 配置节点\n {0}", string.Join("\n", invaldConfig.Select(x => x.Nodes).ToArray())));
                }
            }
            this.ParticularDeviceCollection = particularDeviceSelections.ToArray();
        }
    }
}
