using BOE.PreTaskHand;
using NHibernate.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Wcs;
using Wcs.DefaultImpls.Conveyor;

using Wcs.Framework.Cfg;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using Wcs.Framework;
using Wcs.FrameworkExtend;

namespace BOE
{
    public class 出入库模式自动切换处理程序 : ThreadRunningLog,IApplicationStartup
    {

        static Thread _thread;
        Int32 _interval;
        Logger _logger;
        string _deviceName;
        AlivePlatformCommand cmd;
        Dictionary<String, String> _lastCraneStatus { get; set; }

        Wcs.Framework.Cfg.StartupElement _element;
        List<string> To = new List<string> { "00-001-1001", "00-001-1008", "00-001-1015", "00-001-2001", "00-001-2008", "00-001-2015" };
        public void Initialize(StartupElement element)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _lastCraneStatus = new Dictionary<string, string>();
            _element = element;
        }

        public void Run(IWcsApplication application)
        {
            _interval = _element.GetAttributeOrDefault<Int32>("interval", 5000);
            this.Init("出入库模式切换处理线程");
            ParameterizedThreadStart Start = new ParameterizedThreadStart(check);
            _thread = new Thread(Start);
            _thread.IsBackground = true;
            _thread.Start();
            this._logger.Debug1(String.Format("{0} 线程已经启动！", this), this);
            this.Log($"{this} 线程已经启动！");
        }

        private void check(object obj)
        {
            while (true)
            {
                Thread.Sleep(_interval);
                try
                {
                    List<PreTask> tasks;
                    lock (PreTaskHandHelper.PreTasks)
                    {
                        tasks = PreTaskHandHelper.PreTasks.Where(x=> x.StartLocation.DeviceName != x.EndLocation.DeviceName && To.Contains(x.EndLocation.UserCode) && x.Status != TaskStatus.Completed && x.Status != TaskStatus.Cancelled).ToList();
                    }
                    foreach (string to in To)
                    {                        
                        var location = LocationConverter.UserCodeToLcation(to);
                        var device = DeviceConverter.ToDevice<ConveyorDevice>(location.Device.Name);
                        if (device == null || !device.IsConnected)
                        {
                            this.Log($"位置 {to} 所属设备 {location.Device.Name} 未连接，当前无法判断是否需要切换出入库模式");
                            continue;
                        }

                        var hastasks = tasks.Any(x => x.EndLocation.UserCode == to);
                        if (hastasks)
                        {
                            var result = device.ReadStatus<AlivePlatformNetTransferObject>().Any(x => x.PosNo.ToString() == location.DeviceCode && x.HomePos == HomePosStatus.RightUp);
                            if (!result)
                            {
                                try
                                {
                                    this.Log($"位置 {to} 当前不是 RightUp 状态，本次尝试将其切换为 RightUp");
                                    cmd = new AlivePlatformCommand(Convert.ToUInt16(location.DeviceCode), HomePosStatus.RightUp);
                                    device.Write<AlivePlatformCommand>(cmd, cmd.SendSuccess);
                                    this.Log($"位置 {to} 当前不是 RightUp 状态，本次成功将其切换为 RightUp");
                                }
                                catch (Exception e)
                                {
                                    _logger.Error1(e, this);
                                    this.Log($"尝试将位置 {to} 切换为 RightUp 时发生异常，异常消息:\r\n{e}");
                                }
                            }
                            else
                                this.Log($"位置 {to} 当前处于 RightUp 状态，本次未切换");
                        }
                        else
                        {
                            var result = device.ReadStatus<AlivePlatformNetTransferObject>().Any(x => x.PosNo.ToString() == location.DeviceCode && x.HomePos == HomePosStatus.LeftDown);
                            bool hasTasks = device.ReadStatus<LocationTaskNetTransferObject>().Any(x => x.PosNo.ToString() == location.DeviceCode && x.TaskNo != 0);
                            if (!result && !hasTasks)
                            {
                                try
                                {
                                    this.Log($"位置 {to} 当前不是 LeftDown 状态 并且 货位 {location.DeviceCode} 当前任务号为 0，本次尝试将其切换为 LeftDown");
                                    cmd = new AlivePlatformCommand(Convert.ToUInt16(location.DeviceCode), HomePosStatus.LeftDown);
                                    device.Write<AlivePlatformCommand>(cmd, cmd.SendSuccess);
                                    this.Log($"位置 {to} 当前不是 LeftDown 状态 并且 货位 {location.DeviceCode} 当前任务号为 0，本次成功将其切换为 LeftDown");
                                }
                                catch (Exception e)
                                {
                                    _logger.Error1(e, this);
                                    this.Log($"尝试将位置 {to} 切换为 LeftDown 时发生异常，异常消息:\r\n{e}");
                                }
                            }
                            else
                                this.Log($"位置 {to} 当前是 LeftDown 状态 或者 货位 {location.DeviceCode} 当前任务号不为 0，本次未切换");
                        }
                    }

                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, this);
                    this.Log($"出入库模式切换处理线程发生异常，异常消息:\r\n{ex}");
                }
            }
        }
    }
}
