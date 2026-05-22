using CompressHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NHibernate.Hql.Ast;
using NHibernate.Linq.Functions;
using NHibernate.Mapping;
using NLog;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wcs;
using Wcs.App.Plugins.HomePage;
using Wcs.DefaultImplementCollection.Conveyor;
using Wcs.DefaultImplementCollection.Crane;
using Wcs.Framework;
using Wcs.Framework.Cfg;
using ZHQXC.MonitorEntitys.Entity;

namespace ZHQXC
{
    /// <summary>
    /// 2D数据监控_MQTT
    /// </summary>
    public class MonitorStartUp_2D_Monitor_MQTT : ThreadRunningLog, IApplicationStartup
    {
        static Thread _thread;
        Logger _logger = LogManager.GetCurrentClassLogger();
        StartupElement _element;
        List<string> devices;
        WCSMqttClient_WCS mqttClient = null;
        List<int> vehicles = new List<int>() { 1019 };
        List<string> posNumAbord = new List<string>() { "1020","1021","1022","1023","1024","1025","1026","1027","1028" };


        public void Initialize(StartupElement element)
        {
            _element = element;
        }

        public void Run(IWcsApplication application)
        {
            this.Init("Monitor客户端数据同步线程(MQTT)");

            if (mqttClient == null)
            {
                mqttClient = WCSMqttClient_WCS.CreateInstance();
                mqttClient.client.DisconnectedAsync += Client_DisconnectedAsync;
            }


            ParameterizedThreadStart start = new ParameterizedThreadStart(check);
            _thread = new Thread(start);
            _thread.IsBackground = true;
            _thread.Start();
            _logger.Info1($"{this} 线程已经启动", this);
        }

        private System.Threading.Tasks.Task Client_DisconnectedAsync(MQTTnet.Client.MqttClientDisconnectedEventArgs arg)
        {
            try
            {
                lastsend = new Dictionary<string, MqttMsg>();
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
            }
            return null;
        }

        Dictionary<string, MqttMsg> lastsend = new Dictionary<string, MqttMsg>();

        Dictionary<string, MqttMsg> lastsave = new Dictionary<string, MqttMsg>();

        private async void check(object obj)
        {
            devices = Wcs.Framework.Cfg.WcsConfiguration.Instance.DeviceCollection.ParticularDeviceCollection
                          .SelectMany(x => x.DeviceElements)
                          .Where(x => x.Device is TaskableDevice)
                          .Select(x => x.Name)
                          .ToList();

            while (true)
            {
                Thread.Sleep(1000);
                try
                {
                    if (!mqttClient.IsConnected)
                        continue;

                    foreach (var dev in devices)
                    {
                        var _device = DeviceConverter.ToDevice<TaskableDevice>(dev);
                        #region cv 输送线
                        if (_device is ConveyorDevice)
                        {
                            var device = (ConveyorDevice)_device;
                            
                            foreach (var loc in device.Locations)
                            {
                                if (device.Name == "立库CV"&& posNumAbord.Contains(loc.UnifiedCode))
                                {
                                    continue;
                                }

                                var item = loc.DeviceCode;
                                if (loc is ConveyorLocationWildcard || !UInt16.TryParse(item, out UInt16 posNo))
                                    continue;

                                #region Show2DBasicDeviceInfo
                                Show2DBasicDeviceInfo show2DInfo = new Show2DBasicDeviceInfo();

                                if (device.Locker.IsEmpty)
                                    show2DInfo.Page2DShow.锁定 = false;
                                else
                                    show2DInfo.Page2DShow.锁定 = true;

                                if (vehicles.Contains(posNo)) //输送线托管的穿梭车
                                {
                                    if (device.IsConnected)
                                    {
                                        var vehicle = device.ReadStatus<VehicleBlock>().FirstOrDefault(x => x.VehicleNo == posNo);
                                        if (vehicle != null)
                                        {
                                            switch (vehicle.State)
                                            {
                                                case VehicleStatus.Unknown:
                                                    show2DInfo.Page2DShow.设备状态 = MonitorDeviceStatus.NotReady;
                                                    break;
                                                case VehicleStatus.Manual:
                                                    show2DInfo.Page2DShow.设备状态 = MonitorDeviceStatus.Manual;
                                                    break;
                                                case VehicleStatus.Offline:
                                                    show2DInfo.Page2DShow.设备状态 = MonitorDeviceStatus.Manual;
                                                    break;
                                                case VehicleStatus.Waitting:
                                                    switch (vehicle.Event)
                                                    {
                                                        case VehicleEvents.Unknown:
                                                            show2DInfo.Page2DShow.设备状态 = MonitorDeviceStatus.NotReady;
                                                            break;
                                                        case VehicleEvents.Waitting:
                                                            show2DInfo.Page2DShow.设备状态 = MonitorDeviceStatus.Auto;
                                                            break;
                                                        case VehicleEvents.EmptyMoving:
                                                        case VehicleEvents.NonEmptyMoving:
                                                        case VehicleEvents.Picking:
                                                        case VehicleEvents.Putting:
                                                        case VehicleEvents.PickWaitting:
                                                        case VehicleEvents.PutWaitting:
                                                            show2DInfo.Page2DShow.设备状态 = MonitorDeviceStatus.Running;
                                                            break;
                                                        default:
                                                            show2DInfo.Page2DShow.设备状态 = MonitorDeviceStatus.NotReady;
                                                            break;
                                                    }
                                                    break;
                                                case VehicleStatus.Alarming:
                                                    show2DInfo.Page2DShow.设备状态 = MonitorDeviceStatus.Alarm;
                                                    break;
                                                default:
                                                    show2DInfo.Page2DShow.设备状态 = MonitorDeviceStatus.NotReady;
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            show2DInfo.Page2DShow.设备状态 = MonitorDeviceStatus.UnReceivedData;
                                            show2DInfo.Page2DShow.有货 = false;
                                            show2DInfo.Page2DShow.AdditionalInfos.Add(Consts.LocationInfo, "||0");
                                        }
                                    }
                                    else
                                    {
                                        show2DInfo.Page2DShow.设备状态 = MonitorDeviceStatus.DisConnected;
                                        show2DInfo.Page2DShow.有货 = false;
                                        show2DInfo.Page2DShow.AdditionalInfos.Add(Consts.LocationInfo, "||0");
                                    }

                                    var now = WCSMqttMsgSerializeHelper.SerializeToString(show2DInfo);
                                    var topic = $"Wcs/DeviceInfo_2D/RGV/{posNo}/Show2DInfo";
                                    Sending(topic, now);
                                }
                                else
                                {
                                    if (device.IsConnected)
                                    {
                                        var locInfo = device.ReadStatus<LocationInfoBlock>().FirstOrDefault(x => x.PosNo == posNo);
                                        if (locInfo != null)
                                        {
                                            if (locInfo.HaveGoods != 0)
                                                show2DInfo.Page2DShow.有货 = false;
                                            else
                                                show2DInfo.Page2DShow.有货 = true;

                                            switch (locInfo.State)
                                            {
                                                case LocationStatus.Empty:
                                                    show2DInfo.Page2DShow.设备状态 = MonitorDeviceStatus.NotReady;
                                                    break;
                                                case LocationStatus.Alarming:
                                                    show2DInfo.Page2DShow.设备状态 = MonitorDeviceStatus.Alarm;
                                                    break;
                                                case LocationStatus.Offline:
                                                    show2DInfo.Page2DShow.设备状态 = MonitorDeviceStatus.Offline;
                                                    break;
                                                case LocationStatus.Manual:
                                                    show2DInfo.Page2DShow.设备状态 = MonitorDeviceStatus.Manual;
                                                    break;
                                                case LocationStatus.Waitting:
                                                    show2DInfo.Page2DShow.设备状态 = MonitorDeviceStatus.Auto;
                                                    break;
                                                case LocationStatus.Running:
                                                //case LocationStatus.UnloadedRunning:
                                                //case LocationStatus.LoadedRunning:
                                                    show2DInfo.Page2DShow.设备状态 = MonitorDeviceStatus.Running;
                                                    break;
                                                default:
                                                    show2DInfo.Page2DShow.设备状态 = MonitorDeviceStatus.NotReady;
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            show2DInfo.Page2DShow.设备状态 = MonitorDeviceStatus.UnReceivedData;
                                            show2DInfo.Page2DShow.有货 = false;
                                        }

                                        var requestInfo = device.ReadStatus<RequestBlock>().FirstOrDefault(x => x.PosNo == posNo);
                                        if (requestInfo == null)
                                            show2DInfo.Page2DShow.AdditionalInfos.Add(Consts.Request, "false");
                                        else
                                        {
                                            if (requestInfo.HandShake == 0)
                                                show2DInfo.Page2DShow.AdditionalInfos.Add(Consts.Request, "false");
                                            else
                                                show2DInfo.Page2DShow.AdditionalInfos.Add(Consts.Request, "true");
                                        }

                                        var holdSingleInfo = device.ReadStatus<HoldSingleBlock>().FirstOrDefault(x => x.PosNo == posNo);
                                        if (holdSingleInfo == null)
                                            show2DInfo.Page2DShow.AdditionalInfos.Add(Consts.HoldSingle, "false");
                                        else
                                        {
                                            if (holdSingleInfo.HandShake == 0)
                                                show2DInfo.Page2DShow.AdditionalInfos.Add(Consts.HoldSingle, "false");
                                            else
                                                show2DInfo.Page2DShow.AdditionalInfos.Add(Consts.HoldSingle, "true");
                                        }
                                    }
                                    else
                                    {
                                        show2DInfo.Page2DShow.设备状态 = MonitorDeviceStatus.DisConnected;
                                        show2DInfo.Page2DShow.有货 = false;
                                        show2DInfo.Page2DShow.AdditionalInfos.Add(Consts.Request, "false");
                                        show2DInfo.Page2DShow.AdditionalInfos.Add(Consts.HoldSingle, "false");
                                    }
                                    var now = WCSMqttMsgSerializeHelper.SerializeToString(show2DInfo);
                                    //var _now = WCSMqttMsgSerializeHelper.DeserializeToObject<Show2DBasicDeviceInfo>(now);
                                    var topic = $"Wcs/DeviceInfo_2D/Conveyor/{loc.DeviceCode}/Show2DInfo";
                                    Sending(topic, now);
                                }
                                #endregion

                                #region Show2DBasicDeviceMsg
                                Show2DBasicDeviceMsg show2DMsg = new Show2DBasicDeviceMsg();
                                show2DMsg.显示信息 = show2DInfo.Page2DShow;
                                show2DMsg.设备信息.设备名称 = $"{loc.DeviceCode}@{_device.Name}";
                                show2DMsg.设备信息.IP地址 = $"{device.IPEndPoint.ToString()}";
                                string state = "";
                                string linkState = "";
                                string wanings = "";
                                UInt32 TaskNo = 0;
                                if (device.IsConnected)
                                {
                                    linkState = "已连接";
                                    var locInfo = device.ReadStatus<LocationInfoBlock>().FirstOrDefault(x => x.PosNo == posNo);
                                    if (locInfo == null)
                                        state = "NULL";
                                    else
                                    {
                                        state = $"{locInfo.State.GetDescription()}({(int)locInfo.State})";
                                        TaskNo = locInfo.TaskNo;
                                        wanings = string.Join(",", ((IDictionary<string, object>)locInfo.GetAlarmsDynamic()).Where(x => bool.TryParse(x.Value.ToString(), out bool alarm) && alarm).Select(x => x.Key));
                                    }
                                }
                                else
                                {
                                    linkState = "未连接";
                                    state = "-";
                                    wanings = "未连接";
                                }

                                show2DMsg.设备信息.连接状态 = $"{linkState}";
                                show2DMsg.设备信息.设备状态 = $"{state}";
                                show2DMsg.警告信息.警告信息 = $"{wanings}";
                                show2DMsg.任务信息.子任务号 = $"{TaskNo}";

                                if (TaskNo != 0)
                                {
                                    var action = device.EquipmentActionScheduler.Actions.FirstOrDefault(x => x.EquipmentTaskId == TaskNo);
                                    if (action != null)
                                    {
                                        if (action.Movement.Task.ContainerCodes != null)
                                            show2DMsg.任务信息.托盘号 = string.Join(",", action.Movement.Task.ContainerCodes);
                                        show2DMsg.任务信息.子任务起点 = action.Movement.StartLocation.DeviceCode;
                                        show2DMsg.任务信息.子任务终点 = action.Movement.EndLocation.DeviceCode;
                                        show2DMsg.任务信息.主任务类型 = action.Movement.Task.TaskType;
                                        show2DMsg.任务信息.主任务编号 = action.Movement.Task.TaskCode;
                                        show2DMsg.任务信息.主任务起点 = action.Movement.Task.StartLocation.UserCode;
                                        show2DMsg.任务信息.子任务终点 = action.Movement.Task.EndLocation.UserCode;
                                        if (action.Movement.Task.AdditionalInfo != null)
                                            show2DMsg.附加属性.附加属性 = string.Join(",", action.Movement.Task.AdditionalInfo.Select(x => $"{x.Key}={x.Value}"));
                                    }
                                }
                                var _now = WCSMqttMsgSerializeHelper.SerializeToString(show2DMsg);
                                //var __now = WCSMqttMsgSerializeHelper.DeserializeToObject<Show2DBasicDeviceMsg>(_now);

                                var _topic = $"Wcs/DeviceInfo_2D/Conveyor/{loc.DeviceCode}/Show2DMsg";
                                if (vehicles.Contains(posNo)) //输送线托管的穿梭车
                                    _topic = $"Wcs/DeviceInfo_2D/RGV/{loc.DeviceCode}/Show2DMsg";
                                Sending(_topic, _now);
                                #endregion
                            }
                            #region
                            //foreach (var loc in device.Locations)
                            //{
                            //    var item = loc.DeviceCode;
                            //    if (loc is ConveyorLocationWildcard)
                            //        continue;

                            //    ConveyorLocationInfo conveyorLocationInfo = new ConveyorLocationInfo();
                            //    var conveyor = (ConveyorDevice)loc.Device;

                            //conveyorLocationInfo.任务信息.货位编号 = loc.DeviceCode;
                            //conveyorLocationInfo.货位状态.所属设备 = conveyor.Name;
                            //conveyorLocationInfo.货位状态.连接状态 = conveyor.IsConnected ? "已连接" : "未连接";
                            //conveyorLocationInfo.货位状态.锁信息 = !conveyor.Locker.IsEmpty ? "锁定" : "未锁定";
                            //conveyorLocationInfo.货位状态.IPAddress = conveyor.IPEndPoint.Address.ToString() + ":" + conveyor.IPEndPoint.Port;

                            //if (conveyor.IsConnected)
                            //{
                            //    #region 货位任务
                            //    var locCurrentTask = conveyor.LocationCurrentTasks.FirstOrDefault(x => x.PosNo.ToString() == item);
                            //    if (locCurrentTask == null)
                            //    {
                            //        var action = conveyor.EquipmentActionScheduler.Actions.FirstOrDefault(x => x.Movement.Task.CurrentLocation.DeviceCode == item);
                            //        if (action != null)
                            //        {
                            //            conveyorLocationInfo.任务信息.主任务编号 = action.Movement.Task.TaskCode;
                            //            conveyorLocationInfo.任务信息.主任务类型 = action.Movement.Task.TaskType;
                            //            conveyorLocationInfo.任务信息.主任务起点 = action.Movement.Task.StartLocation.UserCode;
                            //            conveyorLocationInfo.任务信息.主任务终点 = action.Movement.Task.EndLocation.UserCode;
                            //            var containerCodes = action.Movement.Task.ContainerCodes == null || action.Movement.Task.ContainerCodes.Count() == 0 || action.Movement.Task.ContainerCodes.All(x => string.IsNullOrWhiteSpace(x)) ? "" : string.Join("|", action.Movement.Task.ContainerCodes.ToArray());
                            //            conveyorLocationInfo.任务信息.母托Id = containerCodes;
                            //            conveyorLocationInfo.任务信息.PalletId = "";
                            //            if (action.Movement.Task.AdditionalInfo.ContainsKey("palletId"))
                            //                conveyorLocationInfo.任务信息.PalletId = action.Movement.Task.AdditionalInfo["palletId"];
                            //        }
                            //        else
                            //        {
                            //            var task = PreTaskHandHelper.Tasks.FirstOrDefault(x => x.CurrentLocation.DeviceCode == loc.DeviceCode);
                            //            if (task != null)
                            //            {
                            //                conveyorLocationInfo.任务信息.主任务编号 = task.TaskCode;
                            //                conveyorLocationInfo.任务信息.主任务类型 = task.TaskType;
                            //                conveyorLocationInfo.任务信息.主任务起点 = task.StartLocation.UserCode;
                            //                conveyorLocationInfo.任务信息.主任务终点 = task.EndLocation.UserCode;
                            //                var containerCodes = task.ContainerCodes == null || task.ContainerCodes.Count() == 0 || task.ContainerCodes.All(x => string.IsNullOrWhiteSpace(x)) ? "" : string.Join("|", task.ContainerCodes.ToArray());
                            //                conveyorLocationInfo.任务信息.母托Id = containerCodes;
                            //                conveyorLocationInfo.任务信息.PalletId = "";
                            //                if (task.AdditionalInfo.ContainsKey("palletId"))
                            //                    conveyorLocationInfo.任务信息.PalletId = task.AdditionalInfo["palletId"];
                            //            }
                            //        }
                            //    }
                            //    else
                            //    {
                            //        conveyorLocationInfo.任务信息.货位任务号 = locCurrentTask.TaskNo.ToString();
                            //        if (locCurrentTask.TaskNo != 0)
                            //        {
                            //            var locTask = conveyor.Tasks.FirstOrDefault(x => x.AssignmentID == locCurrentTask.TaskNo);
                            //            if (locTask != null)
                            //            {
                            //                conveyorLocationInfo.任务信息.子任务起点 = locTask.StartMotorNo.ToString();
                            //                conveyorLocationInfo.任务信息.子任务终点 = locTask.DestinationNo.ToString();
                            //                var action = conveyor.EquipmentActionScheduler.Actions.FirstOrDefault(x => x.EquipmentTaskId == locTask.AssignmentID);
                            //                if (action != null)
                            //                {
                            //                    conveyorLocationInfo.任务信息.主任务编号 = action.Movement.Task.TaskCode;
                            //                    conveyorLocationInfo.任务信息.主任务类型 = action.Movement.Task.TaskType;
                            //                    conveyorLocationInfo.任务信息.主任务起点 = action.Movement.Task.StartLocation.UserCode;
                            //                    conveyorLocationInfo.任务信息.主任务终点 = action.Movement.Task.EndLocation.UserCode;
                            //                    var containerCodes = action.Movement.Task.ContainerCodes == null || action.Movement.Task.ContainerCodes.Count() == 0 || action.Movement.Task.ContainerCodes.All(x => string.IsNullOrWhiteSpace(x)) ? "" : string.Join("|", action.Movement.Task.ContainerCodes.ToArray());
                            //                    conveyorLocationInfo.任务信息.母托Id = containerCodes;
                            //                    conveyorLocationInfo.任务信息.PalletId = "";
                            //                    if (action.Movement.Task.AdditionalInfo.ContainsKey("palletId"))
                            //                        conveyorLocationInfo.任务信息.PalletId = action.Movement.Task.AdditionalInfo["palletId"];
                            //                }
                            //                else
                            //                {
                            //                    action = conveyor.EquipmentActionScheduler.Actions.FirstOrDefault(x => x.Movement.Task.CurrentLocation.DeviceCode == item);
                            //                    if (action != null)
                            //                    {
                            //                        conveyorLocationInfo.任务信息.主任务编号 = action.Movement.Task.TaskCode;
                            //                        conveyorLocationInfo.任务信息.主任务类型 = action.Movement.Task.TaskType;
                            //                        conveyorLocationInfo.任务信息.主任务起点 = action.Movement.Task.StartLocation.UserCode;
                            //                        conveyorLocationInfo.任务信息.主任务终点 = action.Movement.Task.EndLocation.UserCode;
                            //                        var containerCodes = action.Movement.Task.ContainerCodes == null || action.Movement.Task.ContainerCodes.Count() == 0 || action.Movement.Task.ContainerCodes.All(x => string.IsNullOrWhiteSpace(x)) ? "" : string.Join("|", action.Movement.Task.ContainerCodes.ToArray());
                            //                        conveyorLocationInfo.任务信息.母托Id = containerCodes;
                            //                        conveyorLocationInfo.任务信息.PalletId = "";
                            //                        if (action.Movement.Task.AdditionalInfo.ContainsKey("palletId"))
                            //                            conveyorLocationInfo.任务信息.PalletId = action.Movement.Task.AdditionalInfo["palletId"];
                            //                    }
                            //                }
                            //            }
                            //            else
                            //            {
                            //                var action = conveyor.EquipmentActionScheduler.Actions.FirstOrDefault(x => x.Movement.Task.CurrentLocation.DeviceCode == item);
                            //                if (action != null)
                            //                {
                            //                    conveyorLocationInfo.任务信息.主任务编号 = action.Movement.Task.TaskCode;
                            //                    conveyorLocationInfo.任务信息.主任务类型 = action.Movement.Task.TaskType;
                            //                    conveyorLocationInfo.任务信息.主任务起点 = action.Movement.Task.StartLocation.UserCode;
                            //                    conveyorLocationInfo.任务信息.主任务终点 = action.Movement.Task.EndLocation.UserCode;
                            //                    var containerCodes = action.Movement.Task.ContainerCodes == null || action.Movement.Task.ContainerCodes.Count() == 0 || action.Movement.Task.ContainerCodes.All(x => string.IsNullOrWhiteSpace(x)) ? "" : string.Join("|", action.Movement.Task.ContainerCodes.ToArray());
                            //                    conveyorLocationInfo.任务信息.母托Id = containerCodes;
                            //                    conveyorLocationInfo.任务信息.PalletId = "";
                            //                    if (action.Movement.Task.AdditionalInfo.ContainsKey("palletId"))
                            //                        conveyorLocationInfo.任务信息.PalletId = action.Movement.Task.AdditionalInfo["palletId"];
                            //                }
                            //            }
                            //        }
                            //        else
                            //        {
                            //            var action = conveyor.EquipmentActionScheduler.Actions.FirstOrDefault(x => x.Movement.Task.CurrentLocation.DeviceCode == item);
                            //            if (action != null)
                            //            {
                            //                conveyorLocationInfo.任务信息.主任务编号 = action.Movement.Task.TaskCode;
                            //                conveyorLocationInfo.任务信息.主任务类型 = action.Movement.Task.TaskType;
                            //                conveyorLocationInfo.任务信息.主任务起点 = action.Movement.Task.StartLocation.UserCode;
                            //                conveyorLocationInfo.任务信息.主任务终点 = action.Movement.Task.EndLocation.UserCode;
                            //                var containerCodes = action.Movement.Task.ContainerCodes == null || action.Movement.Task.ContainerCodes.Count() == 0 || action.Movement.Task.ContainerCodes.All(x => string.IsNullOrWhiteSpace(x)) ? "" : string.Join("|", action.Movement.Task.ContainerCodes.ToArray());
                            //                conveyorLocationInfo.任务信息.母托Id = containerCodes;
                            //                conveyorLocationInfo.任务信息.PalletId = "";
                            //                if (action.Movement.Task.AdditionalInfo.ContainsKey("palletId"))
                            //                    conveyorLocationInfo.任务信息.PalletId = action.Movement.Task.AdditionalInfo["palletId"];
                            //            }
                            //            else
                            //            {
                            //                var task = PreTaskHandHelper.Tasks.FirstOrDefault(x => x.CurrentLocation.DeviceCode == loc.DeviceCode);
                            //                if (task != null)
                            //                {
                            //                    conveyorLocationInfo.任务信息.主任务编号 = task.TaskCode;
                            //                    conveyorLocationInfo.任务信息.主任务类型 = task.TaskType;
                            //                    conveyorLocationInfo.任务信息.主任务起点 = task.StartLocation.UserCode;
                            //                    conveyorLocationInfo.任务信息.主任务终点 = task.EndLocation.UserCode;
                            //                    var containerCodes = task.ContainerCodes == null || task.ContainerCodes.Count() == 0 || task.ContainerCodes.All(x => string.IsNullOrWhiteSpace(x)) ? "" : string.Join("|", task.ContainerCodes.ToArray());
                            //                    conveyorLocationInfo.任务信息.母托Id = containerCodes;
                            //                    conveyorLocationInfo.任务信息.PalletId = "";
                            //                    if (task.AdditionalInfo.ContainsKey("palletId"))
                            //                        conveyorLocationInfo.任务信息.PalletId = task.AdditionalInfo["palletId"];
                            //                }
                            //            }
                            //        }
                            //    }
                            //    #endregion 货位任务

                            //    #region 货位状态
                            //    var locState = conveyor.ConveyorLocationStates.FirstOrDefault(x => x.PosNo.ToString() == item);
                            //    if (locState == null)
                            //        conveyorLocationInfo.货位状态.货位状态 = ConveyorLocationInfo.MonitorDeviceStatus.Initialize;
                            //    else
                            //    {
                            //        switch (locState.Status)
                            //        {
                            //            case LocationNetTransferObjectStatus.Warning:
                            //                conveyorLocationInfo.货位状态.货位状态 = ConveyorLocationInfo.MonitorDeviceStatus.Warning;
                            //                break;
                            //            case LocationNetTransferObjectStatus.Offline:
                            //                conveyorLocationInfo.货位状态.货位状态 = ConveyorLocationInfo.MonitorDeviceStatus.Offline;
                            //                break;
                            //            case LocationNetTransferObjectStatus.Manual:
                            //                conveyorLocationInfo.货位状态.货位状态 = ConveyorLocationInfo.MonitorDeviceStatus.Manual;
                            //                break;
                            //            case LocationNetTransferObjectStatus.Stopped:
                            //                conveyorLocationInfo.货位状态.货位状态 = ConveyorLocationInfo.MonitorDeviceStatus.Stopped;
                            //                break;
                            //            case LocationNetTransferObjectStatus.Running:
                            //                conveyorLocationInfo.货位状态.货位状态 = ConveyorLocationInfo.MonitorDeviceStatus.Running;
                            //                break;
                            //            case LocationNetTransferObjectStatus.Initialize:
                            //            default:
                            //                conveyorLocationInfo.货位状态.货位状态 = ConveyorLocationInfo.MonitorDeviceStatus.Initialize;
                            //                break;
                            //        }
                            //    }
                            //    #endregion

                            //    #region 光电信息
                            //    var occupyState = conveyor.OccupyStatus.FirstOrDefault(x => x.PosNo.ToString() == item);
                            //    if (occupyState != null)
                            //    {
                            //        conveyorLocationInfo.光电信息.前保护 = occupyState.FroProPotocell.ToString();
                            //        conveyorLocationInfo.光电信息.前到位 = occupyState.FroPosPotocell.ToString();
                            //        conveyorLocationInfo.光电信息.前减速 = occupyState.FroSloPotocell.ToString();
                            //        conveyorLocationInfo.光电信息.后保护 = occupyState.AftProPotocell.ToString();
                            //        conveyorLocationInfo.光电信息.后到位 = occupyState.AftPosPotocell.ToString();
                            //        conveyorLocationInfo.光电信息.后减速 = occupyState.AftSloPotocell.ToString();
                            //        conveyorLocationInfo.光电信息.高位 = occupyState.UpPotocell.ToString();
                            //        conveyorLocationInfo.光电信息.低位 = occupyState.DownPotocell.ToString();
                            //    }
                            //    else
                            //    {
                            //        conveyorLocationInfo.光电信息 = null;
                            //    }
                            //    var _occupyState = conveyor.ReadStatus<GeneralSensorNetTransferObject>().FirstOrDefault(x => x.PosNo.ToString() == item);
                            //    if (_occupyState != null)
                            //        conveyorLocationInfo.传感器信息 = _occupyState.GetDynamicShowName();
                            //    else
                            //        conveyorLocationInfo.传感器信息 = null;
                            //    #endregion

                            //    #region 报警信息
                            //    var alarm = conveyor.ReadStatus<GeneralAlarmNetTransferObject>().FirstOrDefault(x => x.PosNo.ToString() == item);
                            //    if (alarm != null)
                            //    {
                            //        var alarms = alarm.GetAlarm();
                            //        conveyorLocationInfo.报警信息.详细报警信息 = alarms.Length == 0 ? "" : string.Join("|", alarms);
                            //        conveyorLocationInfo.报警信息.其它报警 = device.Warnings == null || device.Warnings.Length == 0 ? "" : string.Join("|", device.Warnings); ;//这里赋值
                            //    }
                            //    else
                            //    {
                            //        conveyorLocationInfo.报警信息.详细报警信息 = "";
                            //        conveyorLocationInfo.报警信息.其它报警 = "";
                            //    }
                            //    #endregion

                            //    #region 复杂占位
                            //    var holdSignal = conveyor.OccupiedSignals.FirstOrDefault(x => x.PosNo.ToString() == item);
                            //    if (holdSignal == null)
                            //        conveyorLocationInfo.复杂占位 = null;
                            //    else
                            //    {
                            //        conveyorLocationInfo.复杂占位.是否有占位 = holdSignal.HandShake.ToString();
                            //        conveyorLocationInfo.复杂占位.IO_DATA = holdSignal.IO_Data.ToString();
                            //        conveyorLocationInfo.复杂占位.DATA_ID = holdSignal.Data_ID.ToString();
                            //    }
                            //    #endregion

                            //    #region 简单占位
                            //    var simpleHoldSignal = conveyor.ReadStatus<SimpleHoldSignalNetTransferObject>().FirstOrDefault(x => x.PosNo.ToString() == item);
                            //    if (simpleHoldSignal == null)
                            //        conveyorLocationInfo.简单占位 = null;
                            //    else
                            //        conveyorLocationInfo.简单占位.是否有占位 = simpleHoldSignal.HandShake.ToString();
                            //    #endregion
                            //}

                            //#region 出入库模式
                            //conveyorLocationInfo.出入库模式 = null;
                            //#endregion

                            ////var now = JsonConvert.SerializeObject(conveyorLocationInfo);
                            //var now = WCSMqttMsgSerializeHelper.SerializeToString(conveyorLocationInfo);
                            //var topic = $"Wcs/DeviceInfo_2D/Conveyor/{loc.DeviceCode}";
                            //Sending(topic, now);

                            //}
                            #endregion
                        }
                        #endregion
                        #region crane  堆垛机
                        if (_device is CraneDevice)
                        {
                            var device = (CraneDevice)_device;
                            #region Show2DBasicDeviceInfo
                            Show2DBasicDeviceInfo show2DInfo = new Show2DBasicDeviceInfo();
                            if (device.IsConnected)
                            {
                                if (device.LastStatus != null)
                                {
                                    switch (device.LastStatus.CraneWorkModel)
                                    {
                                        case CraneWorkModels.Unknown:
                                        case CraneWorkModels.Repair:
                                            show2DInfo.Page2DShow.设备状态 = MonitorDeviceStatus.NotReady;
                                            break;
                                        case CraneWorkModels.Auto:
                                            switch (device.LastStatus.DeviceState)
                                            {
                                                case CraneStatus.Initilization:
                                                case CraneStatus.CancelRemoteEmergency:
                                                    show2DInfo.Page2DShow.设备状态 = MonitorDeviceStatus.NotReady;
                                                    break;
                                                case CraneStatus.Watting:
                                                    show2DInfo.Page2DShow.设备状态 = MonitorDeviceStatus.Auto;
                                                    break;
                                                case CraneStatus.Running:
                                                    show2DInfo.Page2DShow.设备状态 = MonitorDeviceStatus.Running;
                                                    break;
                                                case CraneStatus.AlarmDown:
                                                case CraneStatus.RemoteEmergency:
                                                    show2DInfo.Page2DShow.设备状态 = MonitorDeviceStatus.Alarm;
                                                    break;
                                                default:
                                                    show2DInfo.Page2DShow.设备状态 = MonitorDeviceStatus.NotReady;
                                                    break;
                                            }
                                            break;
                                        case CraneWorkModels.Manual:
                                        case CraneWorkModels.SemiAuto:
                                            show2DInfo.Page2DShow.设备状态 = MonitorDeviceStatus.Manual;
                                            break;
                                        default:
                                            show2DInfo.Page2DShow.设备状态 = MonitorDeviceStatus.NotReady;
                                            break;
                                    }

                                    if (device.LastStatus.IsLoaded == 1)
                                        show2DInfo.Page2DShow.有货 = true;
                                    else
                                        show2DInfo.Page2DShow.有货 = false;

                                    show2DInfo.Page2DShow.AdditionalInfos.Add(Consts.LocationInfo, $"{device.LastStatus.XColumn}|{device.LastStatus.YLevel}");
                                }
                                else
                                {
                                    show2DInfo.Page2DShow.设备状态 = MonitorDeviceStatus.UnReceivedData;
                                    show2DInfo.Page2DShow.有货 = false;
                                    show2DInfo.Page2DShow.AdditionalInfos.Add(Consts.LocationInfo, "-1|-1");
                                }
                            }
                            else
                            {
                                show2DInfo.Page2DShow.设备状态 = MonitorDeviceStatus.DisConnected;
                                show2DInfo.Page2DShow.有货 = false;
                                show2DInfo.Page2DShow.AdditionalInfos.Add(Consts.LocationInfo, "-1|-1");
                            }
                            var now = WCSMqttMsgSerializeHelper.SerializeToString(show2DInfo);
                            var topic = $"Wcs/DeviceInfo_2D/Crane/{device.Name}/Show2DInfo";
                            Sending(topic, now);
                            #endregion

                            #region Show2DBasicDeviceMsg
                            Show2DBasicDeviceMsg show2DMsg = new Show2DBasicDeviceMsg();
                            show2DMsg.显示信息 = show2DInfo.Page2DShow;
                            show2DMsg.设备信息.设备名称 = $"{device.Name}";
                            show2DMsg.设备信息.IP地址 = $"{device.IPEndPoint.ToString()}";
                            string state = "";
                            string linkState = "";
                            string wanings = "";
                            if (device.IsConnected)
                            {
                                linkState = "已连接";
                                if (device.LastStatus == null)
                                    state = "NULL";
                                else
                                {
                                    state = $"工作模式：{device.LastStatus.CraneWorkModel.GetDescription()}({(int)device.LastStatus.CraneWorkModel})\r\n设备状态：{device.LastStatus.DeviceState.GetDescription()}({(int)device.LastStatus.DeviceState})";
                                    wanings = string.Join(",", device.Warnings);
                                }
                            }
                            else
                            {
                                linkState = "未连接";
                                state = "-";
                                wanings = "未连接";
                            }

                            show2DMsg.设备信息.连接状态 = $"{linkState}";
                            show2DMsg.设备信息.设备状态 = $"{state}";
                            show2DMsg.警告信息.警告信息 = $"{wanings}";

                            var action = device.EquipmentActionScheduler.Actions.FirstOrDefault(x => x is CraneAutomaticTransferWithStepByStepAction && x.Status != EquipmentActionStatus.Cancelled && x.Status != EquipmentActionStatus.Completed && ((CraneAutomaticTransferWithStepByStepAction)x).StateManager != null);
                            if (action != null)
                            {
                                show2DMsg.任务信息.子任务号 = $"{action.EquipmentTaskId}";
                                if (action.Movement.Task.ContainerCodes != null)
                                    show2DMsg.任务信息.托盘号 = string.Join(",", action.Movement.Task.ContainerCodes);
                                show2DMsg.任务信息.子任务起点 = action.Movement.StartLocation.DeviceCode;
                                show2DMsg.任务信息.子任务终点 = action.Movement.EndLocation.DeviceCode;
                                show2DMsg.任务信息.主任务类型 = action.Movement.Task.TaskType;
                                show2DMsg.任务信息.主任务编号 = action.Movement.Task.TaskCode;
                                show2DMsg.任务信息.主任务起点 = action.Movement.Task.StartLocation.UserCode;
                                show2DMsg.任务信息.子任务终点 = action.Movement.Task.EndLocation.UserCode;
                                if (action.Movement.Task.AdditionalInfo != null)
                                    show2DMsg.附加属性.附加属性 = string.Join(",", action.Movement.Task.AdditionalInfo.Select(x => $"{x.Key}={x.Value}"));
                            }
                            var _now = WCSMqttMsgSerializeHelper.SerializeToString(show2DMsg);
                            //var __now = WCSMqttMsgSerializeHelper.DeserializeToObject<Show2DBasicDeviceMsg>(_now);
                            var _topic = $"Wcs/DeviceInfo_2D/Crane/{device.Name}/Show2DMsg";
                            Sending(_topic, _now);
                            #endregion
                            #region old
                            //CraneInfo craneInfo = new CraneInfo();

                            //craneInfo.任务信息 = new CraneInfo._任务信息();
                            //var action = device.EquipmentActionScheduler.Actions.FirstOrDefault(x => x is CraneAutomaticTransferWithStepByStepAction && x.Status != EquipmentActionStatus.Cancelled && x.Status != EquipmentActionStatus.Completed && ((CraneAutomaticTransferWithStepByStepAction)x).StateManager != null);
                            //if (action == null)
                            //{
                            //    craneInfo.任务信息.执行进度 = "";
                            //    craneInfo.任务信息.子任务起点 = "";
                            //    craneInfo.任务信息.子任务终点 = "";
                            //    craneInfo.任务信息.主任务编号 = "";
                            //    craneInfo.任务信息.主任务类型 = "";
                            //    craneInfo.任务信息.主任务起点 = "";
                            //    craneInfo.任务信息.主任务终点 = "";
                            //    craneInfo.任务信息.母托Id = "";
                            //    craneInfo.任务信息.PalletId = "";
                            //}
                            //else
                            //{
                            //    var sm = ((CraneAutomaticTransferWithStepByStepAction)action).StateManager;
                            //    craneInfo.任务信息.执行进度 = sm.CurrentState.Name;
                            //    craneInfo.任务信息.子任务起点 = action.Movement.StartLocation.UserCode;
                            //    craneInfo.任务信息.子任务终点 = action.Movement.EndLocation.UserCode;
                            //    craneInfo.任务信息.主任务编号 = action.Movement.Task.TaskCode;
                            //    craneInfo.任务信息.主任务类型 = action.Movement.Task.TaskType;
                            //    craneInfo.任务信息.主任务起点 = action.Movement.Task.StartLocation.UserCode;
                            //    craneInfo.任务信息.主任务终点 = action.Movement.Task.EndLocation.UserCode;
                            //    string barcodes = "";
                            //    if (action.Movement.Task.ContainerCodes != null && action.Movement.Task.ContainerCodes.Count() > 0)
                            //        barcodes = string.Join("|", action.Movement.Task.ContainerCodes);

                            //    craneInfo.任务信息.母托Id = barcodes;
                            //    craneInfo.任务信息.PalletId = "";
                            //    if (action.Movement.Task.AdditionalInfo.ContainsKey("palletId"))
                            //        craneInfo.任务信息.PalletId = action.Movement.Task.AdditionalInfo["palletId"];
                            //}

                            //craneInfo.设备信息 = new CraneInfo._设备信息();
                            //craneInfo.设备信息.连接状态 = device.IsConnected ? "已连接" : "未连接";
                            //craneInfo.设备信息.锁信息 = !device.Locker.IsEmpty ? "锁定" : "未锁定";
                            //craneInfo.设备信息.IPAddress = device.IPEndPoint.Address.ToString() + ":" + device.IPEndPoint.Port;
                            //craneInfo.设备信息.巷道 = device.No;

                            //craneInfo.系统警告 = new CraneInfo._系统警告();
                            //craneInfo.系统警告.系统警告 = device.SystemWarnings.Length == 0 ? "" : string.Join(" | ", device.SystemWarnings);

                            //craneInfo.设备报警 = new CraneInfo._设备报警();
                            //if (!device.IsConnected)
                            //{
                            //    craneInfo.任务信息.设备任务号 = "";

                            //    craneInfo.设备信息.状态 = CraneStatus.Initialized.GetDescription();
                            //    craneInfo.设备信息.所在列 = 0;
                            //    craneInfo.设备信息.列编码值 = 0;
                            //    craneInfo.设备信息.所在层 = 0;
                            //    craneInfo.设备信息.层编码值 = 0;
                            //    craneInfo.设备信息.货叉水平位置 = "";
                            //    craneInfo.设备信息.Z1编码值 = 0;
                            //    craneInfo.设备信息.Z2编码值 = 0;
                            //    craneInfo.设备信息.货叉高低位置 = "";
                            //    craneInfo.设备信息.是否在站 = false;
                            //    craneInfo.设备信息.事件 = CraneEvent.Initialized.GetDescription();
                            //    //craneInfo.设备信息.托盘条码 = "";
                            //    //craneInfo.设备信息.验证条码 = "";
                            //    //craneInfo.设备信息.行走方向 = "";
                            //    //craneInfo.设备信息.升降方向 = "";
                            //    //craneInfo.设备信息.货叉运行方向 = "";
                            //    craneInfo.设备报警.设备报警 = "-";
                            //    craneInfo.系统警告.系统警告 = "未连接";
                            //}
                            //else
                            //{
                            //    if (device.LastStatus == null)
                            //    {
                            //        craneInfo.任务信息.设备任务号 = "";

                            //        craneInfo.设备信息.状态 = CraneStatus.Initialized.GetDescription();
                            //        craneInfo.设备信息.所在列 = 0;
                            //        craneInfo.设备信息.列编码值 = 0;
                            //        craneInfo.设备信息.所在层 = 0;
                            //        craneInfo.设备信息.层编码值 = 0;
                            //        craneInfo.设备信息.货叉水平位置 = "";
                            //        craneInfo.设备信息.Z1编码值 = 0;
                            //        craneInfo.设备信息.Z2编码值 = 0;
                            //        craneInfo.设备信息.货叉高低位置 = "";
                            //        craneInfo.设备信息.是否在站 = false;
                            //        craneInfo.设备信息.事件 = CraneEvent.Initialized.GetDescription();
                            //        //craneInfo.设备信息.托盘条码 = "";
                            //        //craneInfo.设备信息.验证条码 = "";
                            //        //craneInfo.设备信息.行走方向 = "";
                            //        //craneInfo.设备信息.升降方向 = "";
                            //        //craneInfo.设备信息.货叉运行方向 = "";

                            //        craneInfo.设备报警.设备报警 = "-";
                            //    }
                            //    else
                            //    {
                            //        craneInfo.任务信息.设备任务号 = device.LastStatus.TaskId;

                            //        craneInfo.设备信息.状态 = device.LastStatus.State.GetDescription();
                            //        craneInfo.设备信息.所在列 = device.LastStatus.Column;
                            //        craneInfo.设备信息.列编码值 = device.LastStatus.ColumnCodeValue;
                            //        craneInfo.设备信息.所在层 = device.LastStatus.Level;
                            //        craneInfo.设备信息.层编码值 = device.LastStatus.LevelCodeValue;
                            //        craneInfo.设备信息.货叉水平位置 = device.LastStatus.ForkHorizontalPosition.GetDescription();
                            //        craneInfo.设备信息.Z1编码值 = device.LastStatus.ForkCodeValue_Z1;
                            //        craneInfo.设备信息.Z2编码值 = device.LastStatus.ForkCodeValue_Z2;
                            //        craneInfo.设备信息.货叉高低位置 = device.LastStatus.ForkVerticalPosition.GetDescription();
                            //        craneInfo.设备信息.是否在站 = device.LastStatus.AtStation;
                            //        craneInfo.设备信息.事件 = device.LastStatus.Event.GetDescription();
                            //        //craneInfo.设备信息.托盘条码 = device.LastStatus.Barcode;
                            //        //craneInfo.设备信息.验证条码 = device.LastStatus.Check_Barcode;
                            //        //craneInfo.设备信息.行走方向 = device.LastStatus.WalkDirection.ToString();
                            //        //craneInfo.设备信息.升降方向 = device.LastStatus.LiftDirection.ToString();
                            //        //craneInfo.设备信息.货叉运行方向 = device.LastStatus.ForkDirection.ToString();

                            //        craneInfo.设备报警.设备报警 = device.DeviceAlarms.Length == 0 ? "" : string.Join("|", device.DeviceAlarms);
                            //    }
                            //}

                            ////var now = JsonConvert.SerializeObject(craneInfo);
                            //var now = WCSMqttMsgSerializeHelper.SerializeToString(craneInfo);
                            //var topic = $"Wcs/DeviceInfo_2D/Crane/{device.Name}";
                            //Sending(topic, now);
                            ////var last = RedisHelperFacotry.DeviceMsgRedis.StringGet(dev);
                            ////if (last == null || now != last)
                            ////    RedisHelperFacotry.DeviceMsgRedis.StringSet(dev, now);
                            #endregion
                        }
                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, this);
                }
            }
        }

        private async void Sending(string topic, string da)
        {
            try
            {
                #region 写入questdb供后期监控回溯使用
                //var task = new TaskFactory().StartNew(() =>
                //{
                //    try
                //    {
                //        var msg = $"('{DateTime.Now.ToString("yyyyMMddHHmmss")}', '{topic}', '{da}')";
                //        Monitor_2DHelper.PushWaitSaveTaskList(msg);
                //    }
                //    catch (Exception ex)
                //    {
                //        _logger.Error1(ex, this);
                //    }
                //});
                #endregion

                if (lastsend.ContainsKey(topic) && lastsend[topic].Msg == da)
                    return;

                var obj = new MqttMsg() { Msg = da };
                var _da = JsonConvert.SerializeObject(obj);
                await mqttClient.SendMqttMsgAsync(topic, _da, true);
                if (lastsend.ContainsKey(topic))
                    lastsend[topic] = obj;
                else
                    lastsend.Add(topic, obj);
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                //暂时抛出异常
            }
        }
    }
}