using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework.Devices;

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
        public String DeviceName { get; protected set; }
        /// <summary>
        /// 是否允许并行任务
        /// </summary>
        public Boolean AllowConcurrency { get; protected set; }
        /// <summary>
        /// 设备号
        /// </summary>
        public Int32 DeviceNo { get; protected set; }
        /// <summary>
        /// 端口号
        /// </summary>
        public Int32 Port { get; protected set; }
        /// <summary>
        /// ip 地址
        /// </summary>
        public String Ip { get; protected set; }
        /// <summary>
        /// 接收超时时长
        /// </summary>
        public Int32 ReceiveTimeout { get; protected set; }
        /// <summary>
        /// 连接超时时长
        /// </summary>
        public Int32 ConnectTimeout { get; protected set; }
        /// <summary>
        /// 发送超时时长
        /// </summary>
        public Int32 SendTimeout { get; protected set; }
        /// <summary>
        /// 日志输入目标
        /// </summary>
        public LogTarget LogTarget { get; protected set; }
        /// <summary>
        /// 任务序列配置节点
        /// </summary>
        public SequenceSelection SequenceSelection { get; protected set; }
        /// <summary>
        /// 该节点生成的设备对象
        /// </summary>
        public Device Device { get; protected set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public DeviceElement(XmlNode node)
            : base(node,false)
        {
            if (node.Attributes["name"] == null)
            {
                throw new NotFoundConfigElementAttributeException("name", node);
            }
            if (node.Attributes["no"] == null)
            {
                throw new NotFoundConfigElementAttributeException("no", node);
            }
            if (node.Attributes["port"] == null)
            {
                throw new NotFoundConfigElementAttributeException("port", node);
            }
            if (node.Attributes["ip"] == null)
            {
                throw new NotFoundConfigElementAttributeException("ip", node);
            }

            if (node.Attributes["allowConcurrency"] != null)
            {
                AllowConcurrency = Convert.ToBoolean(node.Attributes["allowConcurrency"].Value);
            }
            DeviceName = node.Attributes["name"].Value;
            DeviceNo = Convert.ToInt32(node.Attributes["no"].Value);
            Port = Convert.ToInt32(node.Attributes["port"].Value);
            Ip = node.Attributes["ip"].Value;
            ReceiveTimeout = 2000;
            if (node.Attributes["receiveTimeout"] != null)
            {
                ReceiveTimeout = Convert.ToInt32(node.Attributes["receiveTimeout"].Value);
            }
            ConnectTimeout = 3000;
            if (node.Attributes["connectTimeout"] != null)
            {
                ConnectTimeout = Convert.ToInt32(node.Attributes["connectTimeout"].Value);
            }
            SendTimeout = 2000;
            if (node.Attributes["sendTimeout"] != null)
            {
                SendTimeout = Convert.ToInt32(node.Attributes["sendTimeout"].Value);
            }

            LogTarget = null;
            string logTargetName = node.Attributes["logTarget"] == null ? "" : node.Attributes["logTarget"].Value;
            if (!string.IsNullOrWhiteSpace(logTargetName))
            {
                LogTarget = Configuration.GetLogTarget(logTargetName);
            }

            this.Deserialize(node);
        }
        /// <summary>
        /// 从指定的位置配置节点中创建一个位置对象 
        /// </summary>
        /// <param name="locationNode">位置配置节点</param>
        /// <returns></returns>
        public abstract Location CreateLocation(XmlNode locationNode);
        /// <summary>
        /// 绑定相同位置关联
        /// </summary>
        public abstract void ResolveLocationSameAs();
        /// <summary>
        /// 创建 sequence，<br />
        /// 不在 Deserialize 创建是因为 sequence 在创建后会立即运行，<br />
        /// 这样在其它配置未初始化之前会引发一些意想不到的错误
        /// </summary>
        public virtual void CreateSequences()
        {
            SequenceSelection.CreateSequence();
        }

        /// <summary>
        /// 获取此设备所有货位
        /// </summary>
        /// <returns>位置集合</returns>
        public abstract Location[] GetLocations();

    }
}
