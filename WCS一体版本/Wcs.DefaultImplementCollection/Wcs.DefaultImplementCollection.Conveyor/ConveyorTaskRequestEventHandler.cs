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
    /// 输送线任务请求事件处理程序
    /// </summary>
    public sealed class ConveyorTaskRequestEventHandler:ITaskEventHandler<TaskRequestEventArgs>
    {
        static Random _rnd = new Random();
        Logger _logger { get; set; }
        public ConveyorTaskRequestEventHandler()
        {
            this._logger = LogManager.GetCurrentClassLogger();
        }

        public void Handle(TaskableDevice device, NHibernate.ISession session, TaskRequestEventArgs args)
        {
            try
            {
                if (!(device is ConveyorDevice))
                {
                    String msg = string.Format("收到了 {0} 发送来的 {1} 任务请求信号,其非 {2} 设备类型，被 {3} 忽略。", device, args, typeof(ConveyorDevice), this);
                    this._logger.Debug1(msg, this, args);
                    args.Handled = true;
                    return;
                }

                var holdSignals = ((ConveyorDevice)device).ReadStatus<HoldSignalNetTransferObject>();
                if (holdSignals == null || holdSignals.Length == 0)
                {
                    String msg = string.Format("收到了 {0} 发送来的 {1} 任务请求信号,但在获取设备占位区数据时失败，操作已中止。", device, args);
                    this._logger.Warn1(msg, this, args);
                    return;
                }

                var holdSignalTransferObject = holdSignals.FirstOrDefault(x => x.PosNo == Convert.ToInt16(args.Location.DeviceCode));
                if (holdSignalTransferObject == null)
                {
                    String msg = string.Format("收到了 {0} 发送来的 {1} 任务请求信号,但在获取设备占位区未发现它，本信号被忽略。", device, args);
                    this._logger.Warn1(msg, this, args);
                    args.Handled = true;
                    return;
                }
                var index = Convert.ToUInt16(holdSignals.ToList().IndexOf(holdSignalTransferObject)+1);
                switch (holdSignalTransferObject.HandShake)
                {
                    case HoldSignalNetTransferObjectHandShake.Empty:
                        String msg = string.Format("收到了 {0} 发送来的 {1} 任务请求信号,但设备占位区获取到的 {2} 握手为 {3}，不再执行清除操作。", device, args,holdSignalTransferObject, holdSignalTransferObject.HandShake.GetDescription());
                        this._logger.Debug1(msg, this, args);
                        args.Handled = true;
                        break;
                    case HoldSignalNetTransferObjectHandShake.New:
                        ApplyDeleteHoldSignalCommand deleteTaskCommand = new ApplyDeleteHoldSignalCommand(holdSignalTransferObject.PosNo, holdSignalTransferObject.IO_Data, index, Convert.ToUInt16(_rnd.Next(1, Int16.MaxValue)));
                        ((ConveyorDevice)device).Write(deleteTaskCommand, (taskableDevice, cmd) =>
                        {
                            ConveyorDevice conveyorDevice = (ConveyorDevice)taskableDevice;
                            var status = conveyorDevice.ReadStatus<TaskNetTransferObject>();
                            return status != null
                                && status.Length > 0
                                && status[cmd.DB1023_Index - 1].HandShake == TaskHandShakes.Empty;
                        });

                        this._logger.Debug1(string.Format("收到了 {0} 发送来的 {1} 任务请求信号,成功向其发送了一个 {2} 指令。", device, args, deleteTaskCommand), this, deleteTaskCommand);
                        args.Handled = true;
                        break;
                    default:
                        this._logger.Warn1(string.Format("收到了 {0} 发送来的 {1} 任务请求信号,但设备占位区获取到的 {2} 状态为 {3}，预期的状态应为 {4}，操作已中止。", device, args, holdSignalTransferObject, holdSignalTransferObject.HandShake.GetDescription(), HoldSignalNetTransferObjectHandShake.New.GetDescription()), this, args);
                        break;
                }

            }
            catch (Exception ex)
            {
                String msg = string.Format("收到了 {0} 发送来的 {1} 任务请求信号,但 {2} 在处理时发生异常，操作已中止", device, args,this);
                this._logger.Error1(new Exception(msg, ex), this, args);
                args.Handled = false;
            }
        }
    }
}
