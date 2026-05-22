using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.FrameworkExtend.Cfg
{
    /// <summary>
    /// 向WMS报完成外挂程序
    /// </summary>
    public abstract class WMSPreTaskCompeletedExternalHandler
    {
        public Logger _logger = LogManager.CreateNullLogger();
        public abstract void Hand(PreTask preTask);
        public virtual Boolean Allowed(PreTask preTask)
        {
            if (TaskTypes != null && (TaskTypes.Contains(preTask.TaskType) || TaskTypes.Contains("*")))
                return true;
            else
                return false;
        }
        public string[] TaskTypes;
    }
}
