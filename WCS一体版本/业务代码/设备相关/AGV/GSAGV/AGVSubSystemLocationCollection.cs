using System.Xml;
using Wcs.Framework.Cfg;

namespace BOE.设备相关.AGV.GSAGV
{
    public sealed class AGVSubSystemLocationCollection : ParticularLocationCollection
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public AGVSubSystemLocationCollection(XmlNode node, WcsConfiguration configuration)
            : base(node, configuration, true)
        {

        }

        public override LocationElement CreateLocationElement(XmlNode LocationNode)
        {
            return new AGVSubSystemLocationElement(LocationNode, this, this.WcsConfiguration);
        }
    }
}
