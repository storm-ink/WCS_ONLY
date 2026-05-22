using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;
using Wcs.Framework.Cfg;
using NHibernate.Linq;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 输送线任务的二次握手信号处理绑定启动程序<br />
    /// 用于给输送线设备绑定 <seealso cref="E:Wcs.DefaultImpls.ConveyorDevice.TaskConfirm"/> 事件的处理程序
    /// </summary>
    public sealed class ConveyorConfirmSignalBindApplicationStartup:ApplicationStartup
    {
        static Random _rnd = new Random();
        public ConveyorConfirmSignalBindApplicationStartup(Wcs.Framework.Cfg.StartupElement element)
            : base(element) 
        { 

        }
        public override void Run(IWcsApplication application)
        {
            var devices = WcsConfiguration
              .Instance
              .DeviceCollection
              .ParticularDeviceCollection.SelectMany(x => x.DeviceElements)
              .Where(x=>x.Device is ConveyorDevice)
              .Select(x => (ConveyorDevice)x.Device);

            foreach (var conveyorDevice in devices)
            {
                conveyorDevice.TaskConfirm += conveyorDevice_TaskConfirm;
            }
        }

        void conveyorDevice_TaskConfirm(ConveyorDevice device, TaskConfirmEventArgs args)
        {
            UInt16 atPlcDBIndex, routeId;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                var action = unitOfWork.session
                    .Query<ConveyorTransferAction>()
                    .FirstOrDefault(x => x.EquipmentTaskId == args.TaskBlock.AssignmentID);

                if (action == null)
                {
                    String msg = string.Format("收到了 {0} 发送来的 {1} 任务二次握手信号,但未找到对应的 {2} 对象，忽略本信号", device, args.TaskBlock.AssignmentID, typeof(ConveyorTransferAction));
                    this._logger.Warn1(msg, this, args,null,Convert.ToInt32(args.TaskBlock.AssignmentID));
                    args.Handled = true;
                    unitOfWork.Commit();
                    return;
                }
                else
                {
                    atPlcDBIndex = Convert.ToUInt16(action.AtPlcDBIndex.Value);
                    routeId = Convert.ToUInt16(action.RouteId);
                    unitOfWork.Commit();
                }
            }
            var tasks = ((ConveyorDevice)device).ReadStatus<TaskNetTransferObject>();
            if (tasks == null || tasks.Length == 0)
            {
                String msg = string.Format("收到了 {0} 发送来的 {1} 任务二次握手信号,但在获取设备任务区数据时失败，操作已中止。", device, args.TaskBlock.AssignmentID);
                this._logger.Warn1(msg, this, args, null, Convert.ToInt32(args.TaskBlock.AssignmentID));
                return;
            }

            if (tasks.Length <= atPlcDBIndex)
            {
                String msg = string.Format("收到了 {0} 发送来的 {1} 任务二次握手信号,但其指示的位置 {2} 不在设备任务区范围内（0 至 {3}），操作已中止。", device, args.TaskBlock.AssignmentID, atPlcDBIndex, tasks.Length);
                this._logger.Warn1(msg, this, args, null, Convert.ToInt32(args.TaskBlock.AssignmentID));
                return;
            }

            var route = RouteHelper.RouteHeads.SingleOrDefault(x => x.Device == device.Name && x.Id == routeId);
            //var route = WcsConfiguration.Instance.RouteCollection.Routes.Single(x => x.Id == routeId);
            var routeNo = route.No;
            var taskNetTransferObject = tasks[atPlcDBIndex - 1];
            if (taskNetTransferObject.AssignmentID != args.TaskBlock.AssignmentID || taskNetTransferObject.RotingNo != routeNo)
            {
                String msg = string.Format("收到了 {0} 发送来的 {1} 任务二次握手信号,但其指示的位置 {2} 存放的任务对象 {3} 和本地数据不一致（任务号 {4}，路径号 no={5},id={6}），操作已中止。", device, args.TaskBlock.AssignmentID, atPlcDBIndex, taskNetTransferObject, args.TaskBlock.AssignmentID, routeNo, routeId);
                this._logger.Warn1(msg, this, args, null, Convert.ToInt32(args.TaskBlock.AssignmentID));
                return;
            }

            switch (taskNetTransferObject.HandShake)
            {
                case TaskHandShakes.Readed:
                    ConfirmTaskCommand confirmTaskCommand = new ConfirmTaskCommand(args.TaskBlock.AssignmentID, Convert.ToUInt16(routeNo), taskNetTransferObject.StartMotorNo, taskNetTransferObject.DestinationNo, atPlcDBIndex, Convert.ToUInt16(_rnd.Next(1, Int16.MaxValue)));
                    confirmTaskCommand.IO_Data = taskNetTransferObject.IO_Data;
                    ((ConveyorDevice)device).Write(confirmTaskCommand, (taskableDevice, cmd) =>
                    {
                        ConveyorDevice conveyorDevice = (ConveyorDevice)taskableDevice;
                        var status = conveyorDevice.ReadStatus<TaskNetTransferObject>();
                        return status != null
                            && status.Length > 0
                            && status[cmd.DB1000_Index - 1].HandShake == TaskHandShakes.SecondConfirm;
                    });

                    this._logger.Info1(string.Format("收到了 {0} 发送来的 {1} 任务二次握手信号,成功向其发送了一个 {2} 指令。", device, args.TaskBlock.AssignmentID, confirmTaskCommand), this, confirmTaskCommand, null, Convert.ToInt32(args.TaskBlock.AssignmentID));
                    args.Handled = true;
                    break;
                case TaskHandShakes.SecondConfirm:
                    this._logger.Trace1(string.Format("收到了 {0} 发送来的 {1} 任务二次握手信号,但其指示的位置 {2} 存放的任务对象 {3} 握手为 {4}，不再发送二次握手指令。", device, args.TaskBlock.AssignmentID, atPlcDBIndex, taskNetTransferObject, taskNetTransferObject.HandShake.GetDescription()), this, args, null, Convert.ToInt32(args.TaskBlock.AssignmentID));
                    args.Handled = true;
                    break;
                default:
                    this._logger.Warn1(string.Format("收到了 {0} 发送来的 {1} 任务二次握手信号,但其指示的位置 {2} 存放的任务对象 {3} 握手为 {4}，预期的状态应为 {5}，操作已中止。", device, args.TaskBlock.AssignmentID, atPlcDBIndex, taskNetTransferObject, taskNetTransferObject.HandShake.GetDescription(), TaskHandShakes.Readed), this, args, null, Convert.ToInt32(args.TaskBlock.AssignmentID));
                    break;
            }
        }
    }
}
