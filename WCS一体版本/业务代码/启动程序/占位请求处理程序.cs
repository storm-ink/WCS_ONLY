using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.DefaultImpls.Conveyor;
using Wcs.Framework;
using Wcs;
using NHibernate.Linq;

namespace BOE
{
    /// <summary>
    /// 占位请求处理程序
    /// </summary>
    public class 占位请求处理程序 : IApplicationStartup
    {
        public Int32 Interval { get; private set; }
        String _taskType;
        /// <summary>
        /// 空托盘请求的位置
        /// </summary>
        public Dictionary<Int16, Location> RequestLocation { get; private set; }

        String[] _requestLocations;
        Wcs.Framework.Cfg.StartupElement _element;
        static Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        static List<System.Threading.Thread> _threads = new List<System.Threading.Thread>();
        public void Initialize(Wcs.Framework.Cfg.StartupElement element)
        {
            _element = element;
            this.Interval = element.GetAttributeOrDefault<Int32>("interval", 500);
            _taskType = element.GetAttributeOrDefault<string>("taskType", null);
            _requestLocations = element.GetAttributeOrDefault<String>("requestLocation", "").Split(new String[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            if (this.Interval < 100)
            {
                this.Interval = 100;
            }
            if (_requestLocations.Length == 0)
            {
                throw new System.Configuration.ConfigurationErrorsException("requestLocation 中至少要指定一个货位可转换编码（多个时用”,“隔开）。", element.Node);
            }

        }

        public void Run(IWcsApplication application)
        {
            Dictionary<Int16, Location> requests = new Dictionary<Int16, Location>();

            foreach (var item in _requestLocations)
            {
                var kv = item.Split('|');
                if (kv.Length != 2)
                {

                    throw new System.Configuration.ConfigurationErrorsException("requestLocation 中指定的位置 " + item + "无效。", _element.Node);
                }
                var posNo = Convert.ToInt16(kv[0]);

                Location loc = null;
                if (kv.Length > 1)
                    loc = LocationConverter.ConvertibleCodeToLcation(kv[1]);

                requests.Add(posNo, loc);
            }

            this.RequestLocation = requests;

            System.Threading.Thread thread = new System.Threading.Thread(Process);
            thread.IsBackground = true;
            thread.Name = "无扫码占位请求处理程序";
            thread.StartAndManaged();

            _threads.Add(thread);
        }

        Dictionary<string, DateTime> _lastRequestSuccess = new Dictionary<string, DateTime>();
        private void Process()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(this.Interval);
                foreach (var loc in RequestLocation)
                {
                    try
                    {
                        var conveyorDevice = (ConveyorDevice)loc.Value.Device;

                        HoldSignalNetTransferObject hs;
                        //IsExit   是否有占位有输出占位无continue
                        if (!conveyorDevice.IsExit(loc.Key, out hs))
                        {
                            continue;
                        }

                        if (_lastRequestSuccess.ContainsKey(loc.Value.DeviceCode))
                        {
                            var sp = DateTime.Now.Subtract(_lastRequestSuccess[loc.Value.DeviceCode]).TotalMilliseconds;
                            if (sp < 10000)
                                continue;
                        }

                        Wcs.Framework.MessageBoard.AbstractMessageBoard.Instance.Add(
                            new Wcs.Framework.MessageBoard.Messages.TipMessage(Wcs.Framework.MessageBoard.MessageLevel.Info,
                                        "占位请求",
                                        String.Format("收到位置 {0} 的占位信号", loc.Key),
                                        null));


                        //if (loc.Value.DeviceCode == "1002")
                        //{
                        //   String[] _endLocations = new String[1] { "NoRead" };
                        //   loc.Value.RequestWms(_endLocations, "空托叠盘入库", 0, null);
                        //}

                        if (loc.Value.DeviceCode == "1913")
                        {
                            Boolean alreadyCreateTask = false;
                            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                            {
                                alreadyCreateTask = unitOfWork.session.Query<Task>().Any(x =>
                                       x.Status != TaskStatus.Cancelled
                                    && x.Status != TaskStatus.Completed
                                    && x.EndLocation.DeviceCode == "1913"
                                 );
                                unitOfWork.Commit();
                            }
                            if (alreadyCreateTask)
                            {
                                continue;
                            }
                            else
                            {
                                loc.Value.RequestWms(null, "空托盘出库");
                                if (_lastRequestSuccess.ContainsKey(loc.Value.DeviceCode))
                                    _lastRequestSuccess[loc.Value.DeviceCode] = DateTime.Now;
                                else
                                    _lastRequestSuccess.Add(loc.Value.DeviceCode, DateTime.Now);
                            }
                        }

                        if (loc.Value.DeviceCode == "1012")
                        {
                            Boolean alreadyCreateTask1 = false;
                            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                            {
                                alreadyCreateTask1 = unitOfWork.session.Query<Task>().Any(x =>
                                       x.Status != TaskStatus.Cancelled
                                    && x.Status != TaskStatus.Completed
                                    && x.StartLocation.DeviceCode == "1012"
                                    && x.Movements.Count <= 1
                                 );
                                unitOfWork.Commit();
                            }

                            if (alreadyCreateTask1)
                            {
                                continue;
                            }
                            else
                            {
                                loc.Value.RequestWms(null, "空托盘入库");
                                if (_lastRequestSuccess.ContainsKey(loc.Value.DeviceCode))
                                    _lastRequestSuccess[loc.Value.DeviceCode] = DateTime.Now;
                                else
                                    _lastRequestSuccess.Add(loc.Value.DeviceCode, DateTime.Now);
                            }
                        }

                        Wcs.Framework.MessageBoard.AbstractMessageBoard.Instance.Add(
                            new Wcs.Framework.MessageBoard.Messages.TipMessage(Wcs.Framework.MessageBoard.MessageLevel.Info,
                                        "占位请求",
                                        String.Format("已通知 Wms 位置 {0} 的请求.", loc.Key),
                                        null));

                        _logger.Info1(string.Format("已通知 Wms 位置 {0} 的请求.", loc.Value), this);

                        //conveyorDevice.清除占位(hs);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error1(ex, this);

                        Wcs.Framework.MessageBoard.AbstractMessageBoard.Instance.Add(
                            new Wcs.Framework.MessageBoard.Messages.TipMessage(Wcs.Framework.MessageBoard.MessageLevel.Emergency,
                                        "占位请求",
                                        String.Format("占位请求处理失败，位置 {0}，异常信息：{1}.", loc.Key, ex.Message),
                                        null));
                    }
                }
            }
        }
    }
}
