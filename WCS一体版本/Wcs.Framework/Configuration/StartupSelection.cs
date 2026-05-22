using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 应用程序启动时自加载对象集合配置节点
    /// </summary>
    /// <example>
    /// <code lang="xml">
    /// <application><startups></startups></application>
    /// </code>
    /// </example>
    public class StartupSelection:ConfigurationElement
    {
        /// <summary>
        /// 获取所有应用程序启动时自加载对象配置节点集合
        /// </summary>
        public StartupElement[] StartupElements { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public StartupSelection(XmlNode node)
            : base(node) { }
        /// <summary>
        /// 执行解析
        /// </summary>
        /// <param name="node">配置节点</param>
        protected override void Deserialize(System.Xml.XmlNode node)
        {
            if (node == null || node.SelectNodes("startup")==null)
            {
                StartupElements = new StartupElement[] { };
                return;
            }

            List<StartupElement> startups = new List<StartupElement>();
            foreach (XmlNode startupNode in node.SelectNodes("startup"))
            {
                startups.Add(new StartupElement(startupNode));
            }
            this.StartupElements = startups.ToArray();
        }
    }
}
