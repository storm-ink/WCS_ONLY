using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs.Framework.EventBus;

namespace Wcs.Framework.Events
{
    [System.ComponentModel.Description("主任务被创建事件")]
    public class TaskAddedEvent:IEvent
    {
        public virtual Task Task { get; set; }
        public TaskAddedEvent(Task task)
        {
            this.CreatedAt = DateTime.Now;
            this.Task = task;
            this.EventKey = string.Format("TaskAddedEvent_Task#{0}_{1}", task.Id, task.TaskCode);
        }


        public virtual bool Preservable
        {
            get { return false; }
            set { }
        }

        public virtual DateTime CreatedAt { get; set; }


        public virtual string ToDescription()
        {
            return string.Format("Task#{0}({1})", Task.Id, Task.TaskCode);
        }


        public virtual string EventKey { get; set; }
    }
}
