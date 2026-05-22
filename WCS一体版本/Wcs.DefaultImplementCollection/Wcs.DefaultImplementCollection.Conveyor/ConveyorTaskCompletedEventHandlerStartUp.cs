using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Wcs;
using Wcs.Framework;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    public class ConveyorTaskCompletedEventHandlerStartUp : IApplicationStartup
    {
        static Thread _thread;

        Int32 _interval;

        Logger _logger;

        StartupElement _element;

        public void Initialize(StartupElement element)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _element = element;
        }

        public void Run(IWcsApplication application)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _interval = _element.GetAttributeOrDefault<Int32>("interval", 1000);

            ParameterizedThreadStart Start = new ParameterizedThreadStart(check);
            _thread = new Thread(Start);
            _thread.IsBackground = true;
            _thread.Start();
            this._logger.Debug1(String.Format("{0} 线程已经启动！", this), this);
        }

        private void check(object obj)
        {
            var devices = Wcs.Framework.Cfg.WcsConfiguration.Instance
                  .DeviceCollection.ParticularDeviceCollection
                  .SelectMany(x => x.DeviceElements).Where(x => x.Device is ConveyorDevice)
                  .Select(x => x.Device as ConveyorDevice);
            Random _rnd = new Random();
            while (true)
            {
                try
                {
                    Thread.Sleep(_interval);
                    foreach (var device in devices)
                    {
                        if (!device.IsConnected || device.TaskBlocks == null || device.TaskBlocks.Count() == 0)
                            continue;

                        var tasks = device.TaskBlocks.Where(x => x.TaskNo != 0 && x.TaskState == TaskBlockTaskStatus.Finished);
                        foreach (var task in tasks)
                        {
                            //if (task.AtPacketIndex > device.AllowTaskBlockSpace)
                            //    continue;

                            if (device.AllowTaskBlockSpace != 0 && task.AtPacketIndex > device.AllowTaskBlockSpace)
                                continue;

                            var action = device.EquipmentActionScheduler.Actions.FirstOrDefault(x => x.EquipmentTaskId == task.TaskNo);
                            if (action == null || action.Status == EquipmentActionStatus.Completed || action.Status == EquipmentActionStatus.Cancelled || action.Status == EquipmentActionStatus.Error)
                            {
                                TaskCommand clearTaskCommand = new TaskCommand(TaskHandShakes.ApplyForDelete, task.TaskNo,"",new UInt16[10], task.RotingNo, task.From, task.To, Convert.ToUInt16(task.AtPacketIndex));
                                try
                                {
                                    ((ConveyorDevice)device).Write(clearTaskCommand, (taskableDevice, cmd) =>
                                    {
                                        ConveyorDevice conveyorDevice = (ConveyorDevice)taskableDevice;
                                        return conveyorDevice.TaskBlocks != null
                                            && conveyorDevice.TaskBlocks.Length > 0
                                            && conveyorDevice.TaskBlocks[cmd.DBIndex - 1].HandShake == TaskHandShakes.Empty;
                                    });

                                    _logger.Debug1(string.Format("①收到了 {0} 发送来的 {1} 任务完成信号,成功向其发送了一个 {2} 指令。", device, task.TaskNo, clearTaskCommand), this, clearTaskCommand, null, (int)task.TaskNo);

                                }
                                catch (Exception ex)
                                {
                                    _logger.Warn1(string.Format("①收到了 {0} 发送来的 {1} 任务完成信号,但在成功向其发送了 {2} 指令时发生异常（后续可能引起该任务残余在电控程序当中,请及时检查并清理）。\n{3}",
                                        device,
                                        task.TaskNo,
                                        clearTaskCommand,
                                        ex), this, clearTaskCommand, null, (int)task.TaskNo);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, this);
                }
            }
        }
    }
}
