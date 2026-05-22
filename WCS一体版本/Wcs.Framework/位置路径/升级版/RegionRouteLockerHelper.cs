using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Linq;
using Wcs;
using NLog;

namespace Wcs.Framework
{
    public static class RegionRouteLockerHelper
    {
        static Logger _logger = LogManager.CreateNullLogger();
        static List<RegionInfo> _regionInfos;
        public static List<RegionInfo> RegionInfos
        {
            get
            {
                if (_regionInfos == null || _regionInfos.Count() == 0)
                {
                    _regionInfos = new List<RegionInfo>();
                    var _str = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("regions", "");
                    if (!String.IsNullOrWhiteSpace(_str))
                    {
                        var _strs = _str.Split(';').ToArray();
                        foreach (var item in _strs)
                        {
                            var _item = item.Split(':').ToArray();
                            if (!_regionInfos.Any(x => x.RegionName == _item[0]))
                            {
                                Dictionary<String, List<string>> _dic = new Dictionary<string, List<string>>();
                                var _infos = _item[1].Split(',').ToArray();
                                foreach (var keyValue in _infos)
                                {
                                    var _keyValue = keyValue.Split('-').ToArray();
                                    if (_dic.ContainsKey(_keyValue[1]))
                                        _dic[_keyValue[1]].Add(_keyValue[0]);
                                    else
                                        _dic.Add(_keyValue[1], new List<string>() { _keyValue[0] });
                                }
                                var _regionInfo = new RegionInfo() { RegionName = _item[0], Infos = _dic };
                                _regionInfos.Add(_regionInfo);
                            }
                        }
                    }
                }

                return _regionInfos;
            }
        }

        public static Boolean IsInitlization = false;

        static List<RegionLock> _regionLocks;
        public static List<Task> RegionLockTasks;
        public static List<RegionLock> RegionLocks
        {
            get
            {
                if (!IsInitlization)
                {
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                    {
                        _regionLocks = unitOfWork.session.Query<RegionLock>().ToList();
                        if (_regionLocks.Count() != 0)
                        {
                            var _taskCodes = _regionLocks.Select(x => x.TaskCode.Trim()).ToArray();
                            RegionLockTasks = unitOfWork.session.Query<Task>().Where(x => _taskCodes.Contains(x.TaskCode)).ToList();
                        }
                        unitOfWork.Commit();
                    }

                    if (RegionLockTasks == null)
                        RegionLockTasks = new List<Task>();
                    IsInitlization = true;
                }

                return _regionLocks;
            }
        }

        static object _objLocker = new object();
        /// <summary>
        /// 增加锁
        /// </summary>
        /// <param name="task"></param>
        /// <param name="regionName"></param>
        /// <param name="checkDeviceCode"></param>
        /// <returns>true加锁成功，false加锁失败</returns>
        public static ActionSchedulerFilterResult AddRegionLock(Task task, String regionName, String checkDeviceCode)
        {
            lock (_objLocker)
            {
                try
                {
                    //如果已经加过则直接返回true
                    if (RegionLocks.Any(x => x.TaskCode == task.TaskCode) || RegionLockTasks.Any(x => x.TaskCode == task.TaskCode))
                        return new ActionSchedulerFilterResult(false, "");

                    //是否可以增加
                    var regionInfo = RegionInfos.First(x => x.RegionName == regionName);
                    var _checkDeviceCodeType = regionInfo.Infos.First(x => x.Value.Contains(checkDeviceCode)).Key;

                    var __regionLocks = RegionLocks.Where(x => x.RegionName == regionName).ToArray();
                    if (__regionLocks.Count() != 0)
                    {
                        var _lockTypes = __regionLocks.Select(x => x.CheckDeviceCodeType).Distinct().ToArray();
                        if (_lockTypes.Length >= 2 || !_lockTypes.Contains(_checkDeviceCodeType))
                            return new ActionSchedulerFilterResult(true, String.Format("已经存在 {0} 的任务区域锁 ", String.Join("/", __regionLocks.Select(x => x.TaskCode + "_" + x.RegionName + "_" + x.CheckDeviceCode + "_" + x.CheckDeviceCodeType))));
                    }

                    var _regionLock = new RegionLock() { RegionName = regionName, TaskCode = task.TaskCode, CheckDeviceCode = checkDeviceCode, CheckDeviceCodeType = _checkDeviceCodeType };
                    //增加锁
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                    {
                        var isExists = unitOfWork.session.Query<RegionLock>().Any(x => x.RegionName == _regionLock.RegionName && x.TaskCode == _regionLock.TaskCode);
                        if (!isExists)
                            unitOfWork.session.Save(_regionLock);
                        unitOfWork.Commit();
                    }

                    if (!RegionLockTasks.Any(x=>x.TaskCode == task.TaskCode))
                        RegionLockTasks.Add(task);

                    _regionLocks.Add(_regionLock);
                    return new ActionSchedulerFilterResult(false, "");
                    //}
                    //else
                    //    return new ActionSchedulerFilterResult(true, String.Format("{0} 区域锁未能锁定成功，当前任务无法执行", regionName));
                }
                catch (Exception ex)
                {
                    _regionLocks = _regionLocks.Where(x => x.TaskCode != x.TaskCode).ToList();
                    RegionLockTasks = RegionLockTasks.Where(x => x.TaskCode != task.TaskCode).ToList();
                    return new ActionSchedulerFilterResult(true, String.Format("{0} 区域锁锁定失败，当前任务无法执行，异常消息：{1}", regionName, ex));
                }
            }
        }

        /// <summary>
        /// 增加锁
        /// </summary>
        /// <param name="task"></param>
        /// <param name="regionName"></param>
        /// <param name="checkDeviceCode"></param>
        /// <returns>true加锁成功，false加锁失败</returns>
        public static List<ActionSchedulerFilterResult> AddRegionLocks(List<LockInfo> lockInfos)
        {
            lock (_objLocker)
            {
                try
                {
                    List<RegionLock> _regionlocks = new List<RegionLock>();
                    Dictionary<LockInfo, ActionSchedulerFilterResult> _lockInfos = new Dictionary<LockInfo, ActionSchedulerFilterResult>();
                    foreach (var info in lockInfos)
                    {
                        //如果已经加过则直接返回true
                        if (RegionLocks.Any(x => x.TaskCode == info.task.TaskCode && x.RegionName == info.regionName))
                        {
                            _lockInfos.Add(info, new ActionSchedulerFilterResult(false, ""));
                            continue;
                        }

                        //是否可以增加
                        var regionInfo = RegionInfos.First(x => x.RegionName == info.regionName);
                        var _checkDeviceCodeType = regionInfo.Infos.First(x => x.Value.Contains(info.checkDeviceCode)).Key;

                        var __regionLocks = RegionLocks.Where(x => x.RegionName == info.regionName).ToArray();
                        if (__regionLocks.Count() != 0)
                        {
                            var _lockTypes = __regionLocks.Select(x => x.CheckDeviceCodeType).Distinct().ToArray();
                            if (_lockTypes.Length >= 2 ||!_lockTypes.Contains(_checkDeviceCodeType))
                            {
                                _lockInfos.Add(info, new ActionSchedulerFilterResult(true, String.Format("已经存在 {0} 的任务区域锁 ", String.Join("/", __regionLocks.Select(x => x.TaskCode + "_" + x.RegionName + "_" + x.CheckDeviceCode + "_" + x.CheckDeviceCodeType)))));
                                continue;
                            }
                        }

                        var _regionLock = new RegionLock() { RegionName = info.regionName, TaskCode = info.task.TaskCode, CheckDeviceCode = info.checkDeviceCode, CheckDeviceCodeType = _checkDeviceCodeType };
                        _regionlocks.Add(_regionLock);
                    }

                    if (_regionlocks.Count() == 0 || _lockInfos.Any(x=>x.Value.Defeated))
                        return _lockInfos.Select(x => x.Value).ToList();

                    //增加锁
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                    {
                        foreach (var item in _regionlocks)
                        {
                            var isExists = unitOfWork.session.Query<RegionLock>().Any(x => x.RegionName == item.RegionName && x.TaskCode == item.TaskCode);
                            if (!isExists)
                            {
                                unitOfWork.session.Save(item);
                                unitOfWork.session.Flush();
                            }
                        }
                        unitOfWork.Commit();
                    }

                    string msg = String.Join("/", lockInfos.Select(x => String.Format("任务 {0}_{1}", x.task, x.regionName)));
                    _logger.Info(string.Format("{0} 区域锁 锁定成功", msg), typeof(RegionRouteLockerHelper));

                    foreach (var item in _regionlocks)
                    {
                        RegionLockTasks.Add(lockInfos.First(x => x.task.TaskCode == item.TaskCode).task);
                        _regionLocks.Add(item);
                    }
                    return new List<ActionSchedulerFilterResult>() { new ActionSchedulerFilterResult(false, "") };
                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, typeof(RegionRouteLockerHelper));
                    string msg = String.Join("/", lockInfos.Select(x => String.Format("任务 {0}_{1}", x.task, x.regionName)));
                    return new List<ActionSchedulerFilterResult>() { new ActionSchedulerFilterResult(true, String.Format("{0} 区域锁 锁定失败，当前任务无法执行，异常消息：{1}", msg, ex)) };
                }
            }
        }

        static List<String> _deviceCodes;
        private static string getRelationshipLocationDeviceCode(Task task)
        {
            if (_deviceCodes == null)
                _deviceCodes = RegionInfos.SelectMany(x => x.Infos).SelectMany(x => x.Value).ToList();
            if (_deviceCodes.Any(x => x == task.EndLocation.DeviceCode))
                return task.EndLocation.DeviceCode;
            else
                return task.StartLocation.DeviceCode;

        }

        /// <summary>
        /// 删除任务的区域锁
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static ActionSchedulerFilterResult DeleteRegionLock(String regionLockName, String taskCode)
        {
            lock (_objLocker)
            {
                try
                {
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                    {
                        var _regionLock = unitOfWork.session.Query<RegionLock>().FirstOrDefault(x => x.RegionName == regionLockName && x.TaskCode == taskCode);
                        if (_regionLock != null)
                            unitOfWork.session.Delete(_regionLock);
                        unitOfWork.Commit();
                    }
                    var __regionLock = _regionLocks.FirstOrDefault(x => x.RegionName == regionLockName && x.TaskCode == taskCode);
                    if (__regionLock != null)
                        _regionLocks.Remove(__regionLock);
                    if (!_regionLocks.Any(x=>x.TaskCode == taskCode))
                        RegionLockTasks = RegionLockTasks.Where(x => x.TaskCode != taskCode).ToList();

                    _logger.Info(String.Format("清除任务{0}_{1}区域锁成功", taskCode, regionLockName), typeof(RegionRouteLockerHelper));
                    return new ActionSchedulerFilterResult(true, String.Format("清除任务{0}_{1}区域锁成功", taskCode, regionLockName));
                }
                catch (Exception ex)
                {
                    return new ActionSchedulerFilterResult(false, String.Format("删除任务{0}_{1}区域锁失败,异常消息：{2}", taskCode, regionLockName, ex));
                }
            }
        }
    }

    public class RegionInfo
    {
        public string RegionName { get; set; }
        public Dictionary<String, List<String>> Infos { get; set; }
    }

    public class LockInfo
    {
        public Task task { get; set; }
        public String regionName { get; set; }
        public String checkDeviceCode { get; set; }
    }
}
