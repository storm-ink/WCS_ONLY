using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Cfg;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 主应用程序配置节点
    /// </summary>
    /// <remarks>
    /// <b>属性</b><br />
    /// logTarget：字符串，日志输出目标名称。引用自 <see cref="T:Wcs.Framework.Cfg.LogTargetsSelection"/> 节点内的 <see cref="T:Wcs.Framework.Cfg.LogTargetElement"/> 配置。<br />
    /// <b>子节点</b><br />
    /// <see cref="T:Wcs.Framework.Cfg.StartupSelection"/>
    /// </remarks>
    /// <example>
    /// <code lang="xml">
    /// <configuration><application logTarget="Wcs.App"></application></configuration>
    /// </code>
    /// </example>
    public class ApplicationElement :ConfigurationElement
    {
        /// <summary>
        /// 日志输出对象<br />
        /// 如果 application 配置节点中未指定输出对象，将自动创建一个名为 Wcs.App 的 NLogTarget 以供使用。
        /// </summary>
        public LogTarget LogTarget { get; private set; }
        /// <summary>
        /// 应用程序启动时加载对象节点
        /// </summary>
        public StartupSelection StartupSelection { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点对象</param>
        public ApplicationElement(XmlNode node) : base(node) { }
        /// <summary>
        /// 解析配置
        /// </summary>
        /// <param name="node">配置节点</param>
        protected override void Deserialize(System.Xml.XmlNode node)
        {
            if (node == null)
            {
                return;
            }

            string logTargetName = node.Attributes["logTarget"] == null ? "" : node.Attributes["logTarget"].Value;
            if (!string.IsNullOrWhiteSpace(logTargetName))
            {
                LogTarget = Configuration.GetLogTarget(logTargetName);
            }
            else
            {
                LogTarget = new Wcs.Framework.Impl.NLogTarget("Wcs.App");
            }

            this.StartupSelection = new StartupSelection(node.SelectSingleNode("startups"));
        }
    }
}
