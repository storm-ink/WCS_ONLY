using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework.MessageBoard.Boards
{
    public sealed class MemoryMessageBoard:AbstractMessageBoard
    {
        static System.Threading.Thread _thread;
        List<AbstractMessage> _messages = new List<AbstractMessage>();
        Boolean _doStop = false;
        public override  AbstractMessage[] Messages
        {
            get { return _messages.ToArray(); }
        }

        public MemoryMessageBoard(XmlNode cfg):base(cfg)
        {
            _thread = new System.Threading.Thread(Proc);
            _thread.Name = "MemoryMessageBoard";
            _thread.IsBackground = true;
            _thread.StartAndManaged(this);
        }

        public override void Add(AbstractMessage newMsg)
        {
            var existsMsg = _messages.Find(x =>  AbstractMessage.IsSynonym(x,newMsg));
            if (existsMsg != null)
            {
                Remove(existsMsg);
                newMsg.Id = existsMsg.Id;
            }
            _messages.Add(newMsg);

            WriteToListeners(newMsg);

            FireAddedEvent(newMsg);
        }

        public override void Remove(AbstractMessage msg)
        {
            _messages.Remove(msg);

            WriteToListeners(msg);

            FireRemovedEvent(msg);
        }

        void Proc(object stat)
        {
            while (!_doStop)
            {
                System.Threading.Thread.Sleep(500);
                try
                {
                    var array = _messages.ToArray();
                    foreach (var item in array)
                    {
                        if (item == null || item.IsOverdued)
                        {
                            Remove(item);
                        }
                    }
                }
                catch (Exception)
                {

                }
            }
            _doStop = false;
        }

        public override AbstractMessage Get(ulong id)
        {
            var msg = _messages.FirstOrDefault(x => x.Id == id);
            if (msg == null)
            {
                throw new InvalidOperationException(string.Format("未找到 id 为 {0} 的消息", id));
            }

            return msg;
        }

        public override void Dispose()
        {
            _doStop = true;
            _messages = null;
        }
    }
}
