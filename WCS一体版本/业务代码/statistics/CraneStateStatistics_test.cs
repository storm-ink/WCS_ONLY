using NLog;
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

namespace ZHQXC.Statistics
{
    public class CraneStateStatistics_test : IApplicationStartup
    {
        static Thread _thread;
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

            _logger = LogManager.GetCurrentClassLogger();
            ParameterizedThreadStart start = new ParameterizedThreadStart(check);
            _thread = new Thread(start);
            _thread.IsBackground = true;
            _thread.Start();

            _logger.Info1($"{_device} OEE统计线程已经启动", this);
        }

        private void check(object obj)
        {
            string currentTableName = $"{_device}_{DateTime.Now.ToString("yyyyMMddHHmm")}";
            try
            {
                var device = DeviceConverter.ToDevice<CraneDevice>(_device);
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                return;
            }

            while (true)
            {
                SqlSugarUtil_QuestDB sqlSugarUtil_QuestDB = new SqlSugarUtil_QuestDB();
                dynamic insertData = new ExpandoObject();
                insertData.Id =
                insertData.Age = 25;
                Thread.Sleep(10);
                try
                {
                    var timeSpan = DateTime.Now;
                    var currentTable = $"{_device}_{timeSpan.ToString("yyyyMMddHHmm")}";
                    if (currentTable != currentTableName)
                        currentTableName = currentTable;

                    if (!sqlSugarUtil_QuestDB.Appdb.DbMaintenance.IsAnyTable(currentTableName))
                    {
                        var sql = $"CREATE TABLE {currentTableName}(Device STRING, State STRING, Timespan STRING);";
                        var result = sqlSugarUtil_QuestDB.Appdb.Ado.ExecuteCommand(sql);
                    }


                    // 插入SQL语句
                    string insertSql = $"INSERT INTO {currentTableName} (Device, State, Timespan) VALUES (@Device,@State,@Timespan)";

                    // 参数列表
                    var data = new { Device = _device, State = DeviceStatus.Start, Timespan = timeSpan.ToString("yyyy-MM-dd HH:mm:ss") };
                    // 执行插入操作
                    var rowInserted = sqlSugarUtil_QuestDB.Appdb.Ado.ExecuteCommand(insertSql, data);
                    if (rowInserted <= 0)
                        continue;
                    //sqlSugarUtil_QuestDB.Appdb.Insertable<dynamic>(deviceState).ExecuteCommand();

                    var oldTime = timeSpan.AddMinutes(-5).ToString("yyyyMMddHHmm");
                    var oldTable = $"{_device}_{oldTime}";
                    if (sqlSugarUtil_QuestDB.Appdb.DbMaintenance.IsAnyTable(oldTable))
                        sqlSugarUtil_QuestDB.Appdb.DbMaintenance.DropTable(oldTable);

                    var lastTable = $"{_device}_{timeSpan.AddMinutes(-1).ToString("yyyyMMddHHmm")}";
                    if (sqlSugarUtil_QuestDB.Appdb.DbMaintenance.IsAnyTable(lastTable))
                    {
                        var builder = sqlSugarUtil_QuestDB.Appdb.DynamicBuilder();
                        var datas = sqlSugarUtil_QuestDB.Appdb.Queryable(lastTable, lastTable);
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
                            //if (_obj.ContainsKey("State"))
                            //{

                            //}
                            //else
                            //{ }
                        }
                        else
                        { }
                    }

                    var _oldTime = long.Parse(oldTime);
                    var tableNames = sqlSugarUtil_QuestDB.Appdb.DbMaintenance.GetTableInfoList().Select(x=>x.Name).Where(x=>x.StartsWith(_device)).ToList();
                    foreach (var tableName in tableNames)
                    {
                        var _dic = tableName.Split('_').ToArray();
                        if (long.Parse(_dic[1]) < _oldTime)
                            sqlSugarUtil_QuestDB.Appdb.DbMaintenance.DropTable(tableName);
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}
