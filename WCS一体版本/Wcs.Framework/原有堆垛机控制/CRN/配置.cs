
using System;
using System.Data;
using System.IO;

namespace Wcs.Framework.CraneControl
{
    /// <summary>配置信息</summary>
    public class Config
    {
        public static bool Initialized { get; private set; }
        /// <summary>工程标题名</summary>
        public static string Title;
        /// <summary>工程名</summary>
        public static string Project;
        /// <summary>记录日志天数</summary>
        public static int LogDay = 30;

        /// <summary>刷新周期</summary>
        public static int Interval = 800;
        /// <summary>串口通信标志</summary>
        public static bool COM;
        /// <summary>测试模块是否启用</summary>
        public static bool Test;

        /// <summary>堆垛机的配置数组</summary>
        public static CRNConfig[] Crans;
        /// <summary>配置文件路径</summary>
        public static string sPath;

        /// <summary>读 货架, 报警码 配置; 创建 堆垛机 通讯实例</summary>
        public static void Init()
        {
            try
            {
                ReadConfig();

                Alarm.ReadConfig();
                Task.ReadGotId();

                CraneArray.New();

                Initialized = true;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("读配置异常\r\n{0}", ex.ToString()));
            }
        }
        private static void ReadConfig()
        {
            DataSet dts = new DataSet(); DataTable dtb; DataRow dtr; string sPoint;
            sPath = Directory.GetDirectories(Directory.GetCurrentDirectory(), "P_*")[0];
            dts.ReadXml(sPath + "/CRN.xml");
            dtr = dts.Tables["Config"].Rows[0]; dtb = dts.Tables["Crane"];

            Title = Convert.ToString(dtr["Title"]);
            Project = Convert.ToString(dtr["Project"]);

            LogDay = Convert.ToInt32(dtr["LogDay"]);
            Interval = Convert.ToInt32(dtr["Interval"]);

            COM = Convert.ToString(dtr["COM"]) == "Y" ? true : false;
            try { Test = Convert.ToString(dtr["Test"]) == "Y" ? true : false; } catch { }

            // 堆垛机 配置
            Crans = new CRNConfig[dtb.Rows.Count];
            for (int i = 0; i < dtb.Rows.Count; i++)
            {
                dtr = dtb.Rows[i];

                CRNConfig crn = new CRNConfig(); sPoint = Convert.ToString(dtr["Point"]).Trim();
                crn.Enabled = Convert.ToString(dtr["Enabled"]).Trim() == "1" ? true : false;
                crn.CName = Convert.ToString(dtr["Name"]).Trim();
                crn.IP = Convert.ToString(dtr["IP"]).Trim();
                crn.Port = Convert.ToInt32(dtr["Port"]);

                crn.Point = new Position();
                crn.Point.MCol = Convert.ToInt32(sPoint.Substring(0, 3));
                crn.Point.MRow = Convert.ToInt32(sPoint.Substring(3, 3));

                if (crn.Enabled) crn.Shelf = Shelf.ReadConfig(crn.CName);

                Crans[i] = crn;
            }
        }

        /// <summary>获取指定堆垛机名称的配置</summary>
        public static CRNConfig Get(string sCName)
        {
            foreach (var c in Crans)
            {
                if (c.CName.Equals(sCName,StringComparison.CurrentCultureIgnoreCase)) return c;
            }

            return null;
        }
    }
}
