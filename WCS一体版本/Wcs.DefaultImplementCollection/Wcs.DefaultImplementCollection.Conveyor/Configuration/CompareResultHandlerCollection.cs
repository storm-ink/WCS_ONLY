using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    public class CompareResultHandlerCollection:Wcs.Framework.Cfg.ConfigurationElement
    {
        public CompareResultHandlerElement[] CompareResultHandlerElements { get; private set; }
        public CompareResultHandlerCollection(XmlNode node, WcsConfiguration configuration)
            : base(node,configuration, true)
        {

        }

        protected override void Deserialize()
        {
            List<CompareResultHandlerElement> compareResultHandlerElements = new List<CompareResultHandlerElement>();
            foreach (XmlNode item in this.Node.SelectNodes("handler"))
            {
                CompareResultHandlerElement element = new CompareResultHandlerElement(item, this.WcsConfiguration);
                compareResultHandlerElements.Add(element);
            }

            this.CompareResultHandlerElements = compareResultHandlerElements.ToArray();
        }
    }
}
