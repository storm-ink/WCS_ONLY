using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.Framework
{
    /// <summary>
    /// 表示继承此接口的对象是一个任务事件处理程序
    /// </summary>
    /// <typeparam name="TArgs">事件参数类型</typeparam>
    public interface ITaskEventHandler<TArgs>
        where TArgs : HandleableEventArgs
    {
        void Handle(TaskableDevice device,NHibernate.ISession session, TArgs args);
    }
}
