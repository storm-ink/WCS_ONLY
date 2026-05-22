using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.Framework.Events
{

    [System.ComponentModel.Description("主任务终点发生改变事件")]
    public class TaskEndLocationChangedEvent : Wcs.Framework.EventBus.IEvent
    {
        public virtual Int32 Id { get; set; }
        public virtual String TaskCode { get; set; }
        public virtual LocationInfo EndLocation { get; set; }
        protected TaskEndLocationChangedEvent()
        {
            this.CreatedAt = DateTime.Now;
        }
        public TaskEndLocationChangedEvent(Int32 id, String taskCode, LocationInfo endLocation):this()
        {
            this.Id = id;
            this.TaskCode = taskCode;
            this.EndLocation = endLocation;

            this.EventKey = string.Format("TaskEndLocationChangedEvent_Task#{0}_{1}_@{2}", id, taskCode, endLocation);
        }



        public virtual bool Preservable
        {
            get { return true; }
            set { }
        }

        public virtual DateTime CreatedAt { get; set; }

        public virtual string ToDescription()
        {
            return string.Format("已修改终点到 {0} 的 Task#{1}({2})", EndLocation, Id, TaskCode);
        }


        public virtual string EventKey { get; set; }
    }
}
