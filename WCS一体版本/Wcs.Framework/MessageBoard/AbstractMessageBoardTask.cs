using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Cfg;
using Wcs.Framework.MessageBoard.Cfg;

namespace Wcs.Framework.MessageBoard
{
    /// <summary>
    /// 表示该类的子类型是一个消息看板任务
    /// </summary>
    /// <remarks>任务是一个同期性的工作</remarks>
    public abstract class AbstractMessageBoardTask
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();
        System.Threading.Thread _thread;
        public AbstractMessageBoardTask(MessageBoardTaskElement element)
        {
            this.Interval = element.GetAttributeOrDefault<Int32>("interval", 15000);

            _thread = new System.Threading.Thread(Proc);
            _thread.Name = this.GetType().Name;
            _thread.IsBackground = true;
            _thread.StartAndManaged();
        }
        /// <summary>
        /// 任务执行周期（毫秒）
        /// </summary>
        public Int32 Interval { get; set; }
        /// <summary>
        /// 所属消息看板
        /// </summary>
        public AbstractMessageBoard MessageBoard { get; private set; }

        internal void SetMessageBoard(AbstractMessageBoard board)
        {
            this.MessageBoard = board;
        }

        /// <summary>
        /// 取出所有消息
        /// </summary>
        /// <returns></returns>
        protected abstract AbstractMessage[] FetchMessages();

        void Proc()
        {
            while (true)
            {
                try
                {
                    if (!Wcs.Framework.Cfg.WcsConfiguration.IsLoaded)
                    {
                        goto sleep;
                    }

                    if (this.MessageBoard == null)
                    {
                        goto sleep;
                    }

                    var messages = FetchMessages();
                    if (messages == null)
                    {
                        goto sleep;
                    }

                    foreach (var msg in messages)
	                {
                        this.MessageBoard.Add(msg);
	                }
                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, this);
                }

            sleep:
                System.Threading.Thread.Sleep(this.Interval);
            }
        }
    }
}
