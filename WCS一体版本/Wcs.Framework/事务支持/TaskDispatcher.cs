using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NHibernate.Linq;
namespace Wcs.Framework
{
    /// <summary>
    /// 任务派发器<br />
    /// 用来对满足条件的任务进行拆分成逻辑动作
    /// </summary>
    public class TaskDispatcher
    {
        Thread _thread;
        public Logger Logger { get; private set; }
        public TaskDispatcher()
        {
            Logger = new Wcs.Logger(this, new Wcs.Framework.Impl.NLogTarget("TaskDispatcher"));
            _thread = new Thread(dispatcher_prc);
            _thread.IsBackground = true;
            _thread.Start();
            //ThreadPool.QueueUserWorkItem(new WaitCallback(dispatcher_prc));
        }
        /// <summary>
        /// 任务派发器主函数
        /// </summary>
        /// <param name="state"></param>
        protected virtual void dispatcher_prc(object state)
        {
            while (true)
            {
                Thread.Sleep(500);
                try
                {
                    List<LogicMovement> movements = new List<LogicMovement>();

                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                    {
                        var tasks = Repository
                            .Query<Task>(unitOfWork)
                            .Fetch(x => x.Movements)
                            .Where(x => x.Status == TaskStatus.New || x.Status == TaskStatus.Executing)
                            .Where(task =>
                                !task
                                .Movements
                                .Any(movement =>
                                    movement.Status == LogicMovementStatus.Error
                                    || movement.Status == LogicMovementStatus.Executing
                                    || movement.Status == LogicMovementStatus.Suspend)
                                    );
                        if (Cfg.Configuration.GetSetting<bool>("出库优先", false))
                        {
                            tasks = tasks
                                .OrderBy(x => x.Direction == TaskDirection.Out ? 0 : 1)
                                .ThenByDescending(x => x.Priority)
                                .ThenBy(x => x.Id);
                        }
                        else
                        {
                            tasks = tasks
                                .OrderByDescending(x => x.Priority)
                                .ThenBy(x => x.Id);
                        }
                        foreach (var task in tasks)
                        {
                            var movement = task.GetNextMovement(unitOfWork);
#warning 多个任务同时保存时会出现动作属性表被锁死的情况
                            //break;
                            if (movement != null)
                            {
                                movements.Add(movement);
                            }
                        }

                        unitOfWork.Commit();

                        tasks = null;
                    }



                    foreach (var movement in movements)
                    {
                        if (movement.Status != LogicMovementStatus.New)
                        {
                            continue;
                        }
                        foreach (var action in movement.EquipmentActions)
                        {
                            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                            {
                                var actionNowStatus = unitOfWork.session.Get<EquipmentAction>(action.Id, NHibernate.LockMode.Upgrade);
                                if (actionNowStatus == null)
                                {
                                    continue;
                                }
                                else
                                {
                                    if (actionNowStatus.Status != EquipmentActionStatus.New || actionNowStatus.Sequence == null || actionNowStatus.Movement.Task.CurrentLocation.Equals(actionNowStatus.Movement.EndLocation))
                                    {
                                        continue;
                                    }

                                    if (actionNowStatus.Sequence.Contains(actionNowStatus))
                                    {
                                        continue;
                                    }
                                    actionNowStatus.Sequence.Push(actionNowStatus);
                                }

                                unitOfWork.Commit();
                            }
                        }
                    }

                    movements = null;
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex.Message, this,ex);
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }
        }
    }
}
