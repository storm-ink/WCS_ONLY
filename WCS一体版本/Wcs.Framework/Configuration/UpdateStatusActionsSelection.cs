using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 获取在状态更新时附加的额外动作集合节点
    /// </summary>
    /// <example>
    /// <code lang="xml">
    /// <updateStatusActions></updateStatusActions>
    /// </code>
    /// </example>
    /// <seealso cref="T:Wcs.Framework.Cfg.UpdateStatusActionElement"/>
    public class UpdateStatusActionsSelection:ConfigurationElement
    {
        List<UpdateStatusActionElement> updateStatusActionElements = new List<UpdateStatusActionElement>();
        /// <summary>
        /// 获取附加的额外动作节点集合
        /// </summary>
        public UpdateStatusActionElement[] UpdateStatusActionElements
        {
            get
            {
                return updateStatusActionElements.ToArray();
            }
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public UpdateStatusActionsSelection(XmlNode node)
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
                return;
            }

            foreach (XmlNode item in node.SelectNodes("action"))
            {
                var element = new UpdateStatusActionElement(item);
                updateStatusActionElements.Add(element);
            }
        }
    }
}
