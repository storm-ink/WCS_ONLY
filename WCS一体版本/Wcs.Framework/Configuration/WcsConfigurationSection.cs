using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.Framework.Cfg
{
    public class WcsConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("application")]
        public ApplicationElement ApplicationElement
        {
            get
            {
                return (ApplicationElement)this["application"];
            }
        }
    }
}
