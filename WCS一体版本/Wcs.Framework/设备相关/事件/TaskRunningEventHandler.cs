using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Linq;
using NLog;

namespace Wcs.Framework
{
    public sealed class TaskRunningEventHandler:TaskEventHandler<TaskRunningEventArgs>
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();
        const String CounterName = "任务状态改变为“执行中”事件处理程序";

        public override void Handle(TaskableDevice device, TaskRunningEventArgs args)
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

            if (act == null)
            {
                args.Handled = true;

                Logger.Warn1(string.Format("{0} 在处理设备任务号 {1} 时发现该物理动作不存在，不再处理。", this, args.EquipmentTaskId), this, null, args.EquipmentTaskId);

                return;
            }

            if (act.Status == EquipmentActionStatus.Executing)
            {
                args.Handled = true;

                Logger.Warn1(string.Format("{0} 在处理 {1} 时发现该物理动作已处于运行状态，不再处理。", this, act), this, act);

                return;
            }

            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                bool locked = false;
                try
                {
                    ExecutiveState.ActiveCounter(CounterName).Increment();

                    if (act != null)
                    {
                        int retries = 0;

retry:
                        locked = act.EnterLock();

                        if (!locked)
                        {
                            if (retries == 0)
                            {
                                Logger.Warn1(String.Format("{0} 在处理 {1} 时锁定失败，将在1000毫秒后重新尝试锁定。", this, act), this, act);

                                System.Threading.Thread.Sleep(1000);

                                retries++;

                                goto retry;
                            }
                            else
                            {
                                throw new Exception(String.Format("{0}锁定失败。", act));
                            }
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
    }
}