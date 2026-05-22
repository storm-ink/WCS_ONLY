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
        public IApplicationStartup ApplicationStartup { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public StartupElement(System.Xml.XmlNode node, WcsConfiguration configuration)
            : base(node,configuration)
        {
        }
        /// <summary>
        /// 执行解析
        /// </summary>
        /// <param name="node">配置节点</param>
        protected override void Deserialize()
        {
            string typeName = GetAttribute<String>("type");
            var type = Type.GetType(typeName);
            if (type == null)
            {
                throw new System.Configuration.ConfigurationErrorsException(String.Format("未找到属性 type 指定的值 “{0}” 类型。", typeName), this.Node);
            }

            if (type.IsSubclassOf(typeof(ApplicationStartup)))
            {
                this.ApplicationStartup = ReflectionHelper.CreateInstance<ApplicationStartup>(typeName, this);
            }
            else
            {
                this.ApplicationStartup = ReflectionHelper.CreateInstance<IApplicationStartup>(typeName);
            }
            this.ApplicationStartup.Initialize(this);
        }
    }
}
