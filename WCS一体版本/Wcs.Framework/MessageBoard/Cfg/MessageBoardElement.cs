using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework.MessageBoard.Boards;

namespace Wcs.Framework.MessageBoard.Cfg
{
    public class MessageBoardElement:Wcs.Framework.Cfg.ConfigurationElement
    {
        public MessageBoardElement(XmlNode node, Wcs.Framework.Cfg.WcsConfiguration configuration)
            : base(CreateDefaultNode(node), configuration, true)
        {

        }

        public MessageBoardListenerElement[] Listeners { get; private set; }

        public Type MessageBoardType { get; private set; }
        protected override void Deserialize()
        {
            var typeName = GetAttribute<String>("type");
            Type type = Type.GetType(typeName);

            if (type == null)
            {
                throw new System.Configuration.ConfigurationErrorsException(string.Format("{0} 中 type 属性中指定的类型 {1} 不存在。", this.Node.GetXPath(), typeName), this.Node);
            }

            if (!type.IsSubclassOf(typeof(AbstractMessageBoard)))
            {
                throw new System.Configuration.ConfigurationErrorsException(string.Format("{0} 中 type 属性中指定的类型 {1} 不是 {2} 的子类。", this.Node.GetXPath(), typeName, typeof(AbstractMessageBoard)), this.Node);
            }

            MessageBoardType = type;

            List<MessageBoardListenerElement> listeners = new List<MessageBoardListenerElement>();
            foreach (XmlNode listenerNode in this.Node.SelectNodes("listener"))
            {
                listeners.Add(new MessageBoardListenerElement(listenerNode, this.WcsConfiguration));
            }

            List<MessageBoardTaskElement> tasks = new List<MessageBoardTaskElement>();
            foreach (XmlNode taskNode in this.Node.SelectNodes("task"))
            {
                tasks.Add(new MessageBoardTaskElement(taskNode, this.WcsConfiguration));
            }

            var messageBoard = Wcs.TypeExtentions.CreateInstance<AbstractMessageBoard>(type, this.Node);

            foreach (var item in listeners)
            {
                try
                {
                    var lsn = item.CreateListener();
                    messageBoard.AddListener(lsn);
                }
                catch (Exception ex)
                {
                    Wcs.Framework.Cfg.WcsConfiguration._logger.Error1(new System.Configuration.ConfigurationErrorsException(String.Format("{0}初始化失败",item.Name),ex,item.Node), this);
                }
            }

            foreach (var item in tasks)
            {
                var tsk = item.CreateTask();
                messageBoard.AddTask(tsk);
            }

            Listeners = listeners.ToArray();

            AbstractMessageBoard.Instance = messageBoard;
        }

        static XmlNode CreateDefaultNode(XmlNode node)
        {
            if (node.Attributes["type"] == null)
            {
                var attr = node.OwnerDocument.CreateAttribute("type");
                attr.Value = string.Format("{0}, {1}", typeof(MemoryMessageBoard).FullName,
                    System.IO.Path.GetFileNameWithoutExtension(new Uri(typeof(MemoryMessageBoard).Assembly.CodeBase).LocalPath)
                    );

                node.Attributes.Append(attr);
            }

            return node;
        }
    }
}
