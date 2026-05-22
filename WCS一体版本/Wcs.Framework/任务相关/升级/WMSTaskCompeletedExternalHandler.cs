using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 向WMS报完成外挂程序
    /// </summary>
    public abstract class WMSTaskCompeletedExternalHandler
    {
        Logger _logger = LogManager.CreateNullLogger();
        public abstract void Hand(ref Task task);
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
