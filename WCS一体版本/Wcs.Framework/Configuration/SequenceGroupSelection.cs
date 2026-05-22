using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    public class SequenceGroupSelection : ConfigurationElement
    {
        List<SequenceGroupElement> sequenceGroupElements = new List<SequenceGroupElement>();
        public SequenceGroupElement[] SequenceGroupElements
        {
            get
            {
                return sequenceGroupElements.ToArray();
            }
        }

        public SequenceGroupSelection(XmlNode node)
            : base(node)
        {

        }
        protected override void Deserialize(System.Xml.XmlNode node)
        {
            if (node == null)
            {
                return ;
            }

            foreach (XmlNode item in node.SelectNodes("group"))
            {
                sequenceGroupElements.Add(new SequenceGroupElement(item,this));
            }
        }
    }
}
