using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Data;

namespace Wcs.Framework.CraneControl
{

    /// <summary>货架配置信息</summary>
    [DataContract]
    public class SShelf
    {
        /// <summary></summary>
        public SShelf(string CName)
        {
            this.CName = CName;
        }

        /// <summary>堆垛机名</summary>
        [DataMember]
        public string CName { get; set; }

        /// <summary>货架最大排编号</summary>
        [DataMember]
        public int MaxL { get; set; }
        /// <summary>货架最小排编号</summary>
        [DataMember]
        public int MinL { get; set; }
        /// <summary>货架最大列编号</summary>
        [DataMember]
        public int MaxC { get; set; }
        /// <summary>货架最小列编号</summary>
        [DataMember]
        public int MinC { get; set; }
        /// <summary>货架最大层编号</summary>
        [DataMember]
        public int MaxR { get; set; }
        /// <summary>货架最小层编号</summary>
        [DataMember]
        public int MinR { get; set; }

        /// <summary>货架配置信息</summary>
        [DataMember]
        public DataTable PBit { get; set; }
    }
}
