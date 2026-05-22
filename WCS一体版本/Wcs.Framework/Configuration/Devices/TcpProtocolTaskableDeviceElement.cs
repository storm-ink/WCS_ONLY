using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
namespace Wcs.Framework.Cfg
{
    public abstract class TcpProtocolTaskableDeviceElement :TaskableDeviceElement
    {
        /// <summary>
        /// 表示网络端点的IP 地址和端口号
        /// </summary>
        public IPEndPoint IPEndPoint { get; protected set; }

        /// <summary>
        /// 表示通讯要绑定到的IP 地址和端口号
        /// </summary>
        public IPEndPoint BindEndPoint { get; protected set; }

        /// <summary>
        /// TCP协议数据接收器
        /// </summary>
        public IDataReceiver DataReceiver { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public TcpProtocolTaskableDeviceElement(XmlNode node, WcsConfiguration configuration)
            : this(node,configuration, true)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public TcpProtocolTaskableDeviceElement(XmlNode node,WcsConfiguration configuration, Boolean deserialize)
            : base(node,configuration,false)
        {
            var ip = GetAttribute<String>("ip");
            IPAddress ipAddress;
            if (!IPAddress.TryParse(ip, out ipAddress))
            {
                throw new InvalidCastException(string.Format("{0} 节点的 ip 属性值 {1} 无法转换为 {2} 类型",this.Node.GetXPath(),ip,typeof(IPAddress)));
            }

            var port = GetAttribute<Int32>("port");
            this.IPEndPoint=new IPEndPoint(ipAddress,port);

            var bindIp = GetAttributeOrDefault<String>("bindIp");
            var bindPort = GetAttributeOrDefault<Int32>("bindPort",0);
            if (!String.IsNullOrWhiteSpace(bindIp))
            {
                this.BindEndPoint = new IPEndPoint(IPAddress.Parse(bindIp), bindPort);
            }

            var dataReceiverName = GetAttribute<String>("dataReceiver");
            var dataReceiver = this.WcsConfiguration.DataReceiverCollection.DataReceiverElements.Where(x => string.Equals(x.Name, dataReceiverName, StringComparison.CurrentCultureIgnoreCase));

            var matchDataReceiverCount = dataReceiver.Count();
            if (matchDataReceiverCount > 1)
            {
                string msg=string.Format("{0} 属性 {1} 值 “{2}” 匹配到多个对象",
                    this.Node.GetXPath(),
                    "dataReceiver",
                    dataReceiverName
                    );

                throw new ConfigurationErrorsException(msg);
            }

            if (matchDataReceiverCount==0)
            {
                string msg = string.Format("{0} 属性 {1} 值 “{2}” 未匹配到任何对象",
                    this.Node.GetXPath(),
                    "dataReceiver",
                    dataReceiverName
                    );

                throw new ConfigurationErrorsException(msg);
            }

            this.DataReceiver = dataReceiver.Single().CreateDataReceiver(this.Name);
            if (this.DataReceiver is ThreadRunningLog _dataReceiver)
                _dataReceiver.Init(this.DataReceiver.Name);

            if (deserialize)
            {
                Deserialize();
            }
        }
    }
}
