using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    public abstract class TaskSourceIsUnKnownCompeletedHandler
    {
        Logger _logger = LogManager.CreateNullLogger();
        public abstract void Hand(Task task);
    }
}
