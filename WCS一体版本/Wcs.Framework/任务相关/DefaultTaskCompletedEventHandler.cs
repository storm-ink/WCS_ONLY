using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Linq;
using NLog;
using Wcs.Framework.Events;
using Wcs.Framework.EventBus;

namespace Wcs.Framework
{
    public class DefaultTaskCompletedEventHandler : ITaskEventHandler<TaskCompletedEventArgs>
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();
        public DefaultTaskCompletedEventHandler()
        {

        }

        public void Handle(TaskableDevice device, NHibernate.ISession session, TaskCompletedEventArgs args)
        {
            try
            {
                _logger.Trace1(string.Format("{0} 开始处理设备任务 {1} 的完成事件...", this, args.EquipmentTaskId), this, args, null, args.EquipmentTaskId);
                List<IEvent> events = new List<IEvent>();

                _logger.Trace1(string.Format("获取任务号为 {0} 的物理动作", args.EquipmentTaskId), this, args, null, args.EquipmentTaskId);
                var action = session.Query<EquipmentAction>().SingleOrDefault(x => x.EquipmentTaskId == args.EquipmentTaskId);
                if (action == null)
                {
                    String msg = string.Format("收到了 {0} 发送来的 {1} 任务完成信号,但未找到对应的物理动作对象，忽略本信号", device, args.EquipmentTaskId);
                    _logger.Warn1(msg, this, args, null, args.EquipmentTaskId);
                    args.Handled = true;
                    return;
                }

                _logger.Trace1(string.Format("找到 {0}", action), this, action);

                //session.Lock(action, NHibernate.LockMode.Upgrade);

                if (action.Status == EquipmentActionStatus.Completed)
                {
                    String msg = string.Format("收到了 {0} 发送来的 {1} 任务完成信号,但发现对应的物理动作已处于完成状态。", device, args.EquipmentTaskId);
                    _logger.Warn(msg, this, action);
                }
                else
                {

                    action.FinishedAt = DateTime.Now;
                    action.Status = EquipmentActionStatus.Completed;
                    if (action.SentAt == null)
                        action.SentAt = action.FinishedAt;

                    _logger.Trace1(string.Format("更新 {0}，FinishedAt:{1}，Status:{2}", action, action.FinishedAt, action.Status.GetDescription()), this, action);

                    if (action.Movement != null)
                    {
                        action.Movement.FinishedAt = action.FinishedAt;
                        action.Movement.Status = LogicMovementStatus.Completed;

                        _logger.Trace1(string.Format("更新 {0}，FinishedAt:{1}，Status:{2}", action.Movement, action.Movement.FinishedAt, action.Movement.Status.GetDescription()), this, action.Movement);

                        if (!action.Movement.Task.CurrentLocation.Equals(action.Movement.EndLocation))
                        {
                            _logger.Trace1(string.Format("{0} 的当前位置和 {1} 的结束位置不一致，更新 {1} 当前位置为 {2}", action.Movement.Task, action.Movement, action.Movement.EndLocation), this, action.Movement.Task);

                            action.Movement.Task.CurrentLocation = action.Movement.EndLocation;
                            events.Add(new TaskCurrentLocationChangedEvent(action.Movement.Task.Id, action.Movement.Task.TaskCode, action.Movement.Task.CurrentLocation));
                        }

                        if (action.Movement.Task != null && action.Movement.Task is UndefinedEndLocationTask)
                        {
                            UndefinedEndLocationTask uet = (UndefinedEndLocationTask)action.Movement.Task;
                            //如果当前位置当前完成的物理动作（当前位置）已是可选结束位置的其中的一个，则任务报完成，并将当前位置设为结束点。
                            if (uet.AvailableEndLocations.Any(loc => loc.Equals(action.Movement.EndLocation)))
                            {
                                var oldEndLocation = action.Movement.Task.EndLocation;
                                uet.EndLocation = action.Movement.EndLocation;

                                _logger.Info1(string.Format("{0} 是终点未明的任务，并且 {1} 的结束点（{2}）是其可选位置中的一个，修正任务的结束点为 {2}（原为{3}）",
                                    action.Movement.Task,
                                    action.Movement,
                                    action.Movement.EndLocation,
                                    oldEndLocation
                                    ), this, action);
                            }
                            else
                            {

                                _logger.Trace1(string.Format("{0} 是终点未明的任务，当前结束点是 {1}，准备尝试重新分配结束位置...", action.Movement.Task, action.Movement.Task.EndLocation), this, action);

                                var oldEndLocation = action.Movement.Task.EndLocation;

                                ((UndefinedEndLocationTask)action.Movement.Task).AssignNewEndLocation();

                                _logger.Info1(string.Format("为 {0} 原结束位置为 {1}，重新分配的结束位置为 {2}", action.Movement.Task, oldEndLocation, action.Movement.Task.EndLocation), this, action);
                            }
                        }


                        events.Add(new EquipmentActionStatusChangedEvent(action.Movement.Task.Id, action.Movement.Id, action.Id, action.Status));
                        events.Add(new LogicMovementStatusChangedEvent(action.Movement.Id, action.Movement.Task.Id, action.Movement.Status));


                        if (LocationConverter.ToLocation(action.Movement.Task.CurrentLocation).Equals(LocationConverter.ToLocation(action.Movement.Task.EndLocation))
                            && (action.Movement.Task.Status != TaskStatus.Cancelled || action.Movement.Task.Status != TaskStatus.Completed))
                        {

                            action.Movement.Task.Status = TaskStatus.Completed;
                            action.Movement.Task.FinishedAt = action.FinishedAt;

                            var total = action.Movement.Task.FinishedAt.Value.Subtract(action.Movement.Task.CreatedAt);
                            if (action.Movement.Task.AdditionalInfo.ContainsKey("TTS"))
                                action.Movement.Task.AdditionalInfo["TTS"] = total.ToString();
                            else
                                action.Movement.Task.AdditionalInfo.Add("TTS", total.ToString());

                            var rts = TaskHelper.CalTaskRunningTimeSpan(action.Movement.Task);
                            if (rts == null)
                                rts = new TimeSpan();
                            if (action.Movement.Task.AdditionalInfo.ContainsKey("RTS"))
                                action.Movement.Task.AdditionalInfo["RTS"] = rts.ToString();
                            else
                                action.Movement.Task.AdditionalInfo.Add("RTS", rts.ToString());

                            var wts = total - rts;
                            if (action.Movement.Task.AdditionalInfo.ContainsKey("WTS"))
                                action.Movement.Task.AdditionalInfo["WTS"] = wts.ToString();
                            else
                                action.Movement.Task.AdditionalInfo.Add("WTS", wts.ToString());

                            _logger.Trace1(string.Format("{0} 的当前位置结束位置一致，已运行至终点，更新 {0}，FinishedAt:{1}，Status:{2}", action.Movement.Task, action.Movement.Task.FinishedAt, action.Movement.Task.Status.GetDescription()), this, action.Movement.Task);

                            events.Add(new TaskStatusChangedEvent(action.Movement.Task.Id, action.Movement.Task.TaskCode, action.Movement.Task.Status, action.Movement.Task.BizType, action.Movement.Task.Source, action.Movement.Task.TaskType));
                            events.Add(new TaskFinishedEvent(action.Movement.Task.Id,
                                action.Movement.Task.TaskCode,
                                action.Movement.Task.Status,
                                action.Movement.Task.BizType,
                                action.Movement.Task.Source,
                                action.Movement.Task.TaskType,
                                action.Movement.Task.FinishedAt.Value));
                        }
                        else if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("TaskDispatcherVersion","") == "V1")
                        {
                            if (!DefaultApplicationStartup.preDic.ContainsKey(action.Movement.Task.TaskCode))
                            {
                                args.Handled = false;
                                Console.WriteLine("①①①①①①①①①①①①①①①①①①①①①①等待获取下一条任务");
                                return;
                            }
                            var preTask = DefaultApplicationStartup.preDic[action.Movement.Task.TaskCode];
                            if (preTask.PreTask.Movements.Count() <= action.Movement.Task.Movements.Count())
                            {
                                DefaultApplicationStartup.preDic.Remove(action.Movement.Task.TaskCode);
                                args.Handled = false;
                                Console.WriteLine("②②②②②②②②②②②②②②②②②②②②②②等待获取下一条任务");
                                return;
                            }
                            var movement = preTask.PreLogicMovement;
                            var preEnd = LocationConverter.ToLocation(movement.EndLocation);
                            var end = LocationConverter.ToLocation(action.Movement.EndLocation);
                            if (preEnd.UnifiedCode == end.UnifiedCode)
                            {
                                DefaultApplicationStartup.preDic.Remove(action.Movement.Task.TaskCode);
                                args.Handled = false;
                                Console.WriteLine("③③③③③③③③③③③③③③③③③③③③③③等待获取下一条任务");
                                return;
                            }
                            var _movement = action.Movement.Task.GetNextMovement(out bool isNewMovement, out string predictRoutes, movement);

                            var lastRouteIds = action.Movement.Task.TaskPredictRoutes != null ? String.Join(",", action.Movement.Task.TaskPredictRoutes.OrderBy(x => x.Key).Select(x => x.Value).ToArray()) : "";
                            if (!lastRouteIds.Contains(preTask.PredictRoutes))
                            {
                                action.Movement.Task.TaskPredictRoutes.Clear();
                                var routeIds = preTask.PredictRoutes.Split(',').Select(x => Convert.ToInt32(x)).ToArray();
                                for (int i = 0; i < routeIds.Length; i++)
                                {
                                    action.Movement.Task.TaskPredictRoutes.Add(i, routeIds[i]);
                                }
                            }
                            //action.Movement.Task.TaskPredictRoutes = preTask.PreTask.TaskPredictRoutes;

                            events.Add(new Wcs.Framework.Events.LogicMovementAddedEvent(_movement));
                        }
                    }
                }

#warning 此处会有问题，action和引发任务的设备有可能不一致！应使用action.DeviceName来转换为动作所属设备

                var owner = DeviceConverter.ToDevice<TaskableDevice>(action.DeviceName);
                _logger.Trace1(string.Format("准备从 {0} 中移除 {1}...", owner.EquipmentActionScheduler, action), this, action);
                owner.EquipmentActionScheduler.Remove(action);
                _logger.Trace1("移除成功.", this, action);

                //_logger.Trace1(string.Format("准备从 {0} 中移除 {1}...", device.EquipmentActionScheduler, action), this, action);
                //device.EquipmentActionScheduler.Remove(action);
                //_logger.Trace1("移除成功.", this, action);


                args.Handled = true;


                args.AddLazyEvents(events);

                _logger.Trace1(string.Format("{0} 已完成对 {1} 的完成事件的处理过程.", this, args.EquipmentTaskId), this, args, null, args.EquipmentTaskId);
            }
            catch (Exception ex)
            {
                String msg = string.Format("收到了 {0} 发送来的 {1} 任务完成信号,但 {2} 在处理时发生异常，操作已中止", device, args.EquipmentTaskId, this);
                _logger.Error1(new Exception(msg, ex), this, args, null, args.EquipmentTaskId);

                args.Handled = false;
            }
        }
    }
}
