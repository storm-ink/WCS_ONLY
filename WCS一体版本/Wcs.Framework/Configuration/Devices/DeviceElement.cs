using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 设备配置节点。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 表示一个具体的设备的配置节点对象。
    /// </para>
    /// <para>
    /// 属性：<br />
    /// deviceName：字符串，在 Devices 节点范围内必须唯一。必填项。<br />
    /// deviceNo:数字，指定设备的编号。同类型的设备中必须唯一。必填项。<br />
    /// ip:ip地址。必填项。<br />
    /// port:数字，网络端口。一般用于配置 ip 地址一起使用。必填项。<br />
    /// allowConcurrency:布尔值，指示设备是否允许并行任务，默认 false。true 时表示设备可以同时接收多个任务(比如输送线);false 表示设备同时只能执行一条任务(比如堆垛机)。<br />
    /// connectTimeout:数字，连接超时时长，默认 3000。单位：ms。<br />
    /// sendTimeout:数字，发送超时时长，默认 2000。单位：ms。用于在给设备发送数据时，如果在指定的时间内还未接收到设备的回应，表示超时。<br />
    /// receiveTimeout：数字，接收超时时长，默认 2000。单位：ms。Wcs 在连接到设备后，如果在指定的时间内未收到设备发回的数据，表示超时。<br />
    /// logTarget：字符串，日志输出目标名称。引用自 <see cref="T:Wcs.Framework.Cfg.LogTargetsSelection"/> 节点内的 <see cref="T:Wcs.Framework.Cfg.LogTargetElement"/> 配置。
    /// </para>
    /// <para>
    /// 子节点：<see cref="T:Wcs.Framework.Cfg.SequenceSelection"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code lang="xml">
    /// <xxDevice name="某类型设备" no="1" allowConcurrency="true" ip="127.0.0.1" port="8000" senderDecoder="netPackageDecoder2" receiverDecoder="netPackageDecoder1" receiveTimeout="2000" connectTimeout="10000" sendTimeout="3000" logTarget="设备状态" />
    /// </code>
    /// </example>
    public abstract class DeviceElement:ConfigurationElement
    {
        /// <summary>
        /// 设备名称
        /// </summary>
        public String Name { get; protected set; }
        /// <summary>
        /// 设备号
        /// </summary>
        public Int32 No { get; protected set; }
        /// <summary>
        /// 接收超时时长
        /// </summary>
        public Int32 ReceiveTimeout { get; protected set; }
        /// <summary>
        /// 连接超时时长
        /// </summary>
        public Int32 ConnectTimeout { get; protected set; }
        /// <summary>
        /// 指示无法连接到设备的异常应该在连接失败多少秒后引发
        /// </summary>
        public Int32? UnableToConnectErrorTimeout { get; protected set; }
        /// <summary>
        /// 报警工厂
        /// </summary>
        public DefaultDeviceWarningFactory DeviceWarningFactory { get; protected set; }
        /// <summary>
        /// 该节点生成的设备对象
        /// </summary>
        public Device Device { get; protected set; }
        /// <summary>
        /// 创建设备
        /// </summary>
        /// <returns></returns>
        protected abstract Device CreateDevice();
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public DeviceElement(XmlNode node, WcsConfiguration configuration)
            : this(node,configuration, true)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public DeviceElement(XmlNode node, WcsConfiguration configuration, Boolean deserialize)
            : base(node,configuration, false)
        {
            Name = GetAttribute<String>("name");
            No = GetAttribute<Int32>("no");
            ReceiveTimeout = GetAttributeOrDefault("receiveTimeout",node, 2000);
            ConnectTimeout = GetAttributeOrDefault("connectTimeout",node, 2000);
            UnableToConnectErrorTimeout = GetAttributeOrDefault<Int32?>("unableToConnectErrorTimeout",node,null);

            String deviceErrorFactoryTypeName = GetAttributeOrDefault<String>("deviceWarningFactory");
            if (!string.IsNullOrWhiteSpace(deviceErrorFactoryTypeName))
            {
                var type = Type.GetType(deviceErrorFactoryTypeName);
                if (type == null)
                {
                    var path = this.Node.GetXPath();
                    throw new System.Configuration.ConfigurationErrorsException(string.Format("未找到 {0} 节点 {1} 属性值 “{2}” 标识的类型", path, "type", deviceErrorFactoryTypeName),this.Node);
                }

                if (!type.IsSubclassOf(typeof(DefaultDeviceWarningFactory)) && type != typeof(DefaultDeviceWarningFactory))
                {
                    var path = this.Node.GetXPath();
                    throw new System.Configuration.ConfigurationErrorsException(string.Format("{0} 节点 {1} 属性值 “{2}” 标识的类型不是 {3} 的子类", path, "type", deviceErrorFactoryTypeName, typeof(DefaultDeviceWarningFactory)), this.Node);
                }

                this.DeviceWarningFactory = ReflectionHelper.CreateInstance<DefaultDeviceWarningFactory>(type.FullName, null);
            }

            if (deserialize)
            {
                Deserialize();
            }
        }

        protected override void Deserialize()
        {
            if (this.Device != null)
            {
                throw new ConfigurationErrorsException("{0} 节点设备 {1} 的 Device 实例被初始化，不允许重复操作。\n这通常是在子类中重写了 Deserialize 方法或在构造函数中对 Device 属性进行了赋值，并重复调用了 Base.Deserialize 方法。");
            }
            this.Device = CreateDevice();

            if (UnableToConnectErrorTimeout != null)
            {
                Device.UnableToConnectErrorTimeout = this.UnableToConnectErrorTimeout.Value;
            }
        }
    }
}
