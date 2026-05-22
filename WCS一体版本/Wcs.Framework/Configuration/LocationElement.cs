using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework.Devices;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 位置配置节点
    /// </summary>
    /// <remarks>
    /// 包含属性如下<br />
    /// id：数字，必填。全局唯一不可重复，标识量。大于 0 小于 32767。此值为电气提供，该值在被设置直接识别。一个项目中即使不同的设备也不能出现同样的货位号，如果电气在为货位编号时出现重置让电气解决（基本原则）。<br />
    /// userCode:字符串，用户编码，必填。在显示位置对象时此值通常会被展示给用户。<br />
    /// sameAs:用户描述和此对象表示的位置一样，但表述不一样的对象。对应到 <see cref="T:Wcs.Framework.Devices.Location"/> 的 <see cref="P:Wcs.Framework.Devices.Location.SameAs"/> 属性。<br />
    /// 此属性通常用在不同设备的对接点位置上<br />
    /// 比如：堆垛机入库时的输送线取货点，需要将输送线位置映射到货架位置。
    /// </remarks>
    /// <example>
    /// <para>此处演示了如何将输送线位置映射到堆垛机的货架位置。</para>
    /// <code lang="xml">
    /// <locations><location id="1" userCode="输送位置1" sameAs="01000001@c001" /></locations>
    /// </code>
    /// <para>
    /// 其中 id 为位置在输送线中的物理位置编号（设备可识别的编码值）；<br />
    /// userCode 为上层软件在使用时展示给用户的值；
    /// sameAs 中指定的是一个可转换为位置对象的编码串，其包含了两部分信息：<br />
    /// 以@符号分割，前一部分数字表示位置在设备中的编码值，后一部分表示设备名称。<br />
    /// 此处的值表示堆垛机c001的货架位置1排0列1层
    /// </para>
    /// </example>
    /// <seealso cref="T:Wcs.Framework.Cfg.LocationsSelection"/>
    public class LocationElement:ConfigurationElement
    {
        /// <summary>
        /// 该配置映射到的位置对象
        /// </summary>
        public Location Location { get; protected set; }
        /// <summary>
        /// 和此对象表示的位置一样，但表述不一样的对象
        /// </summary>
        /// <remarks>
        /// 这里可以填写一组值，用半角逗号隔开。但其值必须为系统可识别（可二次转换的）编码值。
        /// </remarks>
        public String SameAsAttribute { get; protected set; }
        /// <summary>
        /// 与该配置节点关联的设备配置节点
        /// </summary>
        public DeviceElement DeviceElement { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        /// <param name="deviceElement">设备配置节点</param>
        public LocationElement(XmlNode node, DeviceElement deviceElement)
            : base(node,false)
        {
            DeviceElement = deviceElement;
            this.Deserialize(node);
        }
        /// <summary>
        /// 绑定相同位置关联
        /// </summary>
        public void ResolveSameAs()
        {
            if (!DevicesSelection.Initialized)
            {
                throw new Exception("DevicesSelection 未初始化，无法解析 Location.SameAs 属性");
            }

            if (string.IsNullOrWhiteSpace(SameAsAttribute))
            {
                return;
            }

            List<Location> sameAsLocations = new List<Location>();
            foreach (var sameAs in SameAsAttribute.Split(','))
            {
                Device device;
                if (sameAs.Split('@').Length != 2)
                {
                    throw new Exception(String.Format("{0} 无法被识别为位置描述信息，完整的描述信息因为是 deviceCode@deviceName", sameAs));
                }
                else
                {
                    device = Configuration.GetDevice(sameAs.Split('@')[1]);
                    if (device == null)
                    {
                        throw new Exception(String.Format("尝试解析位置描述信息 {0} 未匹配到相应设备对象", sameAs));
                    }
                }
                var location = Location.TryParse(sameAs, null, device);
                if (location == null)
                {
                    throw new Exception(String.Format("{0} 无法转换为 Location 引用", sameAs));
                }
                sameAsLocations.Add(location);
            }

            this.Location.SameAs = sameAsLocations.ToArray();
        }
        /// <summary>
        /// 执行解析
        /// </summary>
        /// <param name="node">配置节点</param>
        protected override void Deserialize(XmlNode node)
        {
            this.Location = DeviceElement.CreateLocation(node);
            this.SameAsAttribute = node.Attributes["sameAs"] == null ? null : node.Attributes["sameAs"].Value;
        }
    }
}
