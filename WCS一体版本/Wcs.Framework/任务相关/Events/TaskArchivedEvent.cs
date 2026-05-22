using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs.Framework.EventBus;

namespace Wcs.Framework.Events
{

    [System.ComponentModel.Description("主任务已归档事件")]
    public class TaskArchivedEvent:IEvent
    {
        public virtual Int32 Id { get; set; }
        public virtual String TaskCode { get; set; }
        protected TaskArchivedEvent()
        {
            this.CreatedAt = DateTime.Now;
        }

        public TaskArchivedEvent(Int32 id, String taskCode)
            : this()
        {
            this.Id = id;
            this.TaskCode = taskCode;
            this.EventKey = string.Format("TaskArchivedEvent_Task#{0}_{1}", id, taskCode);
        }

        public virtual bool Preservable
        {
            get { return true; }
            set { }
        }

        public virtual DateTime CreatedAt { get; set; }

        public virtual string ToDescription()
        {
            return string.Format("Task#{0}({1})", Id, TaskCode);
        }


        public virtual string EventKey { get; set; }
    }
}
