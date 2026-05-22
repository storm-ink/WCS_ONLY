using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Wcs.Framework.Cfg;

namespace Wcs.Framework
{
    /// <summary>
    /// 线程运行日志
    /// </summary>
    public class ThreadRunningLog
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="threadName"></param>
        public virtual void Init(string threadName)
        {
            ThreadName = threadName;
            logPath = LogWriteToFileHelper.Path + $"\\RunningLogs\\{threadName}";
            objlock = new object();
            LogWriteToFileHelper.logHelper(null, out string dir, logPath);
            if (dir != null)
            {
                var _lastFile = System.IO.Directory.GetFiles(dir).Where(x => !x.Contains("副本")).Select(x => x.Replace(dir + "\\", "")).LastOrDefault();
                if (string.IsNullOrWhiteSpace(_lastFile))
                {
                    serialNo = 1;
                    serialDate = DateTime.Now.ToString("yyyy-MM-dd");
                }
                else
                {
                    var items = _lastFile.Split(new char[] { ' ', '.' }).ToArray();
                    if (items.Length == 3 && DateTime.TryParse(items[0], out DateTime dateTime) && dateTime.ToString("yyyy-MM-dd") == DateTime.Now.ToString("yyyy-MM-dd"))
                    {
                        serialNo = int.Parse(items[1]);
                        serialDate = items[0];
                    }
                    else
                    {
                        serialNo = 1;
                        serialDate = DateTime.Now.ToString("yyyy-MM-dd");
                    }
                }
            }
        }

        object objlock;
        /// <summary>
        /// 线程名称
        /// </summary>
        public virtual string ThreadName { get; private set; }

        static object objLocker = new object();

        string lastFile
        {
            get
            {
                return $"{serialDate} {serialNo.ToString().PadLeft(6, '0')}.log";
            }
        }
        int serialNo = 1;
        string serialDate;
        /// <summary>
        /// 写入文件标志
        /// </summary>
        public virtual string WriteToFile
        {
            get
            {
                return Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("LogWriteToFile", "");
            }
        }

        string logPath;

        long fileMaxLength = 0;
        long FileMaxLength
        {
            get
            {
                if (fileMaxLength == 0)
                    fileMaxLength = (long)10 * 1024 * 1024;
                return fileMaxLength;
            }
        }

        DateTime lastWriteToFileAt = DateTime.Now;
        Dictionary<string, List<string>> WaitWriteToFileList = new Dictionary<string, List<string>>();
        static object waitWriteToFileLocker = new object();
        void push(string path, string msg)
        {
            lock (waitWriteToFileLocker)
            {
                if (WaitWriteToFileList.ContainsKey(path))
                    WaitWriteToFileList[path].Add(msg);
                else
                    WaitWriteToFileList.Add(path, new List<string>() { msg });
                Start();
            }
        }

        static object streamLocker = new object();
        void StreamWriteToFile(string path, string msg)
        {
            lock (streamLocker)
            {
                try
                {
                    using (System.IO.StreamWriter stream = new System.IO.StreamWriter(path, true))
                    {
                        stream.WriteLine(msg);
                        stream.Close();
                        stream.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Wcs.LogWriteToFileHelper.log(ex);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        public virtual void Log(string log)
        {
            lock (objlock)
            {
                try
                {
                    LogWriteToFileHelper.logHelper(log, out string dir, logPath);
                    if (!string.IsNullOrWhiteSpace(dir) && WriteToFile.Contains(dir))
                    {
                        var currentDate = DateTime.Now.ToString("yyyy-MM-dd");
                        if (currentDate != serialDate)
                        {
                            serialDate = currentDate;
                            serialNo = 1;
                        }
                        var path = System.IO.Path.Combine(dir, lastFile);
                        if (System.IO.File.Exists(path))
                        {
                            var fileInfo = new System.IO.FileInfo(path);
                            if (fileInfo.Length > FileMaxLength)
                            {
                                serialNo += 1;
                                path = System.IO.Path.Combine(dir, lastFile);
                            }
                            else
                                fileInfo = null;
                        }

                        var msg = string.Format("{0}--------------------- \r\n{1}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"), log);

                        push(path, msg);
                    }
                }
                catch (Exception ex)
                {
                    LogWriteToFileHelper.log(ex);
                }
            }
        }

        /// <summary>
        /// 将内容输入指定文件
        /// </summary>
        /// <param name="log"></param>
        /// <param name="GetName"></param>
        public virtual void LogToFile(string log, string GetName)
        {
            lock (objlock)
            {
                try
                {
                    LogWriteToFileHelper.logHelper(log, out string dir, logPath);
                    if (!string.IsNullOrWhiteSpace(dir) && WriteToFile.Contains(dir))
                    {
                        var path = System.IO.Path.Combine(dir, $"{GetName}.log");
                        var msg = string.Format("{0}--------------------- \r\n{1}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"), log);
                        StreamWriteToFile(path, msg);
                    }
                }
                catch (Exception ex)
                {
                    LogWriteToFileHelper.log(ex);
                }
            }
        }


        bool isRunning = false;
        private void Start()
        {
            if (isRunning)
                return;

            isRunning = true;
            System.Threading.ThreadPool.QueueUserWorkItem((stat) =>
            {
                try
                {
                    while (WaitWriteToFileList != null && WaitWriteToFileList.Any(x => x.Value.Count() > 0))
                    {
                        Thread.Sleep(5000);
                        if (DateTime.Now.Subtract(lastWriteToFileAt).TotalSeconds > 30 || WaitWriteToFileList.Count() > 1 || WaitWriteToFileList.Any(x => x.Value.Count() > 20))
                        {
                            Dictionary<string, string> dic = new Dictionary<string, string>();
                            lock (waitWriteToFileLocker)
                            {
                                dic = WaitWriteToFileList.Where(x => x.Value != null && x.Value.Count > 0).ToDictionary(x => x.Key, x => string.Join("\r\n", x.Value));
                                var keys = dic.Keys.ToArray();
                                for (int i = 0; i < keys.Length; i++)
                                {
                                    var key = keys[i];
                                    if (i + 1 == keys.Length)
                                        WaitWriteToFileList[key].Clear();
                                    else
                                        WaitWriteToFileList.Remove(key);
                                }
                            }
                            foreach (var item in dic)
                            {
                                StreamWriteToFile(item.Key, item.Value);
                            }
                            lastWriteToFileAt = DateTime.Now;
                        }
                    }
                }
                catch
                {
                }

                isRunning = false;
            }, isRunning);
        }
    }
}
