using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.AGV
{
    public class SinevaAGVElement : Wcs.Framework.Cfg.TcpProtocolTaskableDeviceElement
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public SinevaAGVElement(XmlNode node, WcsConfiguration configuration)
            : base(node, configuration)
        {

        }

        protected override void Deserialize()
        {
            base.Deserialize();
        }

        protected override Wcs.Framework.Device CreateDevice()
        {
            SinevaAGVDevice _device = new SinevaAGVDevice(Name, No, ReceiveTimeout, ConnectTimeout, SendTimeout, AllowConcurrency, IPEndPoint, BindEndPoint, DataReceiver);
            return _device;

        }
    }
}
