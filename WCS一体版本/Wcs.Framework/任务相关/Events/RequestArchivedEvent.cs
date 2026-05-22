using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs.Framework.EventBus;

namespace Wcs.Framework.Events
{
    [System.ComponentModel.Description("请求已归档事件")]
    public class RequestArchivedEvent:IEvent
    {
        public virtual Int32 Id { get; set; }
        public virtual LocationInfo Source { get; set; }
        public virtual RequestStatus Status { get; set; }
        protected RequestArchivedEvent()
        {
            this.CreatedAt = DateTime.Now;
        }

        public RequestArchivedEvent(Int32 id, LocationInfo source,RequestStatus status)
            : this()
        {
            this.Id = id;
            this.Source = source;
            this.Status = status;
            this.EventKey = string.Format("RequestArchivedEvent_Request#{0}_{1}_@{2}", id, status.GetDescription(), source);
        }

        public virtual bool Preservable
        {
            get { return true; }
            set { }
        }

        public virtual DateTime CreatedAt { get; set; }
        
        public virtual string ToDescription()
        {
            return string.Format("Request#{0}({1})", Id, Source);
        }


        public virtual string EventKey { get; set; }
    }
}
