using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.Framework.Events
{
    [System.ComponentModel.Description("请求状态被改变事件")]
    public class RequestStatusChangedEvent:Wcs.Framework.EventBus.IEvent
    {
        public virtual Int32 Id { get; set; }
        public virtual RequestStatus Status { get; set; }
        protected RequestStatusChangedEvent()
        {
            this.CreatedAt = DateTime.Now;
        }
        public RequestStatusChangedEvent(Int32 id, RequestStatus status)
            : this()
        {
            this.Id = id;
            this.Status = status;
            this.EventKey = string.Format("RequestStatusChangedEvent_Request#{0}_{1}", id, status.GetDescription());
        }

        public virtual bool Preservable
        {
            get { return true; }
            set { }
        }

        public virtual DateTime CreatedAt { get; set; }

        public virtual string ToDescription()
        {
            return string.Format("Request#{0} 状态改变为 {1}", Id, Status.GetDescription());
        }



        public virtual string EventKey { get; set; }
    }
}
