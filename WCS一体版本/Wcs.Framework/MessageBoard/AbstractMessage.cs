using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Wcs.Framework.MessageBoard
{
    public abstract class AbstractMessage
    {
        public AbstractMessage(MessageLevel level, String source,String title,String description)
        {
            lock (_incrementLocker)
            {
                if (_messageId == ulong.MaxValue)
                {
                    _messageId = ulong.MinValue;
                }
                _messageId++;
                Id = _messageId;
            }

            OccurringTime = DateTime.Now;
            Level = level;
            Source = source;
            Title = title;
            Description = description;
        }
        static ulong _messageId;
        static Object _incrementLocker = new object();
        ulong _id;
        /// <summary>
        /// 消息 Id
        /// </summary>
        public virtual ulong Id
        {
            get
            {
                return _id;

            }
            set
            {
                _id = value;
            }
        }
        /// <summary>
        /// 是否已过期
        /// </summary>
        public abstract Boolean IsOverdued { get; set; }
        /// <summary>
        /// 等级
        /// </summary>
        public virtual MessageLevel Level { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public virtual String Description { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public virtual String Title { get; set; }
        /// <summary>
        /// 消息来源
        /// </summary>
        public virtual String Source { get; set; }
        /// <summary>
        /// 发生时间
        /// </summary>
        public virtual DateTime OccurringTime { get; set; }

        /// <summary>
        /// 判断指定的两个消息是否同义（消息内容完全一样）
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Boolean IsSynonym(AbstractMessage a, AbstractMessage b)
        {
            if (a == b)
            {
                return true;
            }

            if (a == null)
            {
                return false;
            }

            if (b == null)
            {
                return false;
            }

            if (a.Description != b.Description)
            {
                return false;
            }

            if (a.Title != b.Title)
            {
                return false;
            }

            if (a.Level != b.Level)
            {
                return false;
            }

            if (a.Source != b.Source)
            {
                return false;
            }

            if (a.Title != b.Title)
            {
                return false;
            }

            return true;
        }

        public virtual String GetData()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<message>");
            sb.AppendFormat("<编号>{0}</编号>\n", this.Id);
            sb.AppendFormat("<是否过期>{0}</是否过期>\n", this.IsOverdued);
            sb.AppendFormat("<级别>{0}</级别>\n", this.Level);
            sb.AppendFormat("<源><![CDATA[{0}]]></源>\n", this.Source);
            sb.AppendFormat("<标题><![CDATA[{0}]]></标题>\n", this.Title);
            sb.AppendFormat("<描述><![CDATA[{0}]]></描述>\n", this.Description);
            sb.AppendFormat("<时间>{0}</时间>\n", this.OccurringTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

            AppendData(sb);

            sb.AppendLine("</message>");

            return sb.ToString();
        }

        protected virtual void AppendData(StringBuilder messageContext)
        {

        }
    }
}
