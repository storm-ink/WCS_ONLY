using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.SingleLocationDoubleVehicleManager
{
    public sealed class SingleLocationDoubleVehicleSubSystemLocationCollection : ParticularLocationCollection
    {


        public SingleLocationDoubleVehicleSubSystemLocationCollection(XmlNode node, WcsConfiguration configuration)
            : base(node, configuration, true)
        {
        }


        public override LocationElement CreateLocationElement(XmlNode LocationNode)
        {
            return new SingleLocationDoubleVehicleSubSystemLocationElement(LocationNode, this, this.WcsConfiguration);
        }
    }
}
