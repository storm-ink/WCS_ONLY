using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs.Framework.EventBus;

namespace Wcs.Framework.Events
{
    [System.ComponentModel.Description("逻辑动作被创建事件")]
    public class LogicMovementAddedEvent:IEvent
    {
        public virtual LogicMovement Movement { get; set; }
        public LogicMovementAddedEvent(LogicMovement movement)
        {
            this.CreatedAt = DateTime.Now;
            this.Movement = movement;
            this.EventKey = string.Format("LogicMovementAddedEvent#{0}_{1}", Movement.Id,Movement.CreatedAt.ToString("yyyy-MM-dd_HH:mm:ss.ffff"));
        }


        public virtual bool Preservable
        {
            get { return false; }
            set { }
        }
        public virtual DateTime CreatedAt { get; set; }

        public virtual string ToDescription()
        {
            return string.Format("LogicMovementAddedEvent#{0}_{1}", Movement.Id, Movement.CreatedAt.ToString("yyyy-MM-dd_HH:mm:ss.ffff"));
        }


        public virtual string EventKey { get; set; }
    }
}
