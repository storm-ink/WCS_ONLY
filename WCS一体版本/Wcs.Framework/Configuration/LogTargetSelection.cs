using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 日志输出目标集合配置节点
    /// </summary>
    /// <example>
    /// <code lang="xml">
    /// <configuration>
    ///   <logTargets></logTargets>
    /// </configuration>
    /// </code>
    /// </example>
    public class LogTargetsSelection:ConfigurationElement
    {
        List<LogTargetElement> logTargetElements = new List<LogTargetElement>();
        /// <summary>
        /// 获取日志输出目标配置节点集合
        /// </summary>
        public LogTargetElement[] LogTargetElements
        {
            get
            {
                return logTargetElements.ToArray();
            }
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public LogTargetsSelection(XmlNode node)
            : base(node)
        {
        }
        /// <summary>
        /// 解析配置
        /// </summary>
        /// <param name="node">配置节点</param>
        protected override void Deserialize(System.Xml.XmlNode node)
        {
            foreach (XmlNode item in node.SelectNodes("target"))
            {
                var targetElement = new LogTargetElement(item);
                if(logTargetElements
                    .Any(x=>x.LogTarget.Name.Equals(targetElement.LogTarget.Name,StringComparison.CurrentCultureIgnoreCase)))
                {
                    throw new Exception("已存在名为 " + targetElement.LogTarget.Name + " 的 LogTarget");
                }

                logTargetElements.Add(targetElement);
            }
        }
    }
}
