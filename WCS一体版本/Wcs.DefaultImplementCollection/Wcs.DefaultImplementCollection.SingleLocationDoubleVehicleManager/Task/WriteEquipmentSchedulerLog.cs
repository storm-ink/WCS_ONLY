using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.SingleLocationDoubleVehicleManager
{
    /// <summary>
    /// 日志帮助类
    /// </summary>
    public static class WriteEquipmentSchedulerLog
    {
        public static string WritSchedulerLog
        {
            get
            {
                return Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("LogWriteToFile", "");
            }
        }

        static object objlock = new object();
        public static void Log(object obj, string path = null)
        {
            lock (objlock)
            {
                LogWriteToFileHelper.logHelper(obj, out string dir, path);
                if (!string.IsNullOrWhiteSpace(dir) && WritSchedulerLog.Contains(dir))
                {
                    //LogWriteToFileHelper.log(obj, path);
                    //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    //sw.Start();
                    System.IO.File.AppendAllText(
                        System.IO.Path.Combine(dir, string.Format("{0:yyyy-MM-dd}.log", DateTime.Now))
                         , string.Format("{0}--------------------- \r\n{1}\r\n", DateTime.Now, obj)
                         , System.Text.Encoding.UTF8
                         );
                    //sw.Stop();
                    ////Console.WriteLine($"写文件耗时{sw.ElapsedMilliseconds}ms");
                }
            }
        }
    }
}
