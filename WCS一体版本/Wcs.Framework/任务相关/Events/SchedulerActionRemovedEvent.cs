using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs.Framework.EventBus;

namespace Wcs.Framework.Events
{
    [System.ComponentModel.Description("物理动作被从任务序列中移除事件")]
    public class SchedulerActionRemovedEvent : IEvent
    {
        public bool Preservable
        {
            get
            {
                return false;
            }
            set
            {

            }
        }
        public DateTime CreatedAt { get; set; }
        
        public EquipmentAction Action { get; set; }
        public EquipmentActionScheduler Scheduler{get;set;}

        protected SchedulerActionRemovedEvent()
        {
            this.CreatedAt = DateTime.Now;
        }

        public SchedulerActionRemovedEvent(EquipmentActionScheduler scheduler, EquipmentAction action)
            :this()
        {
            this.Scheduler = scheduler;
            this.Action = action;
            this.EventKey = string.Format("SchedulerActionRemovedEvent_Scheduler#{0}_Action:{1}", scheduler.DeviceName, action);
        }

        public virtual string ToDescription()
        {
            return string.Format("{0} 被从 {1} 中移除", Scheduler, Action);
        }


        public virtual string EventKey { get; set; }
    }
}
