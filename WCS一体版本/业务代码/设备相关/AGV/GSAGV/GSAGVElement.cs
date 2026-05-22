using System.Xml;
using Wcs.Framework.Cfg;

namespace BOE.设备相关.AGV.GSAGV
{
    public class GSAGVElement : Wcs.Framework.Cfg.TcpProtocolTaskableDeviceElement
    {
        public GSAGVElement(XmlNode node, WcsConfiguration configuration) : base(node, configuration)
        {
        }

        public GSAGVElement(XmlNode node, WcsConfiguration configuration, bool deserialize) : base(node, configuration, deserialize)
        {
        }

        protected override Wcs.Framework.Device CreateDevice()
        {
            GSAGVDevice _device = new GSAGVDevice(Name, No, ReceiveTimeout, ConnectTimeout, SendTimeout, AllowConcurrency, IPEndPoint, BindEndPoint, DataReceiver);
            return _device;

        }

        protected override void Deserialize()
        {
            base.Deserialize();
        }
    }
}
