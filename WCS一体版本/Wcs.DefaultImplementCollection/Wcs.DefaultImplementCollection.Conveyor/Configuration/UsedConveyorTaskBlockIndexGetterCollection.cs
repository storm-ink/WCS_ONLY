using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    public class UsedConveyorTaskBlockIndexGetterCollection:Wcs.Framework.Cfg.ConfigurationElement
    {
        public UsedConveyorTaskBlockIndexGetterElement[] UsedConveyorTaskBlockIndexGetterElements { get; private set; }
        public UsedConveyorTaskBlockIndexGetterCollection(XmlNode node, WcsConfiguration configuration)
            : base(node,configuration, true)
        {

        }

        protected override void Deserialize()
        {
            List<UsedConveyorTaskBlockIndexGetterElement> items = new List<UsedConveyorTaskBlockIndexGetterElement>();
            foreach (XmlNode item in this.Node.SelectNodes("getter"))
            {
                UsedConveyorTaskBlockIndexGetterElement element = new UsedConveyorTaskBlockIndexGetterElement(item, this.WcsConfiguration);
                items.Add(element);
            }

            this.UsedConveyorTaskBlockIndexGetterElements = items.ToArray();
        }
    }
}
