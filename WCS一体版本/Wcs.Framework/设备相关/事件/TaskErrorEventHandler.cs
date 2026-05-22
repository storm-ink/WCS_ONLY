using NHibernate.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.Framework
{
    public sealed class TaskErrorEventHandler : TaskEventHandler<TaskErrorEventArgs>
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();
        const String CounterName = "任务发生错误处理程序";
        public override void Handle(TaskableDevice device, TaskErrorEventArgs args)
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
                            Logger.Warn1(string.Format("{0} 在处理 {1} 时发生异常 {2}，本次事务未提交", this, handler, ex), this, handler);
                        }

                        if (args.Handled == false)
                        {
                            Logger.Warn1(String.Format("{0} 在处理 {1} 时失败，本次事务未提交", handler, args), this, handler);
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
