using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.Framework.Cfg
{
    public class DeviceSelection:ConfigurationElementCollection
    {

        protected override ConfigurationElement CreateNewElement()
        {
            throw new NotImplementedException();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            throw new NotImplementedException();
        }

        protected override void DeserializeElement(System.Xml.XmlReader reader, bool serializeCollectionKey)
        {
            base.DeserializeElement(reader, serializeCollectionKey);
        }
    }
}
