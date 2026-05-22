
using System;
using System.ComponentModel;
using Microsoft.Win32;

namespace Wcs.Framework.CraneControl
{
    /// <summary>任务信息</summary>
    public class Task
    {
        internal static int iGotTaskId;
        public static RegistryKey rk = Registry.CurrentConfig.CreateSubKey("Gensong");

        /// <summary>任务定长 8 位; GOT00001 回原点; GOT99999 -> GOT00002</summary>
        public static string GetGotId
        {
            get
            {
                try
                {
                    if (++iGotTaskId < 2 || iGotTaskId > 99999) iGotTaskId = 2;

                    rk.SetValue("GOTId", iGotTaskId);

                    return string.Format("GOT{0:D5}", iGotTaskId);
                }
                catch
                {
                    return "GOT00000";
                }
            }
        }

        /// <summary>读 GOT 流水号</summary>
        public static int ReadGotId()
        {
            try
            {
                iGotTaskId = Convert.ToInt32(rk.GetValue("GOTId"));
                return iGotTaskId;
            }
            catch //(Exception ex)
            {
                iGotTaskId = 2;
                return iGotTaskId;
            }
        }
    }
}
