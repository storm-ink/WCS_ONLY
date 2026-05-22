using System;
using System.Collections.Generic;
using System.Linq;
using Wcs;
using Wcs.Framework;
using NHibernate.Linq;

namespace BOE
{
    public class HFJWMinimumPassingDeviceRouteSelector : RouteSelector
    {
        public override Stack<int> GetBestRoute(Task task, List<Stack<int>> _routeList, Location start, Location end)
        {
            try
            {
                //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                //sw.Start();

                Dictionary<Stack<int>, List<String>> _dic = new Dictionary<Stack<int>, List<String>>();
                foreach (var item in _routeList)
                {
                    List<String> _deviceList = new List<string>();

                    foreach (var routeHead in item)
                    {
                        var _route = RouteHelper.RouteHeads.First(x => x.HeadID == routeHead);
                        #region 过滤不支持的路径,注释部分为浩信浩德的范例，为的是在穿梭车卸货点下发任务过滤以穿梭车卸货点为起点的任务错误拆分，如果项目有特殊需求可以参考该范例处理
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
                        #endregion
                        #region C003库口路径选择
                        if (routeHead == 6)
                        {
                            if (end.Device.Name == "C003")
                            {
                                var _line = Convert.ToUInt32(end.UserCode.Substring(0, 2));
                                if (_line >= 9 && _line <= 12)
                                {
                                    _deviceList = null;
                                    break;
                                }
                            }
                        }
                        if (routeHead == 7)
                        {
                            if (end.Device.Name == "C003")
                            {
                                var _line = Convert.ToUInt32(end.UserCode.Substring(0, 2));
                                {
                                    if (_line >= 7 && _line <= 8)
                                    {
                                        _deviceList = null;
                                        break;
                                    }
                                }
                            }
                        }
                        if (routeHead == 5)
                        {
                            if (start.Device.Name == "C003")
                            {
                                var _line = Convert.ToUInt32(start.UserCode.Substring(0, 2));
                                if (_line >= 9 && _line <= 12)
                                {
                                    _deviceList = null;
                                    break;
                                }
                            }
                        }
                        if (routeHead == 8)
                        {
                            if (start.Device.Name == "C003")
                            {
                                var _line = Convert.ToUInt32(start.UserCode.Substring(0, 2));
                                if (_line >= 7 && _line <= 8)
                                {
                                    _deviceList = null;
                                    break;
                                }
                            }
                        }
                        #endregion
                        #region C004库口路径选择
                        if (routeHead == 29)
                        {
                            if (end.Device.Name == "C004")
                            {
                                var _line = Convert.ToUInt32(end.UserCode.Substring(0, 2));
                                if (_line >= 17 && _line <= 18)
                                {
                                    _deviceList = null;
                                    break;
                                }
                            }
                        }
                        if (routeHead == 30)
                        {
                            if (end.Device.Name == "C004")
                            {
                                var _line = Convert.ToUInt32(end.UserCode.Substring(0, 2));
                                {
                                    if (_line >= 13 && _line <= 16)
                                    {
                                        _deviceList = null;
                                        break;
                                    }
                                }
                            }
                        }
                        if (routeHead == 28)
                        {
                            if (start.Device.Name == "C004")
                            {
                                var _line = Convert.ToUInt32(start.UserCode.Substring(0, 2));
                                if (_line >= 17 && _line <= 18)
                                {
                                    _deviceList = null;
                                    break;
                                }
                            }
                        }
                        if (routeHead == 31)
                        {
                            if (start.Device.Name == "C004")
                            {
                                var _line = Convert.ToUInt32(start.UserCode.Substring(0, 2));
                                if (_line >= 13 && _line <= 16)
                                {
                                    _deviceList = null;
                                    break;
                                }
                            }
                        }
                        #endregion
                        #region C005库口路径选择
                        if (routeHead == 32)
                        {
                            if (end.Device.Name == "C005")
                            {
                                var _line = Convert.ToUInt32(end.UserCode.Substring(0, 2));
                                if (_line >= 21 && _line <= 24)
                                {
                                    _deviceList = null;
                                    break;
                                }
                            }
                        }
                        if (routeHead == 34)
                        {
                            if (end.Device.Name == "C005")
                            {
                                var _line = Convert.ToUInt32(end.UserCode.Substring(0, 2));
                                {
                                    if (_line >= 19 && _line <= 20)
                                    {
                                        _deviceList = null;
                                        break;
                                    }
                                }
                            }
                        }
                        if (routeHead == 33)
                        {
                            if (start.Device.Name == "C005")
                            {
                                var _line = Convert.ToUInt32(start.UserCode.Substring(0, 2));
                                if (_line >= 21 && _line <= 24)
                                {
                                    _deviceList = null;
                                    break;
                                }
                            }
                        }
                        if (routeHead == 35)
                        {
                            if (start.Device.Name == "C005")
                            {
                                var _line = Convert.ToUInt32(start.UserCode.Substring(0, 2));
                                if (_line >= 19 && _line <= 20)
                                {
                                    _deviceList = null;
                                    break;
                                }
                            }
                        }
                        #endregion
                        if (_deviceList.Count() == 0 || _route.Device != _deviceList.Last())
                            _deviceList.Add(_route.Device);
                    }
                    if (_deviceList != null)
                        _dic.Add(item, _deviceList);
                }

                //sw.Stop();
                //Console.WriteLine(String.Format("★★★★★★★★★★任务拆分策略耗时：{0}", sw.ElapsedMilliseconds));
                if (task.StartLocation.DeviceCode == "2022" && task.EndLocation.DeviceCode == "2020" && _dic.Any(x => x.Key.Contains(55)))
                    return _dic.First(x => x.Key.Contains(55)).Key;

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
