using NHibernate.Mapping;
using NLog;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wcs;

namespace ZHQXC.Statistics
{
    public static class StatisticHelper
    {
        public static List<string> DeleteDeviceList = new List<string>();
        static Thread _thread;
        static StatisticHelper()
        {
            ParameterizedThreadStart start1 = new ParameterizedThreadStart(OEEBaseDatasArchivePro);
            _thread = new Thread(start1);
            _thread.IsBackground = true;
            _thread.Start();
            logger.Info1($"MQTT数据压缩归档线程", typeof(StatisticHelper));
        }

        public static int SaveDays
        {
            get
            {
                return Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<int>("mqttDataSaveDays", 2);
            }
        }

        public static string MqttDataSavePath
        {
            get
            {
                return Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("mqttDataSavePath", "D:\\questdb-7.2.1-rt-windows-amd64\\bin\\qdbroot_server\\db");
            }
        }

        static string _7zPath = null;
        public static string _7ZPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_7zPath))
                {
                    if (ConfigurationManager.AppSettings.AllKeys.Any(x => x == "_7zPath"))
                    {
                        try
                        {
                            _7zPath = ConfigurationManager.AppSettings["_7zPath"];
                        }
                        catch (Exception ex)
                        {
                            //log(ex);
                            _7zPath = "D:\\Program Files\\7-Zip";
                        }
                    }
                    else
                        _7zPath = "D:\\Program Files\\7-Zip";
                }
                return _7zPath;
            }
        }
        static int _7zTime = 0;
        public static int _7ZTime
        {
            get
            {
                if (_7zTime == 0)
                {
                    if (ConfigurationManager.AppSettings.AllKeys.Any(x => x == "LogWriteToFileOverTimeDelete7z"))
                    {
                        try
                        {
                            if (!int.TryParse(ConfigurationManager.AppSettings["LogWriteToFileOverTimeDelete7z"], out _7zTime))
                                _7zTime = 180;
                        }
                        catch (Exception ex)
                        {
                            //log(ex);
                            _7zTime = 180;
                        }
                    }
                    else
                        _7zTime = 180;
                }
                return _7zTime;
            }
        }

        private static void OEEBaseDatasArchivePro(object obj)
        {
            Thread.Sleep(1000 * 60);
            string filePath = $"{MqttDataSavePath}\\questdbHistory7z.bat";
            while (true)
            {
                try
                {
                    #region 压缩日志
                    var paths = Directory.GetDirectories(MqttDataSavePath);
                    List<string> LIST = new List<string>();
                    foreach (var path in paths)
                    {
                        var item = path.Replace(MqttDataSavePath + "\\", "");
                        if (item.Length > 8 && int.TryParse(item.Substring(item.Length - 8, 8), out int result))
                        {
                            var dateStr = $"{(result / 10000).ToString("0000")}-{((result % 10000) / 100).ToString("00")}-{(result % 100).ToString("00")} 00:00:00";
                            if (DateTime.TryParse(dateStr, out DateTime date) && DateTime.Now.Subtract(date).TotalDays >= 2)
                            {
                                List<string> list = new List<string>();
                                list.Add("chcp 65001");
                                list.Add("echo 设置基础目录文件夹...");
                                list.Add($"set \"pbuf={MqttDataSavePath}\\\"");
                                list.Add("echo 设置文件名...");
                                list.Add($"set \"name={item}\"");
                                list.Add("echo 设置要备份的文件夹...");
                                list.Add($"set \"mbuf={MqttDataSavePath}\\%name%\"");
                                list.Add("if not exist \"%pbuf%\" (");
                                list.Add("    echo 不存在文件夹\"%pbuf%\"");
                                list.Add(")");
                                list.Add("else(");
                                list.Add("echo 初始化压缩程序设置...  ");
                                list.Add("cd \\  ");
                                list.Add("C:  ");
                                list.Add($"cd {_7ZPath}");
                                list.Add("set yes=ok  ");
                                list.Add($"echo {_7ZPath}\\7z.exe  ");
                                list.Add("echo 是否存在已压缩文件，如果存在则删除要备份的文件夹...");
                                list.Add("if exist \"%pbuf%backup\\%name%.7z\" rmdir /s /q \"%mbuf%\"");
                                list.Add("else( ");
                                list.Add("echo 开始执行压缩备份");
                                list.Add("7z a -t7z \"%pbuf%backup\\%name%.7z\" \"%mbuf%\"");
                                list.Add("rmdir /s /q \"%mbuf%\"");
                                list.Add(")");
                                list.Add(")");
                                list.Add("echo %name%备份完毕！");
                                LIST.Add(string.Join("\r\n", list));
                            }
                        }
                    }

                    if (LIST.Count > 0)
                    {
                        var content = string.Join("\r\n", LIST);
                        File.WriteAllText(filePath, content);
                        Process.Start(filePath);
                    }
                    #endregion

                    #region 删除压缩日志
                    var _paths = Directory.GetFiles(MqttDataSavePath + "\\backup");
                    foreach (var path in _paths)
                    {
                        var item = path.Replace(MqttDataSavePath + "\\backup\\", "").Replace(".7z", "");
                        if (item.Length > 8 && int.TryParse(item.Substring(item.Length - 8, 8), out int result))
                        {
                            var dateStr = $"{(result / 10000).ToString("0000")}-{((result % 10000) / 100).ToString("00")}-{(result % 100).ToString("00")} 00:00:00";
                            if (DateTime.TryParse(dateStr, out DateTime date) && DateTime.Now.Subtract(date).TotalDays >= _7ZTime)
                                File.Delete(path);
                        }
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    logger.Error1(ex, typeof(StatisticHelper));
                }
                Thread.Sleep(1000 * 60 * 60);
            }
        }

        static Logger logger = LogManager.GetCurrentClassLogger();
        public const string StatisticTableName = "DEVICE_STATE_STATISTICS";
        public static string StatisticTableCreateSql = $"CREATE TABLE IF NOT EXISTS {StatisticHelper.StatisticTableName}(DEVICE STRING, DEVICETYPE STRING, STATISTICSTYPE STRING, TIMESPAN STRING, RESULT STRING);";


        static object StatisticLocker = new object();

        static Dictionary<string, List<string>> questDBTableNamesDic = new Dictionary<string, List<string>>();
        static readonly object objLocker = new object();
        public static List<string> QuestDBTableNames
        {
            get
            {
                if (questDBTableNamesDic == null || questDBTableNamesDic.Count() == 0 || questDBTableNamesDic.Last().Key != DateTime.Now.ToString("yyyyMMddHH"))
                {
                    lock (objLocker)
                    {
                        if (questDBTableNamesDic == null || questDBTableNamesDic.Count() == 0 || questDBTableNamesDic.Last().Key != DateTime.Now.ToString("yyyyMMddHH"))
                        {
                            questDBTableNamesDic = new Dictionary<string, List<string>>();
                            var _sql = "SHOW TABLES";
                            List<string> tableNames;
                            using (SqlSugarUtil_QuestDB sqlSugarUtil_QuestDB = new SqlSugarUtil_QuestDB())
                            {
                                tableNames = sqlSugarUtil_QuestDB.Appdb.Ado.SqlQuery<string>(_sql);
                                sqlSugarUtil_QuestDB.Dispose();
                            }
                            questDBTableNamesDic.Add(DateTime.Now.ToString("yyyyMMddHH"), tableNames);
                        }
                    }
                }
                if (questDBTableNamesDic == null || questDBTableNamesDic.Count() == 0)
                    return null;
                return questDBTableNamesDic.Last().Value;
            }
        }

        public static bool IsAnyTable(string tableName)
        {
            if (QuestDBTableNames == null)
                return false;
            else
                return QuestDBTableNames.Any(x => x == tableName);
        }

        public static bool IsAnyTableFalseCreate(string tableName, string sql)
        {
            if (!IsAnyTable(tableName))
            {
                lock (objLocker)
                {
                    using (SqlSugarUtil_QuestDB sqlSugarUtil_QuestDB = new SqlSugarUtil_QuestDB())
                    {
                        if (!sqlSugarUtil_QuestDB.Appdb.DbMaintenance.IsAnyTable(tableName))
                            sqlSugarUtil_QuestDB.Appdb.Ado.ExecuteCommand(sql);

                        sqlSugarUtil_QuestDB.Dispose();
                    }
                    if (questDBTableNamesDic.Count() != 0)
                    {
                        var key = questDBTableNamesDic.Last().Key;
                        if (!questDBTableNamesDic[key].Contains(tableName))
                            questDBTableNamesDic[key].Add(tableName);
                    }
                }
            }

            return true;
        }

        public static void DropTables(List<string> tableNames)
        {
            lock (objLocker)
            {
                using (SqlSugarUtil_QuestDB sqlSugarUtil_QuestDB = new SqlSugarUtil_QuestDB())
                {
                    foreach (string tableName in tableNames)
                    {
                        sqlSugarUtil_QuestDB.Appdb.DbMaintenance.DropTable(tableName);

                        if (questDBTableNamesDic.Count() != 0)
                        {
                            var key = questDBTableNamesDic.Last().Key;
                            questDBTableNamesDic[key].Remove(StatisticTableName);
                        }
                    }
                    sqlSugarUtil_QuestDB.Dispose();
                }
            }
        }

        public static void TrySave(string device, DeviceStatus deviceState, string mark, ref DeviceState lastDeviceState, string currentTableName)
        {
            var timeSpan = DateTime.Now;
            if (!currentTableName.EndsWith(timeSpan.ToString("yyyyMMdd")))
                return;//如果时间和表不匹配则本次不做处理，下次再保存数据

            var nowDeviceState = new DeviceState() { Device = device, State = deviceState, Timespan = timeSpan, Mark0 = mark };
            // 插入SQL语句
            string _insertSql = $"INSERT INTO {currentTableName} (Device, LastState, LastTimespan, State, Timespan, TotalSeconds, Mark0, Mark1, Mark2) VALUES (@Device,@LastState,@LastTimespan,@State,@Timespan,@TotalSeconds,@Mark0,@Mark1,@Mark2)";
            if (lastDeviceState == null)
            {
                nowDeviceState.LastState = DeviceStatus.Start;
                nowDeviceState.LastTimespan = nowDeviceState.Timespan;
                nowDeviceState.TotalSeconds = 0;
                // 参数列表
                var _data = new
                {
                    Device = nowDeviceState.Device,
                    LastState = DeviceStatus.Start.ToString(),
                    LastTimespan = nowDeviceState.Timespan.ToString("yyyy-MM-dd HH:mm:ss"),
                    State = nowDeviceState.State.ToString(),
                    Timespan = nowDeviceState.Timespan.ToString("yyyy-MM-dd HH:mm:ss"),
                    TotalSeconds = "0",
                    Mark0 = mark,
                    Mark1 = "",
                    Mark2 = ""
                };
                int _rowInserted = -1;
                using (SqlSugarUtil_QuestDB sqlSugarUtil_QuestDB = new SqlSugarUtil_QuestDB())
                {
                    // 执行插入操作
                    _rowInserted = sqlSugarUtil_QuestDB.Appdb.Ado.ExecuteCommand(_insertSql, _data);
                    sqlSugarUtil_QuestDB.Dispose();
                }
                if (_rowInserted <= 0)
                    return;

                lastDeviceState = nowDeviceState;
            }
            else if (lastDeviceState.State != nowDeviceState.State || lastDeviceState.Mark0 != nowDeviceState.Mark0 || lastDeviceState.Timespan.Date != nowDeviceState.Timespan.Date)
            {
                nowDeviceState.LastState = lastDeviceState.State;
                nowDeviceState.LastTimespan = lastDeviceState.Timespan;
                nowDeviceState.TotalSeconds = nowDeviceState.Timespan.Subtract(nowDeviceState.LastTimespan).TotalSeconds;
                // 参数列表
                var _data = new
                {
                    Device = nowDeviceState.Device,
                    LastState = nowDeviceState.LastState.ToString(),
                    LastTimespan = nowDeviceState.LastTimespan.ToString("yyyy-MM-dd HH:mm:ss"),
                    State = nowDeviceState.State.ToString(),
                    Timespan = nowDeviceState.Timespan.ToString("yyyy-MM-dd HH:mm:ss"),
                    TotalSeconds = $"{Math.Round(nowDeviceState.TotalSeconds, 2)}",
                    Mark0 = mark,
                    Mark1 = "",
                    Mark2 = ""
                };
                int _rowInserted = -1;
                using (SqlSugarUtil_QuestDB sqlSugarUtil_QuestDB = new SqlSugarUtil_QuestDB())
                {
                    // 执行插入操作
                    _rowInserted = sqlSugarUtil_QuestDB.Appdb.Ado.ExecuteCommand(_insertSql, _data);
                    sqlSugarUtil_QuestDB.Dispose();
                }
                if (_rowInserted <= 0)
                    return;

                lastDeviceState = nowDeviceState;
            }
        }

        public static bool TrySave_DeviceStateSatistic(List<DeviceSateStatistics> deviceSateStatistics)
        {
            List<string> list = new List<string>();
            foreach (var item in deviceSateStatistics)
            {
                list.Add($"('{item.Device}','{item.DeviceType}','{item.StatisticsType}','{item.TimeSpan}','{item.Result}')");
            }

            if (list.Count() > 0)
            {
                // 插入SQL语句
                string _insertSql = $"INSERT INTO DEVICE_STATE_STATISTICS (DEVICE, DEVICETYPE, STATISTICSTYPE, TIMESPAN, RESULT) VALUES {string.Join(",", list)}";

                int _rowInserted;
                using (SqlSugarUtil_QuestDB sqlSugarUtil_QuestDB = new SqlSugarUtil_QuestDB())
                {
                    // 执行插入操作
                    _rowInserted = sqlSugarUtil_QuestDB.Appdb.Ado.ExecuteCommand(_insertSql);
                    sqlSugarUtil_QuestDB.Dispose();
                }
                return true;
            }
            else
                return false;
        }
    }
}
