using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 被延迟的 Wms 通知
    /// </summary>
    public class DelayedWmsNotify : IAggregateRoot
    {
        public DelayedWmsNotify()
        {
            this.CreatedAt = DateTime.Now;
        }
        public virtual Int32 Id { get; set; }
        /// <summary>
        /// 通知类型的完整类名
        /// </summary>
        public virtual String NotifyTypeFullName { get; set; }
        /// <summary>
        /// 通知方法
        /// </summary>
        public virtual String NotifyMethod { get; set; }
        /// <summary>
        /// 通知的数据类型
        /// </summary>
        public virtual String NotifyObjectType { get; set; }
        /// <summary>
        /// 通知数据主键（Id）
        /// </summary>
        public virtual Int32 NotifyObjectId { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTime CreatedAt { get; set; }
        /// <summary>
        /// 指定上下文数据是否是占位请求
        /// </summary>
        public virtual Boolean IsRequest()
        {
            return this.NotifyObjectType == typeof(Request).FullName;
        }
        /// <summary>
        /// 指示上下文数据是否是任务
        /// </summary>
        public virtual Boolean IsTask()
        {
            return this.NotifyObjectType == typeof(Task).FullName;
        }
        /// <summary>
        /// 执行通知操作
        /// </summary>
        /// <param name="unitOfWork">   持久化上下文. </param>
        public virtual void Notify(NHUnitOfWork unitOfWork)
        {
            if(WmsNotifyProvider.Notifys==null || WmsNotifyProvider.Notifys.Length==0)
            {
                return ;
            }

            dynamic context = null;
            if (IsRequest())
            {
                context = Repository.Query<Request>(unitOfWork, x => x.Id == this.NotifyObjectId).SingleOrDefault();
            }
            else if(IsTask())
            {
                context = Repository.Query<Task>(unitOfWork, x => x.Id == this.NotifyObjectId).SingleOrDefault();
            }

            if (context == null)
            {
                return;
            }

            WmsNotify notify = WmsNotifyProvider.Notifys.SingleOrDefault(x => x.GetType().FullName == this.NotifyTypeFullName);
            if (notify == null)
            {
                return;
            }

            var method = notify.GetType().GetMethod(this.NotifyMethod);
            if (method == null) 
            {
                return;
            }

            method.Invoke(notify, new object[] { context });
        }

        public override string ToString()
        {
            string type = "未知对象";
            if (IsRequest())
            {
                type = "任务请求";
            }
            else if (IsTask())
            {
                type = "主任务";
            }
            return string.Format("延迟通知 {0}.{1} {2}#{3}", NotifyTypeFullName, NotifyMethod, type, NotifyObjectId);
        }
    }
}
