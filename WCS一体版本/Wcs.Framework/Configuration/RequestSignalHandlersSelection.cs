using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 请求处理配置集合节点，用于附加到在处理 <see cref="T:Wcs.Framework.Request"/> 请求对象过程中的一些操作。
    /// </summary>
    /// <remarks>
    /// 支持的操作包括：
    /// <ul>
    /// <li>请求生成前 <see cref="M:Wcs.Framework.RequestSignalHandler.BeforeSave"/></li>
    /// <li>请求取消前 <see cref="M:Wcs.Framework.RequestSignalHandler.BeforeCancel"/></li>
    /// <li>请求被使用前 <see cref="M:Wcs.Framework.RequestSignalHandler.Processing"/></li>
    /// </ul>
    /// </remarks>
    /// <example>
    /// <code lang="xml">
    /// <configuration><requestSignalHandlers></requestSignalHandlers></configuration>
    /// </code>
    /// </example>
    public class RequestSignalHandlersSelection:ConfigurationElement
    {
        List<RequestSignalHandlerElement> requestSignalHandlerElements = new List<RequestSignalHandlerElement>();
        /// <summary>
        /// 获取所有请求处理配置节点
        /// </summary>
        public RequestSignalHandlerElement[] RequestSignalHandlerElements
        {
            get
            {
                return requestSignalHandlerElements.ToArray();
            }
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public RequestSignalHandlersSelection(XmlNode node)
            : base(node) { }
        /// <summary>
        /// 执行解析
        /// </summary>
        /// <param name="node">配置节点</param>
        protected override void Deserialize(XmlNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            foreach (XmlNode item in node.SelectNodes("handler"))
            {
                requestSignalHandlerElements.Add(new RequestSignalHandlerElement(item));
            }
        }
    }
}
