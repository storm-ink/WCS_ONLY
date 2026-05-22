
using System;
using System.ComponentModel;
using System.Data;
using System.Runtime.Serialization;

namespace Wcs.Framework.CraneControl
{
    /// <summary>堆垛机配置信息</summary>
    [DataContract]
    public class SConfig
    {
        /// <summary>工程标题名</summary>
        [DataMember]
        public string Title { get; set; }
        /// <summary>工程名</summary>
        [DataMember]
        public string Project { get; set; }
        /// <summary>记录日志天数</summary>
        [DataMember]
        public int LogDay { get; set; }

        /// <summary>刷新周期</summary>
        [DataMember]
        public int Interval { get; set; }

        /// <summary>错误码 配置数据</summary>
        [DataMember]
        public DataSet dsAlarm { get; set; }

        /// <summary>货架 配置信息</summary>
        [DataMember]
        public SShelf[] arShelf;
    }
}
