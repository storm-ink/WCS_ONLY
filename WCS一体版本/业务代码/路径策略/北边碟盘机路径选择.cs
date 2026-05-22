using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs;
using Wcs.Framework;
namespace 合肥经纬
{
    public class 北边碟盘机路径选择 : RouteSelector
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
                Dictionary<Stack<int>, List<String>> _dic = new Dictionary<Stack<int>, List<String>>();
                foreach (var item in _routeList)
                {
                    List<String> _deviceList = new List<string>();

                    #region

                    foreach (var routeHead in item)
                    {
                        var _route = RouteHelper.RouteHeads.First(x => x.HeadID == routeHead);
                        if (task.TaskType == "")
                        {
                            if (routeHead == 1)
                            {
                                _deviceList.Add(_route.Device);
                                break;
                            }
                        }

                        ///过滤不支持的路径,注释部分为浩信浩德的范例，为的是在穿梭车卸货点下发任务过滤以穿梭车卸货点为起点的任务错误拆分，如果项目有特殊需求可以参考该范例处理
                        //if (_route.Details.Count() == 1)
                        //{
                        //    Location _start, _end;
                        //    var _index = item.FindLastIndex(x => x == routeHead);
                        //    if (_index == 0)
                        //        _start = start;
                        //    else
                        //    {
                        //        var _frontRouteIndex = item.AsQueryable().ToArray()[_index - 1];
                        //        _start = LocationConverter.ConvertibleCodeToLcation(RouteHelper.RouteHeads.First(x => x.HeadID == _frontRouteIndex).Details.Last().ConvertibleCodeToLcation());
                        //    }

                        //    if (item.Count() == _index + 1)
                        //        _end = end;
                        //    else
                        //    {
                        //        var _backRouteIndex = item.AsQueryable().ToArray()[_index + 1];
                        //        _end = LocationConverter.ConvertibleCodeToLcation(RouteHelper.RouteHeads.First(x => x.HeadID == _backRouteIndex).Details.First().ConvertibleCodeToLcation());
                        //    }
                        //    if (_start.DeviceCode == "1031" || _start.DeviceCode == "1035")
                        //    {
                        //        _deviceList = null;
                        //        break;
                        //    }
                        //}
                        ///过滤不支持的路径,注释部分为浩信浩德的范例，为的是在穿梭车卸货点下发任务过滤以穿梭车卸货点为起点的任务错误拆分
                        if (_deviceList.Count() == 0 || _route.Device != _deviceList.Last())
                            _deviceList.Add(_route.Device);
                    #endregion
                    }
                    if (_deviceList != null)
                        _dic.Add(item, _deviceList);
                }
                return _dic.OrderBy(x => x.Value.Count()).First().Key;
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                return null;
            }
        }
    }
}
