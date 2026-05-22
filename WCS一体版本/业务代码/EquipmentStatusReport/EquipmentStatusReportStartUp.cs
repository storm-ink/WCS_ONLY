using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs;
using Wcs.DefaultImplementCollection.Business;
using Wcs.DefaultImplementCollection.Conveyor;
using Wcs.DefaultImplementCollection.Crane;
using Wcs.Framework;
using NLog;
using System.Threading;
using Wcs.Framework.Cfg;
using static Sineva.WMS.Dto.WCSDto.RequestDto.EquipmentStatusReportRequest;
using Wcs.App.Plugins.HomePage;
using ZHQXC.WebAPI;

namespace ZHQXC.AlarmPool
{
    /// <summary>
    /// 设备状态上报启动线程
    /// </summary>
    public class EquipmentStatusReportStartUp : IApplicationStartup
    {
        Logger _logger;
        static Thread _thread;
        public void Initialize(StartupElement element)
        {
        }

        public void Run(IWcsApplication application)
        {
            _logger = LogManager.GetCurrentClassLogger();

            _thread = new System.Threading.Thread(proc);
            _thread.Name = $"EquipmentStatusReportThread-ReoprtWms";
            _thread.IsBackground = true;
            _thread.StartAndManaged();
            _logger.Debug1($"EquipmentStatusReportStartUp-ReoprtWms线程已启动", this);
        }

        List<DataInfo> list;
        List<string> deviceNames;

        private void proc(object obj)
        {
            var interval = 1000;
            List<DataInfo> lastList = new List<DataInfo>();
            Dictionary<string, string> devices = new Dictionary<string, string>();

            var deviceElements = Wcs.Framework.Cfg.WcsConfiguration.Instance
                 .DeviceCollection.ParticularDeviceCollection
                 .SelectMany(x => x.DeviceElements);

            foreach (var item in deviceElements)
            {
                var deviceType = item.Device.GetDeviceType();
                devices.Add(item.Name, deviceType);

                if (deviceType == "输送线")
                {
                    var conveyor = (ConveyorDevice)item.Device;
                    foreach (var loc in conveyor.Locations)
                    {
                        if (loc is ConveyorLocationWildcard)
                            continue;
                        devices.Add($"{loc.DeviceCode}@{loc.Device.Name}",deviceType);
                    }
                }
            }

            while (true)
            {
                try
                {
                    List<DataInfo> nowList = new List<DataInfo>();
                    List<DataInfo> reportList = new List<DataInfo>();
                    Thread.Sleep(interval);
                    foreach (var item in devices)
                    {
                        var _last = lastList.FirstOrDefault(x => x.DeviceName == item.Key && x.DeviceType == item.Value);
                        var deviceAlarm = HomePageHelper.last.FirstOrDefault(x => x.DeviceName == item.Key && x.DeviceType == item.Value);
                        if (deviceAlarm == null)
                        {
                            continue;
                            //暂时不上报这类
                            //var _now = new DataInfo()
                            //{
                            //    DeviceName = item.Key,
                            //    DeviceType = item.Value,
                            //    IsOk = true,
                            //    IsAlarm = false,
                            //    IsJob = CheckDeviceIsJob(item.Key, item.Value),
                            //    AlarmDescription = "",
                            //    Warehouse = ""
                            //};
                            //if (_last == null || !_now.Equals(_last))
                            //    reportList.Add(_now);
                            //nowList.Add(_now);
                        }
                        else
                        {
                            var _now = new DataInfo()
                            {
                                DeviceName = item.Key,
                                DeviceType = item.Value,
                                IsOk = deviceAlarm.IsOK,
                                IsAlarm = false,
                                IsJob = CheckDeviceIsJob(item.Key, item.Value),
                                AlarmDescription = GetAlarmDescription(deviceAlarm),
                                Warehouse = ""
                            };
                            if (deviceAlarm.IsAlarm == "故障中")
                                _now.IsAlarm = true;

                            if (_last == null || !_now.Equals(_last))
                                reportList.Add(_now);
                            nowList.Add(_now);
                        }
                    }

                    if (reportList.Count() > 0)
                    {
                        Sineva.WMS.Dto.WCSDto.RequestDto.EquipmentStatusReportRequest equipmentStatusReportRequest = new Sineva.WMS.Dto.WCSDto.RequestDto.EquipmentStatusReportRequest();
                        equipmentStatusReportRequest.Info = reportList;
                        //var result = RequestWMSHelper.EquipmentStatusReport(equipmentStatusReportRequest, out string msg);
                        //if (result)
                        //    lastList = nowList;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, this);
                }
            }
        }

        private string GetAlarmDescription(DeviceAlarm deviceAlarm)
        {
            List<string> sb = new List<string>();
            if (deviceAlarm.IsConnected == "未连接")
                sb.Add(deviceAlarm.IsConnected);
            if (deviceAlarm.IsLocked == "锁定")
                sb.Add(deviceAlarm.IsLocked);
            if (!string.IsNullOrWhiteSpace(deviceAlarm.AlarmDescription))
                sb.Add(deviceAlarm.AlarmDescription);
            if (sb.Count() == 0)
                return "";

            return string.Join("/", sb);
        }

        private bool CheckDeviceIsJob(string deviceName, string deviceType)
        {
            if (deviceType == "堆垛机")
            {
                var device = (CraneDevice)DeviceConverter.ToDevice<CraneDevice>(deviceName);
                if (device.LastStatus == null)
                    return false;
                if (device.EquipmentActionScheduler.Actions.Any(x => x.Status == EquipmentActionStatus.Executing))
                    return true;
                else
                    return false;
            }
            else if (deviceType == "输送线")
            {
                if (deviceName.Contains("@"))
                {
                    var location = LocationConverter.ConvertibleCodeToLcation(deviceName);
                    var device = (ConveyorDevice)location.Device;
                    if (device.LocationInfoBlocks.Length == 0)
                        return false;
                    else
                    {
                        var posNo = Convert.ToInt16(location.DeviceCode);
                        if (device.LocationInfoBlocks.Any(x => x.PosNo == posNo && x.TaskNo != 0))
                            return true;
                        else
                            return false;
                    }
                }
                else
                {
                    var device = (ConveyorDevice)DeviceConverter.ToDevice<ConveyorDevice>(deviceName);
                    if (device.TaskBlocks.Length == 0)
                        return false;
                    else
                    {
                        if (device.TaskBlocks.Any(x => x.TaskNo != 0))
                            return true;
                        else
                            return false;
                    }
                }
            }
            else
                return false;
        }
    }
}
