using NHibernate.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Wcs.Framework;
using Wcs.Framework.Cfg;
using Wcs.Framework.Events;

namespace Wcs.DefaultImpls.Business
{
    /// <summary>
    /// 自动循环（穿梭车调度系统）测试任务
    /// </summary>
    public class AutoTestCreateCircleTaskForVehicleStartUp : IApplicationStartup
    {
        static Logger _logger;
        List<String> _taskTypes = new List<string>();
        List<string> _clearList;
        List<List<string>> _randomLocs = new List<List<string>>();
        static Thread _taskHandThread;
        Random random;

        public void Initialize(StartupElement element)
        {
            _logger = LogManager.CreateNullLogger();
            random = new Random();
            _taskTypes = element.GetAttributeOrDefault<string>("taskTypes", "test").Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            _clearList = element.GetAttributeOrDefault<string>("clearList", "").Split(',').ToList();
            var randomList = element.GetAttributeOrDefault<string>("randomLocs", "").Split(';').ToList();
            foreach (var item in randomList)
            {
                _randomLocs.Add(item.Split(',').ToList());
            }
        }

        public void Run(IWcsApplication application)
        {
            Wcs.Framework.EventBus.EventBus.Instance.Subscribe<Wcs.Framework.Events.TaskFinishedEvent>(onTaskFinished);

            _taskHandThread = new System.Threading.Thread(Proc);
            _taskHandThread.Name = $"自动循环（穿梭车调度系统）测试任务";
            _taskHandThread.IsBackground = true;
            _taskHandThread.StartAndManaged();
        }

        private void onTaskFinished(TaskFinishedEvent args)
        {
            if (args.Source != Wcs.Framework.TaskSource.Unknow || !_taskTypes.Contains(args.TaskType) || args.Status != TaskStatus.Completed)
                return;

            Handle(args);
        }

        private void Handle(TaskFinishedEvent args)
        {
            try
            {
                Task _task;

                Boolean alreadyCreateTask = false;
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                {
                    //tsks = unitOfWork.session.Query<Task>().Where(x => _taskTypes.Contains(x.TaskType)).ToList();
                    _task = unitOfWork.session.Get<Task>(args.Id);
                    alreadyCreateTask = unitOfWork.session.Query<Task>().Any(x => x.Description == args.TaskCode);
                    unitOfWork.Commit();
                }
                if (_task == null)
                    return;
                if (alreadyCreateTask)
                {
                    BusinessHelper.PushDeleteTaskList(_task);
                    return;
                }

                if (_clearList.Contains(_task.EndLocation.DeviceCode))
                    //ClearLocationTask(_task.EndLocation);

                PushTaskList(_task);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ClearLocationTask(LocationInfo endLocation)
        {
            //try
            //{
            //    var conveyor = DeviceConverter.ToDevice<Conveyor.ConveyorDevice>(endLocation.DeviceName);
            //    if (conveyor.LocationCurrentTasks == null)
            //        return;
            //    var posNo = UInt16.Parse(endLocation.DeviceCode);
            //    var locTask = conveyor.LocationCurrentTasks.FirstOrDefault(x => x.PosNo == posNo && x.TaskNo != 0);
            //    if (locTask == null)
            //        return;
            //    Conveyor.ClearLocationTaskCommand cmd = new Conveyor.ClearLocationTaskCommand(posNo, locTask.TaskNo, locTask.TUID, (UInt16)locTask.AtPacketIndex);
            //    conveyor.Write(cmd, cmd.SendSuccess);
            //}
            //catch (Exception ex)
            //{
            //}
        }


        #region 完成任务处理
        /// <summary>
        /// 任务待处理列表
        /// </summary>
        public static List<Task> _waitTasks = new List<Task>();

        /// <summary>
        /// 是否已经存在于待处理列表中
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static Boolean ExistProcTaskList(Task task)
        {
            return _waitTasks.Any(x => x.TaskCode == task.TaskCode);
        }

        /// <summary>
        /// 将任务添加到待处理列表
        /// </summary>
        /// <param name="task"></param>
        public void PushTaskList(Task task)
        {

            lock (_waitTasks)
            {
                if (_waitTasks.Any(x => x.TaskCode == task.TaskCode))
                {
                    _logger.Warn1(string.Format("{0}已存在于归档队列当中", task), typeof(BusinessHelper), task);
                    return;
                }

                _waitTasks.Add(task);

                _logger.Debug1(string.Format("{0}被加入归档队列", task), typeof(BusinessHelper), task);
            }
        }

        /// <summary>
        /// 将任务从待处理列表中移除
        /// </summary>
        /// <param name="task"></param>
        public void PopTaskList(Task task)
        {
            lock (_waitTasks)
            {
                var item = _waitTasks.FirstOrDefault(x => x.TaskCode == task.TaskCode);
                if (item == null)
                {
                    _logger.Warn1(string.Format("{0}已被从归档队列移除", item), typeof(BusinessHelper), item);
                    return;
                }

                _waitTasks.Remove(item);

                _logger.Debug1(string.Format("{0}被移出归档队列", item), typeof(BusinessHelper), item);
            }
        }
        /// <summary>
        /// 任务归档线程
        /// </summary>
        public void Proc()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(500);
                try
                {
                    Task[] tasksCloned;
                    lock (_waitTasks)
                    {
                        tasksCloned = _waitTasks.ToArray();
                    }

                    foreach (var item in tasksCloned)
                    {
                        bool locked = false;
                        try
                        {
                            locked = item.EnterLock();
                            if (!locked)
                            {
                                _logger.Warn1(String.Format("{0}锁定失败，推迟处理。", item), typeof(BusinessHelper), item);
                            }
                            else
                            {
                                Task _task;
                                List<Task> tsks;
                                Boolean alreadyCreateTask = false;
                                using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                                {
                                    tsks = unitOfWork.session.Query<Task>().Where(x => _taskTypes.Contains(x.TaskType)).ToList();
                                    _task = unitOfWork.session.Get<Task>(item.Id);
                                    alreadyCreateTask = unitOfWork.session.Query<Task>().Any(x => x.Description == item.TaskCode);
                                    unitOfWork.Commit();
                                }
                                if (_task == null)
                                {
                                    PopTaskList(item);
                                    continue;
                                }
                                if (alreadyCreateTask)
                                {
                                    BusinessHelper.PushDeleteTaskList(_task);
                                    PopTaskList(item);
                                    continue;
                                }

                                String taskCode = Wcs.Framework.SerialNumberFactory.GenerateManualTaskCode();
                                LocationInfo start = _task.EndLocation;
                                LocationInfo end = _task.StartLocation;
                                if (_randomLocs.Any(x=>x.Contains($"{start.DeviceCode}@{start.DeviceName}")))
                                {
                                    List<string> _randomList = _randomLocs.First(x => x.Contains($"{start.DeviceCode}@{start.DeviceName}"));
                                    List<string> list = new List<string>();
                                    tsks = tsks.Where(x => x.Id != item.Id).ToList();
                                    if (tsks.Count() > 0)
                                    {
                                        list.AddRange(tsks.Select(x => x.StartLocation.DeviceCode + "@" + x.StartLocation.DeviceName));
                                        list.AddRange(tsks.Select(x => x.EndLocation.DeviceCode + "@" + x.EndLocation.DeviceName));
                                        list.Add($"{start.DeviceCode}@{start.DeviceName}");
                                    }
                                    var _list = _randomList.Where(x => !list.Contains(x)).ToArray();
                                    end = LocationConverter.ToLocationInfo(LocationConverter.ConvertibleCodeToLcation(_list[random.Next(0, _list.Length)]));
                                }
                                Task task = new Task(taskCode, start, end)
                                {
                                    Source = TaskSource.Unknow,
                                    TaskType = _task.TaskType,
                                    Description = _task.TaskCode
                                };
                                task.ContainerCodes.Add(_task.TaskCode);

                                using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                                {
                                    unitOfWork.session.Save(task);
                                    unitOfWork.Commit();
                                }
                                Wcs.Framework.EventBus.EventBus.Instance.Publish(new Wcs.Framework.Events.TaskAddedEvent(task));

                                BusinessHelper.PushDeleteTaskList(_task);
                                PopTaskList(item);
                                //conveyor.清除占位(holdSingle);
                                _logger.Info(String.Format("成功创建一条从 {0} 到 {1} 的 {2} 任务", start.DeviceCode, end.DeviceCode, task.TaskType), this);
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
    }
}
