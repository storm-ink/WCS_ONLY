using DevExpress.Data.Mask;
using Newtonsoft.Json;
using NLog;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wcs;
using Wcs.DefaultImplementCollection.Crane;
using Wcs.Framework;
using Wcs.Framework.Cfg;
using static DevExpress.XtraBars.Docking2010.Views.BaseRegistrator;

namespace ZHQXC.Statistics
{
    public class CraneStateStatistics : ThreadRunningLog, IApplicationStartup
    {
        static Thread _thread;
        static Thread _threadStatistic;
        string _device;
        Logger _logger;
        public void Initialize(StartupElement element)
        {
            _device = element.GetAttributeOrDefault("device", "");
        }

        public void Run(IWcsApplication application)
        {
            if (string.IsNullOrWhiteSpace(_device))
                return;

            this.Init($"{_device} OEE统计线程");
            _logger = LogManager.GetCurrentClassLogger();
            ParameterizedThreadStart start = new ParameterizedThreadStart(check);
            _thread = new Thread(start);
            _thread.IsBackground = true;
            _thread.Start();

            _logger.Info1($"{_device} OEE数据采集线程已经启动", this);

            ParameterizedThreadStart start0 = new ParameterizedThreadStart(check0);
            _threadStatistic = new Thread(start0);
            _threadStatistic.IsBackground = true;
            _threadStatistic.Start();

            _logger.Info1($"{_device} OEE数据统计线程已经启动", this);
        }

        private void check(object obj)
        {
            CraneDevice device = null;
            try
            {
                device = DeviceConverter.ToDevice<CraneDevice>(_device);
                this.Log($"开始进行 {_device} 数据采集工作");
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                return;
            }
            StatisticHelper.DeleteDeviceList.Add(_device);

            string lastTableName = "";
            DeviceState lastDeviceState = null;
            while (true)
            {
                Thread.Sleep(1000);
                this.Log("start");
                try
                {
                    var currentTableName = CurrentTableName;
                    if (lastTableName != currentTableName)
                    {
                        var sql = $"CREATE TABLE IF NOT EXISTS {currentTableName}(Device STRING, LastState STRING, LastTimespan STRING, State STRING, Timespan STRING, TotalSeconds STRING, Mark0 STRING, Mark1 STRING, Mark2 STRING);";
                        if (StatisticHelper.IsAnyTableFalseCreate(currentTableName, sql))
                            lastTableName = currentTableName;
                        else
                        {
                            this.Log($"数据采集表未创建成功，系统会稍后重试");
                            Thread.Sleep(5000);
                            continue;
                        }
                    }

                    if (lastDeviceState == null)
                    {
                        lastDeviceState = new DeviceState();
                        var sql = $"select * FROM {currentTableName} order by Timespan DESC LIMIT 1";
                        ExpandoObject last = null;
                        using (SqlSugarUtil_QuestDB sqlSugarUtil_QuestDB = new SqlSugarUtil_QuestDB())
                        {
                            last = sqlSugarUtil_QuestDB.Appdb.Ado.SqlQuerySingle<ExpandoObject>(sql);
                            sqlSugarUtil_QuestDB.Dispose();
                        }
                        if (last != null)
                        {
                            var dic = (IDictionary<string, object>)last;
                            foreach (var item in dic)
                            {
                                lastDeviceState[item.Key] = item.Value;
                            }
                        }
                        else
                            lastDeviceState = null;
                    }

                    if (!device.IsConnected)
                    {
                        StatisticHelper.TrySave(_device, DeviceStatus.DISCONNECTED, "device disConnected", ref lastDeviceState, currentTableName);
                        continue;
                    }

                    if (device.LastStatus == null)
                    {
                        StatisticHelper.TrySave(_device, DeviceStatus.NORECEIVEDDATA, "device lastStatus is null", ref lastDeviceState, currentTableName);
                        continue;
                    }


                    //switch (device.LastStatus.DeviceState)
                    //{
                    //    case CraneStatus.Initilization:
                    //        StatisticHelper.TrySave(_device, DeviceStatus.Abnormal, device.LastStatus.DeviceState.ToString(), ref lastDeviceState, currentTableName);
                    //        break;
                    //    case CraneStatus.Watting:
                    //        StatisticHelper.TrySave(_device, DeviceStatus.IDLE, device.LastStatus.DeviceState.ToString(), ref lastDeviceState, currentTableName);
                    //        break;
                    //    case CraneStatus.Running:
                    //        StatisticHelper.TrySave(_device, DeviceStatus.MOVING, device.LastStatus.DeviceState.ToString(), ref lastDeviceState, currentTableName);
                    //        break;
                    //    case CraneStatus.AlarmDown:
                    //        StatisticHelper.TrySave(_device, DeviceStatus.IDLE, device.LastStatus.DeviceState.ToString(), ref lastDeviceState, currentTableName);
                    //        break;
                    //    case CraneStatus.RemoteEmergency:
                    //        StatisticHelper.TrySave(_device, DeviceStatus.IDLE, device.LastStatus.DeviceState.ToString(), ref lastDeviceState, currentTableName);
                    //        break;
                    //    case CraneStatus.CancelRemoteEmergency:
                    //        StatisticHelper.TrySave(_device, DeviceStatus.IDLE, device.LastStatus.DeviceState.ToString(), ref lastDeviceState, currentTableName);
                    //        break;
                    //    default:
                    //        StatisticHelper.TrySave(_device, DeviceStatus.MANUALOROFFLINE, device.LastStatus.DeviceState.ToString(), ref lastDeviceState, currentTableName);
                    //        break;
                    //}
                }
                catch (Exception ex)
                {
                    this.Log($"{ex}");
                    _logger.Error1(ex, this);
                }
            }
        }

        private string CurrentTableName
        {
            get
            {
                return $"{_device}_{DateTime.Now.ToString("yyyyMMdd")}";
            }
        }

        //CREATE TABLE DEVICE_STATE_STATISTICS(DEVICE STRING, STATISTICSTYPE STRING, TIMESPAN STRING, RESULT STRING)
        private void check0(object obj)
        {
            List<string> handCompeleted = new List<string>(); 
            while (true)
            {
                int _interval = 1000 * 60 * 30;
                //_interval = 1000;
                this.Log($"休眠 {_interval} ms后开始执行 {_device} 的数据统计操作");
                Thread.Sleep(_interval);
                this.Log($"start");
                try
                {
                    this.Log("开始进行统计操作，统计分为每日统计和每日24小时统计");

                    if (StatisticHelper.IsAnyTableFalseCreate(StatisticHelper.StatisticTableName, StatisticHelper.StatisticTableCreateSql))
                    {
                        var showDeviceStatistics_day = new List<ShowDeviceStatistics>();
                        var showDeviceStatistics_hour = new List<ShowDeviceStatistics>();

                        List<string> days = new List<string>();
                        var tableNames = StatisticHelper.QuestDBTableNames;
                        foreach (var item in tableNames)
                        {
                            var items = item.Split('_').ToArray();
                            if (items[0] != _device) //非当前设备不统计
                                continue;
                            var day = items[1];
                            var sql = $"select count(*) total from DEVICE_STATE_STATISTICS WHERE DEVICE='{_device}' AND TIMESPAN = '{day}'";
                            List<ExpandoObject> results;
                            using (SqlSugarUtil_QuestDB sqlSugarUtil_QuestDB = new SqlSugarUtil_QuestDB())
                            {
                                results = sqlSugarUtil_QuestDB.Appdb.Ado.SqlQuery<ExpandoObject>(sql);
                                sqlSugarUtil_QuestDB.Dispose();
                            }
                            if (results != null)
                            {
                                var result = results.FirstOrDefault();
                                if (result != null)
                                {
                                    var result_dic = (IDictionary<string, object>)result;
                                    if (int.TryParse(result_dic["total"].ToString(), out int total) && total > 0) //已统计过的不再统计
                                    {
                                        handCompeleted.Add(item);
                                        continue;
                                    }
                                }
                            }
                            if (day == DateTime.Now.ToString("yyyyMMdd"))
                                continue;
                            days.Add(day);
                        }
                        if (days.Count() > 0)
                        {
                            days = days.OrderBy(x => long.Parse(x)).ToList();
                            Dictionary<string, string> tables = new Dictionary<string, string>();
                            foreach (var day in days)
                            {
                                tables.Add($"{_device}_{day}", day);
                            }

                            List<DeviceSateStatistics> deviceSateStatistics = new List<DeviceSateStatistics>();
                            foreach (var table in tables)
                            {
                                if (StatisticHelper.IsAnyTable(table.Key))
                                {
                                    ISugarQueryable<ExpandoObject> datas;
                                    using (SqlSugarUtil_QuestDB sqlSugarUtil_QuestDB = new SqlSugarUtil_QuestDB())
                                    {
                                        datas = sqlSugarUtil_QuestDB.Appdb.Queryable(table.Key, table.Key);
                                        sqlSugarUtil_QuestDB.Dispose();
                                    }
                                    if (datas.Count() > 0)
                                    {
                                        List<DeviceState> deviceStates = new List<DeviceState>();
                                        foreach (var expandoObject in datas.ToList())
                                        {
                                            DeviceState deviceState = new DeviceState();
                                            var _obj = (IDictionary<string, object>)expandoObject;
                                            foreach (var item in _obj)
                                            {
                                                deviceState[item.Key] = item.Value;
                                            }

                                            deviceStates.Add(deviceState);
                                        }
                                        #region 计算稼动率_按天计算
                                        CalOEEAtDay(table, deviceStates, _device, ref deviceSateStatistics);
                                        #endregion

                                        #region 计算稼动率_按小时计算
                                        CalOEEAtHour(table, deviceStates, _device, ref deviceSateStatistics);
                                        #endregion

                                        if (deviceSateStatistics.Count() > 0)
                                        {
                                            if (StatisticHelper.TrySave_DeviceStateSatistic(deviceSateStatistics))
                                                handCompeleted.Add(table.Key);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Log($"{ex}");
                    _logger.Error1(ex, this);
                }
            }
        }

        private void CalOEEAtHour(KeyValuePair<string, string> table, List<DeviceState> list, string device, ref List<DeviceSateStatistics> deviceSateStatistics)
        {
            for (int hour = 0; hour < 24; hour++)
            {
                var deviceStates = list.Where(x => x.LastTimespan.Hour >= hour && x.LastTimespan.Hour < hour + 1).OrderBy(x => x.Timespan).ToList();
                if (hour != 0)//此时需要修正一下deviceStates
                {
                    var first = list.Where(x => x.LastTimespan.Hour < hour).OrderBy(x => x.Timespan).LastOrDefault();
                    if (first != null && first.Timespan.Hour == hour && (first.Timespan.Minute > 0 || first.Timespan.Second > 0))
                    {
                        var _first = new DeviceState() { Device = first.Device, LastState = first.LastState, LastTimespan = DateTime.Parse(first.LastTimespan.AddHours(1).ToString("yyyy-MM-dd HH:00:00")), State = first.State, Timespan = first.Timespan, Mark0 = first.Mark0, Mark1 = first.Mark1, Mark2 = first.Mark2 };
                        _first.TotalSeconds = (_first.Timespan - first.LastTimespan).TotalSeconds;

                        deviceStates = new List<DeviceState> { _first }.Concat(deviceStates).ToList();
                    }
                }
                var last = deviceStates.LastOrDefault();

                var total = new TimeSpan(1, 0, 0);
                var startAt = $"{table.Value.Substring(0, 4)}-{table.Value.Substring(4, 2)}-{table.Value.Substring(6, 2)} {hour.ToString("00")}:00:00";
                var _startAt = DateTime.Parse(startAt);
                var endAt = $"{_startAt.AddHours(1).ToString("yyyy-MM-dd HH")}:00:00";

                DeviceState lastdeviceState = null;
                #region runningRate
                var _deviceStates = deviceStates.Where(x => x.LastState == DeviceStatus.MOVING || x.LastState == DeviceStatus.PICKORPUT).OrderBy(x => x.Timespan).ToArray();
                if (last != null && (last.State == DeviceStatus.MOVING || last.State == DeviceStatus.PICKORPUT))
                    lastdeviceState = last;
                CalTotalTimespan(_deviceStates, lastdeviceState, endAt, _startAt, out TimeSpan totalTimespam_running);
                var rate_running = Math.Round((totalTimespam_running.TotalSeconds / total.TotalSeconds) * 100, 2);
                #endregion

                #region alarmRate
                lastdeviceState = null;
                _deviceStates = deviceStates.Where(x => x.LastState == DeviceStatus.Alarm).OrderBy(x => x.Timespan).ToArray();
                if (last != null && last.State == DeviceStatus.Alarm)
                    lastdeviceState = last;
                CalTotalTimespan(_deviceStates, lastdeviceState, endAt, _startAt, out TimeSpan totalTimespam_alarm);
                var rate_alarm = Math.Round((totalTimespam_alarm.TotalSeconds / total.TotalSeconds) * 100, 2);
                var rate_unAlarm = Math.Round(100 - (totalTimespam_alarm.TotalSeconds / total.TotalSeconds) * 100, 2);
                #endregion

                #region repairRate
                lastdeviceState = null;
                _deviceStates = deviceStates.Where(x => x.LastState == DeviceStatus.REPAIR).OrderBy(x => x.Timespan).ToArray();
                if (last != null && last.State == DeviceStatus.REPAIR)
                    lastdeviceState = last;
                CalTotalTimespan(_deviceStates, lastdeviceState, endAt, _startAt, out TimeSpan totalTimespam_repair);
                var rate_repair = Math.Round((totalTimespam_repair.TotalSeconds / total.TotalSeconds) * 100, 2);
                #endregion

                #region idleRate
                lastdeviceState = null;
                _deviceStates = deviceStates.Where(x => x.LastState == DeviceStatus.IDLE).OrderBy(x => x.Timespan).ToArray();
                if (last != null && last.State == DeviceStatus.IDLE)
                    lastdeviceState = last;
                CalTotalTimespan(_deviceStates, lastdeviceState, endAt, _startAt, out TimeSpan totalTimespam_idle);
                var rate_idle = Math.Round((totalTimespam_idle.TotalSeconds / total.TotalSeconds) * 100, 2);
                #endregion

                #region disconnectedRate
                lastdeviceState = null;
                _deviceStates = deviceStates.Where(x => x.LastState == DeviceStatus.DISCONNECTED).OrderBy(x => x.Timespan).ToArray();
                if (last != null && last.State == DeviceStatus.DISCONNECTED)
                    lastdeviceState = last;
                CalTotalTimespan(_deviceStates, lastdeviceState, endAt, _startAt, out TimeSpan totalTimespam_disconnected);
                var rate_disconnected = Math.Round((totalTimespam_disconnected.TotalSeconds / total.TotalSeconds) * 100, 2);
                #endregion

                #region manualRate
                lastdeviceState = null;
                _deviceStates = deviceStates.Where(x => x.LastState == DeviceStatus.MANUALOROFFLINE).OrderBy(x => x.Timespan).ToArray();
                if (last != null && last.State == DeviceStatus.MANUALOROFFLINE)
                    lastdeviceState = last;
                CalTotalTimespan(_deviceStates, lastdeviceState, endAt, _startAt, out TimeSpan totalTimespam_manual);
                var rate_manual = Math.Round((totalTimespam_manual.TotalSeconds / total.TotalSeconds) * 100, 2);
                #endregion

                var activation = Math.Round(totalTimespam_running.TotalSeconds / (total.TotalSeconds - totalTimespam_manual.TotalSeconds - totalTimespam_disconnected.TotalSeconds - totalTimespam_alarm.TotalSeconds - totalTimespam_repair.TotalSeconds) * 100, 2);
                var rate_ok = Math.Round(((total.TotalSeconds - totalTimespam_alarm.TotalSeconds - totalTimespam_disconnected.TotalSeconds) / total.TotalSeconds) * 100, 2);
                var showDeviceStatistics_hour = new ShowDeviceStatistics()
                {
                    Item = table.Key,
                    StartAt = startAt,
                    EndAt = endAt,
                    Total = total.ToString(),
                    RunningValue = $"{totalTimespam_running}",
                    RunningRate = $"{rate_running}%",
                    AlarmValue = $"{totalTimespam_alarm}",
                    AlarmRate = $"{rate_alarm}%",
                    UnAlarmRate = $"{rate_unAlarm}%",
                    RepairValue = $"{totalTimespam_repair}",
                    RepairRate = $"{rate_repair}%",
                    IdleValue = $"{totalTimespam_idle}",
                    IdleRate = $"{rate_idle}%",
                    OKRate = $"{rate_ok}%",
                    DisconnectedValue = $"{totalTimespam_disconnected}",
                    DisconnectedRate = $"{rate_disconnected}%",
                    ManualValue = $"{totalTimespam_manual}",
                    ManualRate = $"{rate_manual}%",
                    Activation = $"{activation}%",
                    Mark = $"统计成功 {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}"
                };
                deviceSateStatistics.Add(new DeviceSateStatistics()
                {
                    Device = device,
                    DeviceType = (DeviceConverter.ToDevice<TcpProtocolTaskableDevice>(device)).GetDeviceType(),
                    StatisticsType = "HOUR",
                    TimeSpan = table.Value,
                    Result = JsonConvert.SerializeObject(showDeviceStatistics_hour)
                });
            }
        }

        private void CalOEEAtDay(KeyValuePair<string, string> table, List<DeviceState> deviceStates, string device, ref List<DeviceSateStatistics> deviceSateStatistics)
        {
            deviceStates = deviceStates.OrderBy(x => x.Timespan).ToList();
            var first = deviceStates.FirstOrDefault();
            var last = deviceStates.LastOrDefault();

            var total = new TimeSpan(24, 0, 0);
            var startAt = $"{table.Value.Substring(0, 4)}-{table.Value.Substring(4, 2)}-{table.Value.Substring(6, 2)} 00:00:00";
            var _startAt = DateTime.Parse(startAt);
            var endAt = $"{_startAt.AddDays(1).ToString("yyyy-MM-dd 00:00:00")}";

            DeviceState lastdeviceState = null;
            #region runningRate
            var _deviceStates = deviceStates.Where(x => x.LastState == DeviceStatus.MOVING || x.LastState == DeviceStatus.PICKORPUT).OrderBy(x => x.Timespan).ToArray();
            if (last != null && (last.State == DeviceStatus.MOVING || last.State == DeviceStatus.PICKORPUT))
                lastdeviceState = last;
            CalTotalTimespan(_deviceStates, lastdeviceState, endAt, _startAt, out TimeSpan totalTimespam_running);
            var rate_running = Math.Round((totalTimespam_running.TotalSeconds / total.TotalSeconds) * 100, 2);
            #endregion

            #region alarmRate
            lastdeviceState = null;
            _deviceStates = deviceStates.Where(x => x.LastState == DeviceStatus.Alarm).OrderBy(x => x.Timespan).ToArray();
            if (last != null && last.State == DeviceStatus.Alarm)
                lastdeviceState = last;
            CalTotalTimespan(_deviceStates, lastdeviceState, endAt, _startAt, out TimeSpan totalTimespam_alarm);
            var rate_alarm = Math.Round((totalTimespam_alarm.TotalSeconds / total.TotalSeconds) * 100, 2);
            var rate_unAlarm = Math.Round(100 - (totalTimespam_alarm.TotalSeconds / total.TotalSeconds) * 100, 2);
            #endregion

            #region repairRate
            lastdeviceState = null;
            _deviceStates = deviceStates.Where(x => x.LastState == DeviceStatus.REPAIR).OrderBy(x => x.Timespan).ToArray();
            if (last != null && last.State == DeviceStatus.REPAIR)
                lastdeviceState = last;
            CalTotalTimespan(_deviceStates, lastdeviceState, endAt, _startAt, out TimeSpan totalTimespam_repair);
            var rate_repair = Math.Round((totalTimespam_repair.TotalSeconds / total.TotalSeconds) * 100, 2);
            #endregion

            #region idleRate
            lastdeviceState = null;
            _deviceStates = deviceStates.Where(x => x.LastState == DeviceStatus.IDLE).OrderBy(x => x.Timespan).ToArray();
            if (last != null && last.State == DeviceStatus.IDLE)
                lastdeviceState = last;
            CalTotalTimespan(_deviceStates, lastdeviceState, endAt, _startAt, out TimeSpan totalTimespam_idle);
            var rate_idle = Math.Round((totalTimespam_idle.TotalSeconds / total.TotalSeconds) * 100, 2);
            //var rate_ok = Math.Round((totalTimespam_idle.TotalSeconds + totalTimespam_running.TotalSeconds / total.TotalSeconds) * 100, 2);
            #endregion

            #region disconnectedRate
            lastdeviceState = null;
            _deviceStates = deviceStates.Where(x => x.LastState == DeviceStatus.DISCONNECTED).OrderBy(x => x.Timespan).ToArray();
            if (last != null && last.State == DeviceStatus.DISCONNECTED)
                lastdeviceState = last;
            CalTotalTimespan(_deviceStates, lastdeviceState, endAt, _startAt, out TimeSpan totalTimespam_disconnected);
            var rate_disconnected = Math.Round((totalTimespam_disconnected.TotalSeconds / total.TotalSeconds) * 100, 2);
            #endregion

            #region manualRate
            lastdeviceState = null;
            _deviceStates = deviceStates.Where(x => x.LastState == DeviceStatus.MANUALOROFFLINE).OrderBy(x => x.Timespan).ToArray();
            if (last != null && last.State == DeviceStatus.MANUALOROFFLINE)
                lastdeviceState = last;
            CalTotalTimespan(_deviceStates, lastdeviceState, endAt, _startAt, out TimeSpan totalTimespam_manual);
            var rate_manual = Math.Round((totalTimespam_manual.TotalSeconds / total.TotalSeconds) * 100, 2);
            #endregion

            var activation = Math.Round(totalTimespam_running.TotalSeconds / (total.TotalSeconds - totalTimespam_manual.TotalSeconds - totalTimespam_disconnected.TotalSeconds - totalTimespam_alarm.TotalSeconds - totalTimespam_repair.TotalSeconds) * 100, 2);
            var rate_ok = Math.Round(((total.TotalSeconds - totalTimespam_alarm.TotalSeconds - totalTimespam_disconnected.TotalSeconds) / total.TotalSeconds) * 100, 2);
            var showDeviceStatistic = new ShowDeviceStatistics()
            {
                Item = table.Key,
                StartAt = startAt,
                EndAt = endAt,
                Total = total.ToString(),
                RunningValue = $"{totalTimespam_running}",
                RunningRate = $"{rate_running}%",
                AlarmValue = $"{totalTimespam_alarm}",
                AlarmRate = $"{rate_alarm}%",
                UnAlarmRate = $"{rate_unAlarm}%",
                RepairValue = $"{totalTimespam_repair}",
                RepairRate = $"{rate_repair}%",
                IdleValue = $"{totalTimespam_idle}",
                IdleRate = $"{rate_idle}%",
                OKRate = $"{rate_ok}%",
                DisconnectedValue = $"{totalTimespam_disconnected}",
                DisconnectedRate = $"{rate_disconnected}%",
                ManualValue = $"{totalTimespam_manual}",
                ManualRate = $"{rate_manual}%",
                Activation = $"{activation}%",
                Mark = $"统计成功 {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}"
            };
            deviceSateStatistics.Add(new DeviceSateStatistics()
            {
                Device = device,
                DeviceType = (DeviceConverter.ToDevice<TcpProtocolTaskableDevice>(device)).GetDeviceType(),
                StatisticsType = "DAY",
                TimeSpan = table.Value,
                Result = JsonConvert.SerializeObject(showDeviceStatistic)
            });
        }

        private void CalTotalTimespan(DeviceState[] deviceStates, DeviceState lastdeviceState, string endAt, DateTime date, out TimeSpan totalTimespam)
        {
            totalTimespam = new TimeSpan();
            foreach (var item in deviceStates)
            {
                if (item.LastTimespan.Date < date.Date && item.Timespan.Date < date.Date)
                    continue;
                if (item.LastTimespan.Date > date.Date && item.Timespan.Date > date.Date)
                    continue;

                if (item.LastTimespan.Date < date.Date)
                    totalTimespam += item.Timespan - DateTime.Parse($"{date.ToString("yyyy-MM-dd 00:00:00")}");
                else if (item.Timespan.Date > date.Date)
                    totalTimespam += DateTime.Parse($"{date.AddDays(1).ToString("yyyy-MM-dd 00:00:00")}") - item.Timespan;
                else
                    totalTimespam += TimeSpan.FromSeconds(item.TotalSeconds);
            }
            if (lastdeviceState != null)
                totalTimespam += DateTime.Parse(endAt) - lastdeviceState.Timespan;
        }
    }
}
