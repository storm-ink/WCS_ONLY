using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.Framework.Events
{
    [System.ComponentModel.Description("主任务的状态发生改变事件")]
    public class TaskStatusChangedEvent:Wcs.Framework.EventBus.IEvent
    {
        public virtual Int32 Id { get; set; }
        public virtual String TaskCode { get; set; }
        public virtual TaskStatus Status { get; set; }
        public virtual TaskBizType BizType { get; set; }
        public virtual String TaskType { get; set; }
        public virtual TaskSource Source { get; set; }
        protected TaskStatusChangedEvent()
        {
            this.CreatedAt = DateTime.Now;
        }
        public TaskStatusChangedEvent(Int32 id, String taskCode, TaskStatus status, TaskBizType bizType, TaskSource source, String taskType)
            :this()
        {
            this.Id = id;
            this.TaskCode = taskCode;
            this.Status = status;
            this.BizType = bizType;
            this.Source = source;
            this.TaskType = taskType;
            this.EventKey = string.Format("TaskStatusChangedEvent_Task#{0}_{1}_{2}_{3}_{4}_{5}", id, taskCode, status.GetDescription(), BizType.GetDescription(), TaskType, Source.GetDescription());
        }

        public virtual bool Preservable
        {
            get { return true; }
            set { }
        }

        public virtual DateTime CreatedAt { get; set; }

        public virtual string ToDescription()
        {
            return string.Format("{0} 的 Task#{1}({2})", Status.GetDescription(), Id, TaskCode);
        }


        public virtual string EventKey { get;  set; }
    }
}
