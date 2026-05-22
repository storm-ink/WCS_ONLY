using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.Framework
{
    /// <summary>
    /// 任务终点选择外挂方法
    /// </summary>
    public abstract class TaskChooseEndLocationHandler
    {
        public Logger _logger = LogManager.CreateNullLogger();

        public abstract Boolean Hand(Task task, Location currentLocation);
    }
}
