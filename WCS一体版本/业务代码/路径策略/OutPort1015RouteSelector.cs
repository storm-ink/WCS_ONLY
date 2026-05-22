using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs;
using Wcs.Framework;

namespace BOE
{
    public class OutPort1015RouteSelector : RouteSelector
    {
        public override bool Allowed(Task task)
        {
            ///默认全部选择这种拆分方式
            return true;
        }
        public override Stack<int> GetBestRoute(Task task, List<Stack<int>> _routeList, Location start, Location end)
        {
            try
            {
                if (task.EndLocation.DeviceCode != "1015")
                    return _routeList.OrderBy(x => x.Count).FirstOrDefault();
                else
                    return _routeList.OrderBy(x => x.Count).LastOrDefault();
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                return null;
            }
        }
    }
}
