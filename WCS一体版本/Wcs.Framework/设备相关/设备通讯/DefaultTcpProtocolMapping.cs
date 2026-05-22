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
    public class DefaultTcpProtocolMapping : IDefaultTcpProtocolMapping
    {
        public string Header { get; set; }

        public string Length { get; set; }

        public IDefaultTcpProtocolMappingBody[] Body { get; set; }

        public string Checker { get; set; }

        public string Ender { get; set; }

        public XmlDocument XmlDocument { get; set; }
    }
}