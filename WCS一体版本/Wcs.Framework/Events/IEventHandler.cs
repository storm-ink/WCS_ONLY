using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Events
{
    public interface IEventHandler<TEvent>
        where TEvent : class,IEvent
    {
        void Handle(TEvent evnt);
    }
}
