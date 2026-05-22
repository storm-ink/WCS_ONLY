using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Events
{
    public abstract class AbstractEvent<TSource>:IEvent
        where TSource:class
    {
        readonly Guid id;
        readonly DateTime timeStamp;
        readonly TSource source;

        public AbstractEvent(TSource source)
        {
            this.source = source;
            this.id = Guid.NewGuid();
            this.timeStamp = DateTime.Now;
        }

        //public AbstractEvent()
        //    : this(null)
        //{

        //}

        public Guid ID
        {
            get { return id; }
        }

        public DateTime TimeStamp
        {
            get { return timeStamp; }
        }

        public TSource Source
        {
            get
            {
                return source;
            }
        }
    }
}
