using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 表示未找到配置节点的指定属性的错误
    /// </summary>
    public class NotFoundConfigElementAttributeException:Exception
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="attributeName">属性名</param>
        /// <param name="node">配置节点</param>
        public NotFoundConfigElementAttributeException(string attributeName, XmlNode node)
            : base(string.Format("配置节点 {0} 不存在属性 {1}", node.OuterXml, attributeName))
        {
            
        }
    }
}
