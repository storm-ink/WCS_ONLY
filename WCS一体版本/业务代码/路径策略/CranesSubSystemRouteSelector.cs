using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs;
using Wcs.DefaultImpls.Conveyor;
using Wcs.DefaultImpls.Crane;
using Wcs.DefaultImpls.CraneSubSystem;
using Wcs.Framework;

namespace BOE
{
    public class CranesSubSystemRouteSelector : RouteSelector
    {
        static List<String> Cran = new List<string>() { "C003", "C004", "C005" };
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

                        var _routeDetalis = RouteHelper.RouteDetails.Where(x => x.Id == routeHead).ToList();



                        if (item.Count > 1 && Cran.Contains(_routeDetalis.First().Device) && _routeDetalis.First().Path.Count() == 1)
                        {
                            //获取下一条路径
                            var nextrouteHead = item.AsQueryable().ToArray()[1];
                            var _nextroute = RouteHelper.RouteHeads.First(x => x.HeadID == nextrouteHead);
                            var _nextrouteDetalis = RouteHelper.RouteDetails.Where(x => x.Id == nextrouteHead).ToList();

                            //如果下一条路径不是堆垛机调度系统
                            if (_nextrouteDetalis.Count > 1)
                            {
                                //获取下条路径的起始点
                                var _nextstart = _nextrouteDetalis.First().Path;
                                var _nextend = _nextrouteDetalis.Last().Path;

                                RackLocation _startCraneSubSystemLocation;
                                if (start is RackLocation)
                                {
                                    _startCraneSubSystemLocation = (RackLocation)start;
                                }
                                else
                                {
                                    _startCraneSubSystemLocation = (RackLocation)start.Synonymous.Single(x => x is RackLocation);
                                }

                                if (end.Synonymous.Any(x => x is RackLocation) && end.Synonymous.Any(x => x is RackLocation))
                                {
                                    goto next;
                                }

                                var nearest_location = getNextConveyor(_startCraneSubSystemLocation);

                                if (nearest_location.Synonymous.Any(x => x.DeviceCode == _nextstart))
                                {
                                    return item;
                                }
                            }
                        }
                    next: ;
                        if (_deviceList.Count() == 0 || _route.Device != _deviceList.Last())
                            _deviceList.Add(_route.Device);
                    }




                    if (_deviceList != null && _deviceList.Count > 0)
                        _dic.Add(item, _deviceList);
                }

                return _dic.OrderBy(x => x.Value.Count()).ThenBy(x => x.Key.Count()).First().Key;
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                return null;
            }
        }

        /// <summary>
        /// 获取最近的可用输送线
        /// </summary>
        /// <param name="start">当前点</param>
        private Location getNextConveyor(RackLocation start)
        {
            try
            {
                Dictionary<RackLocation, int> list = new Dictionary<RackLocation, int>();
                ////获取当前巷道所有起点到目的输送线的货格
                var _device = DeviceConverter.ToDevice<CraneDevice>(start.Device.Name);

                var _locations = _device.Locations.Select(x => (RackLocation)x).Where(x => x.Column != start.Column && x.Synonymous.Any(y => y is ConveyorLocation));

                foreach (var item in _locations)
                {
                    list.Add(item, GetDistance(start, item));
                }
                if (list != null && list.Count > 0)
                {
                    var _list = list.OrderBy(x => x.Value);


                    //5号出库
                    int _line1 = Convert.ToInt32(start.UserCode.Substring(0, 2));
                    if (_line1 >= 21 && _line1 <= 24)
                    {
                        return _list.FirstOrDefault(x => x.Key.UserCode == "23-991-001").Key;
                    }

                    if (_line1 <= 20 && _line1 >= 19)
                    {
                        return _list.FirstOrDefault(x => x.Key.UserCode == "20-991-001").Key;
                    }

                    //4号出库
                    if (_line1 >= 17 && _line1 <= 18)
                    {
                        return _list.FirstOrDefault(x => x.Key.UserCode == "18-991-001").Key;
                    }

                    if (_line1 <= 16 && _line1 >= 13)
                    {
                        return _list.FirstOrDefault(x => x.Key.UserCode == "14-991-001").Key;
                    }

                    //3号出库
                    if (_line1 >= 9 && _line1 <= 12)
                    {
                        return _list.FirstOrDefault(x => x.Key.UserCode == "10-991-001").Key;
                    }

                    if (_line1 <= 8 && _line1 >= 7)
                    {
                        return _list.FirstOrDefault(x => x.Key.UserCode == "07-991-001").Key;
                    }
                    //foreach (var item in _list)
                    // {
                    //   var nearest_location = (Location)(item.Key);

                    //  ConveyorLocation _conveyorLocation;
                    //    if (nearest_location is ConveyorLocation)
                    //    {
                    //        _conveyorLocation = (ConveyorLocation)nearest_location;
                    //    }
                    //    else
                    //    {
                    //        _conveyorLocation = (ConveyorLocation)nearest_location.Synonymous.Single(x => x is ConveyorLocation);
                    //    }



                    //    //判断占位
                    //    //ConveyorDevice conveyor = (ConveyorDevice)_conveyorLocation.Device;
                    //    //if (!conveyor.IsConnected)
                    //    //{
                    //    //    return nearest_location;
                    //    //}
                    //    //HoldSignalNetTransferObject _holdSignal = null;
                    //    //if (conveyor.OccupiedSignals != null)
                    //    //    _holdSignal = conveyor.OccupiedSignals.FirstOrDefault(x => x.PosNo == Convert.ToInt32(_conveyorLocation.DeviceCode));
                    //    //if (_holdSignal == null)
                    //    //    continue;
                    //    //if (_holdSignal.HandShake == HoldSignalNetTransferObjectHandShake.New)
                    //    //    continue;
                    //   return nearest_location;
                    //}

                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private Int32 GetDistance(RackLocation start, RackLocation end)
        {
            return Math.Abs(start.UserColumn - end.UserColumn) + Math.Abs(start.UserLevel - end.UserLevel);
        }

        private bool isCraneSubSystem(string deviceName)
        {
            try
            {
                DeviceConverter.ToDevice<CraneSubSystemDevice>(deviceName);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        private bool isSameLine(CraneSubSystemLocation start, CraneSubSystemLocation end)
        {
            if (start.UserLine == end.UserLine)
            {
                return true;
            }

            switch (start.UserLine)
            {
                case 1:
                    if (end.UserLine == 2)
                    {
                        return true;
                    }
                    break;
                case 2:
                    if (end.UserLine == 1)
                    {
                        return true;
                    }
                    break;
                case 3:
                    if (end.UserLine == 4)
                    {
                        return true;
                    }
                    break;
                case 4:
                    if (end.UserLine == 3)
                    {
                        return true;
                    }
                    break;
                case 5:
                    if (end.UserLine == 6)
                    {
                        return true;
                    }
                    break;
                case 6:
                    if (end.UserLine == 5)
                    {
                        return true;
                    }
                    break;
                default:
                    break;
            }

            return false;
        }
    }
}


