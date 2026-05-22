using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs.Framework.EventBus;

namespace Wcs.Framework.Events
{
    [System.ComponentModel.Description("逻辑动作状态发生改变事件")]
    public class LogicMovementStatusChangedEvent:IEvent
    {
        public virtual Int32 Id { get; set; }
        public virtual Int32 TaskId { get; set; }
        public virtual LogicMovementStatus Status { get; set; }
        protected LogicMovementStatusChangedEvent()
        {
            this.CreatedAt = DateTime.Now;
        }

        public LogicMovementStatusChangedEvent(Int32 id,Int32 taskId, LogicMovementStatus status):this()
        {
            this.Id = id;
            this.TaskId = taskId;
            this.Status = status;
            this.EventKey = string.Format("LogicMovementStatusChangedEvent_LogicMovement#{0}_{1}", id, status.GetDescription());
        }


        public virtual bool Preservable
        {
            get { return true; }
            set { }
        }

        public virtual DateTime CreatedAt { get; set; }

        public virtual string ToDescription()
        {
            return string.Format("LogicMovement#{0} 状态改变为 {1}", Id, Status.GetDescription());
        }


        public virtual string EventKey { get; set; }
    }
}
