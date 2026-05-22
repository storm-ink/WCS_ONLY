using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    public abstract class BaseTaskRequestHandler
    {
        public Logger _logger = LogManager.CreateNullLogger();
        public abstract void Hand(Task task);
        public virtual Boolean Allowed(Task task)
        {
            if (TaskTypes.Contains(task.TaskType) || TaskTypes.Contains("*"))
                return true;
            else
                return false;
        }
        public string[] TaskTypes;
    }
}
