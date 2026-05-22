using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.MessageBoard.Messages
{
    /// <summary>
    /// 表示一个提示性的消息，自消息产生的时刻起，它就已经失效。
    /// </summary>
    public sealed class TipMessage:AbstractMessage
    {
        public TipMessage(MessageLevel level, String source, String title, String description)
            : base(level,source,title,description)
        {
            
        }
        public override bool IsOverdued
        {
            get
            {
                return true;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
