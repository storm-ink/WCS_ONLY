using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs.Framework.EventBus;

namespace Wcs.Framework.Events
{
    [System.ComponentModel.Description("主任务已完成事件")]
    public class TaskFinishedEvent:IEvent
    {
        public virtual Int32 Id { get; set; }
        public virtual String TaskCode { get; set; }
        public virtual TaskStatus Status { get; set; }
        public virtual TaskBizType BizType { get; set; }
        public virtual String TaskType { get; set; }
        public virtual TaskSource Source { get; set; }
        public virtual DateTime FinishedAt { get; set; }
        protected TaskFinishedEvent()
        {
            this.CreatedAt = DateTime.Now;
        }
        public TaskFinishedEvent(Int32 id,String taskCode, TaskStatus status,TaskBizType bizType,TaskSource source,String taskType, DateTime finishedAt)
            : this()
        {
            this.Id = id;
            this.TaskCode = taskCode;
            this.Status = status;
            this.BizType = bizType;
            this.Source = source;
            this.TaskType = taskType;
            this.FinishedAt = finishedAt;

            this.EventKey = string.Format("TaskFinishedEvent_Task#{0}_{1}_{2}", id, taskCode, status.GetDescription());
        }


        public virtual bool Preservable
        {
            get { return true; }
            set {  }
        }

        public virtual DateTime CreatedAt { get; set; }

        public virtual string ToDescription()
        {
            return string.Format("{0} 的 Task#{1}({2})", Status.GetDescription(), Id, TaskCode);
        }



        public virtual string EventKey { get; set; }
    }
}
