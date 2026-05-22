using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.AGV
{
    public sealed class AGVSubSystemLocationCollection:Wcs.Framework.Cfg.ParticularLocationCollection
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public AGVSubSystemLocationCollection(XmlNode node, WcsConfiguration configuration)
            : base(node, configuration, true)
        {
            
        }

        public override Wcs.Framework.Cfg.LocationElement CreateLocationElement(System.Xml.XmlNode LocationNode)
        {
            return new AGVSubSystemLocationElement(LocationNode, this, this.WcsConfiguration);
        }
    }
}
