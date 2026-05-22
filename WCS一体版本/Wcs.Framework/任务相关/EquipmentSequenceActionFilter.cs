using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework
{
    /// <summary>
    /// 序列动作过滤器，用来在发送任务时对任务进行判断，显示是否可以发送
    /// </summary>
    public abstract class EquipmentSequenceActionFilter
    {
        /// <summary>
        /// 配置节点，可将一些复杂的自定义参数写在这里，然后在各子类内部解析
        /// </summary>
        public XmlNode FilterNode { get; protected set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="filterNode">配置节点</param>
        public EquipmentSequenceActionFilter(XmlNode filterNode)
        {
            FilterNode = filterNode;
        }
        /// <summary>
        /// 判断指定的动作是否可以发送
        /// </summary>
        /// <param name="action">要发送的动作</param>
        /// <returns>返回 true 表示动作可以发送；false 表示动作不可以发送。</returns>
        public abstract Boolean CanSend(EquipmentAction action);
    }
}
