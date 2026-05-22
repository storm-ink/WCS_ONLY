using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.Framework.Events
{
    [System.ComponentModel.Description("主任务的优先级发生改变事件")]
    public class TaskPriorityChangedEvent:Wcs.Framework.EventBus.IEvent
    {
        public virtual Int32 Id { get; set; }
        public virtual String TaskCode { get; set; }
        public virtual Int32 Priority { get; set; }
        protected TaskPriorityChangedEvent()
        {
            this.CreatedAt = DateTime.Now;
        }
        public TaskPriorityChangedEvent(Int32 id, String taskCode, Int32 priority):this()
        {
            this.Id = id;
            this.TaskCode = taskCode;
            this.Priority = priority;
            this.EventKey = string.Format("TaskPriorityChangedEvent_Task#{0}_{1}_优先级：{2}", id, taskCode, priority);
        }

        public virtual bool Preservable
        {
            get { return true; }
            set { }
        }


        public virtual DateTime CreatedAt { get; set; }


        public virtual string ToDescription()
        {
            return string.Format("Task#{0}({1}) 优先级 {2}", Id, TaskCode, Priority);
        }


        public virtual string EventKey { get; set; }
    }
}
