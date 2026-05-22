using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework
{
    /// <summary>
    /// 表示实现此接口的类型是一个默认结构的Tcp传输协议
    /// </summary>
    public interface IDefaultTcpProtocolMapping
    {
        /// <summary>
        /// 报文头
        /// </summary>
        string Header { get; set; }

        /// <summary>
        /// 报文体长度
        /// </summary>
        string Length { get; set; }

        /// <summary>
        /// 报文体
        /// </summary>
        IDefaultTcpProtocolMappingBody[] Body { get; set; }

        /// <summary>
        /// 校验码
        /// </summary>
        string Checker { get; set; }

        /// <summary>
        /// 报文尾
        /// </summary>
        string Ender { get; set; }

        /// <summary>
        /// xml报文
        /// </summary>
        XmlDocument XmlDocument { get; set; }
    }
}