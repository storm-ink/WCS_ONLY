using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 日志输出目标配置节点
    /// </summary>
    /// <remarks>
    /// <b>属性</b><br />
    /// name:字符串，必填。标识量，全局唯一。用于其它对象引用该对象的依据。<br />
    /// type:字符串，必填。指示该配置映射到的日志输入目标类型。必须为继承 <see cref="T:Wcs.LogTarget"/> 的子类。框架提供了两个默认实现 <see cref="T:Wcs.Framework.Impl.NLogTarget"/>、<see cref="T:Wcs.Framework.Impl.BasicAsyncLogFileTarget"/>。建议使用 <see cref="T:Wcs.Framework.Impl.NLogTarget"/>。<see cref="T:Wcs.Framework.Impl.BasicAsyncLogFileTarget"/> 存在很大的性能问题。
    /// </remarks>
    /// <example>
    /// <code lang="xml">
    /// <configuration>
    ///   <logTargets>
    ///     <target name="任务消息" type="Wcs.Framework.Impl.NLogTarget, Wcs.Framework" />
    ///   </logTargets>
    /// </configuration>
    /// </code>
    /// </example>
    public class LogTargetElement:ConfigurationElement
    {
        /// <summary>
        /// 获取此配置映射到的日志输入目标实例
        /// </summary>
        public LogTarget LogTarget { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public LogTargetElement(XmlNode node)
            : base(node)
        {
        }
        /// <summary>
        /// 执行解析
        /// </summary>
        /// <param name="node">配置节点</param>
        protected override void Deserialize(System.Xml.XmlNode node)
        {
            if (node.Attributes["type"] == null)
            {
                throw new Exception("未指定 netPackageDecoder 节点的 type 属性");
            }

            string typeName = node.Attributes["type"].Value;
            LogTarget target = CreateInstance<LogTarget>(typeName, node);
            LogTarget = target;
        }
    }
}
