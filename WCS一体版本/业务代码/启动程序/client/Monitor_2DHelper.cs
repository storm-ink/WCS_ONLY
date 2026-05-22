using ZHQXC.Statistics;
using DevExpress.Utils.Drawing.Helpers;
using NHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wcs;

namespace ZHQXC
{
    public static class Monitor_2DHelper
    {
        public const string BasicTableName = "Monitor2D";

        static object Monitor2DLocker = new object();

        /// <summary>
        /// 检查表是否存在
        /// 按天分表
        /// </summary>
        /// <param name="tableName">表名字 ex:Monitor2D_20240820</param>
        /// <returns></returns>
        public static bool CheckMonitor2DTableIsExist(string tableName)
        {
            lock (Monitor2DLocker)
            {
                try
                {
                    if (lastCheck == null || !lastCheck.ContainsKey(tableName) || !lastCheck[tableName])
                    {
                        var sql = $"CREATE TABLE IF NOT EXISTS {tableName}(TIMESPAN string, TOPIC STRING, MQTTMSG STRING);";
                        if (!StatisticHelper.IsAnyTableFalseCreate(tableName, sql))
                        {
                            //this.Log($"数据采集表未创建成功，系统会稍后重试");
                            Thread.Sleep(5000);
                            return false;
                        }

                        if (lastCheck == null)
                            lastCheck = new Dictionary<string, bool>() { { $"{tableName}", true } };
                        else if (!lastCheck.ContainsKey(tableName))
                        {
                            if (lastCheck.Count() > 5)
                            {
                                var first = lastCheck.First().Key;
                                lastCheck.Remove(first);
                            }
                            lastCheck.Add(tableName, true);
                        }
                        else
                            lastCheck[tableName] = true;

                        return true;
                    }
                    else
                        return lastCheck[tableName];
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return false;
                }
            }
        }

        static Dictionary<string, bool> lastCheck = null;

        #region save

        private static void SaveProc()
        {
            while (true)
            {
                Thread.Sleep(500);
                try
                {
                    string[] array;
                    lock (_waitSaveList)
                    {
                        array = _waitSaveList.ToArray();
                    }

                    if (array.Length > 0)
                    {
                        var tableName = $"{BasicTableName}_{DateTime.Now.ToString("yyyyMMdd")}";
                        if (CheckMonitor2DTableIsExist(tableName))
                        {
                            // 插入SQL语句
                            string _insertSql = $"INSERT INTO {tableName} (TIMESPAN,TOPIC,MQTTMSG) VALUES {string.Join(",", array)}";

                            int _rowInserted = -1;
                            using (SqlSugarUtil_QuestDB sqlSugarUtil_QuestDB = new SqlSugarUtil_QuestDB())
                            {
                                // 执行插入操作
                                _rowInserted = sqlSugarUtil_QuestDB.Appdb.Ado.ExecuteCommand(_insertSql);
                                sqlSugarUtil_QuestDB.Appdb.Close();
                                sqlSugarUtil_QuestDB.Dispose();
                            }
                            if (_rowInserted > 0 && _rowInserted == array.Length)
                                PopWaitSaveTaskList(0, array.Length);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        /// <summary>
        /// msg待保存列表
        /// </summary>
        public static List<string> _waitSaveList = new List<string>();

        /// <summary>
        /// 将msg添加到待保存列表
        /// </summary>
        /// <param name="task"></param>
        public static void PushWaitSaveTaskList(string msg)
        {
            lock (_waitSaveList)
            {
                if (_thread == null)
                {
                    _thread = new System.Threading.Thread(SaveProc);
                    _thread.Name = "SaveProc";
                    _thread.IsBackground = true;
                    _thread.StartAndManaged();
                    //_logger.Debug1("TaskStateReportToPreTask线程已启动", this);
                    _threadDelete = new System.Threading.Thread(DeleteProc);
                    _threadDelete.Name = "DeleteProc";
                    _threadDelete.IsBackground = true;
                    _threadDelete.StartAndManaged();
                }

                if (_waitSaveList.Any(x => x == msg))
                    return;

                _waitSaveList.Add(msg);
            }
        }
        private static void DeleteProc()
        {
            while (true)
            {
                try
                {
                    var replaySaveDays = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<int>("ReplaySaveDays", 30);
                    var currentDeleteDate = int.Parse(DateTime.Now.AddDays(-replaySaveDays).ToString("yyyyMMdd"));

                    var tableNames = StatisticHelper.QuestDBTableNames.Where(x => x.StartsWith(BasicTableName)).ToList();
                    List<string> list = new List<string>();
                    foreach (var tableName in tableNames)
                    {
                        var items = tableName.Split('_').ToArray();
                        if (items.Length >= 2 && int.TryParse(items[1], out int day))
                        {
                            if (day <= currentDeleteDate) 
                                list.Add(tableName);
                        }
                    }
                    StatisticHelper.DropTables(list);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                Thread.Sleep(new TimeSpan(1, 0, 0));
            }
        }

        /// <summary>
        /// 将msg从待保存列表中移除
        /// </summary>
        /// <param name="task"></param>
        public static void PopWaitSaveTaskList(string msg)
        {
            if (string.IsNullOrWhiteSpace(msg))
                return;

            lock (_waitSaveList)
                _waitSaveList.Remove(msg);
        }

        /// <summary>
        /// 将msg从待保存列表中移除
        /// </summary>
        /// <param name="task"></param>
        public static void PopWaitSaveTaskList(int index, int count)
        {
            lock (_waitSaveList)
                _waitSaveList.RemoveRange(0, count);
        }

        static Thread _thread = null;
        static Thread _threadDelete = null;


        #endregion
    }
}
