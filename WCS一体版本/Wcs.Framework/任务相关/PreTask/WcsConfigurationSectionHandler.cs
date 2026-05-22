using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.FrameworkExtend.Cfg
{
    public class WcsConfigurationSectionHandler : IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            return new WcsConfiguration(section);
        }
    }
}
