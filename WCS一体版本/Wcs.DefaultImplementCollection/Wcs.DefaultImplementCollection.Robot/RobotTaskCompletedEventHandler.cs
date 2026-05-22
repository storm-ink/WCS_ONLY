using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;
using NHibernate.Linq;
using NLog;
using Wcs.Framework.Cfg;
using NHibernate;

namespace Wcs.DefaultImplementCollection.Robot
{
    /// <summary>
    /// Robo任务完成处理程序
    /// </summary>
    public sealed class RobotTaskCompletedEventHandler:ITaskEventHandler<TaskCompletedEventArgs>
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();
        static Random _rnd=new Random();
        public RobotTaskCompletedEventHandler()
        {
        }

        public void Handle(TaskableDevice device,ISession session, TaskCompletedEventArgs args)
        {
            try
            {
                if (!(device is RobotDevice _device))
                {
                    String msg = string.Format("收到了 {0} 发送来的 {1} 任务完成信号,其非 {2} 设备类型，被 {3} 忽略。", device, args.EquipmentTaskId, typeof(RobotDevice), this);
                    _logger.Debug1(msg, this, args,null,args.EquipmentTaskId);
                    args.Handled = true;
                    return;
                }

                _logger.Trace1(string.Format("{0} 开始处理设备任务 {1} 的完成事件...", this, args.EquipmentTaskId), this, args,null,args.EquipmentTaskId);

                //如果状态未同步，直接处理失败等待下一轮处理调用
                if (_device.LastState == null)
                {
                    _logger.Trace1(string.Format("{0} 状态未同步，{1} 任务完成信号处理失败", _device, args.EquipmentTaskId), this, args);

                    args.Handled = false;
                    return;
                }
                if (_device.LastState.RobotTask.TaskId != 0)
                {
                    _logger.Trace1(string.Format("准备清除 {0} 的任务 {1} 完成信号", _device, args.EquipmentTaskId), this);
                    RobotTaskClearCommand cmd = new RobotTaskClearCommand();
                    cmd.TaskId = _device.LastState.RobotTask.TaskId;
                    cmd.HandShake = HandShake.Delete;
                    try
                    {
                        _device.Write<RobotTaskClearCommand>(cmd, cmd.SendSuccess);
                    }
                    catch (Exception ex)
                    {
                        _device.LastFireTaskCompletedEventTaskId = 0;
                        _logger.Error1(ex, this);
                    }
                }

                args.Handled = true;

            }
            catch (Exception ex)
            {
                String msg = string.Format("收到了 {0} 发送来的 {1} 任务完成信号,但 {2} 在处理时发生异常，操作已中止", device, args.EquipmentTaskId,this);
                _logger.Error1(new Exception(msg, ex), this, args, null, args.EquipmentTaskId);
                args.Handled = false;
                //throw;
            }
        }
    }
}
