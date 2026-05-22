using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ZHQXC.WebAPI;
using NLog;
using Sineva.WMS.Dto.WCSDto.RequestDto;
using Wcs;
using Wcs.DefaultImplementCollection.Crane;
using Wcs.Framework;

namespace ZHQXC
{
    /// <summary>
    /// 跺机巷道设备状态上报线程WMS
    /// </summary>
    public class TunnelEquipmentStatusReportStartUp : IApplicationStartup
    {
        static Thread _thread;

        Int32 _interval;

        Logger _logger;

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
            _interval = _element.GetAttributeOrDefault<Int32>("interval", 3000);

            ParameterizedThreadStart Start = new ParameterizedThreadStart(check);
            _thread = new Thread(Start);
            _thread.IsBackground = true;
            _thread.Start();
            this._logger.Debug1(String.Format("{0} 线程已经启动！", this), this);
        }

        private void check(object obj)
        {
            var cranes = Wcs.Framework.Cfg.WcsConfiguration.Instance
                       .DeviceCollection.ParticularDeviceCollection
                       .SelectMany(x => x.DeviceElements).Where(x => x.Device is CraneDevice)
                       .Select(x => x.Device as CraneDevice);

            while (true)
            {
                Thread.Sleep(_interval);

                var _wcs手工标记故障 = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("WCS手工标记设备故障", "").Split(',').ToArray();
                Dictionary<String, String> craneStatus = new Dictionary<string, string>();
                const String IsOK = "ok";
                const String IsError = "fault";

                foreach (var item in cranes)
                {
                    if (_wcs手工标记故障.Contains(item.Name))
                    {
                        //1号巷道、2号巷道。。。。。
                        //craneStatus.Add(item.Name, $"{IsError},-1");
                        craneStatus.Add(item.No.ToString(), $"{IsError}");
                        continue;
                    }

                    #region 正常代码
                    if (!item.IsConnected || item.LastStatus == null || !item.Locker.IsEmpty)
                    {
                        //1号巷道、2号巷道。。。。。
                        //craneStatus.Add(item.Name, $"{IsError},-1");
                        craneStatus.Add(item.No.ToString(), $"{IsError}");
                        continue;
                    }

                    //var state = item.LastStatus.State;

                    //switch (state)
                    //{
                    //    case CraneStatus.AlarmAndShutdown:                              //报警停机
                    //    case CraneStatus.Disconnected:                                  //脱机
                    //    case CraneStatus.Initialized:                                   //初始化
                    //    case CraneStatus.ManualMode:                                    //手动操作
                    //    case CraneStatus.ResetAlarm:                                    //报警复位
                    //    case CraneStatus.奇怪的状态:
                    //        //craneStatus.Add(item.Name, $"{IsError},{GetLine(item)}");
                    //        craneStatus.Add(item.No.ToString(), $"{IsError}");
                    //        continue;
                    //    case CraneStatus.BackToTheOrigin:                               //回原点
                    //    case CraneStatus.Pickup:                                        //取货
                    //    case CraneStatus.Putin:                                         //放货
                    //    case CraneStatus.有货运行:
                    //    case CraneStatus.无货待命:
                    //    case CraneStatus.无货运行:
                    //    case CraneStatus.有货待命:
                    //        if (!item.Locker.IsEmpty)
                    //        {
                    //            //craneStatus.Add(item.Name, $"{IsError},{GetLine(item)}");
                    //            craneStatus.Add(item.No.ToString(), $"{IsError}");
                    //            continue;
                    //        }
                    //        //craneStatus.Add(item.Name, $"{IsOK},{GetLine(item)}");
                    //        craneStatus.Add(item.No.ToString(), $"{IsOK}");
                    //        continue;
                    //    default:
                    //        _logger.Error(String.Format("获取到 {0} 号堆垛机的未知状态 {1}", item.Name, item.LastStatus.State), this);
                    //        continue;
                    //}
                    #endregion

                    //#region 测试代码
                    //craneStatus.Add(item.No.ToString().Trim() + "号巷道", IsOK);
                    //#endregion 
                }

                //if (!Comparer(craneStatus, _lastCraneStatus))
                //{
                //    try
                //    {
                //        TunnelEquipmentStatusReportRequest tunnelEquipmentStatusReportRequest = new TunnelEquipmentStatusReportRequest();
                //        tunnelEquipmentStatusReportRequest.IctId = Guid.NewGuid().ToString("N");
                //        tunnelEquipmentStatusReportRequest.IctDatetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                //        tunnelEquipmentStatusReportRequest.Info = new List<TunnelEquipmentStatusReportRequest.DataInfo>();
                //        tunnelEquipmentStatusReportRequest.Info = craneStatus.Select(x => new TunnelEquipmentStatusReportRequest.DataInfo() { TunnelNo = x.Key, TunnelStatus = x.Value }).ToList();
                //        //var result = RequestWMSHelper.TunnelEquipmentStatusReport(tunnelEquipmentStatusReportRequest, out string msg);
                //        //if (result)
                //        //{
                //        //    this._logger.Debug1("巷道设备状态 通知 WMS 成功。", this);
                //        //    _lastCraneStatus = craneStatus;
                //        //}
                //        //else
                //        //    this._logger.Warn1($"巷道设备状态 通知 WMS 失败，异常消息：{msg}", this);
                //    }
                //    catch (Exception ex)
                //    {
                //        this._logger.Error1(ex, this);
                //        //通知失败不更新 _lastCraneStatus 的值
                //        continue;
                //    }
                //}
            }
        }

        Dictionary<string, Dictionary<int, KeyValuePair<int, int>>> _lines = new Dictionary<string, Dictionary<int, KeyValuePair<int, int>>>();

        private Boolean Comparer(Dictionary<String, String> d1, Dictionary<String, String> d2)
        {
            if (d1.Count() == 0 || d2.Count() == 0)
            {
                return false;
            }

            foreach (var item in d1)
            {
                if (!d2.Keys.Contains(item.Key) || d1[item.Key] != d2[item.Key])
                {
                    return false;
                }
            }
            return true;
        }

        public override string ToString()
        {
            return "巷道状态上报WMS线程";
        }
    }
}
