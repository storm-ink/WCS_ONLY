using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs.Framework.EventBus;

namespace Wcs.FrameworkExtend.Events
{
    [System.ComponentModel.Description("预任务被创建事件")]
    public class PreTaskAddedEvent:IEvent
    {
        public virtual PreTask PreTask { get; set; }
        public PreTaskAddedEvent(PreTask pretask)
        {
            this.CreatedAt = DateTime.Now;
            this.PreTask = pretask;
            this.EventKey = string.Format("PreTaskAddedEvent_Task#{0}_{1}", pretask.Id, pretask.TaskCode);
        }


        public virtual bool Preservable
        {
            get { return false; }
            set { }
        }

        public virtual DateTime CreatedAt { get; set; }


        public virtual string ToDescription()
        {
            return string.Format("PreTask#{0}({1})", PreTask.Id, PreTask.TaskCode);
        }


        public virtual string EventKey { get; set; }
    }
}
