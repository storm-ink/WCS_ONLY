using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs.Framework.EventBus;

namespace Wcs.Framework.Events
{
    [Description("物理动作状态改变事件")]
    public class EquipmentActionStatusChangedEvent:IEvent
    {
        public virtual Int32 Id { get; set; }
        public virtual Int32 TaskId { get; set; }
        public virtual Int32 MovementId { get; set; }
        public virtual EquipmentActionStatus Status { get; set; }
        protected EquipmentActionStatusChangedEvent()
        {
            this.CreatedAt = DateTime.Now;
        }
        public EquipmentActionStatusChangedEvent(Int32 taskId,Int32 movementId,Int32 id, EquipmentActionStatus status):this()
        {
            this.TaskId = taskId;
            this.MovementId = movementId;
            this.Id = id;
            this.Status = status;
            this.EventKey = string.Format("EquipmentAction#{0}_{1}", id,status.GetDescription());
        }


        public virtual bool Preservable
        {
            get { return true; }
            set { }
        }

        public virtual DateTime CreatedAt { get; set; }

        public virtual string ToDescription()
        {
            return string.Format("EquipmentActionStatusChangedEvent_EquipmentAction#{0} 状态改变为 {1}", Id, Status.GetDescription());
        }


        public virtual string EventKey { get; set; }
    }
}
                                                           