using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    public sealed class AlarmElement:ConfigurationElement
    {
        /// <summary>
        /// 错误类型
        /// </summary>
        public String Type { get; private set; }
        /// <summary>
        /// 错误编码
        /// </summary>
        public String Code { get; private set; }
        /// <summary>
        /// 错误名称(显示给客户)
        /// </summary>
        public String Name { get; private set; }
        /// <summary>
        /// 错误描述
        /// </summary>
        public String Description { get; private set; }
        /// <summary>
        /// 解决方法
        /// </summary>
        public String Solution { get; private set; }
        /// <summary>
        /// 是否是故障
        /// </summary>
        public Boolean IsFault { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public AlarmElement(XmlNode node, WcsConfiguration configuration)
            : base(node,configuration)
        {

        }

        protected override void Deserialize()
        {
            this.Type = GetAttributeOrDefault<String>("type");
            this.Name = GetAttributeOrDefault<String>("name");
            this.Code = GetAttributeOrDefault<String>("code");
            this.Description = GetOrGenerateNode(this.Node, "description").InnerText;
            this.Solution = GetOrGenerateNode(this.Node, "solution").InnerText;
            this.IsFault = GetAttributeOrDefault<Boolean>("isFault");
        }
    }
}
