using BOE.WebAPI;
using NLog;
using Sineva.WMS.Dto.WCSDto.ReplyDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wcs;
using Wcs.DefaultImpls.Conveyor;
using Wcs.Framework;

namespace BOE
{
    /// <summary>
    /// 出入库模式同步
    /// </summary>
    public class AlivePlatformStatusSynchronizationStartUp : IApplicationStartup
    {
        static Thread _thread;

        Int32 _interval;

        Logger _logger;

        string _deviceName;

        Dictionary<String, String> _lastCraneStatus { get; set; }

        Wcs.Framework.Cfg.StartupElement _element;

        public void Initialize(Wcs.Framework.Cfg.StartupElement element)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _lastCraneStatus = new Dictionary<string, string>();
            _element = element;
        }

        public void Run(IWcsApplication application)
        {
            _interval = _element.GetAttributeOrDefault<Int32>("interval", 5000);
            _deviceName = _element.GetAttributeOrDefault<string>("deviceName", "八号库");

            ParameterizedThreadStart Start = new ParameterizedThreadStart(check);
            _thread = new Thread(Start);
            _thread.IsBackground = true;
            _thread.Start();
            this._logger.Debug1(String.Format("{0} 线程已经启动！", this), this);
        }

        private void check(object obj)
        {
            while (true)
            {
                Thread.Sleep(_interval);
                try
                {
                    var requestResult = RequestWMSHelper.PortInfoRequest(out PortInfoReply portInfoReply, out string msg);
                    if (!requestResult || portInfoReply == null)
                    {
                        RequestWMSHelper.WMSSetPortInfos = null;
                        continue;
                    }
                    RequestWMSHelper.WMSSetPortInfos = portInfoReply.Info;

                    var device = DeviceConverter.ToDevice<ConveyorDevice>(_deviceName);
                    var alivePlatforms = device.ReadStatus<AlivePlatformNetTransferObject>();
                    if (alivePlatforms == null || alivePlatforms.Length == 0)
                        continue;

                    foreach (var item in alivePlatforms)
                    {
                        var portCode = $"00-001-{item.PosNo}";
                        var portInfo = portInfoReply.Info.FirstOrDefault(x => x.PortCode == portCode);
                        if (portInfo == null)
                            continue;

                        AlivePlatformCommand cmd = null;
                        #region 盘点回库任务检查
                        var scheduler = (ConveyorDeviceEquipmentActionScheduler)device.EquipmentActionScheduler;
                        var deviceCode = item.PosNo.ToString();
                        if (scheduler != null
                            && scheduler.Actions != null
                            && scheduler.Actions.Count() > 0 
                            && scheduler.Actions.Any(x => (x.Movement.StartLocation.DeviceCode == deviceCode
                                                    || x.Movement.EndLocation.DeviceCode == deviceCode)))
                        {
                            if (item.HomePos != HomePosStatus.LeftDown)
                                cmd = new AlivePlatformCommand(item.PosNo, HomePosStatus.LeftDown);
                        }
                        #endregion
                        else
                        {
                            ///1059 1056 1053 如果在高位，如果不停用则同步
                            switch (item.HomePos)
                            {
                                case HomePosStatus.UnKnow:
                                    if (portInfo.PortStatus.ToUpper() == "OFFLINE")
                                        cmd = new AlivePlatformCommand(item.PosNo, HomePosStatus.RightUp);
                                    else if (portInfo.PortStatus.ToUpper() == "ONLINE")
                                        cmd = new AlivePlatformCommand(item.PosNo, HomePosStatus.LeftDown);
                                    break;
                                case HomePosStatus.LeftDown:
                                    if (portInfo.PortStatus.ToUpper() == "OFFLINE")
                                        cmd = new AlivePlatformCommand(item.PosNo, HomePosStatus.RightUp);
                                    break;
                                case HomePosStatus.RightUp:
                                    if (portInfo.PortStatus.ToUpper() == "ONLINE")
                                        cmd = new AlivePlatformCommand(item.PosNo, HomePosStatus.LeftDown);
                                    break;
                                default:
                                    break;
                            }
                        }
                        if (cmd != null)
                        {
                            try
                            {
                                device.Write<AlivePlatformCommand>(cmd, cmd.SendSuccess);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error1(ex, this);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, this);
                    Wcs.Framework.MessageBoard.AbstractMessageBoard.Instance.Add(
                            new Wcs.Framework.MessageBoard.Messages.TipMessage(Wcs.Framework.MessageBoard.MessageLevel.Emergency,
                            "PORT口同步",
                            $"PORT口状态从WMS同步失败，异常消息{ex}",
                            null));
                }
            }
        }
    }
}
