using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;
using NLog;
using NHibernate.Linq;

namespace Wcs.DefaultImplementCollection.Business
{
    /// <summary>
    /// 业务帮助
    /// </summary>
    public static class BusinessHelper
    {
        static Logger _logger = LogManager.CreateNullLogger();

        #region 归档处理
        /// <summary>
        /// 任务待归档列表
        /// </summary>
        public static List<Task> _waitDeleteTasks = new List<Task>();

        /// <summary>
        /// 是否已经存在于归档列表中
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static Boolean ExistArchiveProcTaskList(Task task)
        {
            return _waitDeleteTasks.Any(x => x.TaskCode == task.TaskCode);
        }

        /// <summary>
        /// 将任务添加到归档列表
        /// </summary>
        /// <param name="task"></param>
        public static void PushDeleteTaskList(Task task)
        {

            lock (_waitDeleteTasks)
            {
                if (_waitDeleteTasks.Any(x => x.TaskCode == task.TaskCode))
                {
                    _logger.Warn1(string.Format("{0}已存在于归档队列当中", task), typeof(BusinessHelper), task);
                    return;
                }

                _waitDeleteTasks.Add(task);

                _logger.Debug1(string.Format("{0}被加入归档队列", task), typeof(BusinessHelper), task);
            }
        }

        /// <summary>
        /// 将任务从归档列表中移除
        /// </summary>
        /// <param name="task"></param>
        public static void PopDeleteTaskList(Task task)
        {
            lock (_waitDeleteTasks)
            {
                var item = _waitDeleteTasks.FirstOrDefault(x => x.TaskCode == task.TaskCode);
                if (item == null)
                {
                    _logger.Warn1(string.Format("{0}已被从归档队列移除", item), typeof(BusinessHelper), item);
                    return;
                }

                _waitDeleteTasks.Remove(item);

                _logger.Debug1(string.Format("{0}被移出归档队列", item), typeof(BusinessHelper), item);
            }
        }

        /// <summary>
        /// 任务归档线程
        /// </summary>
        public static void ArchiveProc()
        {
            //延时归档列表
            List<Task> delayedArchiveTaskList = new List<Task>();
            while (true)
            {
                var delayedArchiveTaskTypes = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("delayedArchive", "尺检失败回退").Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                var delayedArchiveOverTime = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<int>("delayedArchiveTime", 1);

                System.Threading.Thread.Sleep(500);
                try
                {
                    Task[] tasksCloned;
                    lock (_waitDeleteTasks)
                    {
                        tasksCloned = _waitDeleteTasks.ToArray();
                    }

                    foreach (var item in tasksCloned)
                    {
                        Task sameDelayedArchiveTaskTask = null;
                        //延时归档判断
                        var delayedArchiveTask = delayedArchiveTaskList.FirstOrDefault(x => x.TaskCode == item.TaskCode);
                        if (delayedArchiveTask != null)
                        {
                            sameDelayedArchiveTaskTask = tasksCloned.FirstOrDefault(x => x.TaskCode != delayedArchiveTask.TaskCode
                            && x.StartLocation.DeviceCode == delayedArchiveTask.StartLocation.DeviceCode
                            && x.EndLocation.DeviceCode == delayedArchiveTask.EndLocation.DeviceCode
                            && x.CreatedAt > delayedArchiveTask.CreatedAt);
                            if (sameDelayedArchiveTaskTask != null)
                                delayedArchiveTaskList.Remove(delayedArchiveTask);
                            else
                            {
                                var finishAt = (DateTime)delayedArchiveTask.FinishedAt;
                                if ((DateTime.Now - finishAt).TotalSeconds < delayedArchiveOverTime * 60)
                                    continue;
                                else
                                    delayedArchiveTaskList.Remove(delayedArchiveTask);
                            }
                        }

                        bool locked = false;
                        try
                        {
                            locked = item.EnterLock();
                            if (!locked)
                            {
                                _logger.Warn1(String.Format("{0}锁定失败，推迟归档。", item), typeof(BusinessHelper), item);
                            }
                            else
                            {
                                _logger.Trace1(string.Format("准备将{0}任务{1}归档...", item.TaskType, item), typeof(BusinessHelper), item);

                                using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                                {
                                    var deleteObject = unitOfWork
                                        .session
                                        .Query<Task>()
                                        .Where(x =>
                                            x.Id == item.Id
                                            && (x.Status == TaskStatus.Cancelled || x.Status == TaskStatus.Completed)
                                        )
                                        .FirstOrDefault();

                                    if (deleteObject != null)
                                    {
                                        if (delayedArchiveTaskTypes.Contains(deleteObject.TaskType) && deleteObject.FinishedAt != null && sameDelayedArchiveTaskTask == null)
                                        {
                                            var finishAt = (DateTime)deleteObject.FinishedAt;
                                            if ((DateTime.Now - finishAt).TotalSeconds < delayedArchiveOverTime * 60)
                                            {
                                                delayedArchiveTaskList.Add(deleteObject);
                                                unitOfWork.Commit();
                                                continue;
                                            }
                                        }

                                        unitOfWork.session.Delete(deleteObject);
                                    }
                                    else
                                        PopDeleteTaskList(item);

                                    unitOfWork.Commit();
                                }

                                _logger.Debug1(string.Format("{0} 归档成功.", item), typeof(BusinessHelper), item);

                                PopDeleteTaskList(item);

                                Wcs.Framework.EventBus.EventBus.Instance.Publish(new Wcs.Framework.Events.TaskArchivedEvent(item.Id, item.TaskCode));
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error1(ex, typeof(BusinessHelper), item);
                        }
                        finally
                        {
                            if (locked)
                            {
                                item.ExitLock();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, typeof(BusinessHelper));
                }
            }
        }
        #endregion

        #region 请求处理
        /// <summary>
        /// 等待请求的任务
        /// </summary>
        public static List<Task> _waitRequestTasks = new List<Task>();

        /// <summary>
        /// 是否已经存在于请求列表中
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static Boolean ExistRequestProcTaskList(Task task)
        {
            return _waitRequestTasks.Any(x => x.TaskCode == task.TaskCode);
        }

        /// <summary>
        /// 将任务添加到待请求列表
        /// </summary>
        /// <param name="task"></param>
        public static void PushRequestList(Task task)
        {
            lock (_waitRequestTasks)
            {
                if (_waitRequestTasks.Any(x => x.TaskCode == task.TaskCode))
                {
                    _logger.Warn1(string.Format("{0}已存在于请求队列当中", task), typeof(BusinessHelper), task);
                    return;
                }

                _waitRequestTasks.Add(task);

                _logger.Debug1(string.Format("{0}被加入请求队列", task), typeof(BusinessHelper), task);
            }
        }

        /// <summary>
        /// 将任务从请求列表中移除
        /// </summary>
        /// <param name="task"></param>
        public static void PopRequestList(Task task)
        {
            lock (_waitRequestTasks)
            {
                var item = _waitRequestTasks.FirstOrDefault(x => x.TaskCode == task.TaskCode);
                if (item == null)
                {
                    _logger.Warn1(string.Format("{0}已被从请求队列移除", item), typeof(BusinessHelper), item);
                    return;
                }

                _waitRequestTasks.Remove(item);

                _logger.Debug1(string.Format("{0}被移出请求队列", item), typeof(BusinessHelper), item);
            }
        }

        /// <summary>
        /// 任务完成请求线程
        /// </summary>
        public static void RequestProc()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(1000);
                Task[] tasksCloned;
                lock (_waitRequestTasks)
                {
                    tasksCloned = _waitRequestTasks.ToArray();
                }

                foreach (var item in tasksCloned)
                {
                    try
                    {
                        Task task;
                        using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                        {
                            task = unitOfWork.session.Get<Task>(item.Id);
                            unitOfWork.Commit();
                        }
                        if (task == null)
                        {
                            PopRequestList(item);
                            continue;
                        }

                        if (ExistArchiveProcTaskList(task))
                        {
                            PopRequestList(task);
                            continue;
                        }

                        foreach (var handler in Wcs.Framework.Cfg.WcsConfiguration.Instance.TaskRequestHandlersElement.BaseTaskRequestHandlers)
                        {
                            if (handler.Allowed(item))
                                handler.Hand(item);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error1(ex, typeof(BusinessHelper));
                    }
                }
            }
        }
        #endregion

        #region 位置锁
        static object _lockObj = new object();

        /// <summary>
        /// 锁定位置
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public static Boolean LockLocation(Location location, Int32 equipmentTaskId)
        {
            lock (_lockObj)
            {
                DeviceState _deviceState;
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    _deviceState = unitOfWork.session.Query<DeviceState>().FirstOrDefault(x => x.DeviceName == location.DeviceCode && x.Device == location.Device.Name);
                    if (_deviceState == null)
                    {
                        unitOfWork.Commit();
                        return false;
                    }
                    if (_deviceState.TaskLock == 0)
                    {
                        _deviceState.TaskLock = equipmentTaskId;
                        unitOfWork.session.SaveOrUpdate(_deviceState);
                        unitOfWork.Commit();
                        return true;
                    }
                    else
                        return false;
                }
            }
        }

        /// <summary>
        /// 锁定多个位置
        /// </summary>
        /// <param name="locations">锁定列表</param>
        /// <param name="equipmentTaskId">设备号</param>
        /// <returns>更新结果：true成功，false失败</returns>
        public static Boolean LockLocation(List<Location> locations, Int32 equipmentTaskId)
        {
            lock (_lockObj)
            {
                Boolean result = false;
                List<DeviceState> _deviceStates;
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    var strs = locations.Select(x => x.ToConvertibleCode()).ToList();
                    _deviceStates = unitOfWork.session.Query<DeviceState>().Where(x => strs.Contains((x.DeviceName + "@" + x.Device).ToString())).ToList();
                    unitOfWork.Commit();
                }
                if (_deviceStates != null && !_deviceStates.Any(x => x.TaskLock != 0 && x.TaskLock != equipmentTaskId))
                {
                    using (System.Transactions.TransactionScope scope = new System.Transactions.TransactionScope())
                    {
                        try
                        {
                            foreach (var _deviceState in _deviceStates)
                            {
                                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                                {
                                    _deviceState.TaskLock = equipmentTaskId;
                                    unitOfWork.session.SaveOrUpdate(_deviceState);
                                    unitOfWork.Commit();
                                }
                            }
                            result = true;
                            scope.Complete();
                        }
                        catch
                        {
                            scope.Dispose();
                            result = false;
                        }
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// 任务是否已为位置加锁
        /// </summary>
        /// <param name="location"></param>
        /// <param name="equipmentTaskId"></param>
        /// <returns></returns>
        public static Boolean LocationIsLocked(Location location, Int32 equipmentTaskId)
        {
            DeviceState _deviceState;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                _deviceState = unitOfWork.session.Query<DeviceState>().FirstOrDefault(x => x.DeviceName == location.DeviceCode && x.Device == location.Device.Name);
                unitOfWork.Commit();
            }
            if (_deviceState == null)
                return false;
            if (_deviceState.TaskLock != equipmentTaskId)
                return false;
            else
                return true;
        }
        #endregion
    }
}
