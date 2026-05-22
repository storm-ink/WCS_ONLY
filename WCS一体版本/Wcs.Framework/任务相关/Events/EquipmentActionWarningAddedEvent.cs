using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs.Framework.EventBus;

namespace Wcs.Framework.Events
{
    [System.ComponentModel.Description("物理动作执行时发生异常事件")]
    public class EquipmentActionWarningAddedEvent:IEvent
    {
        public virtual EquipmentActionWarning Warning { get; protected set; }

        protected EquipmentActionWarningAddedEvent()
        {
            this.CreatedAt = DateTime.Now;
        }

        public EquipmentActionWarningAddedEvent(EquipmentActionWarning warning):this()
        {
            this.Warning = warning;
            this.EventKey = string.Format("EquipmentActionWarningAddedEvent_Warning#{0}", Warning.Id);
        }


        public virtual bool Preservable
        {
            get { return false; }
            set { }
        }

        public virtual DateTime CreatedAt { get; set; }

        public virtual string ToDescription()
        {
            return string.Format("{0}", Warning);
        }


        public virtual string EventKey { get; set; }
    }
}
