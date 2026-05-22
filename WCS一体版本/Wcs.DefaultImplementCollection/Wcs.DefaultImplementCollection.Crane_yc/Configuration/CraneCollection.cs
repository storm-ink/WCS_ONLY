using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework.Cfg;
namespace Wcs.DefaultImplementCollection.Crane_yc
{
    public class CraneCollection :Wcs.Framework.Cfg.ParticularDeviceCollection
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public CraneCollection(XmlNode node, WcsConfiguration configuration)
            : base(node,configuration)
        {
        }

        public override DeviceElement CreateDeviceElement(XmlNode deviceNode)
        {
            return new CraneElement(deviceNode,this.WcsConfiguration);
        }
    }
}
