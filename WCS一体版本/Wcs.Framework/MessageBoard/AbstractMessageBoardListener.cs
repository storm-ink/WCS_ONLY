using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.MessageBoard
{
    /// <summary>
    /// 表示一个消息看板的侦听者
    /// </summary>
    public abstract class AbstractMessageBoardListener:IDisposable
    {
        /// <summary>
        /// 是否处于启用状态
        /// </summary>
        public virtual Boolean Enabled { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public virtual String Name { get; set; }

        public virtual Boolean Async { get; set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="enabled">是否启用</param>
        /// <param name="cfg">配置信息</param>
        public AbstractMessageBoardListener(String name,Boolean async,Boolean enabled,System.Xml.XmlNode cfg)
        {
            Enabled = enabled;
            Async = async;
            Name = name;
        }
        /// <summary>
        /// 写入一个消息
        /// </summary>
        /// <param name="messageBoard">消息看板</param>
        /// <param name="message">要写入的消息</param>
        public abstract void Write(AbstractMessageBoard messageBoard, AbstractMessage message);

        public abstract void Dispose();

        public override string ToString()
        {
            return string.Format("消息看板侦听#{0}", this.Name);
        }
    }
}
