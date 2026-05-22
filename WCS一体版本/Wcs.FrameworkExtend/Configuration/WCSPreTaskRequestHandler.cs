using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.FrameworkExtend
{
    public abstract class WCSPreTaskRequestHandler
    {
        public Logger _logger = LogManager.CreateNullLogger();
        public abstract void Hand(PreTask task);
        public virtual Boolean Allowed(PreTask task)
        {
            if (TaskTypes.Contains(task.TaskType) || TaskTypes.Contains("*"))
                return true;
            else
                return false;
        }
        public string[] TaskTypes;
    }
}
