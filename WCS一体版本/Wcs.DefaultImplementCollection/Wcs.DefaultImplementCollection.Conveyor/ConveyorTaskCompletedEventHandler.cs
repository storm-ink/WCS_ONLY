using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;
using NHibernate.Linq;
using NLog;
using Wcs.Framework.Cfg;
using NHibernate;

namespace Wcs.DefaultImplementCollection.Conveyor
{
#warning 应该将其放到配置顶端
    /// <summary>
    /// 输送线任务完成处理程序
    /// </summary>
    public sealed class ConveyorTaskCompletedEventHandler : ITaskEventHandler<TaskCompletedEventArgs>
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();
        static Random _rnd = new Random();
        public ConveyorTaskCompletedEventHandler()
        {
        }

        public void Handle(TaskableDevice device, ISession session, TaskCompletedEventArgs args)
        {
            try
            {
                if (!(device is ConveyorDevice))
                {
                    String msg = string.Format("收到了 {0} 发送来的 {1} 任务完成信号,其非 {2} 设备类型，被 {3} 忽略。", device, args.EquipmentTaskId, typeof(ConveyorDevice), this);
                    _logger.Debug1(msg, this, args, null, args.EquipmentTaskId);
                    args.Handled = true;
                    return;
                }

                _logger.Trace1(string.Format("{0} 开始处理设备任务 {1} 的完成事件...", this, args.EquipmentTaskId), this, args, null, args.EquipmentTaskId);

                int atPlcDBIndex, routeId;
                var action = session.Query<ConveyorTransferAction>().FirstOrDefault(x => x.EquipmentTaskId == args.EquipmentTaskId);
                if (action == null)
                {
                    String msg = string.Format("①收到了 {0} 发送来的 {1} 任务完成信号,但未找到对应的 {2} 对象，忽略本信号", device, args.EquipmentTaskId, typeof(ConveyorTransferAction));
                    _logger.Warn1(msg, this, args, null, args.EquipmentTaskId);
                    args.Handled = true;

                    var _tasks = ((ConveyorDevice)device).TaskBlocks;
                    if (_tasks == null || _tasks.Length == 0)
                    {
                        msg = string.Format("①收到了 {0} 发送来的 {1} 任务完成信号,但在获取设备任务区数据时失败，操作已中止。", device, args.EquipmentTaskId);
                        _logger.Warn1(msg, this, args, null, args.EquipmentTaskId);
                        return;
                    }

                    var _taskNetTransferObject = _tasks.FirstOrDefault(x => x.TaskNo == args.EquipmentTaskId);
                    if (_taskNetTransferObject == null)
                        return;

                    switch (_taskNetTransferObject.TaskState)
                    {
                        case TaskBlockTaskStatus.Empty:
                            break;
                        case TaskBlockTaskStatus.Finished:
                            if (_taskNetTransferObject.HandShake == TaskHandShakes.Empty)
                            {
                                msg = string.Format("①收到了 {0} 发送来的 {1} 任务完成信号,但其指示的位置 {2} 存放的任务对象 {3} 握手为 {4}，不再执行清除操作。", device, args.EquipmentTaskId, _taskNetTransferObject.AtPacketIndex, _taskNetTransferObject, _taskNetTransferObject.HandShake.GetDescription());
                                _logger.Debug1(msg, this, args, null, args.EquipmentTaskId);
                                break;
                            }

                            TaskCommand clearTaskCommand = new TaskCommand(TaskHandShakes.ApplyForDelete, Convert.ToUInt32(args.EquipmentTaskId), "", new UInt16[10], _taskNetTransferObject.RotingNo, _taskNetTransferObject.From, _taskNetTransferObject.To, Convert.ToUInt16(_taskNetTransferObject.AtPacketIndex));
                            try
                            {
                                ((ConveyorDevice)device).Write(clearTaskCommand, (taskableDevice, cmd) =>
                                {
                                    ConveyorDevice conveyorDevice = (ConveyorDevice)taskableDevice;
                                    return conveyorDevice.TaskBlocks != null
                                        && conveyorDevice.TaskBlocks.Length > 0
                                        && conveyorDevice.TaskBlocks[cmd.DBIndex - 1].HandShake == TaskHandShakes.Empty;
                                });

                                _logger.Debug1(string.Format("①收到了 {0} 发送来的 {1} 任务完成信号,成功向其发送了一个 {2} 指令。", device, args.EquipmentTaskId, clearTaskCommand), this, clearTaskCommand, null, args.EquipmentTaskId);

                            }
                            catch (Exception ex)
                            {
                                _logger.Warn1(string.Format("①收到了 {0} 发送来的 {1} 任务完成信号,但在成功向其发送了 {2} 指令时发生异常（后续可能引起该任务残余在电控程序当中,请及时检查并清理）。\n{3}",
                                    device,
                                    args.EquipmentTaskId,
                                    clearTaskCommand,
                                    ex), this, clearTaskCommand, null, args.EquipmentTaskId);
                            }

                            args.Handled = true;
                            break;
                        default:
                            _logger.Warn1(string.Format("收到了 {0} 发送来的 {1} 任务完成信号,但其指示的位置 {2} 存放的任务对象 {3} 状态为 {4}，预期的状态应为 {5}，操作已中止。", device, args.EquipmentTaskId, _taskNetTransferObject.AtPacketIndex, _taskNetTransferObject, _taskNetTransferObject.TaskState.GetDescription(), TaskBlockTaskStatus.Finished.GetDescription()), this, args, null, args.EquipmentTaskId);
                            break;
                    }


                    return;
                }
                else
                {
                    atPlcDBIndex = action.AtPlcDBIndex.Value;
                    routeId = action.RouteId;
                }

                var tasks = ((ConveyorDevice)device).TaskBlocks;
                if (tasks == null || tasks.Length == 0)
                {
                    String msg = string.Format("收到了 {0} 发送来的 {1} 任务完成信号,但在获取设备任务区数据时失败，操作已中止。", device, args.EquipmentTaskId);
                    _logger.Warn1(msg, this, args, null, args.EquipmentTaskId);
                    return;
                }

                if (tasks.Length < atPlcDBIndex)
                {
                    String msg = string.Format("收到了 {0} 发送来的 {1} 任务完成信号,但其指示的位置 {2} 不在设备任务区范围内（0 至 {3}），操作已中止。", device, args.EquipmentTaskId, atPlcDBIndex, tasks.Length);
                    _logger.Warn1(msg, this, args, null, args.EquipmentTaskId);
                    return;
                }

                var route = RouteHelper.RouteHeads.First(x => x.Id == routeId);
                var routeNo = route.No;
                var taskNetTransferObject = tasks[atPlcDBIndex - 1];

                //if (taskNetTransferObject.AssignmentID != args.EquipmentTaskId || taskNetTransferObject.RotingNo != routeNo)
                //{
                //    String msg = string.Format("收到了 {0} 发送来的 {1} 任务完成信号,但其指示的位置 {2} 存放的任务对象 {3} 和本地数据不一致（任务号 {4}，路径号 no={5},id={6}），操作已中止。", device, args.EquipmentTaskId, atPlcDBIndex, taskNetTransferObject, args.EquipmentTaskId, routeNo, routeId);
                //    _logger.Warn1(msg, this, args, null, args.EquipmentTaskId);
                //    //return;
                //}

                if (taskNetTransferObject.TaskNo != args.EquipmentTaskId)
                {
                    String msg = string.Format("收到了 {0} 发送来的 {1} 任务完成信号,但其指示的位置 {2} 存放的任务对象 {3} 和本地数据不一致（任务号 {4}，路径号 no={5},id={6}），操作已中止。", device, args.EquipmentTaskId, atPlcDBIndex, taskNetTransferObject, args.EquipmentTaskId, routeNo, routeId);
                    _logger.Warn1(msg, this, args, null, args.EquipmentTaskId);
                    return;
                }

                if (taskNetTransferObject.RotingNo != routeNo)
                {
                    var newRoute = WcsConfiguration.Instance.RouteCollection.Routes.Single(x => x.No == taskNetTransferObject.RotingNo);
                    action.RouteId = newRoute.Id;
                    action.Movement.RouteId = newRoute.Id;
                    _logger.Info1(String.Format("{0}和{1}路径号已由电控更新为{2}(#{3})（原路径为号为{4}(#{5})）", action, action.Movement, taskNetTransferObject.RotingNo, newRoute.Id, routeNo, routeId), this, action);
                }

                if (taskNetTransferObject.To != Convert.ToInt32(action.Movement.EndLocation.DeviceCode))
                {
                    var oldEndLocation = action.Movement.EndLocation.DeviceCode;

                    var newEndLocation = ((ConveyorDevice)device).Locations.Single(x => x.DeviceCode == taskNetTransferObject.To.ToString());
                    action.EndLocation = LocationConverter.ToLocationInfo(newEndLocation);
                    action.Movement.EndLocation = LocationConverter.ToLocationInfo(newEndLocation);

                    _logger.Info1(String.Format("{0}和{1}结束位置已由电控更新为{2}（原结束位置为{3}）", action, action.Movement, taskNetTransferObject.To, oldEndLocation), this, action);

                    if (action.Movement.Task.EndLocation.DeviceCode == oldEndLocation)
                    {
                        action.Movement.Task.EndLocation = LocationConverter.ToLocationInfo(newEndLocation);
                        _logger.Info1(String.Format("{0}结束由于物理动作的结束位置发生了改变,已更新为{1}（原结束位置为{2}）", action.Movement.Task, taskNetTransferObject.To, oldEndLocation), this, action);
                    }
                }

                switch (taskNetTransferObject.TaskState)
                {
                    case TaskBlockTaskStatus.Empty:
                        _logger.Debug1(string.Format("收到了 {0} 发送来的 {1} 任务完成信号,但其指示的位置 {2} 存放的任务对象 {3} 状态为 {4}，不再执行清除操作。", device, args.EquipmentTaskId, atPlcDBIndex, taskNetTransferObject, taskNetTransferObject.TaskState.GetDescription()), this, args, null, args.EquipmentTaskId);
                        args.Handled = true;
                        break;
                    case TaskBlockTaskStatus.Finished:
                        if (taskNetTransferObject.HandShake == TaskHandShakes.Empty)
                        {
                            String msg = string.Format("收到了 {0} 发送来的 {1} 任务完成信号,但其指示的位置 {2} 存放的任务对象 {3} 握手为 {4}，不再执行清除操作。", device, args.EquipmentTaskId, atPlcDBIndex, taskNetTransferObject, taskNetTransferObject.HandShake.GetDescription());
                            _logger.Debug1(msg, this, args, null, args.EquipmentTaskId);
                            args.Handled = true;
                            break;
                        }

                        TaskCommand clearTaskCommand = new TaskCommand(TaskHandShakes.ApplyForDelete, Convert.ToUInt32(args.EquipmentTaskId), "", new UInt16[10], Convert.ToUInt16(routeNo), taskNetTransferObject.From, taskNetTransferObject.To, Convert.ToUInt16(atPlcDBIndex));
                        try
                        {
                            ((ConveyorDevice)device).Write(clearTaskCommand, (taskableDevice, cmd) =>
                            {
                                ConveyorDevice conveyorDevice = (ConveyorDevice)taskableDevice;
                                return conveyorDevice.TaskBlocks != null
                                    && conveyorDevice.TaskBlocks.Length > 0
                                    && conveyorDevice.TaskBlocks[cmd.DBIndex - 1].HandShake == TaskHandShakes.Empty;
                            });

                            _logger.Debug1(string.Format("收到了 {0} 发送来的 {1} 任务完成信号,成功向其发送了一个 {2} 指令。", device, args.EquipmentTaskId, clearTaskCommand), this, clearTaskCommand, null, args.EquipmentTaskId);

                        }
                        catch (Exception ex)
                        {
                            _logger.Warn1(string.Format("收到了 {0} 发送来的 {1} 任务完成信号,但在成功向其发送了 {2} 指令时发生异常（后续可能引起该任务残余在电控程序当中,请及时检查并清理）。\n{3}",
                                device,
                                args.EquipmentTaskId,
                                clearTaskCommand,
                                ex), this, clearTaskCommand, null, args.EquipmentTaskId);
                        }

                        args.Handled = true;
                        break;
                    default:
                        _logger.Warn1(string.Format("收到了 {0} 发送来的 {1} 任务完成信号,但其指示的位置 {2} 存放的任务对象 {3} 状态为 {4}，预期的状态应为 {5}，操作已中止。", device, args.EquipmentTaskId, atPlcDBIndex, taskNetTransferObject, taskNetTransferObject.TaskState.GetDescription(), TaskBlockTaskStatus.Finished.GetDescription()), this, args, null, args.EquipmentTaskId);
                        break;
                }

            }
            catch (Exception ex)
            {
                String msg = string.Format("收到了 {0} 发送来的 {1} 任务完成信号,但 {2} 在处理时发生异常，操作已中止", device, args.EquipmentTaskId, this);
                _logger.Error1(new Exception(msg, ex), this, args, null, args.EquipmentTaskId);
                args.Handled = false;
                throw;
            }
        }
    }
}
