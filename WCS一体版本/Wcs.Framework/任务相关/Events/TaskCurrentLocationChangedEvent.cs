using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.Framework.Events
{

    [System.ComponentModel.Description("主任务当前位置发生改变事件")]
    public class TaskCurrentLocationChangedEvent:Wcs.Framework.EventBus.IEvent
    {
        public virtual Int32 Id { get; set; }
        public virtual String TaskCode { get; set; }
        public virtual LocationInfo CurrentLocation { get; set; }
        protected TaskCurrentLocationChangedEvent()
        {
            this.CreatedAt = DateTime.Now;
        }
        public TaskCurrentLocationChangedEvent(Int32 id, String taskCode, LocationInfo currentLocation):this()
        {
            this.Id = id;
            this.TaskCode = taskCode;
            this.CurrentLocation = currentLocation;

            this.EventKey = string.Format("TaskCurrentLocationChangedEvent_Task#{0}_{1}_@{2}", id, taskCode, currentLocation);
        }



        public virtual bool Preservable
        {
            get { return true; }
            set { }
        }

        public virtual DateTime CreatedAt { get; set; }

        public virtual string ToDescription()
        {
            return string.Format("已运行到 {0} 的 Task#{1}({2})", CurrentLocation, Id, TaskCode);
        }


        public virtual string EventKey { get; set; }
    }
}
