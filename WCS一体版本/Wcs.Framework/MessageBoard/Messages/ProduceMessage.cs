using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.MessageBoard.Messages
{
    /// <summary>
    /// 表示一个根据指定判断条件失效的消息。根据传入的判断要件判断是否已过期
    /// </summary>
    /// <typeparam name="TDevice">泛型参数，设备类型</typeparam>
    public sealed class ProduceMessage : AbstractMessage
    {
        Func<Boolean> _isOverdued;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="isOverdued">消息是否过期判定函数</param>
        public ProduceMessage(MessageLevel level,String source,String title,String description, Func<Boolean> isOverdued)
            : base(level, source, title, description)
        {
            _isOverdued = isOverdued;
        }

        public override bool IsOverdued
        {
            get
            {
                return _isOverdued();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
