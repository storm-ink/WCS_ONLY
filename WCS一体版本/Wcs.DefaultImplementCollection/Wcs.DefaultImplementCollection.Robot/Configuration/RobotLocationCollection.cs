using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Robot
{
    public sealed class RobotLocationCollection : Wcs.Framework.Cfg.ParticularLocationCollection
    {
         /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public RobotLocationCollection(XmlNode node, WcsConfiguration configuration)
            : base(node,configuration, true)
        {
            
        }
        public override Framework.Cfg.LocationElement CreateLocationElement(System.Xml.XmlNode LocationNode)
        {
            return new RobotLocationElement(LocationNode,this, this.WcsConfiguration);
        }
    }
}
