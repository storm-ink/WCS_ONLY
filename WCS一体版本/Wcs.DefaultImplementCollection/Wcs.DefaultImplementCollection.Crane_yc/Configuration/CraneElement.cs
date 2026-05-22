using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    public class CraneElement :Wcs.Framework.Cfg.TcpProtocolTaskableDeviceElement
    {
        public String ConveyorDeviceName { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public CraneElement(XmlNode node, WcsConfiguration configuration)
            : base(node,configuration,true)
        {
        }

        protected override Framework.Device CreateDevice()
        {
            CraneDevice craneDevice = new CraneDevice(Name, No, ReceiveTimeout, ConnectTimeout, SendTimeout, IPEndPoint, BindEndPoint, DataReceiver);
            return craneDevice;
        }
    }
}
