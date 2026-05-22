using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Wcs.Framework;
using Wcs.Framework.EventBus;
using Wcs.Framework.Events;
//Scheduler 调度程序
namespace Wcs.DefaultImplementCollection.Conveyor
{
    public sealed class ConveyorDeviceEquipmentActionScheduler: EquipmentActionScheduler
    {
        public ConveyorDeviceEquipmentActionScheduler(TaskableDevice device, EquipmentActionSchedulerFilter[] actionFilters)
            : base(device,actionFilters)
        {
        }

        protected ConveyorDeviceEquipmentActionScheduler()
            : base()
        {
        }

        protected override void Tick()
        {
            this._logger.Trace1(string.Format("{0} 轮询线程已启动", this), this);
            while (true)
            {
                try
                {
                    Thread.Sleep(this.Interval);
                    _methodDescriptorTree.ClearAccess();

                    _methodDescriptorTree["开始"].Access(true);

                    if (this.Actions.Length == 0)
                    {
                        var actions = _actions.Where(x => x.Id == 0).Select(x => x.EquipmentTaskId);
                        if (actions.Count() > 0)
                            _methodDescriptorTree["是否包含动作"].Access(false, $"不包含任何动作(其中 {string.Join("|", actions)} 共 {actions.Count()} 条任务id=0)");
                        else
                            _methodDescriptorTree["是否包含动作"].Access(false, $"不包含任何动作");
                        break;
                    }
                    _methodDescriptorTree["是否包含动作"].Access(true);

                    if (!this.Device.IsConnected)
                    {
                        _methodDescriptorTree["设备是否已连接"].Access(false, "设备未连接");
                        continue;
                    }
                    _methodDescriptorTree["设备是否已连接"].Access(true);

                    var isidle = this.Device.IsIdle;
                    if (!isidle.Result)
                    {
                        //String msg = "";
                        //if (this.Device.Warnings != null)
                        //{
                        //    msg = string.Join("\n", this.Device.Warnings);
                        //}
                        _methodDescriptorTree["设备是否空闲"].Access(false, isidle.Information);
                        continue;
                    }
                    _methodDescriptorTree["设备是否空闲"].Access(true);

                    var availableActions = GetAvailableActions();
                    if (availableActions == null)
                    {
                        _methodDescriptorTree["是否包含待可发送任务"].Access(false, _GetAvailableActionsDescription);
                        continue;
                    }

                    if (availableActions.Count() == 0)
                    {
                        _methodDescriptorTree["是否包含待可发送任务"].Access(false, _GetAvailableActionsDescription);
                        continue;
                    }
                    _methodDescriptorTree["是否包含待可发送任务"].Access(true);

                    if (this.Device.Holder != null)
                    {
                        _methodDescriptorTree["设备是否已被其它对象持有"].Access(false, string.Format("{0} 已被 {1} 持有", this.Device, this.Device.Holder));
                        continue;
                    }
                    _methodDescriptorTree["设备是否已被其它对象持有"].Access(true);

                    this.Device.Hold(this);
                    _methodDescriptorTree["持有设备"].Access(true);
                    try
                    {

                        
                        _logger.Trace1(String.Format("本次准备了{0}个动作准备发送：{1}", availableActions.Count(),String.Join(",", availableActions.Select(x => x.EquipmentTaskId.ToString()))), this);

                        availableActions.AsParallel().ForAll(action =>
                        {
                            try
                            {
                                Boolean negatory = false;
                                foreach (var filter in this.ActionFilters)
                                {
                                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                                    sw.Start();
                                    var filterResult = filter.Filter(this, action);
                                    sw.Stop();
                                    this.Log($"任务{action.Movement.Task}，物理动作{action},任务过滤器{filter.GetType()}过滤结果({filterResult.Defeated + "," + filterResult.Reason}),耗时{sw.ElapsedMilliseconds}ms");
                                    
                                    if (filterResult.Defeated == true)
                                    {
                                        _methodDescriptorTree["是否被过滤器否决"].Access(false, string.Format("{0} 被 {1} 否决了发送操作，原因：{2}。", action, filter, filterResult.Reason));
                                        negatory = true;
                                        this._logger.Debug1(string.Format("{0} 被 {1} 否决了发送操作，原因：{2}。", action, filter, filterResult.Reason), this, action);
                                        break;
                                    }
                                }

                                if (negatory)
                                {
                                    return;
                                }
                                _methodDescriptorTree["是否被过滤器否决"].Access(true);

                                Int32 tries = 0;
                            resend:
                                try
                                {
                                    if (tries > 0)
                                    {
                                        _logger.Warn1(String.Format("由于{0}发送失败，准备第{1}次重试...", action, tries), this, action);
                                    }

                                    Device.SendTask(action);
                                }
                                catch (Exception ex)
                                {
                                    tries++;

                                    if (tries <= 3)
                                    {
                                        _logger.Warn1(ex.ToString(), this, action);

                                        goto resend;
                                    }
                                    else
                                    {
                                        _logger.Error1(ex, this, action);

                                        _logger.Warn1(String.Format("由于{0}连续发送了3次均失败，不再重试。", action), this, action);
                                    }

                                    throw;
                                }

                                _logger.Trace1(string.Format("{0} 发送成功", action), this, action);

                                action.Status = EquipmentActionStatus.Executing;
                                action.Movement.Status = LogicMovementStatus.Executing;
                                action.Movement.Task.Status = TaskStatus.Executing;

                                //此处状态更新失败后亦无影响，原因是如果任务被成功发送。设备将不断发送任务状态信号，任务状态事件的处理程序会处理其关的状态信息
                                bool locked = false;
                                try
                                {
                                    int relockNum = 0;
                                relock:
                                    if (relockNum != 0)
                                    {
                                        _logger.Debug1(string.Format("准备第{0}尝试锁定{1}...", relockNum, action), this, action);
                                    }

                                    locked = action.EnterLock();
                                    if (!locked)
                                    {
                                        if (relockNum < 3)
                                        {
                                            relockNum++;
                                            System.Threading.Thread.Sleep(200);
                                            goto relock;
                                        }
                                        else
                                        {
                                            _logger.Warn1(String.Format("{0}在发送成功后，准备更新状态为“{1}”时锁定失败，但仍将尝试直接更新该状态。", action, EquipmentActionStatus.Executing.GetDescription()), this, action);
                                        }
                                    }
                                    
                                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.Suppress))
                                    {
                                        using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                                        {
                                            var act = unitOfWork.session.Get<EquipmentAction>(action.Id);
                                            act.Status = EquipmentActionStatus.Executing;
                                            act.Movement.Status = LogicMovementStatus.Executing;
                                            act.Movement.Task.Status = TaskStatus.Executing;
                                            if (act.SentAt == null)
                                            {
                                                act.SentAt = DateTime.Now;
                                            }

                                            unitOfWork.session.Flush();
                                            unitOfWork.Commit();
                                        }

                                        ts.Complete();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    this._logger.Error1(ex, this, action);
                                    _methodDescriptorTree["结束"].Access(false, ex.ToString());
                                }
                                finally
                                {
                                    if (locked)
                                    {
                                        action.ExitLock();
                                    }
                                }


                                EventBus.Instance.Publish(new EquipmentActionStatusChangedEvent(action.Movement.Task.Id, action.Movement.Id, action.Id, action.Status));
                                EventBus.Instance.Publish(new LogicMovementStatusChangedEvent(action.Movement.Id, action.Movement.Task.Id, action.Movement.Status));
                                EventBus.Instance.Publish(new TaskStatusChangedEvent(action.Movement.Task.Id, action.Movement.Task.TaskCode, action.Movement.Task.Status, action.Movement.Task.BizType, action.Movement.Task.Source, action.Movement.Task.TaskType));

                            }
                            catch (Exception ex)
                            {
                                this._logger.Error1(ex, this, action);

                                _methodDescriptorTree["结束"].Access(false, ex.ToString());
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        this._logger.Error1(ex, this);

                        _methodDescriptorTree["结束"].Access(false, ex.ToString());
                    }
                    finally
                    {
                        this.Device.Unhold(this);
                    }

                }
                catch (Exception ex)
                {
                    _methodDescriptorTree["结束"].Access(false, ex.ToString());
                    this._logger.Error1(ex, this);
                }
            }

            this._logger.Trace1(string.Format("{0} 轮询线程已停止", this), this);
            this.IsRunning = false;
        }

    }
}
