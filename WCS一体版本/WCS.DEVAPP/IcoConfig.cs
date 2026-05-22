using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WCS.APP
{
    [XmlRoot("icoConfig")]
    public class IcoConfig
    {
        [XmlElement("ico")]
        public List<Ico> Settings { get; set; }
    }

    public class Ico
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("showText")]
        public string ShowText { get; set; }

        [XmlAttribute("path")]
        public string Path { get; set; }
    }
}
