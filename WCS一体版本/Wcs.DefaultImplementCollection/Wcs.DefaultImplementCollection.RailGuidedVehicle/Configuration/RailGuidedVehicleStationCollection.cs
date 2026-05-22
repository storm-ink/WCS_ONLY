using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    public class RailGuidedVehicleStationCollection : ParticularLocationCollection
    {


        public RailGuidedVehicleStationCollection(XmlNode node, WcsConfiguration configuration)
            : base(node, configuration, true)
        {
        }


        public override LocationElement CreateLocationElement(XmlNode LocationNode)
        {
            return new RailGuidedVehicleStationElement(LocationNode, this, this.WcsConfiguration);
        }
    }
}
