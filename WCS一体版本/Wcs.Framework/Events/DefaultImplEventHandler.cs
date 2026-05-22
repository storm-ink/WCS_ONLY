using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Events
{
    public class DefaultImplEventHandler<TEvent>:IEventHandler<TEvent>
        where TEvent:class,IEvent
    {
        readonly Action<TEvent> action;
        public Action<TEvent> Action
        {
            get
            {
                return action;
            }
        }

        public DefaultImplEventHandler(Action<TEvent> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            this.action = action;
        }

        public void Handle(TEvent evnt)
        {
            this.Action.Invoke(evnt);
        }
    }
}
