using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Wcs.Framework.EventBus
{
    public interface IEventHandler<TEvent> where TEvent:IEvent
    {
        /// <summary>
        /// 指示该处理程序在发生错误（未正常完成）时，是否需要保持并尝试重新通知，直到成功。
        /// </summary>
        Boolean Hold { get; }
        /// <summary>
        /// 执行该方法
        /// </summary>
        /// <param name="evt"></param>
        void Handle(TEvent evt);
    }
}
