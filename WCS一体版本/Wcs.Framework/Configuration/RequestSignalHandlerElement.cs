using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 请求处理配置节点
    /// </summary>
    /// <remarks>
    /// <b>属性</b><br />
    /// type:字符串，必填。指示该配置映射到的请求处理程序类型。必须为继承 <see cref="T:Wcs.Framework.RequestSignalHandler"/> 的子类。
    /// </remarks>
    /// <example>
    /// <code lang="xml">
    /// <configuration><requestSignalHandlers><handler type="Wcs.Framework.Class1, Wcs.Framework" /></requestSignalHandlers></configuration>
    /// </code>
    /// </example>
    public class RequestSignalHandlerElement:ConfigurationElement
    {
        /// <summary>
        /// 获取此配置映射到的请求处理程序实例。
        /// </summary>
        public RequestSignalHandler RequestSignalHandler { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public RequestSignalHandlerElement(XmlNode node)
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
                throw new ArgumentNullException("node");
            }

            if (node.Attributes["type"] == null || string.IsNullOrWhiteSpace(node.Attributes["type"].Value))
            {
                throw new Exception("未配置 requestSignalHandlers 节点 handler 子节点的 type 属性");
            }

            LogTarget logTarget = null;
            string logTargetName = node.Attributes["logTarget"] == null ? "" : node.Attributes["logTarget"].Value;
            if (!string.IsNullOrWhiteSpace(logTargetName))
            {
                logTarget = Configuration.GetLogTarget(logTargetName);
            }

            this.RequestSignalHandler = CreateInstance<RequestSignalHandler>(node.Attributes["type"].Value, new object[] { logTarget });
        }
    }
}
