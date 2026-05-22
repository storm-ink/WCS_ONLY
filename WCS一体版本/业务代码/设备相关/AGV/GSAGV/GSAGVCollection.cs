using Wcs.Framework.Cfg;

namespace BOE.设备相关.AGV.GSAGV
{
    public class GSAGVCollection : Wcs.Framework.Cfg.ParticularDeviceCollection
    {
        public GSAGVCollection(System.Xml.XmlNode node, WcsConfiguration configuration)
            : base(node, configuration)
        {
        }
        public override Wcs.Framework.Cfg.DeviceElement CreateDeviceElement(System.Xml.XmlNode deviceNode)
        {
            return new GSAGVElement(deviceNode, WcsConfiguration);
        }
    }
}
