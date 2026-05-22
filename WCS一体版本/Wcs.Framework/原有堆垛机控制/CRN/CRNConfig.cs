
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Wcs.Framework.CraneControl
{
    /// <summary>堆垛机 配置信息</summary>
    //[DebuggerNonUserCode()]
    [DataContract]
    public class CRNConfig
    {
        /// <summary>堆垛机启用标志</summary>
        public bool Enabled;
        /// <summary>堆垛机的名称</summary>
        public string CName;
        /// <summary>堆垛机的 IP</summary>
        public string IP;
        /// <summary>堆垛机的端口</summary>
        public int Port;

        /// <summary>定点位置, 堆垛机的列层</summary>
        public Position Point;

        /// <summary>货架信息</summary>
        public Shelf Shelf { get; set; }
    }

    /// <summary>获取 指定堆垛机 通讯实例</summary>
    public class CraneArray
    {
        /// <summary></summary>
        public static Dictionary<string, Crane> dCrane;

        /// <summary>堆垛机 名称</summary>
        public static IEnumerable<string> Names
        {
            get
            {
                return dCrane.Keys; 
            }
        }

        /// <summary>获取 指定堆垛机 通讯实例</summary>
        public static Crane Get(string sCName)
        {
            return dCrane[sCName];
        }

        /// <summary>创建 堆垛机 通讯实例</summary>
        public static void New()
        {
            dCrane = new Dictionary<string, Crane>();

            foreach (var c in Config.Crans)
            {
                if (dCrane.ContainsKey(c.CName))
                {
                    throw new Exception(string.Format("堆垛机名 '{0}' 配置重复!", c.CName));
                }

                dCrane.Add(c.CName, new Crane(c.Enabled, c.CName, c.IP, c.Port));
            }
        }
    }
}
