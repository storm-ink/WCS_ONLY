using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 应用程序启动时自加载对象配置节点集合
    /// </summary>
    /// <remarks>
    /// <b>属性</b><br />
    /// type:字符串，必填。指示该配置映射到的应用程序启动时自加载类型。必须为继承 <see cref="T:Wcs.Framework.ApplicationStartup"/> 的子类。
    /// </remarks>
    /// <example>
    /// <code lang="xml">
    /// <application><startups><startup type="Wcs.Framework.ClassXX, Wcs.Framework" /></startups></application>
    /// </code>
    /// </example>
    public class StartupElement:ConfigurationElement
    {
        /// <summary>
        /// 获取该配置映射到的应用程序启动时自加载实例
        /// </summary>
        public ApplicationStartup Startup { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public StartupElement(System.Xml.XmlNode node)
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

            if (node.Attributes["type"] == null)
            {
                throw new Exception("未指定 startup 节点的 type 属性");
            }

            string typeName = node.Attributes["type"].Value;

            this.Startup = CreateInstance<ApplicationStartup>(typeName);
        }
    }
}
