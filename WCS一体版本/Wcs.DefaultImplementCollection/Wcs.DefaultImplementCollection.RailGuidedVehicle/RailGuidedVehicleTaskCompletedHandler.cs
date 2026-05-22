using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;
using Wcs;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    /// <summary>
    /// 穿梭车的完成处理程序<br />
    /// 该处理程序的目的是用于清除穿梭车的完成信号。根据于穿梭车的协议规则，当穿梭车的任务完成后，必须由上位机发送HP命令来清除当前所有状态，否则无法执行其它指令。<br />
    /// 该处理程序应该放在完成事件处理程序配置的最后面，原因是如果完成事件前面的处理未成功就发送了清除指令，那么在Wcs关闭后任务的完成信号将永久丢失。
    /// </summary>
    public sealed class RailGuidedVehicleTaskCompletedHandler : ITaskEventHandler<TaskCompletedEventArgs>
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();

        public void Handle(TaskableDevice device, NHibernate.ISession session, TaskCompletedEventArgs args)
        {
            try
            {
                RailGuidedVehicleDevice railGuidedVehicleDevice = device as RailGuidedVehicleDevice;

                if (railGuidedVehicleDevice == null)
                {
                    String msg = string.Format("收到了 {0} 发送来的 {1} 任务完成信号,其非 {2} 设备类型，被 {3} 忽略。", device, args.EquipmentTaskId, typeof(RailGuidedVehicleDevice), this);

                    args.Handled = true;
                    return;
                }

                _logger.Trace1(string.Format("{0} 开始处理设备任务 {1} 的完成事件...", this, args.EquipmentTaskId), this, args);

                //如果状态未同步，直接处理失败等待下一轮处理调用
                if (railGuidedVehicleDevice.LastStatus == null)
                {
                    _logger.Trace1(string.Format("{0} 状态未同步，{1} 任务完成信号处理失败", railGuidedVehicleDevice, args.EquipmentTaskId), this, args);

                    args.Handled = false;
                    return;
                }

                //如果处于完成状态，则清除该信号
                if (railGuidedVehicleDevice.LastStatus.Event == RailGuidedVehicleEvent.AutomaticTaskCompletion
                    || railGuidedVehicleDevice.LastStatus.Event == RailGuidedVehicleEvent.TaskCompletionByManual)
                {

                    _logger.Trace1(string.Format("准备清除 {0} 的任务 {1} 完成信号", railGuidedVehicleDevice, args.EquipmentTaskId), this);

                    var cmd = new ClearTaskCommand();
                    railGuidedVehicleDevice.Write(cmd, cmd.SendSuccess);
                }
                else
                {
                    _logger.Trace1(string.Format("收到 {0} 的任务完成信号，但是 {1} 当前事件不是 {2}、{3}，而是 {4}，未发送清除指令。"
                        , args.EquipmentTaskId
                        , railGuidedVehicleDevice
                        , RailGuidedVehicleEvent.AutomaticTaskCompletion.GetDescription()
                        , RailGuidedVehicleEvent.TaskCompletionByManual.GetDescription()
                        , railGuidedVehicleDevice.LastStatus.Event.GetDescription()
                        ), this, args);

                }

                args.Handled = true;
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
