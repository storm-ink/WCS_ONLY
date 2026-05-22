using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs.Framework.EventBus;

namespace Wcs.Framework.Events
{

    [System.ComponentModel.Description("物理动作被添加到任务序列事件")]
    public class SchedulerActionAddedEvent : IEvent
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
        public EquipmentActionScheduler Scheduler { get; set; }
        public EquipmentAction Action { get; set; }

        protected SchedulerActionAddedEvent()
        {
            this.CreatedAt = DateTime.Now;
        }

        public SchedulerActionAddedEvent(EquipmentActionScheduler scheduler,EquipmentAction action)
            :this()
        {
            this.Scheduler = scheduler;
            this.Action = action;
            this.EventKey = string.Format("SchedulerActionAddedEvent_Scheduler#{0}_Action:{1}", scheduler.DeviceName, action);
        }


        public virtual string ToDescription()
        {
            return string.Format("{0} 被添加到 {1} 中", Action, Scheduler);
        }



        public virtual string EventKey { get; set; }
    }
}
