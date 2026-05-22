using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs;
using Wcs.Framework;

namespace BOE
{
    public class _穿梭车卸货站点选择策略 : RouteSelector
    {
        public override Stack<int> GetBestRoute(Task task, List<Stack<int>> _routeList, Location start, Location end)
        {
            try
            {
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
                        #region 如果是输送线则过滤手动状态下的设备
                        //var _device = Wcs.Framework.Cfg.WcsConfiguration.Instance.DeviceCollection.ParticularDeviceCollection
                        //    .SelectMany(x => x.DeviceElements)
                        //    .Where(x => x.Device is Wcs.DefaultImpls.Conveyor.ConveyorDevice)
                        //    .Select(x => x.Device)
                        //    .FirstOrDefault(x => x.Name == _route.Device);
                        //if (_device != null)
                        //{
                        //    //var 
                        //}
                        #endregion
                        #region 如果有出库任务则不选择16号路径行走
                        //if (start.DeviceCode == "1026" && routeHead == 16)
                        //{
                        //    int _outTask;
                        //    using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                        //    {
                        //        _outTask = unitOfWork.session.Query<Task>().Count(x => x.EndLocation.DeviceCode == "1020" && x.Movements.Count() < 4);
                        //        unitOfWork.Commit();
                        //    }
                        //    if (_outTask > 0)
                        //    {
                        //        _deviceList = null;
                        //        break;
                        //    }
                        //}
                        #endregion
                        #region 2025和2226检查
                        //if (start.DeviceCode == "1138" && (end.DeviceCode == "1156" || end.DeviceCode == "1159") && (routeHead == 2226 || routeHead == 2025))
                        //if (start.DeviceCode == "1138" && end.DeviceCode == "1153" && (routeHead == 2226 || routeHead == 2025))
                        //{
                        //    Boolean _当前位置在1142的任务, _当前位置在1140的任务, _当前位置在1143的任务;
                        //    ///判断1142 1140是否可以卸货
                        //    using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                        //    {
                        //        _当前位置在1142的任务 = unitOfWork.session.Query<Task>().Any(x => x.CurrentLocation.DeviceCode == "1142");
                        //        _当前位置在1140的任务 = unitOfWork.session.Query<Task>().Any(x => x.CurrentLocation.DeviceCode == "1140");
                        //        _当前位置在1143的任务 = unitOfWork.session.Query<Task>().Any(x => x.CurrentLocation.DeviceCode == "1143");
                        //        unitOfWork.Commit();
                        //    }
                        //    if (!_当前位置在1142的任务 && !_当前位置在1143的任务)
                        //    {
                        //        if (routeHead == 2025)
                        //        {
                        //            _deviceList = null;
                        //            break;
                        //        }
                        //    }
                        //    else if (!_当前位置在1140的任务)
                        //    {
                        //        if (routeHead == 2226)
                        //        {
                        //            _deviceList = null;
                        //            break;
                        //        }
                        //    }
                        //    else
                        //    {
                        //        _deviceList = null;
                        //        break;
                        //    }
                        //}
                        #endregion

                        #region  3号堆垛机 小车卸货口选择
                        ///7、8排为C003            3号巷道排号 对应穿梭车卸货点为1008
                        ///9、10、11、12排为C003   4号巷道排号 对应穿梭车卸货点为1009
                        int _line = Convert.ToInt32(end.UserCode.Substring(0, 2));
                        if (_line <= 8 && _line >= 7)
                        {
                            if (routeHead == 7)
                            {
                                _deviceList = null;
                                break;
                            }
                        }

                        if (_line >= 9 && _line <= 12)
                        {
                            if (routeHead == 6)
                            {
                                _deviceList = null;
                                break;
                            }
                        }
                        #endregion
                        
                        #region  4号堆垛机 小车卸货口选择
                        ///13、14、15、16排为C004  5号巷道排号 对应穿梭车卸货点为1008
                        ///17、18排为C004          6号巷道排号 对应穿梭车卸货点为1009
                        if (_line <= 16 && _line >= 13)
                        {
                            if (routeHead == 30)
                            {
                                _deviceList = null;
                                break;
                            }
                        }

                        if (_line >= 17 && _line <= 18)
                        {
                            if (routeHead == 29)
                            {
                                _deviceList = null;
                                break;
                            }
                        }
                        #endregion

                        #region  5号堆垛机 小车卸货口选择
                        ///19、20排为C004           7号巷道排号 对应穿梭车卸货点为1008
                        ///21、22、23、24排为C004   8号巷道排号 对应穿梭车卸货点为1009
                        if (_line <= 20 && _line >= 19)
                        {
                            if (routeHead == 34)
                            {
                                _deviceList = null;
                                break;
                            }
                        }

                        if (_line >= 21 && _line <= 24)
                        {
                            if (routeHead == 32)
                            {
                                _deviceList = null;
                                break;
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


