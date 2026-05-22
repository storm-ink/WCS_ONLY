using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;
using NHibernate.Linq;
using NLog;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Conveyor
{
#warning 应该将其放到配置尾部
    /// <summary>
    /// 输送线任务报警处理程序
    /// </summary>
    public sealed class ConveyorTaskErrorEventHandler:ITaskEventHandler<TaskErrorEventArgs>
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();
        static Random _rnd = new Random();
        public ConveyorTaskErrorEventHandler()
        {
            
        }

        public void Handle(TaskableDevice device, NHibernate.ISession session, TaskErrorEventArgs args)
        {
            try
            {
                if (!(device is ConveyorDevice))
                {
                    String msg = string.Format("收到了 {0} 发送来的 {1} 任务报警信号,其非 {2} 设备类型，被 {3} 忽略。", device, args.EquipmentTaskId, typeof(ConveyorDevice), this, null, args.EquipmentTaskId);
                    _logger.Debug1(msg, this, args);
                    args.Handled = true;
                    return;
                }

                int atPlcDBIndex, routeId;
                var action = session.Query<ConveyorTransferAction>().FirstOrDefault(x => x.EquipmentTaskId == args.EquipmentTaskId);
                if (action == null)
                {
                    String msg = string.Format("收到了 {0} 发送来的 {1} 任务报警信号,但未找到对应的 {2} 对象，忽略本信号", device, args.EquipmentTaskId, typeof(ConveyorTransferAction));
                    _logger.Warn1(msg, this, args, null, args.EquipmentTaskId);
                    args.Handled = true;

                    return;
                }
                else
                {
                    atPlcDBIndex = action.AtPlcDBIndex.Value;
                    routeId = action.RouteId;
                }

                var tasks = ((ConveyorDevice)device).ReadStatus<TaskNetTransferObject>();
                if (tasks == null || tasks.Length == 0)
                {
                    String msg = string.Format("收到了 {0} 发送来的 {1} 任务报警信号,但在获取设备任务区数据时失败，操作已中止。", device, args.EquipmentTaskId);
                    _logger.Warn1(msg, this, args, null, args.EquipmentTaskId);
                    return;
                }

                if (tasks.Length <= atPlcDBIndex)
                {
                    String msg = string.Format("收到了 {0} 发送来的 {1} 任务报警信号,但其指示的位置 {2} 不在设备任务区范围内（0 至 {3}），操作已中止。", device, args.EquipmentTaskId, atPlcDBIndex, tasks.Length);
                    _logger.Warn1(msg, this, args, null, args.EquipmentTaskId);
                    return;
                }

                var route = WcsConfiguration.Instance.RouteCollection.Routes.Single(x => x.Id == routeId);
                var routeNo = route.No;
                var taskNetTransferObject = tasks[atPlcDBIndex - 1];
                if (taskNetTransferObject.AssignmentID != args.EquipmentTaskId || taskNetTransferObject.RotingNo != routeNo)
                {
                    String msg = string.Format("收到了 {0} 发送来的 {1} 任务报警信号,但其指示的位置 {2} 存放的任务对象 {3} 和本地数据不一致（任务号 {4}，路径号 no={5},id={6}），操作已中止。", device, args.EquipmentTaskId, atPlcDBIndex, taskNetTransferObject, args.EquipmentTaskId, routeNo, routeId);
                    _logger.Warn1(msg, this, args, null, args.EquipmentTaskId);
                    return;
                }

                switch (taskNetTransferObject.TaskStatus)
                {
                    case TaskNetTransferObjectStatus.Empty:
                        args.Handled = true;
                        break;
                    case TaskNetTransferObjectStatus.Error:
                        if (taskNetTransferObject.HandShake == TaskHandShakes.Empty)
                        {
                            String msg = string.Format("收到了 {0} 发送来的 {1} 任务报警信号,但其指示的位置 {2} 存放的任务对象 {3} 握手为 {4}，不再执行清除操作。", device, args.EquipmentTaskId, atPlcDBIndex, taskNetTransferObject, taskNetTransferObject.HandShake.GetDescription());
                            _logger.Debug1(msg, this, args, null, args.EquipmentTaskId);
                            args.Handled = true;
                            break;
                        }

                        ApplyDeleteTaskCommand deleteTaskCommand = new ApplyDeleteTaskCommand(Convert.ToUInt32(args.EquipmentTaskId), Convert.ToUInt16(routeNo), taskNetTransferObject.StartMotorNo, taskNetTransferObject.DestinationNo, Convert.ToUInt16(atPlcDBIndex), Convert.ToUInt16(_rnd.Next(1, Int16.MaxValue)));
                        ((ConveyorDevice)device).Write(deleteTaskCommand, (taskableDevice, cmd) =>
                        {
                            ConveyorDevice conveyorDevice = (ConveyorDevice)taskableDevice;
                            var status = conveyorDevice.ReadStatus<TaskNetTransferObject>();
                            return status != null
                                && status.Length > 0
                                && status[cmd.DB1000_Index - 1].HandShake == TaskHandShakes.Empty;
                        });

                        _logger.Debug1(string.Format("收到了 {0} 发送来的 {1} 任务报警信号,成功向其发送了一个 {2} 指令。", device, args.EquipmentTaskId, deleteTaskCommand), this, deleteTaskCommand, null, args.EquipmentTaskId);
                        args.Handled = true;
                        break;
                    default:
                        _logger.Warn1(string.Format("收到了 {0} 发送来的 {1} 任务报警信号,但其指示的位置 {2} 存放的任务对象 {3} 状态为 {4}，预期的状态应为 {5}，操作已中止。", device, args.EquipmentTaskId, atPlcDBIndex, taskNetTransferObject, taskNetTransferObject.TaskStatus.GetDescription(), TaskNetTransferObjectStatus.Error.GetDescription()), this, args, null, args.EquipmentTaskId);
                        break;
                }

            }
            catch (Exception ex)
            {
                String msg = string.Format("收到了 {0} 发送来的 {1} 任务报警信号,但 {2} 在处理时发生异常，操作已中止", device, args.EquipmentTaskId,this);
                _logger.Error1(new Exception(msg, ex), this, args, null, args.EquipmentTaskId);
                args.Handled = false;
            }
        }
    }
}
