using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.EventBus;

namespace Wcs.Framework.Events
{
    [System.ComponentModel.Description("主任务更新事件")]
    public class TaskUpdateEvent : IEvent
    {
        public string EventKey { get; set; }

        public bool Preservable
        {
            get { return true; }
            set { }
        }

        public TaskUpdateEvent(Task oldTask, Task newTask)
        {
            this.oldTask = oldTask;
            this.newTask = newTask;
        }

        public virtual DateTime CreatedAt { get; set; }

        public Task newTask, oldTask;
        CompareResult<Task> _compareResult;
        public CompareResult<Task> CompareResult
        {
            get
            { 
                 _compareResult = oldTask.Compare(newTask);
                 return _compareResult;
            }
        }
        public string ToDescription()
        {
            return string.Format("任务 {0} 信息变更,变更范围 {1}", oldTask, String.Join("/", CompareResult.differences.Select(x => x.ToString())));
        }
    }
}
