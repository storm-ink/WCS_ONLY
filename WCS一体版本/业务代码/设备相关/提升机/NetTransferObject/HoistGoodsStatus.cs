using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BOE
{
    public enum HoistGoodsStatus
    {

        [Description("无货")]
        [EnumMember]
        Initialize = 0,
        // <summary>
        /// 链条1
        /// </summary>
        [Description("链条1")]
        [EnumMember]
        ChainOne = 1,

        /// <summary>
        /// 链条2
        /// </summary>
        [Description("链条2")]
        [EnumMember]
        ChainTwo = 2,

        /// <summary>
        /// 双货
        /// </summary>
        [Description("双货")]
        [EnumMember]
        DoubleChain = 3,

    }
}
