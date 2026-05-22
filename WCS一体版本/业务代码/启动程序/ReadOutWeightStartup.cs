using BOE.PreTaskHand;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Wcs;
using Wcs.DefaultImpls.Conveyor;
using Wcs.Framework;
using Wcs.Framework.Cfg;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace BOE
{
    /// <summary>
    /// 出库重量读取程序
    /// </summary>
    public class ReadOutWeightStartup : IApplicationStartup
    {
        static Thread _thread;
        Int32 _interval;
        Logger _logger;
        string _deviceName;
        List<string> locs;
        List<string> ends;
        Wcs.Framework.Cfg.StartupElement _element;
        public void Initialize(StartupElement element)
        {
            _element = element;
        }

        public void Run(IWcsApplication application)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _interval = _element.GetAttributeOrDefault<Int32>("interval", 500);
            _deviceName = _element.GetAttribute<string>("device");
            locs = _element.GetAttribute<string>("locations").Split(',').ToList();
            ends = _element.GetAttribute<string>("ends").Split(',').ToList();

            ParameterizedThreadStart Start = new ParameterizedThreadStart(check);
            _thread = new Thread(Start);
            _thread.IsBackground = true;
            _thread.Start();
            this._logger.Debug1(String.Format("{0} 线程已经启动！", this), this);
        }

        List<string> alreadyHands = new List<string>();
        private void check(object obj)
        {
            var device = DeviceConverter.ToDevice<ConveyorDevice>(_deviceName);
            var weightScale = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<int>("weightScale");
            var key = "WCSWEIGHT";
            while (true)
            {
                try
                {
                    List<Task> tasks = new List<Task>();
                    lock (PreTaskHandHelper.Tasks)
                    {
                        tasks = PreTaskHandHelper.Tasks.Where(x => x.StartLocation.DeviceName != x.EndLocation.DeviceName && ends.Contains(x.EndLocation.DeviceCode) && locs.Contains(x.CurrentLocation.DeviceCode)).ToList();
                    }
                    if (alreadyHands.Count > 0)
                    {
                        var taskCodes = tasks.Select(x => x.TaskCode);
                        alreadyHands = alreadyHands.Where(x => taskCodes.Contains(x)).ToList();
                    }

                    if (tasks.Count == 0)
                        continue;

                    foreach (var item in tasks)
                    {
                        if (alreadyHands.Contains(item.TaskCode))
                            continue;
                        if (item.AdditionalInfo != null && item.AdditionalInfo.ContainsKey(key))
                            continue;

                        var port = int.Parse(item.CurrentLocation.DeviceCode);
                        var weightObject = device.ReadStatus<ReportScannerWeightNetTransferObject>().FirstOrDefault(x => x.PosNo == port);
                        if (weightObject == null)
                            continue;

                        var weight = ((double)weightObject.Weight) / weightScale;
                        if (weight == 0)
                        {
                            var action = device.EquipmentActionScheduler.Actions.FirstOrDefault(x => x.Movement.Status == LogicMovementStatus.New && x.Movement.StartLocation.DeviceCode == item.CurrentLocation.DeviceCode);
                            if (action == null)
                                continue;
                            if (DateTime.Now.Subtract(action.CreatedAt).TotalMilliseconds < 5000)
                                continue;
                        }

                        Wcs.Framework.Events.TaskUpdateEvent taskUpdateEvent = null;
                        using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                        {
                            var task = unitOfWork.session.Get<Task>(item.Id);
                            if (task != null && task.AdditionalInfo != null && !task.AdditionalInfo.ContainsKey(key))
                            {
                                task.AdditionalInfo.Add(key, weight.ToString());
                                task.AdditionalInfo.Add("WeightTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                                taskUpdateEvent = new Wcs.Framework.Events.TaskUpdateEvent(item, task);
                            }
                            unitOfWork.Commit();
                        }

                        if (taskUpdateEvent != null)
                            Wcs.Framework.EventBus.EventBus.Instance.Publish(taskUpdateEvent);

                        alreadyHands.Add(item.TaskCode);
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
