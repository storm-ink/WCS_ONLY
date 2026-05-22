using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 表示实现此接口的类型是一个默认结构的Tcp传输协议-消息体
    /// </summary>
    public interface IDefaultTcpProtocolMappingBody
    {
        /// <summary>
        /// 报文标志
        /// </summary>
        string Flag { get; set; }
        /// <summary>
        /// 集合名称
        /// </summary>
        string CollectionName { get; set; }
    }
}
