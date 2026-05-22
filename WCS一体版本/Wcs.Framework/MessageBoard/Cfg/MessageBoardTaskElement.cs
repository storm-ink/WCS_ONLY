using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework.MessageBoard.Cfg
{
    public class MessageBoardTaskElement : Wcs.Framework.Cfg.ConfigurationElement
    {
        public Type MessageBoardTaskType { get; private set; }
        public MessageBoardTaskElement(XmlNode node, Wcs.Framework.Cfg.WcsConfiguration configuration)
            : base(node, configuration)
        {

        }

        protected override void Deserialize()
        {
            var typeName = GetAttribute<String>("type");
            Type type = Type.GetType(typeName);

            if (type == null)
            {
                throw new System.Configuration.ConfigurationErrorsException(string.Format("{0} 中 type 属性中指定的类型 {1} 不存在。", this.Node.GetXPath(), typeName), this.Node);
            }

            if (!type.IsSubclassOf(typeof(AbstractMessageBoardTask)))
            {
                throw new System.Configuration.ConfigurationErrorsException(string.Format("{0} 中 type 属性中指定的类型 {1} 不是 {2} 的子类。", this.Node.GetXPath(), typeName, typeof(AbstractMessageBoardTask)), this.Node);
            }

            MessageBoardTaskType = type;
        }

        internal AbstractMessageBoardTask CreateTask()
        {
            var lsn = Wcs.TypeExtentions.CreateInstance<AbstractMessageBoardTask>(this.MessageBoardTaskType, this);

            return lsn;
        }
    }
}
