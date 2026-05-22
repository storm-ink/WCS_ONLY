using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Wcs.Framework;
using Wcs;
using NHibernate.Linq;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 堆垛机远程急停按钮盒
    /// </summary>
    public class CraneWithRomoteEmergencyStopStartup : IApplicationStartup
    {
        List<Int32> result = new List<Int32> { };

        List<CraneDevice> craneDevice = new List<CraneDevice> { };

        static Thread _thread;
        Int32 Interval { get; set; }
        ConveyorDevice ConveyorDevice { get; set; }

        static Logger _logger = LogManager.GetCurrentClassLogger();

        public void Run(Wcs.IWcsApplication application)
        {

            ConveyorDevice = DeviceConverter.ToDevice<ConveyorDevice>(_deviceName);

            _thread = new Thread(触发按钮事件);
            _thread.Name = "堆垛机控制盒急停按钮";
            _thread.IsBackground = true;
            _thread.StartAndManaged();

        }

        private void 触发按钮事件(object obj)
        {
            _logger.Trace1("堆垛机控制盒急停按钮启动程序", this);

            List<CraneDevice> _all = Wcs.Framework.Cfg.WcsConfiguration.Instance.DeviceCollection.ParticularDeviceCollection.SelectMany(x => x.DeviceElements)
                           .Where(x => x.Device is CraneDevice)
                           .Select(x => x.Device as CraneDevice)
                           .ToList();



            while (true)
            {
                Thread.Sleep(Interval);

                var 堆垛机急停按钮状态 = this.ConveyorDevice.ReadStatus<WithRemoteControllerCraneDeviceNetTransferObject>();
                if (堆垛机急停按钮状态 == null || 堆垛机急停按钮状态.Length == 0)
                {
                    continue;
                }

                //一拍急停,所有的堆垛机都停下来了
                //if (堆垛机急停按钮状态.Any(x => x.IsEmergency && _all.Any(c => c.No == x.DeviceNo)))
                //{
                //    foreach (var crane in _all)
                //    {
                //        try
                //        {
                //            if (crane.LastStatus != null
                //                && crane.LastStatus.Event != CraneEvent.EmergencyStop
                //                && crane.LastStatus.ErrorCode != 1072)
                //            {
                //                crane.EmergencyStop();
                //            }

                //        }
                //        catch (Exception ex)
                //        {
                //            _logger.Error1(ex, this);
                //        }


                        foreach (var item in 堆垛机急停按钮状态)
                        {
                            if (item.IsEmergency)
                            {
                                try
                                {
                                    _all.First(x => x.No == item.DeviceNo).EmergencyStop();
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error1(ex, this);
                                }
                            }

                    }

                       //高速堆垛机暂停任务
                        //foreach (var crane in _all)
                        //{
                        //    try
                        //    {
                        //        if (crane.No == 5 || crane.No == 7 || crane.No == 8)
                        //        {
                        //            if (crane.LastStatus != null
                        //                && crane.LastStatus.Event != CraneEvent.EmergencyStop
                        //                && crane.LastStatus.ErrorCode != 0
                        //                //&& crane.No == 5
                        //                )
                        //            {
                        //                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                        //                {
                        //                    var _task = unitOfWork.session.Query<Wcs.Framework.Task>().
                        //                        Where(x => x.StartLocation.DeviceName == "C005"
                        //                                || x.EndLocation.DeviceName == "C005"
                        //                                || x.StartLocation.DeviceName == "C007"
                        //                                || x.EndLocation.DeviceName == "C007"
                        //                                || x.StartLocation.DeviceName == "C008"
                        //                                || x.EndLocation.DeviceName == "C008"
                        //                        && x.Status == Wcs.Framework.TaskStatus.Executing
                        //                        ).FirstOrDefault();

                        //                    if (_task != null)
                        //                    {
                        //                        TaskHelper.Suspend(_task.Id);
                        //                    }
                        //                    unitOfWork.Commit();
                        //                }
                        //            }
                        //        }

                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        _logger.Error1(ex, this);
                        //    }
                        //  }
                }
            }
         
        String _deviceName;
        public void Initialize(Wcs.Framework.Cfg.StartupElement element)
        {
            Interval = element.GetAttributeOrDefault<int>("interval", 200);
            _deviceName = element.GetAttribute<string>("deviceName");
        }
    }
}
