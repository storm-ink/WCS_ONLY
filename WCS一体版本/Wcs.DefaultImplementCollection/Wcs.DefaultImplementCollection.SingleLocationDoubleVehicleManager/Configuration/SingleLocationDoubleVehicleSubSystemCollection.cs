using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework.Cfg;
namespace Wcs.DefaultImplementCollection.SingleLocationDoubleVehicleManager
{
    public class SingleLocationDoubleVehicleSubSystemCollection : Wcs.Framework.Cfg.ParticularDeviceCollection
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public SingleLocationDoubleVehicleSubSystemCollection(XmlNode node, WcsConfiguration configuration)
            : base(node, configuration)
        {
        }

        public override DeviceElement CreateDeviceElement(XmlNode deviceNode)
        {
            return new SingleLocationDoubleVehicleSubSystemElement(deviceNode, this.WcsConfiguration);
        }
    }
}
