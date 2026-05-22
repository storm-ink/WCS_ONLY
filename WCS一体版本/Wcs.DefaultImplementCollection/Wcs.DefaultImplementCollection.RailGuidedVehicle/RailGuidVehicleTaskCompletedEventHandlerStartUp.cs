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

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    public class RailGuidVehicleTaskCompletedEventHandlerStartUp : IApplicationStartup
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

        Dictionary<string, DateTime> lastSendSuccessAt;
        private void check(object obj)
        {
            lastSendSuccessAt = new Dictionary<string, DateTime>();
            var devices = Wcs.Framework.Cfg.WcsConfiguration.Instance
                  .DeviceCollection.ParticularDeviceCollection
                  .SelectMany(x => x.DeviceElements).Where(x => x.Device is RailGuidedVehicleDevice)
                  .Select(x => x.Device as RailGuidedVehicleDevice);
            while (true)
            {
                try
                {
                    Thread.Sleep(_interval);
                    foreach (var device in devices)
                    {
                        if (!device.IsConnected || device.LastStatus == null || (device.LastStatus.Event != RailGuidedVehicleEvent.AutomaticTaskCompletion && device.LastStatus.Event != RailGuidedVehicleEvent.TaskCompletionByManual))
                            continue;

                        if (lastSendSuccessAt.ContainsKey(device.Name) && DateTime.Now.Subtract(lastSendSuccessAt[device.Name]).TotalMilliseconds < 5000)
                            continue;

                        _logger.Trace1(string.Format("准备清除 {0} 的任务 {1} 完成信号", device, device.LastStatus.TaskId), this);

                        var cmd = new ClearTaskCommand();
                        device.Write(cmd, cmd.SendSuccess);
                        if (lastSendSuccessAt.ContainsKey(device.Name))
                            lastSendSuccessAt[device.Name] = DateTime.Now;
                        else
                            lastSendSuccessAt.Add(device.Name, DateTime.Now);
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
