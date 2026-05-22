using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
  public  class RailGuidedVehicleElement:Wcs.Framework.Cfg.TcpProtocolTaskableDeviceElement

    {
      /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
      public RailGuidedVehicleElement(XmlNode node, WcsConfiguration configuration)
            : base(node,configuration,true)
        {
        }

        protected override Wcs.Framework.Device CreateDevice()
        {
            RailGuidedVehicleDevice rgvDevice = new RailGuidedVehicleDevice(Name, No, ReceiveTimeout, ConnectTimeout, SendTimeout, IPEndPoint,BindEndPoint, DataReceiver);
            return rgvDevice;
        }
    }
}
