using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    public sealed class ConveyorLocationCollection:Wcs.Framework.Cfg.ParticularLocationCollection
    {
         /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public ConveyorLocationCollection(XmlNode node, WcsConfiguration configuration)
            : base(node,configuration, true)
        {
            
        }
        public override Framework.Cfg.LocationElement CreateLocationElement(System.Xml.XmlNode LocationNode)
        {
            return new ConveyorLocationElement(LocationNode,this, this.WcsConfiguration);
        }
    }
}
