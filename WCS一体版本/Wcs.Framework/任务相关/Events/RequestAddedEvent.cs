using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs.Framework.EventBus;

namespace Wcs.Framework.Events
{

    [System.ComponentModel.Description("请求被创建事件")]
    public class RequestAddedEvent:IEvent
    {
        public virtual Request Request { get; set; }
        protected RequestAddedEvent()
        {
            this.CreatedAt = DateTime.Now;
        }
        public RequestAddedEvent(Request request):this()
        {
            this.Request = request;
            this.EventKey = string.Format("RequestAddedEvent_Request#{0}_@{1}", request.Id, request.Source);
        }


        public virtual bool Preservable
        {
            get { return true; }
            set { }
        }

        public virtual DateTime CreatedAt { get; set; }

        public virtual string ToDescription()
        {
            return string.Format("Request#{0}({1})", Request.Id, Request.Source); 
        }


        public virtual string EventKey { get; set; }
    }
}
