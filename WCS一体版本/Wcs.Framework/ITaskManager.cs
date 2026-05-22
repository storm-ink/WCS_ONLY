using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.Framework
{
    /// <summary>
    /// 任务管理器接口
    /// </summary>
    public interface ITaskManager
    {
        /// <summary>
        /// 暂停指定的任务
        /// </summary>
        /// <param name="task">任务</param>
        void Suspend(Task task);
        /// <summary>
        /// 暂停指定的任务
        /// </summary>
        /// <param name="task">任务</param>
        /// <param name="unitOfWork">数据库持久化上下文</param>
        void Suspend(Task task, INHUnitOfWork unitOfWork);

        /// <summary>
        /// 取消指定的任务
        /// </summary>
        /// <param name="task">任务</param>
        void Cancel(Task task);
        /// <summary>
        /// 取消指定的任务
        /// </summary>
        /// <param name="task">任务</param>
        /// <param name="unitOfWork">数据库持久化上下文</param>
        void Cancel(Task task, INHUnitOfWork unitOfWork);
        
        /// <summary>
        /// 继续执行指定的任务
        /// </summary>
        /// <param name="task">任务</param>
        void Resume(Task task);
        /// <summary>
        /// 继续执行指定的任务
        /// </summary>
        /// <param name="task">任务</param>
        /// <param name="unitOfWork">数据库持久化上下文</param>
        void Resume(Task task, INHUnitOfWork unitOfWork);
    }
}
