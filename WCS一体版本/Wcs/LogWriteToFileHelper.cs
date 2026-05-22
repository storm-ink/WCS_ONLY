using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;

namespace Wcs
{
    public static class LogWriteToFileHelper
    {
        static object logLocker = new object();
        static Thread _thread = null;
        static String CurrentDir
        {
            get
            {
                var path = new Uri(typeof(LogWriteToFileHelper).Assembly.CodeBase).LocalPath;
                var dir = System.IO.Path.GetDirectoryName(path);

                return dir;
            }
        }

        static string _path;
        public static string Path
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_path))
                {
                    lock (objlock)
                    {
                        if (ConfigurationManager.AppSettings.AllKeys.Any(x => x == "LogWriteToFileBasePath"))
                        {
                            var path = ConfigurationManager.AppSettings["LogWriteToFileBasePath"];
                            try
                            {
                                if (!System.IO.Directory.Exists(path))
                                    System.IO.Directory.CreateDirectory(path);
                                _path = path;
                            }
                            catch (Exception ex)
                            {
                                log(ex);
                                _path = System.IO.Path.Combine(CurrentDir, "logs");
                            }
                        }
                        else
                            _path = System.IO.Path.Combine(CurrentDir, "logs");
                    }
                }
                return _path;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    return;
                else
                {
                    try
                    {
                        if (!System.IO.Directory.Exists(value))
                            System.IO.Directory.CreateDirectory(value);
                        _path = value;
                    }
                    catch (Exception ex)
                    {
                        log(ex);
                    }
                }
            }
        }

        static int _overTime = 0;
        public static int OverTime
        {
            get
            {
                if (_overTime == 0)
                {
                    if (ConfigurationManager.AppSettings.AllKeys.Any(x => x == "LogWriteToFileOverTimeDelete"))
                    {
                        try
                        {
                            if (!int.TryParse(ConfigurationManager.AppSettings["LogWriteToFileOverTimeDelete"], out _overTime))
                                _overTime = 7;
                        }
                        catch (Exception ex)
                        {
                            log(ex);
                            _overTime = 7;
                        }
                    }
                    else
                        _overTime = 7;
                }
                return _overTime;
            }
        }

        public static event EventHandler<FileLogWriteEventArgs> FileLogWrite;

        static object objlock = new object();

        public static void log(Object o, string path = null)
        {
            try
            {
                if (paths == null)
                    paths = new List<string>();

                if (_thread == null)
                {
                    _thread = new Thread(DeleteHistoryFileThread);
                    _thread.IsBackground = true;
                    _thread.Start();
                }

                string dir = Path;
                if (path != null)
                    dir = System.IO.Path.Combine(Path, $"{path}\\logs");

                if (string.IsNullOrWhiteSpace(dir))
                {
                    Console.WriteLine("写文件时，路径不可以为null");
                    return;
                }

                if (!System.IO.Directory.Exists(dir))
                    System.IO.Directory.CreateDirectory(dir);

                if (!paths.Contains(dir))
                    paths.Add(dir);

                System.Threading.ThreadPool.QueueUserWorkItem((stat) =>
                {
                    FileLogWrite?.Invoke(typeof(LogWriteToFileHelper), new FileLogWriteEventArgs(dir, string.Format("{0} {1}\r\n", DateTime.Now, o)));
                }, null);

                lock (objlock)
                {
                    System.IO.File.AppendAllText(
                        System.IO.Path.Combine(dir, string.Format("{0:yyyy-MM-dd}.log", DateTime.Now))
                         , string.Format("{0} {1}\r\n", DateTime.Now, o)
                         , System.Text.Encoding.UTF8
                         );
                    //Console.WriteLine($"写文件耗时{sw.ElapsedMilliseconds}ms");
                }
            }
            catch (Exception ex)
            {
                LogWriteToFileHelper.log(ex);
            }
        }

        static object objlock_l = new object();
        public static void logHelper(Object o,out string pathStr, string path = null)
        {
            try
            {
                lock (objlock_l)
                {
                    if (paths == null)
                        paths = new List<string>();

                    if (_thread == null)
                    {
                        _thread = new Thread(DeleteHistoryFileThread);
                        _thread.IsBackground = true;
                        _thread.Start();
                    }

                    string dir = Path;
                    if (path != null)
                        dir = System.IO.Path.Combine(Path, $"{path}\\logs");

                    if (string.IsNullOrWhiteSpace(dir))
                    {
                        Console.WriteLine("写文件时，路径不可以为null");
                        pathStr = null;
                        return;
                    }

                    if (!System.IO.Directory.Exists(dir))
                        System.IO.Directory.CreateDirectory(dir);

                    if (!paths.Contains(dir))
                        paths.Add(dir);

                    if (o != null)
                    {
                        System.Threading.ThreadPool.QueueUserWorkItem((stat) =>
                        {
                            FileLogWrite?.Invoke(typeof(LogWriteToFileHelper), new FileLogWriteEventArgs(dir, string.Format("{0:yyyy-MM-dd HH:mm:ss.ffff} {1}\r\n", DateTime.Now, o)));
                        }, null);
                    }

                    pathStr = dir;
                }
            }
            catch (Exception ex)
            {
                LogWriteToFileHelper.log(ex);
                pathStr = null;
            }
        }

        private static void DeleteHistoryFileThread()
        {
            while (true)
            {
                try
                {
                    if (paths != null && paths.Count() > 0)
                    {
                        var _paths = paths.Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
                        foreach (var item in _paths)
                        {
                            if (item == null)
                                continue;

                            var files = System.IO.Directory.GetFiles(item, "*.*", System.IO.SearchOption.TopDirectoryOnly);
                            foreach (string file in files)
                            {
                                var createAt = System.IO.File.GetCreationTime(file).Date;
                                if (DateTime.Now.Date.Subtract(createAt).TotalDays > OverTime)
                                //System.IO.File.Delete(item);
                                {
                                    var deletefile = new System.IO.FileInfo(file);
                                    if (deletefile.Exists)//文件是否存在，存在则执行删除
                                        deletefile.Delete();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogWriteToFileHelper.log(ex);
                }

                Thread.Sleep(1000 * 60);
            }
        }

        public static List<string> paths { get; set; }
    }

    public class FileLogWriteEventArgs : EventArgs
    {
        public FileLogWriteEventArgs(string path, string msg)
        {
            Path = path;
            Message = msg;
            CreateAt = DateTime.Now;
        }
        public string Path { get; set; }
        public string Message { get; set; }

        public DateTime CreateAt { get; set; }
    }
}