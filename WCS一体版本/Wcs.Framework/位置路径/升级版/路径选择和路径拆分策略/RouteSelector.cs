using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 路径选择器
    /// </summary>
    public abstract class RouteSelector : ThreadRunningLog
    {
        public RouteSelector() { this.Init("RouteSelector"); }
        public Logger _logger = LogManager.CreateNullLogger();
        public abstract Stack<int> GetBestRoute(Task task, List<Stack<int>> _routeList, Location start, Location end);
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
