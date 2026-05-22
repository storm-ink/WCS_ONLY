using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework.MessageBoard.Cfg
{
    public class MessageBoardListenerElement : Wcs.Framework.Cfg.ConfigurationElement
    {
        public Type MessageBoardListenerType { get; private set; }
        public String Name { get; private set; }
        public Boolean Enabled { get; private set; }
        public Boolean Async { get; private set; }
        public MessageBoardListenerElement(XmlNode node, Wcs.Framework.Cfg.WcsConfiguration configuration)
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

            if (!type.IsSubclassOf(typeof(AbstractMessageBoardListener)))
            {
                throw new System.Configuration.ConfigurationErrorsException(string.Format("{0} 中 type 属性中指定的类型 {1} 不是 {2} 的子类。", this.Node.GetXPath(), typeName, typeof(AbstractMessageBoardListener)), this.Node);
            }

            MessageBoardListenerType = type;

            this.Name = GetAttributeOrDefault<String>("name", type.Name);
            this.Enabled = GetAttributeOrDefault<Boolean>("enabled", true);
            this.Async = GetAttributeOrDefault<Boolean>("async", true);
        }

        internal AbstractMessageBoardListener CreateListener()
        {
            var lsn = Wcs.TypeExtentions.CreateInstance<AbstractMessageBoardListener>(this.MessageBoardListenerType, this.Name, this.Async, this.Enabled, this.Node);

            return lsn;
        }
    }
}
