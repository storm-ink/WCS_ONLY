using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs.Framework.EventBus;

namespace Wcs.Framework.Events
{
    [System.ComponentModel.Description("任务序列的当前正在执行的物理动作发生改变事件")]
    public class SchedulerCurrentActionChangedEvent : IEvent
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
        
        public EquipmentAction oldCurrentAction { get; set; }
        public EquipmentAction newCurrentAction { get; set; }
        public EquipmentActionScheduler Scheduler{get;set;}

        protected SchedulerCurrentActionChangedEvent()
        {
            this.CreatedAt = DateTime.Now;
        }

        public SchedulerCurrentActionChangedEvent(EquipmentActionScheduler scheduler, EquipmentAction oldAction, EquipmentAction newAction)
            :this()
        {
            this.Scheduler = scheduler;
            this.oldCurrentAction = oldAction;
            this.newCurrentAction = newAction;
            this.EventKey = string.Format("SchedulerCurrentActionChangedEvent_Scheduler#{0}_oldCurrentAction:{1}_newCurrentAction:{2}", scheduler.DeviceName, oldAction, newAction);
        }

        public virtual string ToDescription()
        {
            return string.Format("{0} 当前执行的物理动作由 {1} 变为 {2}", Scheduler, oldCurrentAction, newCurrentAction);
        }


        public virtual string EventKey { get; set; }
    }
}
