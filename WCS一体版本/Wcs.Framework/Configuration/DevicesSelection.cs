using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework.Devices;

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
    public class DevicesSelection:ConfigurationElement
    {
        public static bool Initialized { get; private set; }
        Device[] devices = new Device[] { };
        Location[] locations = new Location[]{};
        /// <summary>
        /// 获取所有设备
        /// </summary>
        public Device[] Devices
        {
            get
            {
                lock (devices)
                {
                    if (devices.Length==0)
                    {
                        devices = ParticularDeviceSelections
                            .SelectMany(x => x.DeviceElements)
                            .Select(x => x.Device).ToArray();
                    }
                }

                return devices;
            }
        }
        /// <summary>
        /// 获取设备集合中所有位置
        /// </summary>
        public Location[] Locations
        {
            get
            {
                lock (locations)
                {
                    if (locations.Length==0)
                    {
                        List<Location> result = new List<Location>();
                        foreach (var deviceElement in ParticularDeviceSelections.SelectMany(x=>x.DeviceElements))
                        {
                            result.AddRange(deviceElement.GetLocations()); 
                        }

                        locations = result.ToArray();
                    }
                }

                return locations;
            }
        }
        /// <summary>
        /// 获取所有具体设备集合的配置节点
        /// </summary>
        public ParticularDeviceSelection[] ParticularDeviceSelections { get; protected set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public DevicesSelection(XmlNode node)
            : base(node)
        {
        }
        protected override void Deserialize(System.Xml.XmlNode node)
        {
            List<ParticularDeviceSelection> particularDeviceSelections = new List<ParticularDeviceSelection>();
            foreach (XmlNode deviceNode in node.ChildNodes)
            {
                if (deviceNode.Attributes["selectionType"] == null || string.IsNullOrWhiteSpace(deviceNode.Attributes["selectionType"].Value))
                {
                    throw new Exception(string.Format("{0} 节点未配置 selectionType 属性", deviceNode.LocalName));
                }
                var deviceSelection = CreateInstance<ParticularDeviceSelection>(deviceNode.Attributes["selectionType"].Value, deviceNode);
                particularDeviceSelections.Add(deviceSelection);
            }
            this.ParticularDeviceSelections = particularDeviceSelections.ToArray();

            Initialized = true;
        }
        /// <summary>
        /// 绑定相同位置关联
        /// </summary>
        public virtual void ResolveLocationSameAs()
        {
            foreach (var particularDeviceSelection in ParticularDeviceSelections)
            {
                foreach (var deviceElement in particularDeviceSelection.DeviceElements)
                {
                    deviceElement.ResolveLocationSameAs();
                }
            }
        }

        /// <summary>
        /// 创建 sequence，<br />
        /// 不在 Deserialize 创建是因为 sequence 在创建后会立即运行，<br />
        /// 这样在其它配置未初始化之前会引发一些意想不到的错误
        /// </summary>
        public virtual void CreateSequences()
        {
            foreach (var particularDeviceSelection in ParticularDeviceSelections)
            {
                foreach (var deviceElement in particularDeviceSelection.DeviceElements)
                {
                    deviceElement.CreateSequences();
                }
            }
        }
    }
}
