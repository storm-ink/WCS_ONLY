using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using NHibernate.Linq;
using NLog;

namespace Wcs.Framework
{
    public sealed class TaskCompletedEventHandler : TaskEventHandler<TaskCompletedEventArgs>
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();
        const String CounterName = "任务完成处理程序";
        static Thread _taskCompletedThread;
        public TaskCompletedEventHandler()
        {
            _taskCompletedThread = new System.Threading.Thread(CompletedProc);
            _taskCompletedThread.Name = "设备任务完成处理程序";
            _taskCompletedThread.IsBackground = true;
            _taskCompletedThread.StartAndManaged();
        }
        public override void Handle(TaskableDevice device, TaskCompletedEventArgs args)
        {
            if (ExecutiveState.ApplicationExiting)
            {
                _logger.Warn1("应用程序正在退出，不再处理该事件。", this, args);
                args.Handled = false;
                return;
            }

            if (_handlers.Count == 0)
            {
                args.Handled = true;
                return;
            }

            EquipmentAction act;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                act = unitOfWork.session.Query<EquipmentAction>()
                    .FirstOrDefault(x => x.EquipmentTaskId == args.EquipmentTaskId);
                unitOfWork.Commit();
            }

            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {


                //if (act == null)
                //{
                //    args.Handled = true;

                //    Logger.Warn1(string.Format("{0} 在处理设备任务号 {1} 时发现该物理动作不存在，不再处理。", this, args.EquipmentTaskId), this, null, args.EquipmentTaskId);

                //    unitOfWork.Commit();
                //    tsc.Complete();
                //    return;
                //}

                bool locked = false;
                try
                {
                    ExecutiveState.ActiveCounter(CounterName).Increment();

                    if (act != null)
                    {
                        locked = act.EnterLock();

                        if (!locked)
                        {
                            throw new Exception(String.Format("{0}锁定失败。", act));
                        }

                    }

                    foreach (var handler in _handlers)
                    {
                        try
                        {
                            handler.Handle(device, unitOfWork.session, args);
                        }
                        catch (Exception ex)
                        {
                            args.Handled = false;
                            Logger.Warn1(string.Format("{0} 在处理 {1} 时发生异常 {2}，本次事务未提交", this, handler, ex), this, act);
                        }

                        if (args.Handled == false)
                        {
                            Logger.Warn1(String.Format("{0} 在处理 {1} 时失败，本次事务未提交", handler, args), this, act);
                            break;
                        }
                    }
                    if (args.Handled)
                    {
                        unitOfWork.session.Flush();
                        unitOfWork.Commit();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error1(ex, this, act);
                    throw;
                }
                finally
                {
                    ExecutiveState.ActiveCounter(CounterName).Decrement();

                    if (locked)
                    {
                        act.ExitLock();
                    }
                }
            }
        }
        public void CompletedHandle(TaskableDevice device, TaskCompletedEventArgs args)
        {
            if (ExecutiveState.ApplicationExiting)
            {
                //_logger.Warn1("应用程序正在退出，不再处理该事件。", this, args);
                _logger.Warn1("应用程序正在退出，不再处理该事件。", typeof(TaskCompletedEventHandler), args);
                args.Handled = false;
                return;
            }

            if (_handlers.Count == 0)
            {
                args.Handled = true;
                return;
            }

            EquipmentAction act;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                act = unitOfWork.session.Query<EquipmentAction>()
                    .FirstOrDefault(x => x.EquipmentTaskId == args.EquipmentTaskId);
                unitOfWork.Commit();
            }
            if (act == null || act.Status == EquipmentActionStatus.Completed || act.Status == EquipmentActionStatus.Cancelled || act.Status == EquipmentActionStatus.Error)
            {
                args.Handled = true;
                return;
            }

            using (TransactionScope tsc = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    bool locked = false;
                    try
                    {
                        ExecutiveState.ActiveCounter(CounterName).Increment();

                        if (act != null)
                        {
                            locked = act.EnterLock();

                            if (!locked)
                            {
                                unitOfWork.Commit();
                                tsc.Complete();
                                throw new Exception(String.Format("{0}锁定失败。", act));
                            }

                        }

                        foreach (var handler in _handlers)
                        {
                            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                            sw.Start();
                            try
                            {
                                handler.Handle(device, unitOfWork.session, args);
                            }
                            catch (Exception ex)
                            {
                                args.Handled = false;
                                //Logger.Warn1(string.Format("{0} 在处理 {1} 时发生异常 {2}，本次事务未提交", this, handler, ex), this, act);
                                Logger.Warn1(string.Format("{0} 在处理 {1} 时发生异常 {2}，本次事务未提交", typeof(TaskCompletedEventHandler), handler, ex), typeof(TaskCompletedEventHandler), act);
                            }
                            sw.Stop();
                            Console.WriteLine($"处理程序 {handler.GetType().Name}，处理耗时 {sw.ElapsedMilliseconds} ms");
                            if (args.Handled == false)
                            {
                                //Logger.Warn1(String.Format("{0} 在处理 {1} 时失败，本次事务未提交", handler, args), this, act);
                                Logger.Warn1(String.Format("{0} 在处理 {1} 时失败，本次事务未提交", handler, args), typeof(TaskCompletedEventHandler), act);
                                break;
                            }
                        }
                        if (args.Handled)
                        {
                            unitOfWork.session.Flush();
                            unitOfWork.Commit();
                            tsc.Complete();
                        }
                    }
                    catch (Exception ex)
                    {
                        args.Reset();
                        //Logger.Error1(ex, this, act);
                        Logger.Error1(ex, typeof(TaskCompletedEventHandler), act);
                        throw;
                    }
                    finally
                    {
                        ExecutiveState.ActiveCounter(CounterName).Decrement();

                        if (locked)
                        {
                            act.ExitLock();
                        }
                    }
                }
            }
        }

        #region 完成处理
        /// <summary>
        /// 任务待完成列表
        /// </summary>
        public static List<TaskCompletedEventHandlerArgs> _waitCompeltedTasks = new List<TaskCompletedEventHandlerArgs>();

        /// <summary>
        /// 是否已经存在于完成列表中
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static Boolean ExistArchiveProcTaskList(TaskCompletedEventHandlerArgs args)
        {
            return _waitCompeltedTasks.Any(x => x.Device.Name == args.Device.Name && x.Args.EquipmentTaskId == args.Args.EquipmentTaskId);
        }

        /// <summary>
        /// 将任务添加到完成列表
        /// </summary>
        /// <param name="task"></param>
        public static TaskCompletedEventHandlerArgs PushCompeltedTaskList(TaskCompletedEventHandlerArgs args)
        {
            lock (_waitCompeltedTasks)
            {
                var item = _waitCompeltedTasks.FirstOrDefault(x => x.Equals(args));
                if (item != null)
                {
                    Console.WriteLine(string.Format("☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆{0}已存在于完成处理队列当中", args));
                    //_logger.Warn1(string.Format("{0}已存在于完成处理队列当中", args), this);
                    return item;
                }
                else
                {
                    _waitCompeltedTasks.Add(args);
                    //_logger.Debug1(string.Format("{0}被加入完成处理队列", args), this);
                    Console.WriteLine(string.Format("☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆{0}被加入完成处理队列", args));
                    return args;
                }
            }
        }

        /// <summary>
        /// 更新状态
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static void UpdateCompeltedTaskList(TaskCompletedEventHandlerArgs args)
        {
            lock (_waitCompeltedTasks)
            {
                var item = _waitCompeltedTasks.FirstOrDefault(x => x.Equals(args));
                if (item == null)
                    _waitCompeltedTasks.Add(args);
                else
                    item = args;

                //if (_waitCompeltedTasks.Any(x => x.Equals(args)))
                //{
                //    _logger.Warn1(string.Format("{0}已存在于完成处理队列当中", args), this);
                //    return item.Args.Handled;
                //}
                //item.Args.Handled = true;

                //_logger.Debug1(string.Format("{0}被更新成已处理", args), this);
                //return args.Args.Handled;
            }
        }

        /// <summary>
        /// 将任务从完成列表中移除
        /// </summary>
        /// <param name="task"></param>
        public static void PopCompeltedTaskList(TaskCompletedEventHandlerArgs args)
        {
            lock (_waitCompeltedTasks)
            {
                var item = _waitCompeltedTasks.FirstOrDefault(x => x.Equals(args));
                if (item == null)
                {
                    //_logger.Warn1(string.Format("{0}已被从完成队列移除", item), this);
                    _logger.Warn1(string.Format("{0}已被从完成队列移除", item), typeof(TaskCompletedEventHandler));
                    return;
                }

                _waitCompeltedTasks.Remove(item);

                //_logger.Debug1(string.Format("{0}被移出完成队列", item), this);
                _logger.Debug1(string.Format("{0}被移出完成队列", item), typeof(TaskCompletedEventHandler));
            }
        }

        /// <summary>
        /// 将任务从完成列表中移除
        /// </summary>
        /// <param name="task"></param>
        public static void PopsCompeltedTaskList(List<int> eIds)
        {
            lock (_waitCompeltedTasks)
            {
                _waitCompeltedTasks = _waitCompeltedTasks.Where(x => eIds.Contains(x.Args.EquipmentTaskId)).ToList();
                //var item = _waitCompeltedTasks.FirstOrDefault(x => x.Equals(args));
                //if (item == null)
                //{
                //    //_logger.Warn1(string.Format("{0}已被从完成队列移除", item), this);
                //    _logger.Warn1(string.Format("{0}已被从完成队列移除", item), typeof(TaskCompletedEventHandler));
                //    return;
                //}

                //_waitCompeltedTasks.Remove(item);

                ////_logger.Debug1(string.Format("{0}被移出完成队列", item), this);
                //_logger.Debug1(string.Format("{0}被移出完成队列", item), typeof(TaskCompletedEventHandler));
            }
        }

        /// <summary>
        /// 任务完成线程
        /// </summary>
        public void CompletedProc()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(100);
                try
                {
                    //Console.WriteLine($"★★★★★★★★★★★★★★★★★缓存处理{_waitCompeltedTasks.Count()}条");
                    TaskCompletedEventHandlerArgs[] completedArgsTasksCloned;
                    lock (_waitCompeltedTasks)
                    {
                        if (_waitCompeltedTasks.Count() > 0)
                            Console.WriteLine($"★★★★★★★★★★★★★★★★★缓存数据{string.Join(" | ", _waitCompeltedTasks.Select(x => x.Args.EquipmentTaskId + "_" + x.Args.Handled))}");
                        completedArgsTasksCloned = _waitCompeltedTasks.Where(x => !x.Args.Handled).ToArray();
                    }
                    //Console.WriteLine($"★★★★★★★★★★★★★★★★★实际处理{completedArgsTasksCloned.Length}条");
                    if (completedArgsTasksCloned.Length == 0 && _waitCompeltedTasks.Count() > 0 && _waitCompeltedTasks.All(x => x.Args.Handled))
                    {
                        List<int> eIds = new List<int>();
                        using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                        {
                            eIds = unitOfWork.session.Query<EquipmentAction>().Select(x => x.EquipmentTaskId).ToList();
                            unitOfWork.Commit();
                        }
                        PopsCompeltedTaskList(eIds);
                    }

                    foreach (var item in completedArgsTasksCloned)
                    {
                        Console.WriteLine($"√√√√√√√√√√√√√√开始进行 {item} 任务完成处理");
                        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                        sw.Start();
                        CompletedHandle(item.Device, item.Args);
                        sw.Stop();
                        if (item.Args.Handled)
                            UpdateCompeltedTaskList(item);
                        Console.WriteLine($"XXXXXXXXXXXXXXXXXXXXXXXXXXX已完成 {item} 任务完成处理，处理结果{item.Args.Handled},处理耗时 {sw.ElapsedMilliseconds} ms");
                        //异步处理
                        if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<bool>("设备事件.异步处理", true))
                        {
                            item.Args.FireLazyEvents(true);
                        }
                        else
                        {
                            item.Args.FireLazyEvents(false);
                        }
                        System.Threading.Thread.Sleep(10);
                    }
                }
                catch (Exception ex)
                {
                    //_logger.Error1(ex, this);
                    _logger.Error1(ex, typeof(TaskCompletedEventHandler));
                }
            }
        }
        #endregion
    }

    public class TaskCompletedEventHandlerArgs
    {
        public TaskableDevice Device { get; set; }
        public TaskCompletedEventArgs Args { get; set; }

        public override bool Equals(object obj)
        {
            var _obj = (TaskCompletedEventHandlerArgs)obj;
            return this.ToString() == _obj.ToString();
        }

        public override string ToString()
        {
            return $"{Device.Name}_{Args.EquipmentTaskId}";
        }
    }
}
